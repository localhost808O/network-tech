using System.Net;
using System.Net.Sockets;
using Client.Core.Dto;
using Client.Infrastructure;
using Client.Infrastructure.Io;
using Microsoft.Extensions.Logging;

namespace Client.Application;

public class LogicClient
{
    private readonly ILogger<LogicClient> _logger;
    private const string PathToConfig = "Property\\Config.json";
    private ClientProperty _clientProperty;
    
    public LogicClient()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Debug);
        });
        
        _logger = loggerFactory.CreateLogger<LogicClient>();
        _logger.LogInformation("Logger initialized");
        SetUp();
        
    }

    private void SetUp()
    {
        try
        {
            _clientProperty = ConfigWorker.GetConfigFromJson<ClientProperty>(PathToConfig);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
    }
    
    public async Task Run()
    {
        _logger.LogInformation("Run client");
        var ipAddress = IPAddress.Parse(_clientProperty.ip);
        var ipEndPoint = new IPEndPoint(ipAddress, _clientProperty.port);
        using TcpClient tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(ipEndPoint);
        await using NetworkStream stream = tcpClient.GetStream();
        var sender = new Sender();
        try
        {
            sender.SendDataToServer(_clientProperty.path, tcpClient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }

    }
    
}