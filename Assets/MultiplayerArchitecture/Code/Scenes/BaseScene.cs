using ImageCampus.ToolBox.Dataflow;
using System;
using ImageCampus.ToolBox.Services;

namespace MultiplayerArchitecture.Code.Scenes
{
    public abstract class BaseScene : IInitable, ITickable, IDisposable, IService
    {
        public bool IsPersistance => false;

        public abstract void Init();
        public abstract void LateInit();
        public abstract void Tick(float deltaTime);
        public abstract void Dispose();
    }
}
