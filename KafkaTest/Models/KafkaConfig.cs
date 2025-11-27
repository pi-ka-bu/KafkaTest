namespace KafkaTest.Models;

public class KafkaConfig
{
    public string BootstrapServers { get; set; } = "localhost:29092";
    public string DefaultTopic { get; set; } = "test-topic";
    public ProducerConfig Producer { get; set; } = new();
    public ConsumerConfig Consumer { get; set; } = new();

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(BootstrapServers))
            throw new ArgumentException("BootstrapServers cannot be empty");

        if (string.IsNullOrWhiteSpace(DefaultTopic))
            throw new ArgumentException("DefaultTopic cannot be empty");
    }

    public class ProducerConfig
    {
        public string Acks { get; set; } = "all";
        public int Retries { get; set; } = 3;
        public int MaxInFlight { get; set; } = 5;
        public int LingerMs { get; set; } = 10;
        public string CompressionType { get; set; } = "snappy";
        public int RequestTimeoutMs { get; set; } = 30000;
        public bool EnableIdempotence { get; set; } = true;
    }

    public class ConsumerConfig
    {
        public string AutoOffsetReset { get; set; } = "earliest";
        public bool EnableAutoCommit { get; set; } = false;
        public bool EnableAutoOffsetStore { get; set; } = true;
        public int SessionTimeoutMs { get; set; } = 6000;
        public int MaxPollIntervalMs { get; set; } = 300000;
        public string IsolationLevel { get; set; } = "read_committed";
    }
}
