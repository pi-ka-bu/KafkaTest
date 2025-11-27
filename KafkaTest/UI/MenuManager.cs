using KafkaTest.Models;
using KafkaTest.Services;

namespace KafkaTest.UI;

public class MenuManager
{
    private readonly KafkaConfig _config;
    private readonly IKafkaProducerService _producer;
    private readonly ConsumerManager _consumerManager;
    private bool _running = true;

    public MenuManager(KafkaConfig config, IKafkaProducerService producer, ConsumerManager consumerManager)
    {
        _config = config;
        _producer = producer;
        _consumerManager = consumerManager;
    }

    public async Task RunAsync()
    {
        Console.Clear();
        DisplayHeader();

        while (_running)
        {
            DisplayMenu();
            var choice = Console.ReadLine()?.Trim();

            try
            {
                await ProcessChoice(choice);
            }
            catch (Exception ex)
            {
                WriteColoredLine.WriteColorLine($"\n[ERROR] {ex.Message}\n", ConsoleColor.Yellow, ConsoleColor.DarkRed);
                WriteColoredLine.WriteColorLine("Press any key to continue...");
                Console.ReadKey();
            }
        }
    }

    private static void DisplayHeader()
    {
        WriteColoredLine.WriteColorLine("╔═══════════════════════════════════════════════════════╗", ConsoleColor.Cyan);
        WriteColoredLine.WriteColorLine("║         KAFKA TESTING APPLICATION - .NET 10.0         ║", ConsoleColor.Cyan);
        WriteColoredLine.WriteColorLine("╚═══════════════════════════════════════════════════════╝", ConsoleColor.Cyan);
        WriteColoredLine.WriteColorLine();
    }

    private void DisplayMenu()
    {
        WriteColoredLine.WriteColorLine("\n┌────────────────────────────────────────────────────────┐");
        WriteColoredLine.WriteColorLine("│                    MAIN MENU                           │");
        WriteColoredLine.WriteColorLine("├────────────────────────────────────────────────────────┤");
        WriteColoredLine.WriteColorLine("│  PRODUCER OPTIONS:                                     │");
        WriteColoredLine.WriteColorLine("│    1. Send Single Message                              │");
        WriteColoredLine.WriteColorLine("│    2. Send Batch Messages                              │");
        WriteColoredLine.WriteColorLine("│    3. View Producer Metrics                            │");
        WriteColoredLine.WriteColorLine("│    4. Reset Producer Metrics                           │");
        WriteColoredLine.WriteColorLine("│                                                        │");
        WriteColoredLine.WriteColorLine("│  CONSUMER OPTIONS:                                     │");
        WriteColoredLine.WriteColorLine("│    5. Start New Consumer                               │");
        WriteColoredLine.WriteColorLine("│    6. Stop Consumer                                    │");
        WriteColoredLine.WriteColorLine("│    7. List Active Consumers                            │");
        WriteColoredLine.WriteColorLine("│    8. View Consumer Metrics                            │");
        WriteColoredLine.WriteColorLine("│    9. View All Consumer Metrics                        │");
        WriteColoredLine.WriteColorLine("│                                                        │");
        WriteColoredLine.WriteColorLine("│  OFFSET MANAGEMENT:                                    │");
        WriteColoredLine.WriteColorLine("│   10. Seek to Specific Offset                          │");
        WriteColoredLine.WriteColorLine("│   11. Seek to Beginning                                │");
        WriteColoredLine.WriteColorLine("│   12. Seek to End                                      │");
        WriteColoredLine.WriteColorLine("│                                                        │");
        WriteColoredLine.WriteColorLine("│  GENERAL:                                              │");
        WriteColoredLine.WriteColorLine("│   13. View Aggregate Metrics                           │");
        WriteColoredLine.WriteColorLine("│   14. View Configuration                               │");
        WriteColoredLine.WriteColorLine("│   15. Stop All Consumers                               │");
        WriteColoredLine.WriteColorLine("│    0. Exit                                             │");
        WriteColoredLine.WriteColorLine("└────────────────────────────────────────────────────────┘");
        Console.Write("\nEnter choice: \n");
    }

