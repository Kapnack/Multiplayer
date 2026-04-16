using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using KapNet;

namespace MM
{
    public class MatchMakerArchitecture
    {
        private MatchMaker matchMaker;

        public MatchMakerArchitecture()
        {
        }

        public void Init()
        {
            ServiceProvider.Instance.AddService<EventBus>(new EventBus());

            matchMaker = new MatchMaker();

            matchMaker.Init();
        }

        public void LateInit()
        {
            matchMaker.LateInit();
        }

        public void Tick()
        {
            matchMaker.Tick();
        }

        public void Dispose()
        {
            matchMaker.Dispose();
        }
    }

}