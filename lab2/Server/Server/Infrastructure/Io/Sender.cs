using System.Net.Sockets;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;
using Server.Core.Interfaces;

namespace Server.Application;

public class Sender : ISender
{
    
    public bool SendMessageToClient(NetworkStream stream, string message)
    {
        try
        {
            var data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            
        }
        catch (IOException e)
        {
            Console.WriteLine("[IOException]" + e);
            return false;
        }
                
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return true;
    }
}