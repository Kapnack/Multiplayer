namespace ImageCampus.ToolBox.Rules
{
    [RuleOperator("!=")]
    public sealed class NotEqualOperation : RuleOperation
    {
        public override bool Evaluate(int a, int b)
        {
            return a != b;
        }
    }
}
