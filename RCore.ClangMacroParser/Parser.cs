namespace RCore.ClangMacroParser
{
    public static class Parser
    {
        public static object Parse(string expression)
        {
            var tokens = Tokenizer.Tokenize(expression);
            return null;
        }
    }
}