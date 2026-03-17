using System;

namespace ImageCampus.ToolBox.Blueprints
{
    public sealed class BlueprintParameterAttribute : Attribute 
    {
        private string parameterHeader;
        internal string ParameterHeader  => parameterHeader; 

        public BlueprintParameterAttribute(string parameterHeader)
        {
            this.parameterHeader = parameterHeader;
        }
    }
}