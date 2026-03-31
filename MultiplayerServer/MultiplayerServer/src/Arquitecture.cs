using KapNet;
using System;

namespace MultiplayerServer.src
{
    internal sealed class Arquitecture : IInitiable, ITickable, IDisposable
    {
        private Server server;

        public void Init()
        {
            server = new Server();

            server.Init();
        }

        public void LateInit()
        {
            server.LateInit();
        }

        public void Tick()
        {
            server.Tick();
        }

        public void Dispose()
        {
          
        }
    }
}
