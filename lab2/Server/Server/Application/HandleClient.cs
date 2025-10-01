using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Server.Core.Interfaces;

namespace Server.Application;

public class HandleClient : IDisposable
{
    private bool _isClientActive;
    private bool _disposed;
    private readonly ILogger<HandleClient> _logger;
    private readonly TcpClient _tcpClient;
    
    public delegate void AsyncWriteToFileDelegate(IFileWorker fileWorker, string fileSystemName, byte[] data);
    private AsyncWriteToFileDelegate AsyncWriteToFile = (IFileWorker fileWorker, 
        string fileSystemName, byte[] data) =>
    {
        try
        {
            fileWorker.WriteDataToFile(fileSystemName, data);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    };

    public HandleClient(TcpClient tcpClient)
    {
        _disposed = false;
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<HandleClient>();
        try
        {
            _isClientActive = true;
            _tcpClient = tcpClient;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
    
    public void Start() 
    {
        try
        {
            _logger.LogInformation("handling client from {Endpoint}", 
                _tcpClient.Client.RemoteEndPoint);
                
            IoClientOperations();
            _logger.LogInformation("===========Ending Server===============");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error handling client");
        }
        finally
        {
            _tcpClient?.Close();
            _logger.LogInformation("client handling completed");
        }
    }
    
    private void IoClientOperations()
    {
        try
        {
            using (_tcpClient)
            {
                var receiver = new Receiver();
                while (_isClientActive && _tcpClient.Connected)
                {
                    receiver.StartReceive(_tcpClient, AsyncWriteToFile);
                    
                    if (!_tcpClient.Connected)
                    {
                        _isClientActive = false;
                    }
                }                
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "error handling client");
            _isClientActive = false;
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _tcpClient?.Dispose();
                _logger.LogInformation("client disposing!!!!");
            }
            _disposed = true;
        }
    }

    ~HandleClient()
    {
        Dispose(false);
    }
}
