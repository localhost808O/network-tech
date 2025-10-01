using Server.Application;

var server = new LogicServer();
try
{
    await server.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}