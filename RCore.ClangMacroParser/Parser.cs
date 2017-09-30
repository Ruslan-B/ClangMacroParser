using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RCore.ClangMacroParser
{
    public static class TokenExtensions
    {
        public static bool IsIdentifier(this Token token) =>
            token.TokenType == TokenType.Identifier;

        public static bool IsOperator(this Token token) =>
            token.TokenType == TokenType.Operator;

        public static bool IsConstant(this Token token) =>
            token.TokenType == TokenType.Number || token.TokenType == TokenType.Char || token.TokenType == TokenType.String;

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

            var i = 0;
            bool CanRead() => i < tokens.Length;
            Token Read() => tokens[i++];
            Token Current() => tokens[i];
            Token Next() => tokens[i + 1];

            Expression Constant()
            {
                var t = Read();
                var value = t.Value; // todo finalize constant parser
                return new ConstantExpression(value);
            }

            Expression Variable() => new VariableExpression(Read().Value);

            Expression Atom()
            {
                var t = Current();
                if (t.IsPunctuator("("))
                {
                    Read();
                    var exp = Expression();
                    while (CanRead() && !Current().IsPunctuator(")"))
                        return MaybeBinary(() => exp);
                    Debug.Assert(Read().IsPunctuator(")"));
                    return MaybeBinary(() => exp);
                }
                if (Current().IsOperator()) return new UnaryExpression(Read().Value, Expression());
                if (t.IsConstant() || t.IsString()) return Constant();
                if (t.IsIdentifier()) return Variable();
                throw new NotImplementedException();
            }

            Expression MaybeUnary(Func<Expression> other)
            {
                if (Current().IsOperator()) return new UnaryExpression(Read().Value, Atom());
                return other();
            }

            Expression MaybeBinary(Func<Expression> other, int x = 0)
            {
                if (CanRead() && Current().IsOperator())
                {
                    var t = Read();
                    var left = other();
                    var right = Expression();

                    return new BinaryExpression(left, t.Value, right);
                }
                return other();
            }

            IEnumerable<Expression> Args()
            {
                Debug.Assert(Read().IsPunctuator("("));
                while (CanRead() && !Current().IsPunctuator(")"))
                {
                    yield return Expression();
                    if (Current().IsPunctuator(",")) Read();
                }
                Debug.Assert(Read().IsPunctuator(")"));
            }

            Expression Call()
            {
                var t = Read();
                return new CallExpression(t.Value, Args());
            }

            Expression MaybeCall(Func<Expression> other)
            {
                if (CanRead() && Current().IsIdentifier() && Next().IsPunctuator("(")) return Call();
                return other();
            }

            Expression Expression()
            {
                var e = MaybeCall(() => MaybeUnary(() => MaybeBinary(() => Atom())));
                if (CanRead())
                    return MaybeBinary(() => e);
                return e;
            }

            return Expression();
        }
    }
    
    public class CallExpression : Expression
    {
        public CallExpression(string name, IEnumerable<Expression> args)
        {
            Name = name;
            Args = args.ToArray();
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public IReadOnlyCollection<Expression> Args { get; }

        public string Name { get; }
    }
    
    public class ConstantExpression : Expression
    {
        public ConstantExpression(object value) => Value = value;

        public object Value { get; }
    }
    
    public class VariableExpression : Expression
    {
        public VariableExpression(string name) => Name = name;

        public string Name { get; }
    }
    
    public class UnaryExpression : Expression
    {
        public UnaryExpression(string methodType, Expression operand)
        {
            MethodType = methodType;
            Operand = operand;
        }

        public string MethodType { get; }

        public Expression Operand { get; }
    }
    
    public class BinaryExpression : Expression
    {
        public BinaryExpression(Expression left, string methodType, Expression right)
        {
            Left = left;
            MethodType = methodType;
            Right = right;
        }

        public Expression Left { get; }
        public string MethodType { get; }
        public Expression Right { get; }
    }

    public abstract class Expression
    {
    }
}