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
        return $"[{status}] {ConsumerId} | Group: {ConsumerGroup} | Topic: {Topic} | Messages: {MessageCount} | Last: {lastMsg}";
    }
}
