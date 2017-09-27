using System;

namespace RCore.ClangMacroParser
{
    public enum TokenType
    {
        Keyword,
        Identifier,
        Constant,
        String,
        Punctuator,
        Operator
    }
}