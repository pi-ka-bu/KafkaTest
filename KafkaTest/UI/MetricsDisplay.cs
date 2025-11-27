using KafkaTest.Services;

namespace KafkaTest.UI;

public static class MetricsDisplay
{
    public static void DisplayProducerMetrics(ProducerMetrics metrics)
    {
        WriteColoredLine.WriteColorLine();
        WriteColoredLine.WriteColorLine("═══════════════════════════════════════════════════════");
        WriteColoredLine.WriteColorLine("              PRODUCER METRICS");
        WriteColoredLine.WriteColorLine("═══════════════════════════════════════════════════════");
        WriteColoredLine.WriteColorLine($"  Messages Sent:       {metrics.MessagesSent}");
        WriteColoredLine.WriteColorLine($"  Messages Failed:     {metrics.MessagesFailed}", ConsoleColor.Yellow, ConsoleColor.DarkRed);

        var successRate = metrics.MessagesSent + metrics.MessagesFailed > 0
            ? (metrics.MessagesSent * 100.0 / (metrics.MessagesSent + metrics.MessagesFailed))
            : 0;
        WriteColoredLine.WriteColorLine($"  Success Rate:        {successRate:F2}%");
        WriteColoredLine.WriteColorLine($"  Avg Latency:         {metrics.AverageLatencyMs:F2} ms");
        WriteColoredLine.WriteColorLine($"  P95 Latency:         {metrics.P95LatencyMs:F2} ms");
        WriteColoredLine.WriteColorLine($"  Last Sent:           {metrics.LastSentTimestamp?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}");
        WriteColoredLine.WriteColorLine("═══════════════════════════════════════════════════════");
        WriteColoredLine.WriteColorLine();
    }

    public static void DisplayConsumerMetrics(ConsumerMetrics metrics)
    {
        WriteColoredLine.WriteColorLine();
        WriteColoredLine.WriteColorLine("═══════════════════════════════════════════════════════");
        WriteColoredLine.WriteColorLine($"         CONSUMER METRICS - {metrics.ConsumerId}");
        WriteColoredLine.WriteColorLine("═══════════════════════════════════════════════════════");
        WriteColoredLine.WriteColorLine($"  Consumer ID:         {metrics.ConsumerId}");
        WriteColoredLine.WriteColorLine($"  Consumer Group:      {metrics.ConsumerGroup}");
        WriteColoredLine.WriteColorLine($"  Messages Consumed:   {metrics.MessagesConsumed}");
        WriteColoredLine.WriteColorLine($"  Errors:              {metrics.ErrorsCount}", ConsoleColor.Yellow, ConsoleColor.DarkRed);
        WriteColoredLine.WriteColorLine($"  Avg Processing:      {metrics.AverageProcessingTimeMs:F2} ms");
        WriteColoredLine.WriteColorLine($"  Current Offset:      {metrics.CurrentOffset}");
        WriteColoredLine.WriteColorLine($"  High Watermark:      {metrics.HighWatermarkOffset?.ToString() ?? "N/A"}");
        WriteColoredLine.WriteColorLine($"  Lag:                 {metrics.Lag}");
        WriteColoredLine.WriteColorLine($"  Last Consumed:       {metrics.LastConsumedTimestamp?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}");
        WriteColoredLine.WriteColorLine("═══════════════════════════════════════════════════════");
        WriteColoredLine.WriteColorLine();
    }

    public static void DisplayAllConsumerMetrics(Dictionary<string, ConsumerMetrics> allMetrics)
    {
        WriteColoredLine.WriteColorLine();
        WriteColoredLine.WriteColorLine("═══════════════════════════════════════════════════════════════════════════════════════════");
        WriteColoredLine.WriteColorLine("                                    ALL CONSUMER METRICS");
        WriteColoredLine.WriteColorLine("═══════════════════════════════════════════════════════════════════════════════════════════");

        if (allMetrics.Count == 0)
        {
            WriteColoredLine.WriteColorLine("  No active consumers");
        }
        else
        {
            WriteColoredLine.WriteColorLine($"{"Consumer ID",-20} {"Group",-15} {"Consumed",-10} {"Errors",-8} {"Lag",-8} {"Avg (ms)",-10}");
            WriteColoredLine.WriteColorLine(new string('─', 95));

            foreach (var kvp in allMetrics)
            {
                var m = kvp.Value;
                WriteColoredLine.WriteColorLine($"{m.ConsumerId,-20} {m.ConsumerGroup,-15} {m.MessagesConsumed,-10} {m.ErrorsCount,-8} {m.Lag,-8} {m.AverageProcessingTimeMs,-10:F2}");
            }
        }

        WriteColoredLine.WriteColorLine("═══════════════════════════════════════════════════════════════════════════════════════════");
        WriteColoredLine.WriteColorLine();
    }

    public static void DisplayAggregateMetrics(ProducerMetrics producerMetrics, Dictionary<string, ConsumerMetrics> consumerMetrics)
    {
        WriteColoredLine.WriteColorLine();
        WriteColoredLine.WriteColorLine("═══════════════════════════════════════════════════════");
        WriteColoredLine.WriteColorLine("              AGGREGATE METRICS");
        WriteColoredLine.WriteColorLine("═══════════════════════════════════════════════════════");
        WriteColoredLine.WriteColorLine();
        WriteColoredLine.WriteColorLine("  PRODUCER:");
        WriteColoredLine.WriteColorLine($"    Total Sent:        {producerMetrics.MessagesSent}");
        WriteColoredLine.WriteColorLine($"    Failed:            {producerMetrics.MessagesFailed}", ConsoleColor.Yellow, ConsoleColor.DarkRed);
        WriteColoredLine.WriteColorLine($"    Avg Latency:       {producerMetrics.AverageLatencyMs:F2} ms");
        WriteColoredLine.WriteColorLine();
        WriteColoredLine.WriteColorLine("  CONSUMERS:");
        WriteColoredLine.WriteColorLine($"    Active Count:      {consumerMetrics.Count}");

        if (consumerMetrics.Count > 0)
        {
            var totalConsumed = consumerMetrics.Sum(c => c.Value.MessagesConsumed);
            var totalErrors = consumerMetrics.Sum(c => c.Value.ErrorsCount);
            var avgProcessing = consumerMetrics.Average(c => c.Value.AverageProcessingTimeMs);
            var totalLag = consumerMetrics.Sum(c => c.Value.Lag);

            WriteColoredLine.WriteColorLine($"    Total Consumed:    {totalConsumed}");
            WriteColoredLine.WriteColorLine($"    Total Errors:      {totalErrors}", ConsoleColor.Yellow, ConsoleColor.DarkRed);
            WriteColoredLine.WriteColorLine($"    Avg Processing:    {avgProcessing:F2} ms");
            WriteColoredLine.WriteColorLine($"    Total Lag:         {totalLag}");
        }
        else
        {
            WriteColoredLine.WriteColorLine("    No active consumers");
        }

        WriteColoredLine.WriteColorLine("═══════════════════════════════════════════════════════");
        WriteColoredLine.WriteColorLine();
    }
}
