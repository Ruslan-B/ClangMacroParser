using System;

namespace RCore.ClangMacroParser
{
    [Flags]
    public enum TokenType
    {
        Identifier = 1 << 0,
        Constant = 1 << 1,
        Operator = 1 << 2,
        Punctuator = 1 << 4,
        Number = 1 << 5,
        String = 1 << 6,
        Char = 1 << 7,
        Left = 1 << 10,
        Right = 1 << 11,
        Comma = 1 << 12,
        Parenthesis = 1 << 13,
        Indexer = 1 << 14,
        Block = 1 << 15
    }
}