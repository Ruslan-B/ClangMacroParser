// Copyright 2017 Baker Hughes. All rights reserved. Company proprietary and confidential.

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RCore.ClangMacroParser.Test
{
    [TestClass]
    public class TokenizerTest
    {
        [TestMethod]
        public void Test0()
        {
            var tokens = Tokenize(@"-1.1f");
            Assert.AreEqual(8, tokens.Length);
        }

        [TestMethod]
        public void Test1()
        {
            var tokens = Tokenize(@" AV_VERSION_INT(LIBAVCODEC_VERSION_MAJOR, \
                                               LIBAVCODEC_VERSION_MINOR, \
                                               LIBAVCODEC_VERSION_MICRO)");
            Assert.AreEqual(8, tokens.Length);
        }

        [TestMethod]
        public void Test2()
        {
            var tokens = Tokenize(@"(LIBAVCODEC_VERSION_MAJOR < 58)");
            Assert.AreEqual(8, tokens.Length);
        }

        [TestMethod]
        public void Test3()
        {
            var tokens = Tokenize(@"MKBETAG('N','O','N','E')");
            Assert.AreEqual(8, tokens.Length);
        }

        [TestMethod]
        public void Test4()
        {
            var tokens = Tokenize(@"MKBETAG(a,b,c,d) ((d) | ((c) << 8) | ((b) << 16) | ((unsigned)(a) << 24))");
            Assert.AreEqual(8, tokens.Length);
        }

        private static Token[] Tokenize(string expression) => Tokenizer.Tokenize(expression).ToArray();
    }
}