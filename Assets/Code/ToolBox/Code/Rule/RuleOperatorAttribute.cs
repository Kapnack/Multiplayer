using System;

namespace ImageCampus.ToolBox.Rules
{
    public sealed class RuleOperatorAttribute : Attribute 
    {
        public string operatorKey;

        public RuleOperatorAttribute(string operatorKey)
        {
            this.operatorKey = operatorKey;
        }
    }
}
