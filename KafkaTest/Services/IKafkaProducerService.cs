using KafkaTest.Models;

namespace KafkaTest.Services;

public interface IKafkaProducerService : IDisposable
{
    Task<bool> ProduceAsync(string topic, TestMessage message);
    Task<int> ProduceBatchAsync(string topic, int count, string contentPrefix = "Test message");
    ProducerMetrics GetMetrics();
    void ResetMetrics();
}

public class ProducerMetrics
{
    public long MessagesSent { get; set; }
    public long MessagesFailed { get; set; }
    public double AverageLatencyMs { get; set; }
    public double P95LatencyMs { get; set; }
    public DateTime? LastSentTimestamp { get; set; }
    public List<double> LatencyHistory { get; set; } = new();

    public override string ToString()
    {
        var successRate = MessagesSent + MessagesFailed > 0
            ? (MessagesSent * 100.0 / (MessagesSent + MessagesFailed))
            : 0;

        return $"Sent: {MessagesSent} | Failed: {MessagesFailed} | Success Rate: {successRate:F2}% | " +
               $"Avg Latency: {AverageLatencyMs:F2}ms | P95: {P95LatencyMs:F2}ms";
    }
}
