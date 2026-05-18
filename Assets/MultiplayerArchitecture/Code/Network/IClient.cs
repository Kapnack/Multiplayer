using MultiplayerArchitecture;

namespace Assets.MultiplayerArchitecture.Code.Network
{
    public interface IClient
    {
        void OnHandShake(uint myID);
        void OnClientLeft(uint clientID);
        void OnSpawn(uint clientID, uint entityID, Coordinate coordinate, string entityToSpawn);
        void OnDestroyEntity(uint clientID, uint entityID);
        void OnPositionRecieve(uint clientID, uint entityID, Coordinate coordinate, Rotation eulerRotation);
        void OnRejectedQueue();
        void OnServerShutDown();
    }
}
