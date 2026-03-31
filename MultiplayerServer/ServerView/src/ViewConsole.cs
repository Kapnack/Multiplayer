using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using Logs.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerView.src
{
    internal class ViewConsole : IDisposable, IInitable
    {
        private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        public ViewConsole()
        {
        }

        public void Init()
        {
        }

        public void LateInit()
        {
            EventBus.Subscribe<ConsoleLogEvent>(LogMessage);
            EventBus.Subscribe<ConsoleWarningEvent>(LogWarning);
            EventBus.Subscribe<ConsoleErrorEvent>(LogError);
        }

        private void LogMessage(in ConsoleLogEvent consoleLogEvent)
        {
            Console.WriteLine($"[LOG]: {consoleLogEvent.message}.\n");
        }

        private void LogWarning(in ConsoleWarningEvent consoleWarningEvent)
        {
            Console.WriteLine($"[WARNING]: {consoleWarningEvent.message}.\n");
        }

        private void LogError(in ConsoleErrorEvent consoleErrorEvent)
        {
            Console.WriteLine($"[ERROR]: {consoleErrorEvent.message}.\n");
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<ConsoleLogEvent>(LogMessage);
            EventBus.Unsubscribe<ConsoleWarningEvent>(LogWarning);
            EventBus.Unsubscribe<ConsoleErrorEvent>(LogError);
        }
    }
}
