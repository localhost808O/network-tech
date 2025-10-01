using System.Net;
using System.Text;
using System.Net.Sockets;
using Server.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Server.Application;

/**
 * Setup tcp app and async receive new client.
 * Create new thread and process handle client
 */
public class LogicServer
{
    private readonly ILogger<LogicServer> _logger;
    private ServerSetUp _server;
   
    private const string PathToConfig = "Utils\\Configs\\ServerConfig.json";
    public LogicServer()
    {
        _server = new ServerSetUp(PathToConfig);
        /* Logger initial process */
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Debug);
        });
        
        _logger = loggerFactory.CreateLogger<LogicServer>();
        _logger.LogInformation("Logger initialized");
        _logger.LogInformation($"Get ser portt: port: {_server.serverProperty.Port}, " +
                               $"address: {_server.serverProperty.Address}");
    }
    
    public async Task Run()
    {
        _logger.LogInformation("=========Starting Server================");
         try
         {
             await LoopClients();            
         }
         catch (Exception e)
         {
             Console.WriteLine(e);
         }
         _logger.LogInformation("===========Ending Server===============");
    }

    private async Task LoopClients()
    {
        var ipAddress = IPAddress.Parse(_server.serverProperty.Address);
        var ipEndPoint = new IPEndPoint(ipAddress, _server.serverProperty.Port);
        TcpListener tcpListener = new TcpListener(ipEndPoint);
        tcpListener.Start();
        while (true)
        { 
            try
            {
                _logger.LogInformation($"listening for connections on port {_server.serverProperty.Port} ....");
                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync(); 
                
                Thread clientThread = new Thread(new HandleClient(tcpClient).Start);
                clientThread.IsBackground = true;
                clientThread.Start();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "EError occured");
            }
        }
    }

}