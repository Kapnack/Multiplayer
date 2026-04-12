using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using ImageCampus.ToolBox.Scheduling;
using KapNet;
using System;

namespace MultiplayerServer.src
{
    public sealed class Architecture : IInitable, IDisposable, ITickable
    {
        private Server server;
        Time Time => ServiceProvider.Instance.GetService<Time>();

        public Architecture(string[] args)
        {
            ServiceProvider.Instance.AddService<Time>(new Time());
            ServiceProvider.Instance.AddService<EventBus>(new EventBus());

            if (args.Length >= 3)
            {
                string matchMakingAdress = args[0];

                if (!int.TryParse(args[1], out int portToConnect))
                {
                    Console.WriteLine("Invalid portToConnect");
                    return;
                }

                if (!int.TryParse(args[2], out int portToHost))
                {
                    Console.WriteLine("Invalid portToHost");
                    return;
                }

                server = new Server(matchMakingAdress, portToConnect, portToHost);
            
                return;
            }

            server = new Server();
        }

        public void Init()
        {
            Time.Init();
            server.Init();
        }

        public void LateInit()
        {
            Time.LateInit();
            server.LateInit();
        }

        public void Tick(float deltaTime)
        {
            server.Tick(deltaTime);
            Time.Tick();
        }

        public void Dispose()
        {
            server.Dispose();
        }
    }
}
