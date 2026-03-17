namespace ImageCampus.ToolBox.Pool
{
    public interface IResettable
    {
        public void Assign(params object[] parameters);
        public void Reset();
    }
}