    private async Task ProcessChoice(string? choice)
    {
        WriteColoredLine.WriteColorLine();

        switch (choice)
        {
            case "1":
                await SendSingleMessage();
                break;
            case "2":
                await SendBatchMessages();
                break;
            case "3":
                ViewProducerMetrics();
                break;
            case "4":
                ResetProducerMetrics();
                break;
            case "5":
                await StartConsumer();
                break;
            case "6":
                StopConsumer();
                break;
            case "7":
                ListActiveConsumers();
                break;
            case "8":
                ViewConsumerMetrics();
                break;
            case "9":
                ViewAllConsumerMetrics();
                break;
            case "10":
                await SeekToOffset();
                break;
            case "11":
                SeekToBeginning();
                break;
            case "12":
                SeekToEnd();
                break;
            case "13":
                ViewAggregateMetrics();
                break;
            case "14":
                ViewConfiguration();
                break;
            case "15":
                StopAllConsumers();
                break;
            case "0":
                Exit();
                break;
            default:
                WriteColoredLine.WriteColorLine("Invalid choice. Please try again.");
                break;
        }

        if (_running && choice != "0")
        {
            WriteColoredLine.WriteColorLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }

    private async Task SendSingleMessage()
    {
        Console.Write("Enter topic (or press Enter for default): ");
        var topic = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(topic))
            topic = _config.DefaultTopic;

        Console.Write("Enter message content: ");
        var content = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(content))
            content = "Test message";

