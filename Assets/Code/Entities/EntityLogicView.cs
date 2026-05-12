using ImageCampus.ToolBox.Dataflow;
using System;

namespace Assets.Code.Entities
{
    internal class EntityLogicView : IInitable, ITickable, IDisposable
    {
        CarLogic carLogic;
        ItemBoxLogic itemBoxLogic;

        public EntityLogicView()
        {
            carLogic = new CarLogic();
            itemBoxLogic = new ItemBoxLogic();
        }

        public void Init()
        {
            carLogic.Init();
            itemBoxLogic.Init();
        }

        public void LateInit()
        {
            carLogic.LateInit();
            itemBoxLogic.LateInit();
        }

        public void Tick(float deltaTime)
        {
            carLogic.Tick(deltaTime);
            itemBoxLogic.Tick(deltaTime);
        }

        public void Dispose()
        {
            carLogic.Dispose();
            itemBoxLogic.Dispose();
        }

    }
}
