using Confluent.Kafka;
using KafkaTest.Models;
using KafkaTest.UI;
using System.Diagnostics;
using System.Text.Json;

namespace KafkaTest.Services;

public class KafkaConsumerService : IKafkaConsumerService
{
    private readonly KafkaConfig _config;
    private IConsumer<string, string>? _consumer;
    private readonly ConsumerMetrics _metrics = new();
    private readonly object _metricsLock = new();
    private readonly List<double> _processingTimes = new();
    private bool _isRunning;
    private string _currentTopic = string.Empty;

    public bool IsRunning => _isRunning;
    public string ConsumerId { get; private set; } = string.Empty;
    public string ConsumerGroup { get; private set; } = string.Empty;

    public KafkaConsumerService(KafkaConfig config)
    {
        _config = config;
    }

    public async Task StartAsync(string topic, string consumerGroup, string consumerId, CancellationToken cancellationToken)
    {
        if (_isRunning)
        {
            throw new InvalidOperationException("Consumer is already running");
        }

        ConsumerId = consumerId;
        ConsumerGroup = consumerGroup;
        _currentTopic = topic;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _config.BootstrapServers,
            GroupId = consumerGroup,
            ClientId = consumerId,
            AutoOffsetReset = ParseAutoOffsetReset(_config.Consumer.AutoOffsetReset),
            EnableAutoCommit = _config.Consumer.EnableAutoCommit,
            EnableAutoOffsetStore = _config.Consumer.EnableAutoOffsetStore,
            SessionTimeoutMs = _config.Consumer.SessionTimeoutMs,
            MaxPollIntervalMs = _config.Consumer.MaxPollIntervalMs,
            IsolationLevel = ParseIsolationLevel(_config.Consumer.IsolationLevel)
        };

        _consumer = new ConsumerBuilder<string, string>(consumerConfig)
            .SetErrorHandler((_, error) =>
            {
                WriteColoredLine.WriteColorLine($"[CONSUMER {consumerId}] ERROR: {error.Reason}", ConsoleColor.Yellow, ConsoleColor.DarkRed);
                lock (_metricsLock)
                {
                    _metrics.ErrorsCount++;
                }
            })
            .SetPartitionsAssignedHandler((_, partitions) =>
            {
                WriteColoredLine.WriteColorLine($"[CONSUMER {consumerId}] Partitions assigned: {string.Join(", ", partitions.Select(p => p.Partition.Value))}");
            })
            .SetPartitionsRevokedHandler((_, partitions) =>
            {
                WriteColoredLine.WriteColorLine($"[CONSUMER {consumerId}] Partitions revoked: {string.Join(", ", partitions.Select(p => p.Partition.Value))}");
            })
            .Build();

        _consumer.Subscribe(topic);
        _isRunning = true;

        lock (_metricsLock)
        {
            _metrics.ConsumerId = consumerId;
            _metrics.ConsumerGroup = consumerGroup;
        }

        WriteColoredLine.WriteColorLine($"[CONSUMER {consumerId}] Started | Group: {consumerGroup} | Topic: {topic}");

