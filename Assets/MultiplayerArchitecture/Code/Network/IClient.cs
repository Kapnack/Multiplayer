namespace Assets.MultiplayerArchitecture.Code.Network
{
    public interface IClient
    {
        void OnHandShake(uint myID);
        void OnClienJoined(uint clientID);
        void OnPayloadRecieve(byte[] payload, uint clientID);
        void OnClientLeft(uint clientID);
        void OnServerShutDown();
    }
}
