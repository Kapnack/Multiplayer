using System.Diagnostics;

using ImageCampus.ToolBox.Services;

namespace MultiplayerServer.src
{
    public class Time : IService
    {
        private static Stopwatch stopwatch = Stopwatch.StartNew();

        private double lastTime;
        public float DeltaTime { get; private set; }

        public double RealTimeSinceStartUp => stopwatch.Elapsed.TotalSeconds;

        public bool IsPersistance => true;

        public void Update()
        {
            UpdateDeltaTime();
        }

        private void UpdateDeltaTime()
        {
            double currentTime = stopwatch.Elapsed.TotalSeconds;

            DeltaTime = (float)(currentTime - lastTime);

            lastTime = currentTime;
        }
    }
}
