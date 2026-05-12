using MultiplayerArchitecture.Entities;

namespace MultiplayerArchitecture
{
    [Item]
    public class ChasingBullet : Entity
    {
        private ChasingBullet(uint ownerNetworkID, uint objectNetworkID, Coordinate coordinate) : base(ownerNetworkID, objectNetworkID, coordinate)
        {
        }
    }
}
