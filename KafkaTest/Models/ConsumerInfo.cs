namespace KafkaTest.Models;

public class ConsumerInfo
{
    public string ConsumerId { get; set; } = string.Empty;
    public string ConsumerGroup { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public bool IsRunning { get; set; }
    public int MessageCount { get; set; }
    public DateTime? LastMessageTimestamp { get; set; }
    public DateTime StartedAt { get; set; }
    public long ProcessingTimeMs { get; set; }
    public int[]? AssignedPartitions { get; set; }  // null = all partitions, array = specific partitions

    public ConsumerInfo(string consumerId, string consumerGroup, string topic)
    {
        ConsumerId = consumerId;
        ConsumerGroup = consumerGroup;
        Topic = topic;
        IsRunning = false;
        MessageCount = 0;
        StartedAt = DateTime.UtcNow;
    }

    public override string ToString()
    {
        var status = IsRunning ? "RUNNING" : "STOPPED";
        var lastMsg = LastMessageTimestamp?.ToString("HH:mm:ss") ?? "N/A";
        var partitionsInfo = AssignedPartitions != null
            ? $"Partitions: [{string.Join(", ", AssignedPartitions)}]"
            : "Partitions: [All]";
        return $"[{status}] {ConsumerId} | Group: {ConsumerGroup} | Topic: {Topic} | {partitionsInfo} | Messages: {MessageCount} | Last: {lastMsg}";
    }
}
