using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using KapNet;
using MultiplayerServer.src;

namespace MM
{
    public class MatchMakerArchitecture
    {
        private MatchMaker matchMaker;
        Time Time => ServiceProvider.Instance.GetService<Time>();

        public MatchMakerArchitecture()
        {
        }

        public void Init()
        {
            ServiceProvider.Instance.AddService<Time>(new Time());
            ServiceProvider.Instance.AddService<EventBus>(new EventBus());
            ServiceProvider.Instance.AddService<PacketFactory>(new PacketFactory());

            matchMaker = new MatchMaker();

            Time.Init();
            matchMaker.Init();
        }

        public void LateInit()
        {
            Time.LateInit();
            matchMaker.LateInit();
        }

        public void Tick()
        {
            matchMaker.Tick();
            Time.Tick();
        }

        public void Dispose()
        {
            matchMaker.Dispose();
        }
    }

}