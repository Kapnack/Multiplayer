using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using KapNet;
using System;

namespace MultiplayerServer.src
{
    public sealed class Architecture : IInitable, IDisposable
    {
        private Server server;

        ImageCampus.ToolBox.Scheduling.TaskScheduler TaskScheduler => ServiceProvider.Instance.GetService<ImageCampus.ToolBox.Scheduling.TaskScheduler>();
        Time Time => ServiceProvider.Instance.GetService<Time>();
        public void Init()
        {
            ServiceProvider.Instance.AddService<Time>(new Time());
            ServiceProvider.Instance.AddService<ImageCampus.ToolBox.Scheduling.TaskScheduler>(new ImageCampus.ToolBox.Scheduling.TaskScheduler());
            ServiceProvider.Instance.AddService<EventBus>(new EventBus());

            server = new Server();

            server.Init();
        }

        public void LateInit()
        {
            server.LateInit();
        }

        public void Tick()
        {
            float deltaTime = Time.DeltaTime;

            server.Tick(deltaTime);
            TaskScheduler.Tick(deltaTime);
        }

        public void Dispose()
        {
          
        }
    }
}
