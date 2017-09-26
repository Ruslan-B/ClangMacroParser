using System.Diagnostics;

namespace RCore.ClangMacroParser
{
    [DebuggerDisplay("{" + nameof(Value) + "}, {" + nameof(TokenType) + "}")]
    public struct Token
    {
        public Token(TokenType tokenType, object value, int startPosition, int length)
        {
            TokenType = tokenType;
            Value = value;
            Length = length;
            StartPosition = startPosition;
        }

        public TokenType TokenType { get; }
        public object Value { get; }
        public int StartPosition { get; }
        public int Length { get; }
    }
}