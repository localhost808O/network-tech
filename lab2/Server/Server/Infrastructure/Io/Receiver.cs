using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Server.Core.Interfaces;
using Server.Infrastructure.Io;

namespace Server.Application;

public class Receiver : IReceiver
{
    private const int BufferSize = 1048576;  
    private const int MaxFileName = 4096;
    private const int FileNameLength = 2;
    private const int FileSizeBytes = 8;
    private long fileSize; 
    private ServerSetUp server;
    private string _fileName;
    private ILogger<Receiver> _logger;
    private TransferStats transferStats;
    private Timer _statsTimer;
    private long _totalReceived;
    private DateTime _transferStartTime;
    
    public Receiver()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<Receiver>();
        transferStats = new TransferStats();
    }
    
    public void StartReceive(TcpClient tcpClient, HandleClient.AsyncWriteToFileDelegate asyncWriteToFile)
    {
        _totalReceived = 0;
        _transferStartTime = DateTime.Now;
        _statsTimer = new Timer(LogTransferStats, null, TimeSpan.Zero, TimeSpan.FromSeconds(3));
        
        try
        {
            var responsePart = new byte[BufferSize];
            var fileSizeBytes = new byte[FileSizeBytes];
            var fileWorker = new FileWorker();
            using NetworkStream networkStream = tcpClient.GetStream();
            
            ReadFileName(networkStream, fileWorker);
            
            int i;
            i = networkStream.Read(fileSizeBytes, 0, FileSizeBytes);        
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(fileSizeBytes);
            }
            fileSize = BitConverter.ToInt64(fileSizeBytes, 0);
            
            _logger.LogInformation($"starting file transfer. current name: {_fileName}, suze: {fileSize} bytes");
            
            while (_totalReceived < fileSize)
            {
                int bytesToRead = (int)Math.Min(BufferSize, fileSize - _totalReceived);
                i = networkStream.Read(responsePart, 0, bytesToRead);
                
                if (i == 0)
                {
                    break;
                } 
                asyncWriteToFile(fileWorker, _fileName, responsePart.Take(i).ToArray());
                _totalReceived += i;
                
                transferStats.UpdateStats(i);
            }
            
            LogFinalStats();
            SendResultToClient(networkStream, _totalReceived == fileSize);
            
        }
        finally
        {
            
            _statsTimer?.Dispose();
        }
    }

    private void LogTransferStats(object state)
    {
        if (_totalReceived > 0)
        {
            var elapsed = DateTime.Now - _transferStartTime;
            double instantSpeed = transferStats.GetInstantSpeed();
            double averageSpeed = _totalReceived / elapsed.TotalSeconds;
            
            _logger.LogInformation(
                $"Client: {_fileName} | " +
                $"Progress: {_totalReceived}/{fileSize} bytes ({GetProgressPercentage():F1}%) | " +
                $"Instant: {instantSpeed:F2} KB/s | " +
                $"Average: {averageSpeed/1024:F2} KB/s | " +
                $"Elapsed: {elapsed:mm\\:ss}");
        }
    }
    
    private void LogFinalStats()
    {
        var elapsed = DateTime.Now - _transferStartTime;
        double averageSpeed = _totalReceived / elapsed.TotalSeconds;
        
        _logger.LogInformation(
            $"Transfer completed: {_fileName} | " +
            $"Total: {_totalReceived} bytes | " +
            $"Time: {elapsed:mm\\:ss} | " +
            $"Average speed: {averageSpeed/1024:F2} KB/s");
    }
    
    private double GetProgressPercentage()
    {
        return fileSize > 0 ? (_totalReceived * 100.0) / fileSize : 0;
    }
    
    private void SendResultToClient(NetworkStream networkStream, bool success)
    {
        try
        {
            byte response = success ? (byte)1 : (byte)0;
            networkStream.Write(new[] { response }, 0, 1);
            _logger.LogInformation($"Sent response to client: {(success ? "SUCCESS" : "FAILURE")}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send response to client");
        }
    }

    private void ReadFileName(NetworkStream networkStream, IFileWorker fileWorker)
    {
        var fileNameLengthBytes = new byte[FileNameLength];
        int bytesRead = networkStream.Read(fileNameLengthBytes, 0, FileNameLength);
        if (bytesRead != FileNameLength)
        {
            throw new IOException("File name length is incorrect");
        }
        
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(fileNameLengthBytes);
        }
        ushort currentFileNameLength = BitConverter.ToUInt16(fileNameLengthBytes, 0);
        
        if (currentFileNameLength > MaxFileName)
        {
            throw new IOException($"File name too long: {currentFileNameLength} bytes");
        }
        
        var fileNameBytes = new byte[currentFileNameLength];
        bytesRead = networkStream.Read(fileNameBytes, 0, currentFileNameLength);
        
        if (bytesRead != currentFileNameLength)
        {
            throw new IOException("File name is incorrect size");
        }
        
        _fileName = Encoding.UTF8.GetString(fileNameBytes);
        fileWorker.CreateDirectory();
        fileWorker.CreateFile(_fileName);
        _logger.LogInformation($"File name received: {_fileName}");
    }
}