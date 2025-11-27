using KafkaTest.Models;
using KafkaTest.Services;
using KafkaTest.UI;
using Microsoft.Extensions.Configuration;

namespace KafkaTest;

internal class Program
{
    static async Task Main(string[] args)
    {
        WriteColoredLine.WriteColorLine("Starting Kafka Testing Application...\n");

        KafkaConfig? config = null;
        IKafkaProducerService? producer = null;
        ConsumerManager? consumerManager = null;

        try
        {
            // Load configuration
            config = LoadConfiguration();
            config.Validate();

            WriteColoredLine.WriteColorLine($"Configuration loaded successfully");
            WriteColoredLine.WriteColorLine($"Bootstrap Servers: {config.BootstrapServers}");
            WriteColoredLine.WriteColorLine($"Default Topic: {config.DefaultTopic}\n");

            // Initialize services
            producer = new KafkaProducerService(config);
            consumerManager = new ConsumerManager(config);

            WriteColoredLine.WriteColorLine("Services initialized successfully\n");

            // Run menu
            var menuManager = new MenuManager(config, producer, consumerManager);
            await menuManager.RunAsync();
        }
        catch (Exception ex)
        {
            WriteColoredLine.WriteColorLine($"\n[FATAL ERROR] {ex.Message}", ConsoleColor.Yellow, ConsoleColor.DarkRed);
            WriteColoredLine.WriteColorLine($"Stack Trace: {ex.StackTrace}", ConsoleColor.Red);
        }
        finally
        {
            // Cleanup
            WriteColoredLine.WriteColorLine("\nCleaning up resources...");
            consumerManager?.Dispose();
            producer?.Dispose();
            WriteColoredLine.WriteColorLine("Goodbye!");
        }
    }

    private static KafkaConfig LoadConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var config = new KafkaConfig();
        configuration.GetSection("Kafka").Bind(config);

        return config;
    }
}
