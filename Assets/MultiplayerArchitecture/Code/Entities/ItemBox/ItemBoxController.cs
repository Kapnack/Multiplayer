using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.MultiplayerArchitecture.Code.Entities.ItemBox
{
    internal class ItemBoxController : IInitable, ITickable, IDisposable
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        public void Init()
        {
            
        }

        public void LateInit()
        {
            throw new NotImplementedException();
        }

        public void Tick(float deltaTime)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
