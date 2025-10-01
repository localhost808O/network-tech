using System.Net.Sockets;

namespace Server.Core.Interfaces;

public interface ISender
{
    public bool SendMessageToClient(NetworkStream stream, string message);
}