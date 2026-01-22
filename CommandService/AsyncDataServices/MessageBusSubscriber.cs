using System.Text;
using CommandsService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandsService.AsyncDataServices;

public class MessageBusSubscriber : BackgroundService, IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private readonly IEventProcessor _eventProcessor;
    private IConnection? _connection;
    private IChannel? _channel;
    private QueueDeclareOk? _queueName;


    public MessageBusSubscriber(IConfiguration configuration, IEventProcessor eventProcessor)
    {
        _configuration = configuration;
        _eventProcessor = eventProcessor;
    }


    private async Task InitializeRabbitMQAsync()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQHost"]!,
            Port = int.Parse(_configuration["RabbitMQPort"]!)
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
        await _channel.ExchangeDeclareAsync(exchange: "trigger", type: ExchangeType.Fanout);
        _queueName = await _channel.QueueDeclareAsync();
        await _channel.QueueBindAsync(queue: _queueName, exchange: "trigger", routingKey: "");

        Console.WriteLine("--> Listening on the MessageBus...");

        _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdown;
    }

    private static Task RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
    {
        Console.WriteLine("--> RabbitMQ Connection Shutdown");
        return Task.CompletedTask;
    }

    // ✅ Proper cleanup
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await DisposeAsync();
        await base.StopAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        Console.WriteLine("--> MessageBus Disposed");
        if (_channel is { IsOpen: true })
        {
            await _channel.CloseAsync();
            if (_connection is { IsOpen: true }) await _connection.CloseAsync();
        }

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //stoppingToken.ThrowIfCancellationRequested();
        try
        {
            await InitializeRabbitMQAsync();

            var consumer = new AsyncEventingBasicConsumer(_channel!);
            consumer.ReceivedAsync += async (sender, ea) =>
            {
                Console.WriteLine("--> Event Received!");
                var body = ea.Body.ToArray();
                var notificationMessage = Encoding.UTF8.GetString(body);
                _eventProcessor.ProcessEvent(notificationMessage);
                await Task.CompletedTask;
            };

            await _channel!.BasicConsumeAsync(queue: _queueName!, autoAck: true, consumer: consumer, cancellationToken: stoppingToken);

            // keep background service alive
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception e)
        {
            Console.WriteLine($"--> Failed to initialize the MQ: {e.Message}");
        }
        
    }
}