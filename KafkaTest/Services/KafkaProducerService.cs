using Confluent.Kafka;
using KafkaTest.Models;
using KafkaTest.UI;
using System.Diagnostics;
using System.Text.Json;

namespace KafkaTest.Services;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<string, string> _producer;
    private readonly ProducerMetrics _metrics = new();
    private readonly object _metricsLock = new();

    public KafkaProducerService(KafkaConfig config)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config.BootstrapServers,
            Acks = ParseAcks(config.Producer.Acks),
            MessageSendMaxRetries = config.Producer.Retries,
            MaxInFlight = config.Producer.MaxInFlight,
            LingerMs = config.Producer.LingerMs,
            CompressionType = ParseCompressionType(config.Producer.CompressionType),
            RequestTimeoutMs = config.Producer.RequestTimeoutMs,
            EnableIdempotence = config.Producer.EnableIdempotence,
            ClientId = $"producer-{Guid.NewGuid():N}"
        };

        _producer = new ProducerBuilder<string, string>(producerConfig)
            .SetErrorHandler((_, error) =>
            {
                WriteColoredLine.WriteColorLine($"[PRODUCER ERROR] {error.Reason}", ConsoleColor.Yellow, ConsoleColor.DarkRed);
            })
            .Build();
    }

    public async Task<bool> ProduceAsync(string topic, TestMessage message)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var json = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<string, string>
            {
                Key = message.Id.ToString(),
                Value = json,
                Timestamp = new Timestamp(message.Timestamp)
            };

            var result = await _producer.ProduceAsync(topic, kafkaMessage);
            sw.Stop();

            lock (_metricsLock)
            {
                _metrics.MessagesSent++;
                _metrics.LastSentTimestamp = DateTime.UtcNow;
                _metrics.LatencyHistory.Add(sw.Elapsed.TotalMilliseconds);
                UpdateLatencyMetrics();
            }

            WriteColoredLine.WriteColorLine($"[PRODUCER] ✓ Message sent to {topic} | Partition: {result.Partition.Value} | " +
                            $"Offset: {result.Offset.Value} | Latency: {sw.Elapsed.TotalMilliseconds:F2}ms");
            return true;
        }
        catch (ProduceException<string, string> ex)
        {
            sw.Stop();
            lock (_metricsLock)
            {
                _metrics.MessagesFailed++;
            }

            WriteColoredLine.WriteColorLine($"[PRODUCER ERROR] Failed to send message: {ex.Error.Reason}", ConsoleColor.Yellow, ConsoleColor.DarkRed);
            return false;
        }
    }

    public async Task<int> ProduceBatchAsync(string topic, int count, string contentPrefix = "Test message")
    {
        WriteColoredLine.WriteColorLine($"[PRODUCER] Sending {count} messages to {topic}...");
        var successCount = 0;
        var sw = Stopwatch.StartNew();

        for (int i = 1; i <= count; i++)
        {
            var message = new TestMessage($"{contentPrefix} {i}", i);
            if (await ProduceAsync(topic, message))
            {
                successCount++;
            }

            // Small delay to avoid overwhelming the broker during testing
            if (i % 100 == 0)
            {
                await Task.Delay(10);
            }
        }

        sw.Stop();
        var throughput = count / sw.Elapsed.TotalSeconds;
        WriteColoredLine.WriteColorLine($"[PRODUCER] Batch complete: {successCount}/{count} sent | " +
                         $"Duration: {sw.Elapsed.TotalSeconds:F2}s | " +
                         $"Throughput: {throughput:F2} msg/s");

        return successCount;
    }

    public ProducerMetrics GetMetrics()
    {
        lock (_metricsLock)
        {
            return new ProducerMetrics
            {
                MessagesSent = _metrics.MessagesSent,
                MessagesFailed = _metrics.MessagesFailed,
                AverageLatencyMs = _metrics.AverageLatencyMs,
                P95LatencyMs = _metrics.P95LatencyMs,
                LastSentTimestamp = _metrics.LastSentTimestamp,
                LatencyHistory = new List<double>(_metrics.LatencyHistory)
            };
        }
    }

    public void ResetMetrics()
    {
        lock (_metricsLock)
        {
            _metrics.MessagesSent = 0;
            _metrics.MessagesFailed = 0;
            _metrics.AverageLatencyMs = 0;
            _metrics.P95LatencyMs = 0;
            _metrics.LastSentTimestamp = null;
            _metrics.LatencyHistory.Clear();
        }
        WriteColoredLine.WriteColorLine("[PRODUCER] Metrics reset");
    }

    private void UpdateLatencyMetrics()
    {
        if (_metrics.LatencyHistory.Count == 0) return;

        _metrics.AverageLatencyMs = _metrics.LatencyHistory.Average();

        var sorted = _metrics.LatencyHistory.OrderBy(x => x).ToList();
        var p95Index = (int)Math.Ceiling(sorted.Count * 0.95) - 1;
        _metrics.P95LatencyMs = sorted[Math.Max(0, p95Index)];

        // Keep only last 1000 latency measurements to avoid memory growth
        if (_metrics.LatencyHistory.Count > 1000)
        {
            _metrics.LatencyHistory.RemoveRange(0, _metrics.LatencyHistory.Count - 1000);
        }
    }

    private static Acks ParseAcks(string acks) => acks.ToLower() switch
    {
        "all" or "-1" => Acks.All,
        "1" => Acks.Leader,
        "0" => Acks.None,
        _ => Acks.All
    };

    private static CompressionType ParseCompressionType(string type) => type.ToLower() switch
    {
        "none" => CompressionType.None,
        "gzip" => CompressionType.Gzip,
        "snappy" => CompressionType.Snappy,
        "lz4" => CompressionType.Lz4,
        "zstd" => CompressionType.Zstd,
        _ => CompressionType.Snappy
    };

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
        WriteColoredLine.WriteColorLine("[PRODUCER] Disposed");
    }
}
