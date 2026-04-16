using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using Logs.Events;
using System;

namespace ServerView.src
{
    internal class ViewConsole : IDisposable, IInitable
    {
        const string RED = "\x1B[31m";
        const string YELLOW = "\x1B[33m";
        const string GREEN = "\x1B[32m";
        const string RESET = "\x1B[0m";

        private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        public ViewConsole()
        {
        }

        public void Init()
        {
            EventBus.Subscribe<ConsoleLogEvent>(LogMessage);
            EventBus.Subscribe<ConsoleWarningEvent>(LogWarning);
            EventBus.Subscribe<ConsoleErrorEvent>(LogError);
        }

        public void LateInit()
        {
        }

        private void LogMessage(in ConsoleLogEvent consoleLogEvent)
        {
            Console.WriteLine($"{GREEN}[LOG]{RESET}: {consoleLogEvent.message}.\n");
        }

        private void LogWarning(in ConsoleWarningEvent consoleWarningEvent)
        {
            Console.WriteLine($"{YELLOW}[WARNING]{RESET}: {consoleWarningEvent.message}.\n");
        }

        private void LogError(in ConsoleErrorEvent consoleErrorEvent)
        {
            Console.WriteLine($"{RED}[ERROR]{RESET}: {consoleErrorEvent.message}.\n");
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<ConsoleLogEvent>(LogMessage);
            EventBus.Unsubscribe<ConsoleWarningEvent>(LogWarning);
            EventBus.Unsubscribe<ConsoleErrorEvent>(LogError);
        }
    }
}
