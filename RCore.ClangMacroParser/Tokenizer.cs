﻿// Copyright 2017 Baker Hughes. All rights reserved. Company proprietary and confidential.

using System;
using System.Collections.Generic;
using System.Linq;

namespace RCore.ClangMacroParser
{
    public static class Tokenizer
    {
        public static IEnumerable<Token> Tokenize(string expression)
        {
            bool IsAz(char x) => char.ToLower(x) >= 'a' && char.ToLower(x) <= 'z';
            bool IsQuote(char x) => x == '\'';
            bool IsDoubleQuote(char x) => x == '"';
            bool IsNumberStart(char x) => x == '.' || char.ToLower(x) == 'x' || Digits.Contains(x);
            bool IsNumberEnd(char x) => NumberEnd.Contains(char.ToLower(x));
            bool IsIdentifierStart(char x) => x == '_' || IsAz(x);
            bool IsId(char x) => IsIdentifierStart(x) || Digits.Contains(x);

            var characters = expression.ToCharArray();
            var i = 0;

            bool CanRead() => i < characters.Length;
            char Current() => characters[i];
            char Read() => characters[i++];


            IEnumerable<char> YieldWhile(params Func<char, bool>[] tests)
            {
                foreach (var test in tests) while (CanRead() && test(Current())) yield return Read();
            }

            Token Token(TokenType type, IEnumerable<char> x)
            {
                var s = new string(x.ToArray());
                return new Token(type, s, i - s.Length, i);
            }

            Token OneCharToken(TokenType type, char x) => Token(type, new[] {x});

            void SkipSeparators()
            {
                while (CanRead() && Separators.Contains(Current())) Read();
            }

            Token Number() => Token(TokenType.Constant, YieldWhile(IsNumberStart, IsNumberEnd));

            Token IdentifierOrKeyword()
            {
                var value = new string(YieldWhile(IsIdentifierStart, IsId).ToArray());
                return Token(Keywords.Contains(value) ? TokenType.Keyword : TokenType.Identifier, value);
            }

            Token String() => Token(TokenType.String, YieldWhile(x => !IsDoubleQuote(x))); //todo is not complete 

            Token Char() => Token(TokenType.Constant, YieldWhile(x => !IsQuote(x))); //todo is not complete 

            Token Operator() => Token(TokenType.Operator, YieldWhile(Operators.Contains));

            Token Punctuator()
            {
                var c = Read();

                switch (c)
                {
                    case ',': return OneCharToken(TokenType.Punctuator, c);
                    case '(': return OneCharToken(TokenType.Punctuator, c);
                    case ')': return OneCharToken(TokenType.Punctuator, c);
                    case '[': return OneCharToken(TokenType.Punctuator, c);
                    case ']': return OneCharToken(TokenType.Punctuator, c);
                    case '{': return OneCharToken(TokenType.Punctuator, c);
                    case '}': return OneCharToken(TokenType.Punctuator, c);
                    default:
                        throw new NotSupportedException();
                }
            }

            while (CanRead())
            {
                var c = Current();

                if (Separators.Contains(c)) SkipSeparators();
                else if (IsNumberStart(c)) yield return Number();
                else if (IsIdentifierStart(c)) yield return IdentifierOrKeyword();
                else if (IsDoubleQuote(c)) yield return String();
                else if (IsQuote(c)) yield return Char();
                else if (Operators.Contains(c)) yield return Operator();
                else if (Punctuators.Contains(c)) yield return Punctuator();
                else throw new NotSupportedException();
            }
        }

        private static readonly HashSet<char> Digits = new HashSet<char>("0123456789");
        private static readonly HashSet<char> Separators = new HashSet<char>(" \\\r\n\t");
        private static readonly HashSet<char> NumberEnd = new HashSet<char>("ufdbsil");
        private static readonly HashSet<char> Operators = new HashSet<char>("+-*/<>=|~!^&");
        private static readonly HashSet<char> Punctuators = new HashSet<char>(",()[]{}");

        private static readonly HashSet<string> Keywords = new HashSet<string>
        {
            "auto",
            "break",
            "case",
            "char",
            "const",
            "continue",
            "default",
            "do",
            "double",
            "else",
            "enum",
            "extern",
            "float",
            "for",
            "goto",
            "if",
            "int",
            "long",
            "register",
            "return",
            "short",
            "signed",
            "sizeof",
            "static",
            "struct",
            "switch",
            "typedef",
            "union",
            "unsigned",
            "void",
            "volatile",
            "while"
        };
    }
}