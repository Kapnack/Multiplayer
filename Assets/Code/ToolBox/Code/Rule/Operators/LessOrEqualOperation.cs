namespace ImageCampus.ToolBox.Rules
{
    [RuleOperator("<=")]
    public sealed class LessOrEqualOperation : RuleOperation 
    {
        public override bool Evaluate(int a, int b)
        {
            return a <= b;
        }
    }

}
