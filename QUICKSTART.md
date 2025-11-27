# Quick Start Guide

## Step 1: Start Kafka (5 minutes)

```bash
cd c:\Projects\KafkaTest
docker-compose up -d
```

Wait for services to be healthy (about 30-60 seconds):
```bash
docker-compose ps
```

You should see all services with status "Up (healthy)".

## Step 2: Run the Application

```bash
cd KafkaTest
dotnet run
```

## Step 3: Simple Test Scenario

### A. Start a Consumer

1. From the main menu, select **5** (Start New Consumer)
2. Press Enter for default topic (`test-topic`)
3. Enter consumer group: `test-group`
4. Enter consumer ID: `consumer-1`

You should see: `[CONSUMER consumer-1] Started`

### B. Send Messages

1. Select **2** (Send Batch Messages)
2. Press Enter for default topic
3. Enter number of messages: `10`
4. Press Enter for default message prefix

Watch the consumer receive messages in real-time!

### C. View Metrics

1. Select **3** to view producer metrics
2. Select **8** to view consumer metrics (select consumer-1)
3. Select **13** to view aggregate metrics

## Step 4: Test Multi-Consumer Scenarios

### Scenario 1: Load Balancing (Same Group)

1. Start **Consumer 1**: Group `group-A`, ID `consumer-1`
2. Start **Consumer 2**: Group `group-A`, ID `consumer-2` (same group!)
3. Send **100 messages**
4. Result: Messages split between both consumers

### Scenario 2: Broadcasting (Different Groups)

1. Start **Consumer 1**: Group `group-X`, ID `consumer-1`
2. Start **Consumer 2**: Group `group-Y`, ID `consumer-2` (different group!)
3. Send **50 messages**
4. Result: Both consumers receive ALL 50 messages

## Step 5: Test Offset Management

1. Start a consumer and let it consume some messages
2. Select **10** (Seek to Specific Offset)
3. Enter offset: `0` (go back to beginning)
4. Watch consumer re-read messages!

Try:
- **11**: Seek to beginning
- **12**: Seek to end

## Step 6: Cleanup

From the menu:
1. Select **15** (Stop All Consumers)
2. Select **0** (Exit)

Stop Kafka:
```bash
cd c:\Projects\KafkaTest
docker-compose down
```

## Troubleshooting

**Problem**: Cannot connect to Kafka
```bash
# Check if Kafka is running
docker-compose ps

# View Kafka logs
docker-compose logs kafka

# Restart Kafka
docker-compose restart kafka
```

**Problem**: Consumer not receiving messages
- Check producer sent messages successfully (option 3)
- Try seeking to beginning (option 11)
- Verify topic name matches

**Problem**: Port already in use
- Edit `docker-compose.yml` to use different ports
- Or stop the conflicting service

## Next Steps

- Explore the full [README.md](README.md) for detailed documentation
- Run unit tests: `dotnet test` in KafkaTest.Tests folder
- Access Kafka UI at `http://localhost:8080`
- Experiment with different configurations in `appsettings.json`

Enjoy testing Kafka!
