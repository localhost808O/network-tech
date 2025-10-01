using Microsoft.Extensions.Logging;
using Server.Application;

namespace Server.Infrastructure.Io;

public class TransferStats
{
    private long _totalBytesReceived;
    private long _lastBytesReceived;
    private DateTime _lastUpdateTime;
    private DateTime _startTime;
    private readonly object _lockObject = new object();
    private ILogger _logger;

    public TransferStats()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<TransferStats>();
        _startTime = DateTime.Now;
        _lastUpdateTime = _startTime;
    }

    public void UpdateStats(int bytesReceived)
    {
        /* aka monitor in java */
        lock (_lockObject)
        {
            _totalBytesReceived += bytesReceived;
        }
    }
    
    public double GetInstantSpeed()
    {
        lock (_lockObject)
        {
            var now = DateTime.Now;
            var timeDiff = (now - _lastUpdateTime).TotalSeconds;
            
            if (timeDiff > 0)
            {
                var bytesDiff = _totalBytesReceived - _lastBytesReceived;
                var instantSpeed = bytesDiff / timeDiff / 1024; // KB/s
                
                _lastBytesReceived = _totalBytesReceived;
                _lastUpdateTime = now;
                
                return instantSpeed;
            }
            
            return 0;
        }
    }
    
    public void PrintCurrentStats(string fileName)
    {
        lock (_lockObject)
        {
            var elapsed = DateTime.Now - _startTime;
            double averageSpeed = _totalBytesReceived / elapsed.TotalSeconds / 1024; // KB/s
            
            _logger.LogInformation(
                $"Stats for {fileName} | " +
                $"Total: {_totalBytesReceived} bytes | " +
                $"Average: {averageSpeed:F2} KB/s | " +
                $"Time: {elapsed:mm\\:ss}");
        }
    }
}