        var message = new TestMessage(content, 1);
        await _producer.ProduceAsync(topic, message);
    }

    private async Task SendBatchMessages()
    {
        Console.Write("Enter topic (or press Enter for default): ");
        var topic = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(topic))
            topic = _config.DefaultTopic;

        Console.Write("Enter number of messages: ");
        if (!int.TryParse(Console.ReadLine(), out var count) || count <= 0)
        {
            WriteColoredLine.WriteColorLine("Invalid number. Using default: 10");
            count = 10;
        }

        Console.Write("Enter message prefix (or press Enter for default): ");
        var prefix = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(prefix))
            prefix = "Batch message";

        await _producer.ProduceBatchAsync(topic, count, prefix);
    }

    private void ViewProducerMetrics()
    {
        var metrics = _producer.GetMetrics();
        MetricsDisplay.DisplayProducerMetrics(metrics);
    }

    private void ResetProducerMetrics()
    {
        _producer.ResetMetrics();
    }

    private async Task StartConsumer()
    {
        Console.Write("Enter topic (or press Enter for default): ");
        var topic = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(topic))
            topic = _config.DefaultTopic;

        Console.Write("Enter consumer group ID: ");
        var group = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(group))
        {
            WriteColoredLine.WriteColorLine("Consumer group is required!");
            return;
        }

        Console.Write("Enter consumer ID: ");
        var consumerId = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(consumerId))
        {
            consumerId = $"consumer-{Guid.NewGuid():N}";
            WriteColoredLine.WriteColorLine($"Using auto-generated ID: {consumerId}");
        }

        await _consumerManager.StartConsumerAsync(topic, group, consumerId);
    }

    private void StopConsumer()
    {
        var consumers = _consumerManager.GetActiveConsumers();
        if (consumers.Count == 0)
        {
            WriteColoredLine.WriteColorLine("No active consumers to stop.");
            return;
        }

        WriteColoredLine.WriteColorLine("Active consumers:");
        for (int i = 0; i < consumers.Count; i++)
        {
            WriteColoredLine.WriteColorLine($"  {i + 1}. {consumers[i]}");
        }

        Console.Write("\nEnter consumer number to stop: ");
        if (int.TryParse(Console.ReadLine(), out var index) && index > 0 && index <= consumers.Count)
        {
            _consumerManager.StopConsumer(consumers[index - 1].ConsumerId);
        }
        else
        {
            WriteColoredLine.WriteColorLine("Invalid selection.");
        }
    }

    private void ListActiveConsumers()
    {
        var consumers = _consumerManager.GetActiveConsumers();
        WriteColoredLine.WriteColorLine($"\nActive Consumers: {consumers.Count}");
        WriteColoredLine.WriteColorLine(new string('─', 80));

        if (consumers.Count == 0)
        {
            WriteColoredLine.WriteColorLine("No active consumers.");
        }
        else
        {
            foreach (var consumer in consumers)
            {
                WriteColoredLine.WriteColorLine($"  {consumer}");
            }
        }
    }

    private void ViewConsumerMetrics()
    {
        var consumers = _consumerManager.GetActiveConsumers();
        if (consumers.Count == 0)
        {
            WriteColoredLine.WriteColorLine("No active consumers.");
            return;
        }

        WriteColoredLine.WriteColorLine("Select consumer:");
        for (int i = 0; i < consumers.Count; i++)
        {
            WriteColoredLine.WriteColorLine($"  {i + 1}. {consumers[i].ConsumerId}");
        }

        Console.Write("\nEnter consumer number: ");
        if (int.TryParse(Console.ReadLine(), out var index) && index > 0 && index <= consumers.Count)
        {
            var metrics = _consumerManager.GetConsumerMetrics(consumers[index - 1].ConsumerId);
            if (metrics != null)
            {
                MetricsDisplay.DisplayConsumerMetrics(metrics);
            }
        }
        else
        {
            WriteColoredLine.WriteColorLine("Invalid selection.");
        }
    }

    private void ViewAllConsumerMetrics()
    {
        var allMetrics = _consumerManager.GetAllConsumerMetrics();
        MetricsDisplay.DisplayAllConsumerMetrics(allMetrics);
    }

    private async Task SeekToOffset()
    {
        var consumer = SelectConsumer();
        if (consumer == null) return;

        Console.Write("Enter offset: ");
        if (long.TryParse(Console.ReadLine(), out var offset))
        {
            consumer.SeekToOffset(offset);
            await Task.Delay(100);
        }
        else
        {
            WriteColoredLine.WriteColorLine("Invalid offset.");
        }
    }

    private void SeekToBeginning()
    {
        var consumer = SelectConsumer();
        consumer?.SeekToBeginning();
    }

    private void SeekToEnd()
    {
        var consumer = SelectConsumer();
        consumer?.SeekToEnd();
    }

    private IKafkaConsumerService? SelectConsumer()
    {
        var consumers = _consumerManager.GetActiveConsumers();
        if (consumers.Count == 0)
        {
            WriteColoredLine.WriteColorLine("No active consumers.");
            return null;
        }

        WriteColoredLine.WriteColorLine("Select consumer:");
        for (int i = 0; i < consumers.Count; i++)
        {
            WriteColoredLine.WriteColorLine($"  {i + 1}. {consumers[i].ConsumerId}");
        }

        Console.Write("\nEnter consumer number: ");
        if (int.TryParse(Console.ReadLine(), out var index) && index > 0 && index <= consumers.Count)
        {
            return _consumerManager.GetConsumer(consumers[index - 1].ConsumerId);
        }

        WriteColoredLine.WriteColorLine("Invalid selection.");
        return null;
    }

    private void ViewAggregateMetrics()
    {
        var producerMetrics = _producer.GetMetrics();
        var consumerMetrics = _consumerManager.GetAllConsumerMetrics();
        MetricsDisplay.DisplayAggregateMetrics(producerMetrics, consumerMetrics);
    }

    private void ViewConfiguration()
    {
        WriteColoredLine.WriteColorLine("\n═══════════════════════════════════════════════════════");
        WriteColoredLine.WriteColorLine("              CONFIGURATION");
        WriteColoredLine.WriteColorLine("═══════════════════════════════════════════════════════");
        WriteColoredLine.WriteColorLine($"  Bootstrap Servers:   {_config.BootstrapServers}");
        WriteColoredLine.WriteColorLine($"  Default Topic:       {_config.DefaultTopic}");
        WriteColoredLine.WriteColorLine();
        WriteColoredLine.WriteColorLine("  PRODUCER:");
        WriteColoredLine.WriteColorLine($"    Acks:              {_config.Producer.Acks}");
        WriteColoredLine.WriteColorLine($"    Retries:           {_config.Producer.Retries}");
        WriteColoredLine.WriteColorLine($"    Compression:       {_config.Producer.CompressionType}");
        WriteColoredLine.WriteColorLine($"    Idempotence:       {_config.Producer.EnableIdempotence}");
        WriteColoredLine.WriteColorLine();
        WriteColoredLine.WriteColorLine("  CONSUMER:");
        WriteColoredLine.WriteColorLine($"    Auto Offset Reset: {_config.Consumer.AutoOffsetReset}");
        WriteColoredLine.WriteColorLine($"    Auto Commit:       {_config.Consumer.EnableAutoCommit}");
        WriteColoredLine.WriteColorLine($"    Isolation Level:   {_config.Consumer.IsolationLevel}");
        WriteColoredLine.WriteColorLine("═══════════════════════════════════════════════════════");
    }

    private void StopAllConsumers()
    {
        WriteColoredLine.WriteColorLine("Are you sure you want to stop all consumers? (y/n): ");
        var confirm = Console.ReadLine()?.Trim().ToLower();
        if (confirm == "y" || confirm == "yes")
        {
            _consumerManager.StopAllConsumers();
        }
    }

    private void Exit()
    {
        WriteColoredLine.WriteColorLine("Shutting down...");
        _consumerManager.StopAllConsumers();
        _running = false;
    }
}
