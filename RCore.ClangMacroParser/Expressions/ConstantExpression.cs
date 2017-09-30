namespace RCore.ClangMacroParser.Expressions
{
    public class ConstantExpression : IExpression
    {
        public ConstantExpression(object value) => Value = value;

        public object Value { get; }
    }
}