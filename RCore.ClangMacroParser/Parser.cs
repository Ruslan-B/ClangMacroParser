
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RCore.ClangMacroParser
{
    public static class TokenExtensions
    {
        public static bool IsIdentifier(this Token token) =>
            token.TokenType == TokenType.Punctuator;

        public static bool IsOperator(this Token token) =>
            token.TokenType == TokenType.Operator;

        public static bool IsConstant(this Token token) =>
            token.TokenType == TokenType.Constant;

        public static bool IsString(this Token token) =>
            token.TokenType == TokenType.String;
        
        public static bool IsPunctuator(this Token token, string value) =>
            token.TokenType == TokenType.Punctuator && token.Value == value;
    }
    public static class Parser
    {
        public static object Parse(string expression)
        {
            var tokens = Tokenizer.Tokenize(expression).ToArray();

            int i = 0;
            bool CanRead() => i < tokens.Length;
            Token Read() => tokens[i++];
            Token Current() => tokens[i];
            Token Next() => tokens[i];

            Expression Constant()
            {
                var t = Read();
                var value = t.Value;// todo finalize constant parser
                return new ConstantExpression(value);
            }

            Expression Variable() => new VariableExpression(Read().Value);

            Expression Atom()
            {
                return MaybeCall(() => {
                    var t = Current();
                    if (t.IsPunctuator("("))
                    {
                        Read();
                        var exp = Expression();
                        Debug.Assert(Read().IsPunctuator(")"));
                        return exp;
                    }
                    if (t.IsConstant() || t.IsString()) return Constant();
                    if (t.IsIdentifier()) return Variable();
                    throw new NotImplementedException();
                });
            }

            Expression MaybeBinary(Func<Expression> other, int x)
            {
                if (Current().IsOperator())
                {
                    throw new NotImplementedException();
                    
                }
                return other();
            }

            IEnumerable<Expression> Args()
            {
                Debug.Assert(Read().IsPunctuator("("));
                while (CanRead() && Current().IsPunctuator(")"))
                {
                    yield return Expression();
                    if (Current().IsPunctuator(",")) Read();

                }
                Debug.Assert(Read().IsPunctuator(")"));
            }

            Expression Call() => new CallExpression(Current().Value, Args());

            Expression MaybeCall(Func<Expression> other)
            {
                if (CanRead() && Current().IsIdentifier() && Next().IsPunctuator("(")) return Call();
                return other();
            }

            Expression Expression()
            {
                return MaybeCall(() => MaybeBinary(() => Atom(), 0));
            }
            
            return Expression();
        }
    }

    public class CallExpression : Expression
    {
        public string Name { get; }

        public CallExpression(string name, object args)
        {
            Name = name;
        }
    }

    public 
        class ConstantExpression : Expression
    {
        public object Value { get; }

        public ConstantExpression(object value)
        {
            Value = value;
        }
    }

    public  class VariableExpression : Expression
    {
        public string Name { get; }

        public VariableExpression(string name)
        {
            Name = name;
        }
    }

    public abstract class Expression
    {
    }
}