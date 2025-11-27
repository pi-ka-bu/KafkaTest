using KafkaTest.Models;

namespace KafkaTest.Tests;

public class TestMessageTests
{
    [Fact]
    public void TestMessage_Constructor_SetsDefaultValues()
    {
        // Act
        var message = new TestMessage();

        // Assert
        Assert.NotEqual(Guid.Empty, message.Id);
        Assert.True((DateTime.UtcNow - message.Timestamp).TotalSeconds < 1);
        Assert.Equal(string.Empty, message.Content);
        Assert.Equal(0, message.MessageNumber);
    }

    [Fact]
    public void TestMessage_ParameterizedConstructor_SetsValues()
    {
        // Arrange
        var content = "Test content";
        var messageNumber = 42;

        // Act
        var message = new TestMessage(content, messageNumber);

        // Assert
        Assert.NotEqual(Guid.Empty, message.Id);
        Assert.Equal(content, message.Content);
        Assert.Equal(messageNumber, message.MessageNumber);
    }

    [Fact]
    public void TestMessage_ToString_ReturnsFormattedString()
    {
        // Arrange
        var message = new TestMessage("Test", 1);

        // Act
        var result = message.ToString();

        // Assert
        Assert.Contains("[1]", result);
        Assert.Contains(message.Id.ToString(), result);
        Assert.Contains("Test", result);
    }

    [Fact]
    public void ConsumerInfo_Constructor_SetsValues()
    {
        // Arrange
        var consumerId = "consumer-1";
        var consumerGroup = "group-1";
        var topic = "test-topic";

        // Act
        var info = new ConsumerInfo(consumerId, consumerGroup, topic);

        // Assert
        Assert.Equal(consumerId, info.ConsumerId);
        Assert.Equal(consumerGroup, info.ConsumerGroup);
        Assert.Equal(topic, info.Topic);
        Assert.False(info.IsRunning);
        Assert.Equal(0, info.MessageCount);
        Assert.Null(info.LastMessageTimestamp);
    }

    [Fact]
    public void ConsumerInfo_ToString_ReturnsFormattedString()
    {
        // Arrange
        var info = new ConsumerInfo("consumer-1", "group-1", "test-topic")
        {
            IsRunning = true,
            MessageCount = 10,
            LastMessageTimestamp = DateTime.UtcNow
        };

        // Act
        var result = info.ToString();

        // Assert
        Assert.Contains("RUNNING", result);
        Assert.Contains("consumer-1", result);
        Assert.Contains("group-1", result);
        Assert.Contains("test-topic", result);
        Assert.Contains("10", result);
    }
}
