using System.Net.Sockets;
using Server.Core.Dto;
using Server.Infrastructure.Io;

namespace Server.Application;

public class ServerSetUp
{
    
    private readonly string _path;
    public ServerProperty serverProperty {set;get;}
    public ServerSetUp(string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException(nameof(path));
        }
        _path = path;
        SetUp();
    }
    
        private void SetUp()
        {
            try
            {
                serverProperty = ConfigWorker.GetConfigFromJson<ServerProperty>(_path);
            }
            catch (SocketException e)
            {
                Console.WriteLine("[SOCKET EXP]" + e);
            }
            
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }
}

