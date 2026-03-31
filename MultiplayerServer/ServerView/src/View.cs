using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageCampus.ToolBox.Dataflow;
using MultiplayerServer.src;

namespace ServerView.src
{
    internal class View
    {
        private Architecture architecture;
        private ViewConsole console;

        private bool running = true;

        public void Run()
        {
            Init();
            LateInit();
            Tick();
            Dispose();
        }

        void Init()
        {
            architecture = new Architecture();
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
                    {
                        Console.WriteLine("Shutting down...");
                        running = false;
                    }
                }
            }
        }

        void Dispose()
        {
            console.Dispose();
        }
    }
}
