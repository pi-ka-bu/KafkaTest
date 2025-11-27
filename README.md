# Kafka Testing Application - .NET 10.0

A comprehensive Kafka testing application built with .NET 10.0 that supports producing and multi-consuming messages with flexible consumer group configurations.
This version is for old Kafka implementations with Zookeeeper.

## Features

### Producer Capabilities

- ✅ Send single JSON messages on-demand
- ✅ Send batch messages with configurable count
- ✅ Performance metrics (throughput, latency, P95)
- ✅ Automatic retry and error handling
- ✅ Message compression (Snappy)
- ✅ Idempotent producer support

### Consumer Capabilities

- ✅ Multiple concurrent consumers
- ✅ Flexible consumer group assignment
  - Same group = load balancing (parallel processing)
  - Different groups = broadcast (each gets all messages)
- ✅ Real-time message consumption
- ✅ Offset management (manual commit, seek)
- ✅ Consumer lag tracking
- ✅ Processing time metrics

### Testing Features

- ✅ Performance metrics (messages/sec, latency)
- ✅ Error handling and retry scenarios
- ✅ Offset management (seek to offset, beginning, end)
- ✅ Manual and automatic offset commit
- ✅ Interactive console menu
- ✅ Configurable via appsettings.json

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

## Quick Start

### 1. Start Kafka Infrastructure

```bash
# Navigate to project root
cd c:\Projects\KafkaTest

# Start Kafka and Zookeeper with Docker Compose
docker-compose up -d

# Verify services are running
docker-compose ps
```

Expected output:

```
NAME         IMAGE                              STATUS
kafka        confluentinc/cp-kafka:7.8.0        Up (healthy)
kafka-ui     provectuslabs/kafka-ui:latest      Up
zookeeper    confluentinc/cp-zookeeper:7.8.0    Up (healthy)
```

### 2. Build and Run the Application

```bash
# Build the project
cd KafkaTest
dotnet build

# Run the application
dotnet run
```

### 3. Access Kafka UI (Optional)

Open your browser and navigate to: `http://localhost:8080`

This provides a web interface to view topics, messages, consumer groups, and more.

## Configuration

Edit `appsettings.json` to customize Kafka settings:

```json
{
  "Kafka": {
    "BootstrapServers": "localhost:29092",
    "DefaultTopic": "test-topic",
    "Producer": {
      "Acks": "all",
      "Retries": 3,
      "CompressionType": "snappy",
      "EnableIdempotence": true
    },
    "Consumer": {
      "AutoOffsetReset": "earliest",
      "EnableAutoCommit": false,
      "IsolationLevel": "read_committed"
    }
  }
}
```

## Usage Examples

### Example 1: Basic Producer-Consumer Test

1. **Start the application**

   ```bash
   dotnet run
   ```

2. **Start a consumer** (Menu option 5)

   - Topic: `test-topic` (or press Enter for default)
   - Consumer Group: `group-1`
   - Consumer ID: `consumer-1`

3. **Send a single message** (Menu option 1)

   - Topic: `test-topic`
   - Content: `Hello Kafka!`

4. **View metrics** (Menu option 3 for producer, option 8 for consumer)

### Example 2: Load Balancing with Multiple Consumers

Test parallel processing with multiple consumers in the same group:

1. **Start Consumer 1** (Option 5)

   - Topic: `test-topic`
   - Group: `group-1`
   - ID: `consumer-1`

2. **Start Consumer 2** (Option 5)

   - Topic: `test-topic`
   - Group: `group-1` (same group!)
   - ID: `consumer-2`

3. **Send batch messages** (Option 2)
   - Count: `100`

**Expected Result**: Messages are distributed between consumer-1 and consumer-2 (load balancing)

### Example 3: Broadcast with Different Consumer Groups

Test broadcasting where each consumer gets all messages:

1. **Start Consumer 1** (Option 5)

   - Topic: `test-topic`
   - Group: `group-A`
   - ID: `consumer-1`

2. **Start Consumer 2** (Option 5)

   - Topic: `test-topic`
   - Group: `group-B` (different group!)
   - ID: `consumer-2`

3. **Send batch messages** (Option 2)
   - Count: `50`

**Expected Result**: Both consumers receive all 50 messages

### Example 4: Offset Management

Test seeking to different positions:

1. **Start a consumer**
2. **Send 100 messages**
3. **Use offset management options**:
   - Option 10: Seek to specific offset (e.g., 50)
   - Option 11: Seek to beginning
   - Option 12: Seek to end

### Example 5: Performance Testing

1. **Reset producer metrics** (Option 4)
2. **Send large batch** (Option 2)
   - Count: `10000`
3. **View aggregate metrics** (Option 13)
   - See throughput, latency, consumer lag

## Menu Options Reference

