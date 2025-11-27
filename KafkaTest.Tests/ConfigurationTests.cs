using KafkaTest.Models;
using Microsoft.Extensions.Configuration;

namespace KafkaTest.Tests;

public class ConfigurationTests
{
    [Fact]
    public void KafkaConfig_Validate_ThrowsException_WhenBootstrapServersIsEmpty()
    {
        // Arrange
        var config = new KafkaConfig { BootstrapServers = "" };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => config.Validate());
        Assert.Contains("BootstrapServers", exception.Message);
    }

    [Fact]
    public void KafkaConfig_Validate_ThrowsException_WhenDefaultTopicIsEmpty()
    {
        // Arrange
        var config = new KafkaConfig { DefaultTopic = "" };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => config.Validate());
        Assert.Contains("DefaultTopic", exception.Message);
    }

    [Fact]
    public void KafkaConfig_Validate_Succeeds_WhenAllRequiredFieldsAreSet()
    {
        // Arrange
        var config = new KafkaConfig
        {
            BootstrapServers = "localhost:29092",
            DefaultTopic = "test-topic"
        };

        // Act & Assert (should not throw)
        config.Validate();
    }

    [Fact]
    public void KafkaConfig_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var config = new KafkaConfig();

        // Assert
        Assert.Equal("localhost:29092", config.BootstrapServers);
        Assert.Equal("test-topic", config.DefaultTopic);
        Assert.NotNull(config.Producer);
        Assert.NotNull(config.Consumer);
    }

    [Fact]
    public void ProducerConfig_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var config = new KafkaConfig.ProducerConfig();

        // Assert
        Assert.Equal("all", config.Acks);
        Assert.Equal(3, config.Retries);
        Assert.Equal(5, config.MaxInFlight);
        Assert.Equal(10, config.LingerMs);
        Assert.Equal("snappy", config.CompressionType);
        Assert.Equal(30000, config.RequestTimeoutMs);
        Assert.True(config.EnableIdempotence);
    }

    [Fact]
    public void ConsumerConfig_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var config = new KafkaConfig.ConsumerConfig();

        // Assert
        Assert.Equal("earliest", config.AutoOffsetReset);
        Assert.False(config.EnableAutoCommit);
        Assert.True(config.EnableAutoOffsetStore);
        Assert.Equal(6000, config.SessionTimeoutMs);
        Assert.Equal(300000, config.MaxPollIntervalMs);
        Assert.Equal("read_committed", config.IsolationLevel);
    }
}
