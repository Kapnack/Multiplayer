namespace Assets.Code.Architecture.Code.Network
{
    public interface IClient
    {
        void OnHandShake(byte[] payload, uint myID);
        void OnClienJoined(byte[] payload, uint clientID);
        void OnPayloadRecieve(byte[] payload, uint clientID);
        void OnClientLeft(byte[] payload, uint clientID);
        void OnServerShutDown();
    }
}
