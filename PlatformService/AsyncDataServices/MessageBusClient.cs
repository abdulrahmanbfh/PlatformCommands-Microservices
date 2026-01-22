using PlatformService.Dtos;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace PlatformService.AsyncDataServices;

public class MessageBusClient : IMessageBusClient, IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IChannel? _channel;

    private const string ExchangeName = "trigger";

    private MessageBusClient(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    public static async Task<MessageBusClient> CreateAsync(IConfiguration configuration)
    {
        var client = new MessageBusClient(configuration);
        await client.InitializeAsync();
        return client;
    }

    private async Task InitializeAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQHost"]!,
            Port = int.Parse(_configuration["RabbitMQPort"]!),
        };

        try
        {
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Fanout
            );

            _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdown;

            Console.WriteLine("--> Connected to RabbitMQ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
        }
    }

    private static Task RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
    {
        Console.WriteLine("--> RabbitMQ Connection Shutdown");
        return Task.CompletedTask;
    }



    // ✅ Proper cleanup
    public async ValueTask DisposeAsync()
    {
        Console.WriteLine("--> MessageBus Disposed");
        if (_channel is { IsOpen: true })
        {
            await _channel.CloseAsync();
            if (_connection is { IsOpen: true }) await _connection.CloseAsync();
        }

    }

    public async Task PublishNewPlatformAsync(PlatformPublishedDto platformPublishedDto)
    {
        if (_connection is null || !_connection.IsOpen)
        {
            Console.WriteLine("--> RabbitMQ Connection Closed, not sending");
            return;
        }

        var message = JsonSerializer.Serialize(platformPublishedDto);

        Console.WriteLine("--> RabbitMQ Connection Open, sending message...");
        await SendMessage(message);
    }

    private async Task SendMessage(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        if (_channel != null)
            await _channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: string.Empty,
                //basicProperties: null,
                body: body
            );
        Console.WriteLine($"--> We have sent {message}");
    }
}