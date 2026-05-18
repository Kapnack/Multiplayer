using MultiplayerArchitecture;
using MultiplayerArchitecture.Entities;

namespace Assets.Code.Entities
{
    [Item]
    public class Banana : Entity
    {
        protected Banana(uint ownerNetworkID, uint objectNetworkID, Coordinate coordinate) : base(ownerNetworkID, objectNetworkID, coordinate)
        {
        }
    }
}