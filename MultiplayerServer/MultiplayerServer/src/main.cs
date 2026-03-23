using KapNet;
namespace MultiplayerServer.src
{
    internal class main
    {
        static void Main(string[] args)
        {
            Server server = new Server();

            server.Run();
        }
    }
}
