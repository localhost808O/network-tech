using Client.Application;

LogicClient logicClient = new LogicClient();
try
{
    await logicClient.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}