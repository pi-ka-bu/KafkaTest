using KafkaTest.Models;
using KafkaTest.UI;
using System.Collections.Concurrent;

namespace KafkaTest.Services;

public class ConsumerManager : IDisposable
{
    private readonly KafkaConfig _config;
    private readonly ConcurrentDictionary<string, ConsumerInstance> _consumers = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _cancellationTokens = new();

    public ConsumerManager(KafkaConfig config)
    {
        _config = config;
    }

    public async Task<bool> StartConsumerAsync(string topic, string consumerGroup, string consumerId)
    {
        if (_consumers.ContainsKey(consumerId))
        {
            WriteColoredLine.WriteColorLine($"[MANAGER] Consumer {consumerId} already exists");
            return false;
        }

        try
        {
            var consumer = new KafkaConsumerService(_config);
            var cts = new CancellationTokenSource();
            var instance = new ConsumerInstance(consumer, topic, consumerGroup);

            _consumers.TryAdd(consumerId, instance);
            _cancellationTokens.TryAdd(consumerId, cts);

            // Start consumer in background task
            _ = Task.Run(async () =>
            {
                try
                {
                    await consumer.StartAsync(topic, consumerGroup, consumerId, cts.Token);
                }
                catch (Exception ex)
                {
                    WriteColoredLine.WriteColorLine($"[MANAGER] Consumer {consumerId} error: {ex.Message}", ConsoleColor.Yellow, ConsoleColor.DarkRed);
                }
            }, cts.Token);

            // Give it a moment to start
            await Task.Delay(500);

            WriteColoredLine.WriteColorLine($"[MANAGER] Consumer {consumerId} started successfully");
            return true;
        }
        catch (Exception ex)
        {
            WriteColoredLine.WriteColorLine($"[MANAGER] Failed to start consumer {consumerId}: {ex.Message}", ConsoleColor.Yellow, ConsoleColor.DarkRed);
            return false;
        }
    }

    public bool StopConsumer(string consumerId)
    {
        if (!_consumers.TryGetValue(consumerId, out var instance))
        {
            WriteColoredLine.WriteColorLine($"[MANAGER] Consumer {consumerId} not found");
            return false;
        }

        try
        {
            if (_cancellationTokens.TryRemove(consumerId, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }

            instance.Consumer.Stop();
            instance.Consumer.Dispose();
            _consumers.TryRemove(consumerId, out _);

            WriteColoredLine.WriteColorLine($"[MANAGER] Consumer {consumerId} stopped successfully");
            return true;
        }
        catch (Exception ex)
        {
            WriteColoredLine.WriteColorLine($"[MANAGER] Failed to stop consumer {consumerId}: {ex.Message}", ConsoleColor.Yellow, ConsoleColor.DarkRed);
            return false;
        }
    }

    public void StopAllConsumers()
    {
        WriteColoredLine.WriteColorLine("[MANAGER] Stopping all consumers...");
        var consumerIds = _consumers.Keys.ToList();
        foreach (var consumerId in consumerIds)
        {
            StopConsumer(consumerId);
        }
    }

    public List<ConsumerInfo> GetActiveConsumers()
    {
        var activeConsumers = new List<ConsumerInfo>();

        foreach (var kvp in _consumers)
        {
            var consumerId = kvp.Key;
            var instance = kvp.Value;
            var metrics = instance.Consumer.GetMetrics();

            var info = new ConsumerInfo(consumerId, instance.ConsumerGroup, instance.Topic)
            {
                IsRunning = instance.Consumer.IsRunning,
                MessageCount = (int)metrics.MessagesConsumed,
                LastMessageTimestamp = metrics.LastConsumedTimestamp,
                ProcessingTimeMs = (long)metrics.AverageProcessingTimeMs
            };

            activeConsumers.Add(info);
        }

        return activeConsumers;
    }

    public ConsumerMetrics? GetConsumerMetrics(string consumerId)
    {
        if (_consumers.TryGetValue(consumerId, out var instance))
        {
            return instance.Consumer.GetMetrics();
        }
        return null;
    }

    public Dictionary<string, ConsumerMetrics> GetAllConsumerMetrics()
    {
        var allMetrics = new Dictionary<string, ConsumerMetrics>();

        foreach (var kvp in _consumers)
        {
            allMetrics[kvp.Key] = kvp.Value.Consumer.GetMetrics();
        }

        return allMetrics;
    }

    public IKafkaConsumerService? GetConsumer(string consumerId)
    {
        return _consumers.TryGetValue(consumerId, out var instance) ? instance.Consumer : null;
    }

    public int GetActiveConsumerCount() => _consumers.Count(c => c.Value.Consumer.IsRunning);

    public int GetTotalConsumerCount() => _consumers.Count;

    public void Dispose()
    {
        StopAllConsumers();
        WriteColoredLine.WriteColorLine("[MANAGER] Disposed");
    }

    private class ConsumerInstance
    {
        public IKafkaConsumerService Consumer { get; }
        public string Topic { get; }
        public string ConsumerGroup { get; }

        public ConsumerInstance(IKafkaConsumerService consumer, string topic, string consumerGroup)
        {
            Consumer = consumer;
            Topic = topic;
            ConsumerGroup = consumerGroup;
        }
    }
}
