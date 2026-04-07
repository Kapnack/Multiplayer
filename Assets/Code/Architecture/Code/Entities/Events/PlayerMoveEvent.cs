using ImageCampus.ToolBox.Events;

public struct PlayerMoveEvent : IEvent
{
    public float x;
    public float y;
    public float z;

    public void Assign(params object[] parameters)
    {
        x = (float)parameters[1];
        y = (float)parameters[2];
        z = (float)parameters[3];
    }

    public void Reset()
    {
        x = default(float);
        y = default(float);
        z = default(float);
    }
}