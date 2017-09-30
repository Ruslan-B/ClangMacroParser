using System;

namespace RCore.ClangMacroParser.Expressions
{
    public static class OperationTypeConverter
    {
        public static OperationType FromString(string value)
        {
            switch (value)
            {
                case "+": return OperationType.Add;
                case "/": return OperationType.Divide;
                case "%": return OperationType.Modulo;
                case "*": return OperationType.Multiply;
                case "^": return OperationType.Power;
                case "-": return OperationType.Subtract;
                case "&": return OperationType.And;
                case "|": return OperationType.Or;
                case "~": return OperationType.ExclusiveOr;
                case "<<": return OperationType.LeftShift;
                case ">>": return OperationType.RightShift;
                case "&&": return OperationType.AndAlso;
                case "||": return OperationType.OrElse;
                case "==": return OperationType.Equal;
                case "!=": return OperationType.NotEqual;
                case ">=": return OperationType.GreaterThanOrEqual;
                case ">": return OperationType.GreaterThan;
                case "<": return OperationType.LessThan;
                case "<=": return OperationType.LessThanOrEqual;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }
}