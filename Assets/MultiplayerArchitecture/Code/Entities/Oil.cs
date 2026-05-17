using MultiplayerArchitecture;
using MultiplayerArchitecture.Entities;

namespace Assets.MultiplayerArchitecture.Code.Entities
{
    [Item]
    public class Oil : Entity
    {
        private Oil(uint ownerNetworkID, uint objectNetworkID, Coordinate coordinate) : base(ownerNetworkID, objectNetworkID, coordinate)
        {
        }
    }
}
