using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RCore.ClangMacroParser.Expressions;
using RCore.ClangMacroParser.Tokenization;

namespace RCore.ClangMacroParser
{
    public static class Parser
    {
        // http://en.cppreference.com/w/c/language/operator_precedence
        private static readonly Dictionary<OperationType, int> OperationPrecedence = new Dictionary<OperationType, int>
        {
            {OperationType.Add, 4},
            {OperationType.Divide, 3},
            {OperationType.Modulo, 3},
            {OperationType.Multiply, 3},
            {OperationType.Power, 9},
            {OperationType.Subtract, 4},
            {OperationType.And, 11},
            {OperationType.Or, 10},
            {OperationType.ExclusiveOr, 9},
            {OperationType.LeftShift, 5},
            {OperationType.RightShift, 5},
            {OperationType.AndAlso, 11},
            {OperationType.OrElse, 12},
            {OperationType.Equal, 7},
            {OperationType.NotEqual, 7},
            {OperationType.GreaterThanOrEqual, 6},
            {OperationType.GreaterThan, 6},
            {OperationType.LessThan, 6},
            {OperationType.LessThanOrEqual, 6}
        };

        public static IExpression Parse(string expression)
        {
            var tokens = Tokenizer.Tokenize(expression).ToArray();

            var i = 0;
            bool CanRead() => i < tokens.Length;
            Token Read() => tokens[i++];
            Token Current() => tokens[i];

            bool IsSequenceOf(params Func<Token, bool>[] tests) =>
                i + tests.Length < tokens.Length
                && tests.Select((test, index) => new {test, token = tokens[i + index]}).All(x => x.test(x.token));

            IExpression Constant()
            {
                var t = Read();
                var value = t.Value; // todo finalize constant parser
                return new ConstantExpression(value);
            }

            IExpression Variable() => new VariableExpression(Read().Value);

            TResult InParentheses<TResult>(Func<TResult> func)
            {
                Debug.Assert(Read().IsPunctuator("("));
                var result = func();
                Debug.Assert(Read().IsPunctuator(")"));
                return result;
            }

            IEnumerable<IExpression> Args()
            {
                return InParentheses(() =>
                {
                    var args = new List<IExpression>();
                    while (CanRead() && !Current().IsPunctuator(")"))
                    {
                        args.Add(Expression());
                        if (Current().IsPunctuator(",")) Read();
                    }
                    return args;
                });
            }

            IExpression Call()
            {
                var t = Read();
                return new CallExpression(t.Value, Args());
            }

            IExpression Unary()
            {
                var t = Read();
                var operationType = OperationTypeConverter.FromString(t.Value);
                return new UnaryExpression(operationType, Expression());
            }
            
            IExpression Atomic()
            {
                if (Current().IsPunctuator("(")) return InParentheses(Expression);
                if (Current().IsConstant() || Current().IsString()) return Constant();
                if (Current().IsIdentifier()) return Variable();
                throw new NotSupportedException();
            }

            bool IsCast() => IsSequenceOf(x => x.IsPunctuator("("), x => x.IsKeyword() || x.IsIdentifier(), x => x.IsPunctuator(")"));

            IExpression Cast() => new CastExpression(InParentheses(() => Read().Value), Atomic());

            IExpression NoneAtomic()
            {
                if (CanRead())
                {
                    if (IsSequenceOf(x => x.IsIdentifier(), x => x.IsPunctuator("("))) return Call();
                    if (Current().IsOperator()) return Unary();
                    if (IsCast()) return Cast();
                    return Atomic();
                }
                throw new NotSupportedException();
            }

            IExpression MaybeBinary(IExpression left, int precedence = int.MaxValue)
            {
                if (CanRead() && Current().IsOperator())
                {
                    var operationType = OperationTypeConverter.FromString(Current().Value);
                    var thisPrecedence = OperationPrecedence[operationType];
                    if (thisPrecedence < precedence)
                    {
                        Read();
                        var right = MaybeBinary(Atomic(), thisPrecedence);
                        var binary = new BinaryExpression(left, operationType, right);

                        return MaybeBinary(binary, precedence);
                    }
                }
                return left;
            }

            IExpression Expression() => MaybeBinary(NoneAtomic());

            return Expression();
        }
    }
}