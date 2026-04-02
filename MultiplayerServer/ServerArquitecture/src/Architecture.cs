using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using ImageCampus.ToolBox.Scheduling;
using KapNet;
using ServerArquitecture.src;
using System;

namespace MultiplayerServer.src
{
    public sealed class Architecture : IInitable, IDisposable, ITickable
    {
        private Server server;

        TaskScheduler TaskScheduler => ServiceProvider.Instance.GetService<TaskScheduler>();
        Time Time => ServiceProvider.Instance.GetService<Time>();

        public void Init()
        {
            ServiceProvider.Instance.AddService<Time>(new Time());
            ServiceProvider.Instance.AddService<TaskScheduler>(new TaskScheduler());
            ServiceProvider.Instance.AddService<EventBus>(new EventBus());
            ServiceProvider.Instance.AddService<PacketFactory>(new PacketFactory());

            server = new Server();

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
            TaskScheduler.Tick(deltaTime);
            Time.Tick();
        }

        public void Dispose()
        {
            ServerConsole.Log("Server Closed");
        }
    }
}
