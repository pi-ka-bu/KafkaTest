using KafkaTest.Models;

namespace KafkaTest.Services;

public interface IKafkaConsumerService : IDisposable
{
    Task StartAsync(string topic, string consumerGroup, string consumerId, CancellationToken cancellationToken, int[]? partitions = null);
    void Stop();
    ConsumerMetrics GetMetrics();
    void ResetMetrics();
    bool IsRunning { get; }
    string ConsumerId { get; }
    string ConsumerGroup { get; }
    void SeekToOffset(long offset);
    void SeekToBeginning();
    void SeekToEnd();
}

public class ConsumerMetrics
{
    public string ConsumerId { get; set; } = string.Empty;
    public string ConsumerGroup { get; set; } = string.Empty;
    public long MessagesConsumed { get; set; }
    public long ErrorsCount { get; set; }
    public double AverageProcessingTimeMs { get; set; }
    public DateTime? LastConsumedTimestamp { get; set; }
    public long CurrentOffset { get; set; }
    public long? HighWatermarkOffset { get; set; }
    public long Lag => HighWatermarkOffset.HasValue ? HighWatermarkOffset.Value - CurrentOffset : 0;
    public int[]? AssignedPartitions { get; set; }  // null = all partitions (subscribed), array = specific partitions (assigned)

    public override string ToString()
    {
        var partitionsInfo = AssignedPartitions != null
            ? $"Partitions: [{string.Join(", ", AssignedPartitions)}]"
            : "Partitions: [All]";
        return $"[{ConsumerId}] Group: {ConsumerGroup} | {partitionsInfo} | Consumed: {MessagesConsumed} | " +
               $"Errors: {ErrorsCount} | Avg Processing: {AverageProcessingTimeMs:F2}ms | " +
               $"Lag: {Lag} | Offset: {CurrentOffset}";
    }
}
