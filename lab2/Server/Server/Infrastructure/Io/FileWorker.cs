using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Microsoft.Extensions.Logging;
using Server.Core.Interfaces;

namespace Server.Application;

public class FileWorker : IFileWorker, IDisposable
{
    private const string DirName = "\\uploads";
    private string _fullPath = "";
    private readonly Encoding _utf8 = Encoding.UTF8;
    private FileStream _currentFileStream;
    private readonly ILogger _logger;
    public FileWorker()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<FileWorker>();

    }
    
    public Boolean CreateDirectory()
    {
        var currentPath = Directory.GetCurrentDirectory();
        _fullPath = currentPath + DirName;
        try
        {
            Directory.CreateDirectory(currentPath + DirName);
            _logger.LogInformation($"Directory {_fullPath} has been created.");
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return false;
        }
    }

    public Boolean CreateFile(string fileName)
    {
        string fileNameStr = fileName.TrimEnd('\0');
        var savePath = Path.Combine(_fullPath, fileNameStr);
        try
        {
            _currentFileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None);
            _logger.LogInformation($"File {fileNameStr} has been created.");
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create file");
            return false;
            
        }
    }

    public Boolean DeleteFile(string fileName)
    {
        return true;
    }

    public int GetFileSize(string fileName)
    {
        return 0;
    }

    public Boolean WriteDataToFile(string fileName, byte[] data)
    {
        if (_currentFileStream == null)
        {
            return false;
        }
        
        try
        {
            _currentFileStream.Write(data, 0, data.Length);
            _currentFileStream.Flush();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return false;
        }
    }

    private void CloseCurrentFileStream()
    {
        _currentFileStream?.Close();
        _currentFileStream?.Dispose();
        _currentFileStream = null;
    }
    
    public void Dispose()
    {
        CloseCurrentFileStream();
    }
    ~FileWorker() {
        Dispose();
    }

}
