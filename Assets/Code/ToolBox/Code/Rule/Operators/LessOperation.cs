namespace ImageCampus.ToolBox.Rules
{
    [RuleOperator("<")]
    public sealed class LessOperation : RuleOperation 
    {
        public override bool Evaluate(int a, int b)
        {
            return a < b;
        }
    }

}
