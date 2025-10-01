using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Client.Infrastructure.Io;

public class Sender
{
    private ILogger<Sender> _logger;
    private const int BufferSize = 8192; 
    private const int MaxFileNameSize = 4096;
    public Sender()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<Sender>(); 
    }

    public void SendDataToServer(string pathToFile, TcpClient tcpClient)
    {
        try
        {
            NetworkStream networkStream = tcpClient.GetStream();
            FileInfo fileInfo = new FileInfo(pathToFile);
            if (!fileInfo.Exists)
            {
                _logger.LogInformation($"file {pathToFile} does not exist");
                return;
            }
            
            string fileName = Path.GetFileName(pathToFile);
            long fileSize = fileInfo.Length;
            
            SendFileHeader(networkStream, fileName, fileSize);
            SendFileContent(networkStream, pathToFile, fileSize);
            
            bool isSuccess = ReceiveServerResponse(networkStream);
            if (isSuccess)
            {
                _logger.LogInformation("file transfer completed suc");
            }
            else
            {
                _logger.LogError("file transfer fail ");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error ");
            throw;
        }
    }

    private void SendFileHeader(NetworkStream networkStream, string fileName, long fileSize)
    {
        var fileNameBytes = Encoding.UTF8.GetBytes(fileName);
        
        if (fileNameBytes.Length > MaxFileNameSize)
        {
            throw new InvalidOperationException("error : file name very long ");
        }
        
        int headerSize = 2 + fileNameBytes.Length + 8;
        byte[] headerBuffer = new byte[headerSize];
        int offset = 0;
        
        byte[] fileNameLengthBytes = BitConverter.GetBytes((ushort)fileNameBytes.Length);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(fileNameLengthBytes);
        }
        
        Array.Copy(fileNameLengthBytes, 0, headerBuffer, offset, 2);
        offset += 2;
        Array.Copy(fileNameBytes, 0, headerBuffer, offset, fileNameBytes.Length);
        offset += fileNameBytes.Length;
        
        byte[] fileSizeBytes = BitConverter.GetBytes(fileSize);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(fileSizeBytes);
        }
        Array.Copy(fileSizeBytes, 0, headerBuffer, offset, 8);
        offset += 8;
        networkStream.Write(headerBuffer, 0, headerSize);
        _logger.LogInformation($"File header sent. Name: {fileName}, Size: {fileSize} bytes");
    }

    private void SendFileContent(NetworkStream networkStream, string filePath, long fileSize)
    {
        byte[] buffer = new byte[BufferSize];
        long totalSent = 0;
        
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            int bytesRead;
            DateTime startTime = DateTime.Now;
            DateTime lastLogTime = startTime;

            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                networkStream.Write(buffer, 0, bytesRead);
                totalSent += bytesRead;
                
                DateTime currentTime = DateTime.Now;
                if ((currentTime - lastLogTime).TotalSeconds >= 3)
                {
                    LogTransferProgress(totalSent, fileSize, startTime);
                    lastLogTime = currentTime;
                }
            }
            
            LogTransferProgress(totalSent, fileSize, startTime);
        }

        _logger.LogInformation($" {totalSent} bytes received    ");
    }

    private void LogTransferProgress(long totalSent, long totalSize, DateTime startTime)
    {
        double progress = (double)totalSent / totalSize * 100;
        double speed = totalSent / 1024 / 1024;

        _logger.LogInformation(
            $" CURRENT: progress -> {progress:F1}% ({totalSent}/{totalSize} bytes) | " +
            $"speed: {speed:F2} MB/secc | ");
    }

    private bool ReceiveServerResponse(NetworkStream networkStream)
    {
        byte[] responseBuffer = new byte[1];
        int bytesRead = networkStream.Read(responseBuffer, 0, 1);
        
        if (bytesRead == 1)
        {
            return responseBuffer[0] == 1; 
        }
        
        throw new IOException("no response received from server!!!!!");
    }
}