using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RCore.ClangMacroParser.Test
{
    [TestClass]
    public class ParserTest
    {
        [TestMethod]
        public void A()
        {
            var a = Parser.Parse("(-1 + 2us) / (4 - 3.f)");
        }
    }
}
