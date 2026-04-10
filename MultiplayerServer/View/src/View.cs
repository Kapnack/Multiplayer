using System;
using ImageCampus.ToolBox.Services;
using MultiplayerServer.src;

namespace ServerView.src
{
    internal class View
    {
        private Architecture architecture;
        private ViewConsole console;

        private bool running = true;

        Time Time => ServiceProvider.Instance.GetService<Time>();

        public void Run(string[] args)
        {
            Init(args);
            LateInit();
            Tick();
            Dispose();
        }

        void Init(string[] args)
        {
            architecture = new Architecture(args);
            console = new ViewConsole();

            architecture.Init();
            console.Init();
        }

        void LateInit()
        {
            architecture.LateInit();
            console.LateInit();
        }

        void Tick()
        {
            while (running)
            {
                architecture.Tick(Time.DeltaTime);

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.Escape)
                        running = false;
                }
            }
        }

        void Dispose()
        {
            architecture.Dispose();
            console.Dispose();
            ServiceProvider.Instance.ClearAllServices();
        }
    }
}
