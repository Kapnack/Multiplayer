using Assets.MultiplayerArchitecture.Code.Entities;
using ImageCampus.ToolBox.Events;

public struct PlayerMove : IEvent
{
    public Coordinate coordinate;
    public void Assign(params object[] parameters)
    {
        coordinate = (Coordinate)parameters[0];
    }

    public void Reset()
    {
        coordinate = default(Coordinate);
    }
}