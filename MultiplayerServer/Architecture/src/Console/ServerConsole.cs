using Logs.Events;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;

namespace ServerArquitecture.src
{
    internal static class ServerConsole
    {
        private static EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        public static void Log(string message)
        {
            EventBus.Raise<ConsoleLogEvent>(message);
        }

        public static void Warning(string message)
        {
            EventBus.Raise<ConsoleWarningEvent>(message);
        }

        public static void Error(string message)
        {
            EventBus.Raise<ConsoleErrorEvent>(message);
        }
    }
}
