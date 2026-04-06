using ImageCampus.ToolBox.Events;

namespace Logs.Events
{
    public struct ConsoleWarningEvent : IEvent
    {
        public string message;

        public void Assign(params object[] parameters)
        {
            message = parameters[0] as string;
        }

        public void Reset()
        {
            message = default(string);
        }
    }
}
