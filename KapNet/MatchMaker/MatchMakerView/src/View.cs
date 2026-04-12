using System;
using ImageCampus.ToolBox.Services;
using KapNet;
using MM;

namespace ServerView.src
{
    internal class View
    {
        private MatchMakerArchitecture architecture;
        private ViewConsole console;

        private bool running = true;

        MultiplayerServer.src.Time Time => ServiceProvider.Instance.GetService<MultiplayerServer.src.Time>();

        public void Run()
        {
            Init();
            LateInit();
            Tick();
            Dispose();
        }

        void Init()
        {
            architecture = new MatchMakerArchitecture();
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
                architecture.Tick();

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
