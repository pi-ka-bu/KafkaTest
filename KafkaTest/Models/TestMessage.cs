namespace KafkaTest.Models;

public class TestMessage
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Content { get; set; } = string.Empty;
    public int MessageNumber { get; set; }

    public TestMessage()
    {
        Id = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
    }

    public TestMessage(string content, int messageNumber) : this()
    {
        Content = content;
        MessageNumber = messageNumber;
    }

    public override string ToString()
    {
        return $"[{MessageNumber}] {Id} | {Timestamp:yyyy-MM-dd HH:mm:ss.fff} | {Content}";
    }
}
