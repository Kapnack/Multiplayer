using Assets.MultiplayerArchitecture.Code.Entities;

namespace MultiplayerArchitecture.Entities
{
    public class Player : Entity
    {
        private Player(uint ownerNetworkID, uint objectNetworkID, Coordinate coordinate) : base(ownerNetworkID, objectNetworkID, coordinate)
        {

        }
    }
}
