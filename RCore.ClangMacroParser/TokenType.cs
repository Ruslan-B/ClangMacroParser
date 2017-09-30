using System;

namespace RCore.ClangMacroParser
{
    public enum TokenType
    {
        Keyword,
        Identifier,
        Number,
        Char,
        String,
        Punctuator,
        Operator
    }
}