        await Task.Run(() => ConsumeLoop(cancellationToken), cancellationToken);
    }

    private void ConsumeLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer!.Consume(TimeSpan.FromMilliseconds(1000));
                    if (consumeResult != null)
                    {
                        ProcessMessage(consumeResult);
                    }
                }
                catch (ConsumeException ex)
                {
                    WriteColoredLine.WriteColorLine($"[CONSUMER {ConsumerId}] Consume error: {ex.Error.Reason}", ConsoleColor.Yellow, ConsoleColor.DarkRed);
                    lock (_metricsLock)
                    {
                        _metrics.ErrorsCount++;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            WriteColoredLine.WriteColorLine($"[CONSUMER {ConsumerId}] Consumption canceled");
        }
        finally
        {
            _isRunning = false;
        }
    }

    private void ProcessMessage(ConsumeResult<string, string> result)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var message = JsonSerializer.Deserialize<TestMessage>(result.Message.Value);
            sw.Stop();

            // Get high watermark offset
            var watermarkOffsets = _consumer!.GetWatermarkOffsets(result.TopicPartition);
            var highWatermark = watermarkOffsets.High.Value;

            lock (_metricsLock)
            {
                _metrics.MessagesConsumed++;
                _metrics.LastConsumedTimestamp = DateTime.UtcNow;
                _metrics.CurrentOffset = result.Offset.Value;
                _metrics.HighWatermarkOffset = highWatermark;

                _processingTimes.Add(sw.Elapsed.TotalMilliseconds);
                _metrics.AverageProcessingTimeMs = _processingTimes.Average();

                // Keep only last 1000 processing times
                if (_processingTimes.Count > 1000)
                {
                    _processingTimes.RemoveAt(0);
                }
            }

            var lag = highWatermark - result.Offset.Value - 1; // -1 because high watermark is next offset to be written
            WriteColoredLine.WriteColorLine($"[CONSUMER {ConsumerId}] ✓ Partition: {result.Partition.Value} | " +
                            $"Offset: {result.Offset.Value} | " +
                            $"Lag: {lag} | " +
                            $"Message: {message?.ToString() ?? "null"}");

            // Manual commit if auto-commit is disabled
            if (!_config.Consumer.EnableAutoCommit)
            {
                _consumer!.Commit(result);
            }
        }
        catch (JsonException ex)
        {
            sw.Stop();
            WriteColoredLine.WriteColorLine($"[CONSUMER {ConsumerId}] Deserialization error: {ex.Message}", ConsoleColor.Yellow, ConsoleColor.DarkRed);
            lock (_metricsLock)
            {
                _metrics.ErrorsCount++;
            }
        }
    }

    public void SeekToOffset(long offset)
    {
        if (_consumer == null || !_isRunning)
        {
            WriteColoredLine.WriteColorLine($"[CONSUMER {ConsumerId}] Cannot seek: consumer not running");
            return;
        }

        var assignment = _consumer.Assignment;
        foreach (var partition in assignment)
        {
            _consumer.Seek(new TopicPartitionOffset(partition, new Offset(offset)));
            WriteColoredLine.WriteColorLine($"[CONSUMER {ConsumerId}] Seeked to offset {offset} on partition {partition.Partition.Value}");
        }
    }

    public void SeekToBeginning()
    {
        if (_consumer == null || !_isRunning)
        {
            WriteColoredLine.WriteColorLine($"[CONSUMER {ConsumerId}] Cannot seek: consumer not running");
            return;
        }

        var assignment = _consumer.Assignment;
        foreach (var partition in assignment)
        {
            _consumer.Seek(new TopicPartitionOffset(partition, Offset.Beginning));
            WriteColoredLine.WriteColorLine($"[CONSUMER {ConsumerId}] Seeked to beginning on partition {partition.Partition.Value}");
        }
    }

    public void SeekToEnd()
    {
        if (_consumer == null || !_isRunning)
        {
            WriteColoredLine.WriteColorLine($"[CONSUMER {ConsumerId}] Cannot seek: consumer not running");
            return;
        }

        var assignment = _consumer.Assignment;
        foreach (var partition in assignment)
        {
            _consumer.Seek(new TopicPartitionOffset(partition, Offset.End));
            WriteColoredLine.WriteColorLine($"[CONSUMER {ConsumerId}] Seeked to end on partition {partition.Partition.Value}");
        }
    }

    public void Stop()
    {
        if (!_isRunning)
        {
            return;
        }

        _isRunning = false;
        WriteColoredLine.WriteColorLine($"[CONSUMER {ConsumerId}] Stopping...");
    }

    public ConsumerMetrics GetMetrics()
    {
        lock (_metricsLock)
        {
            return new ConsumerMetrics
            {
                ConsumerId = _metrics.ConsumerId,
                ConsumerGroup = _metrics.ConsumerGroup,
                MessagesConsumed = _metrics.MessagesConsumed,
                ErrorsCount = _metrics.ErrorsCount,
                AverageProcessingTimeMs = _metrics.AverageProcessingTimeMs,
                LastConsumedTimestamp = _metrics.LastConsumedTimestamp,
                CurrentOffset = _metrics.CurrentOffset,
                HighWatermarkOffset = _metrics.HighWatermarkOffset
            };
        }
    }

    public void ResetMetrics()
    {
        lock (_metricsLock)
        {
            _metrics.MessagesConsumed = 0;
            _metrics.ErrorsCount = 0;
            _metrics.AverageProcessingTimeMs = 0;
            _metrics.LastConsumedTimestamp = null;
            _processingTimes.Clear();
        }
        WriteColoredLine.WriteColorLine($"[CONSUMER {ConsumerId}] Metrics reset");
    }

    private static AutoOffsetReset ParseAutoOffsetReset(string value) => value.ToLower() switch
    {
        "earliest" => AutoOffsetReset.Earliest,
        "latest" => AutoOffsetReset.Latest,
        _ => AutoOffsetReset.Earliest
    };

    private static IsolationLevel ParseIsolationLevel(string value) => value.ToLower() switch
    {
        "read_uncommitted" => IsolationLevel.ReadUncommitted,
        "read_committed" => IsolationLevel.ReadCommitted,
        _ => IsolationLevel.ReadCommitted
    };

    public void Dispose()
    {
        Stop();
        _consumer?.Close();
        _consumer?.Dispose();
        WriteColoredLine.WriteColorLine($"[CONSUMER {ConsumerId}] Disposed");
    }
}
