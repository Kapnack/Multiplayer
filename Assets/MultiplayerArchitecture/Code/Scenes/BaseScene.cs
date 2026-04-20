using ImageCampus.ToolBox.Dataflow;
using System;

namespace MultiplayerArchitecture.Code.Scenes
{
    public abstract class BaseScene : IInitable, ITickable, IDisposable
    {
        public abstract void Init();
        public abstract void Init(params object[] parameters);
        public abstract void LateInit();
        public abstract void Tick(float deltaTime);
        public abstract void Dispose();
    }
}