| Option | Description                                       |
| ------ | ------------------------------------------------- |
| 1      | Send a single message to a topic                  |
| 2      | Send batch messages (specify count)               |
| 3      | View producer metrics (sent, failed, latency)     |
| 4      | Reset producer metrics to zero                    |
| 5      | Start a new consumer (specify topic, group, ID)   |
| 6      | Stop a specific consumer                          |
| 7      | List all active consumers with status             |
| 8      | View detailed metrics for a specific consumer     |
| 9      | View metrics for all consumers in a table         |
| 10     | Seek consumer to a specific offset                |
| 11     | Seek consumer to the beginning of the topic       |
| 12     | Seek consumer to the end of the topic             |
| 13     | View aggregate metrics (producer + all consumers) |
| 14     | View current configuration from appsettings.json  |
| 15     | Stop all running consumers                        |
| 0      | Exit application (gracefully stops all consumers) |

## Project Structure

```
KafkaTest/
├── docker-compose.yml          # Kafka infrastructure
├── README.md                   # This file
├── KafkaTest/
│   ├── KafkaTest.csproj       # Project file with dependencies
│   ├── Program.cs             # Application entry point
│   ├── appsettings.json       # Configuration
│   ├── Models/
│   │   ├── TestMessage.cs     # Message model
│   │   ├── KafkaConfig.cs     # Configuration model
│   │   └── ConsumerInfo.cs    # Consumer tracking model
│   ├── Services/
│   │   ├── IKafkaProducerService.cs
│   │   ├── KafkaProducerService.cs
│   │   ├── IKafkaConsumerService.cs
│   │   ├── KafkaConsumerService.cs
│   │   └── ConsumerManager.cs
│   └── UI/
│       ├── MenuManager.cs     # Interactive menu
│       └── MetricsDisplay.cs  # Metrics formatting
└── KafkaTest.Tests/
    ├── KafkaTest.Tests.csproj
    ├── ConfigurationTests.cs
    └── TestMessageTests.cs
```

## Running Tests

```bash
# Run all unit tests
cd KafkaTest.Tests
dotnet test

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"
```

## Troubleshooting

### Issue: Cannot connect to Kafka

**Solution:**

1. Ensure Docker containers are running:
   ```bash
   docker-compose ps
   ```
2. Check Kafka health:
   ```bash
   docker-compose logs kafka
   ```
3. Restart containers if needed:
   ```bash
   docker-compose down
   docker-compose up -d
   ```

### Issue: Consumer not receiving messages

**Possible causes:**

1. Check if topic exists in Kafka UI (`http://localhost:8080`)
2. Verify consumer group and offset position
3. Try seeking to beginning (Option 11)
4. Check producer sent messages successfully (Option 3)

### Issue: Port already in use

**Solution:**

```bash
# Change ports in docker-compose.yml
# Default ports: 9092, 29092, 2181, 8080
```

### Issue: Messages not balanced between consumers

**Check:**

1. Consumers are in the **same group** (for load balancing)
2. Topic has multiple partitions (default: 1 partition)
3. Use Kafka UI to create topic with multiple partitions

## Advanced Configuration

### Creating Topics with Multiple Partitions

```bash
# Connect to Kafka container
docker exec -it kafka bash

# Create topic with 3 partitions
kafka-topics --create \
  --bootstrap-server localhost:9092 \
  --topic multi-partition-topic \
  --partitions 3 \
  --replication-factor 1

# Exit container
exit
```

### Monitoring Consumer Lag

Use Option 9 to view all consumer metrics, including lag:

- **Lag = 0**: Consumer is caught up
- **Lag > 0**: Consumer is behind by N messages

## Dependencies

- **Confluent.Kafka** (2.6.1) - Kafka client library
- **Microsoft.Extensions.Configuration** (9.0.0) - Configuration management
- **Microsoft.Extensions.Configuration.Json** (9.0.0) - JSON config provider
- **Microsoft.Extensions.Configuration.Binder** (9.0.0) - Config binding
- **xUnit** (2.9.3) - Testing framework
- **Moq** (4.20.72) - Mocking library

## Architecture Highlights

### Producer Service

- Async message production
- Built-in retry logic
- Delivery confirmation
- Latency tracking (average and P95)
- Batch sending with throughput calculation

### Consumer Service

- Async message consumption
- Manual offset commit control
- Error handling and recovery
- Offset seek capabilities
- Consumer lag tracking

### Consumer Manager

- Thread-safe consumer lifecycle management
- Support for multiple concurrent consumers
- Dynamic consumer group assignment
- Aggregate metrics across all consumers

## License

This is a testing application for educational and development purposes.

## Support

For issues or questions:

1. Check the troubleshooting section
2. View Kafka logs: `docker-compose logs kafka`
3. Check application logs in console output
