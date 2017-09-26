using System;
using System.Collections.Generic;
using System.Linq;

namespace RCore.ClangMacroParser
{
    public static class Tokenizer
    {
        private static readonly HashSet<char> Digits = new HashSet<char>("0123456789");
        private static readonly HashSet<char> Punctuators = new HashSet<char>(",()[]{}");
        private static readonly HashSet<char> NumberEnd = new HashSet<char>("uUfFdDbBiIlL");
        private static readonly HashSet<char> Operators = new HashSet<char>("+-*/<>=|~!^&");
        private static readonly HashSet<char> Separators = new HashSet<char>(" \\\r\n\t");

        public static IEnumerable<Token> Tokenize(string expression)
        {
            bool IsAz(char x) => x >= 'a' && x <= 'z' || x >= 'A' && x <= 'Z';
            bool IsQuote(char x) => x == '\'';
            bool IsDoubleQuote(char x) => x == '"';
            bool IsNumber(char x) => x == '.' || x == 'x' || x == 'X' || Digits.Contains(x);
            bool IsNumberStart(char x) => x == '-' || IsNumber(x);
            bool IsNumberEnd(char x) => NumberEnd.Contains(x);
            bool IsIdStart(char x) => x == '_' || IsAz(x);
            bool IsId(char x) => IsIdStart(x) || Digits.Contains(x);
            
            var characters = expression.ToCharArray();
            var i = 0;

            bool CanPeek() => i < characters.Length;
            char Peek() => characters[i];
            char Read2() => characters[i++];

            Func<char> Read = () => characters[i++];

            IEnumerable<char> YieldWhile(params Func<char, bool>[] tests)
            {
                foreach (var test in tests)
                    while (CanPeek() && test(Peek())) yield return Read();
            }
            
            Token TokenValue(TokenType type, object value, int start) => new Token(type, value, start, i - start);

            Token TokenString(TokenType type, IEnumerable<char> x)
            {
                var s = new string(x.ToArray());
                return new Token(type, s, i - s.Length, i);
            }

            Token OneCharToken(TokenType type, char x) => TokenString(type, new[] {x});

            void SkipSeparators()
            {
                while (CanPeek() && Separators.Contains(Peek())) Read();
            }

            Token Number()
            {
                var start = i;
                var number = YieldWhile(IsNumberStart, IsNumber).ToArray();
                var numberType = YieldWhile(IsNumberEnd).ToArray();
                var value = GetNumberValue(number, numberType);
                return TokenValue(TokenType.Number | TokenType.Constant, value, start);
            }

            Token Id() => TokenString(TokenType.Identifier, YieldWhile(IsIdStart, IsId));

            Token String() => TokenString(TokenType.String | TokenType.Constant, YieldWhile(x => !IsDoubleQuote(x))); //todo is not complete 

            Token Char() => TokenString(TokenType.Char | TokenType.Constant, YieldWhile(x => !IsQuote(x))); //todo is not complete 

            Token Operator() => TokenString(TokenType.Operator, YieldWhile(Operators.Contains));

            Token Punctuator()
            {
                var c = Read();

                switch (c)
                {
                    case ',': return OneCharToken(TokenType.Punctuator | TokenType.Comma, c);
                    case '(': return OneCharToken(TokenType.Punctuator | TokenType.Parenthesis | TokenType.Left, c);
                    case ')': return OneCharToken(TokenType.Punctuator | TokenType.Parenthesis | TokenType.Right, c);
                    case '[': return OneCharToken(TokenType.Punctuator | TokenType.Indexer | TokenType.Left, c);
                    case ']': return OneCharToken(TokenType.Punctuator | TokenType.Indexer | TokenType.Right, c);
                    case '{': return OneCharToken(TokenType.Punctuator | TokenType.Block | TokenType.Left, c);
                    case '}': return OneCharToken(TokenType.Punctuator | TokenType.Block | TokenType.Right, c);
                    default:
                        throw new NotSupportedException();
                }
            }

            while (CanPeek())
            {
                var c = Peek();

                if (Separators.Contains(c)) SkipSeparators();
                else if (IsNumberStart(c)) yield return Number();
                else if (IsIdStart(c)) yield return Id();
                else if (IsDoubleQuote(c)) yield return String();
                else if (IsQuote(c)) yield return Char();
                else if (Operators.Contains(c)) yield return Operator();
                else if (Punctuators.Contains(c)) yield return Punctuator();
                else throw new NotSupportedException();
            }
        }

        private static object GetNumberValue(IEnumerable<char> number, IEnumerable<char> numberType)
        {
            var numberString = new string(number.ToArray());

            bool isNegative = false;
            if (numberString.StartsWith("-"))
            {
                isNegative = true;
                numberString = numberString.Substring(1);
            }

            bool isHex = false;
            if (numberString.StartsWith("0x"))
            {
                isHex = true;
                numberString = numberString.Substring(2);
            }
            bool isDecimal = numberString.Contains(".");

            return new string(number.Concat(numberType).ToArray());
        }
    }
}