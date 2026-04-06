namespace Assets.Code.Architecture.Code.Network
{
    public interface IClient
    {
        void OnHandShake(uint myID);
        void OnClienJoined(uint clientID);
        void OnPayloadRecieve(byte[] data, uint clientID);
        void OnClientLeft(uint clientID);
        void OnServerShutDown();
    }
}
