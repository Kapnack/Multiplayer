using ImageCampus.ToolBox.Dataflow;
using System;

namespace Assets.Code.Entities
{
    internal class EntityLogicView : IInitable, ITickable, IDisposable
    {
        CarLogic carLogic;
        ItemBoxLogic itemBoxLogic;

        public void Init()
        {
            carLogic = new CarLogic();
            itemBoxLogic = new ItemBoxLogic();

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
            itemBoxLogic.Tick(deltaTime);
        }

        public void Dispose()
        {
            carLogic.Dispose();
            itemBoxLogic.Dispose();
        }

    }
}
