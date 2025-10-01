using System.Net.Sockets;
using Server.Application;

namespace Server.Core.Interfaces;

public interface IReceiver
{ 
    void StartReceive(TcpClient client, HandleClient.AsyncWriteToFileDelegate writeToFileDelegate);
}