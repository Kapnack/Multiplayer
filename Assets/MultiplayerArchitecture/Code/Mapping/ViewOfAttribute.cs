using System;

namespace ZooArchitect.View.Mapping
{
    public sealed class ViewOfAttribute : Attribute
    {
        public Type architectureType;

        public ViewOfAttribute(Type architectureType)
        {
            this.architectureType = architectureType;
        }
    }

}
