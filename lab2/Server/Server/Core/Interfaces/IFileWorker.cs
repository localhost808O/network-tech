namespace Server.Core.Interfaces;

public interface IFileWorker
{
    Boolean CreateDirectory();
    Boolean CreateFile(string fileName);
    Boolean DeleteFile(string fileName);
    Boolean WriteDataToFile(string fileName, byte[] data);
    int GetFileSize(string fileName);
    
   
}