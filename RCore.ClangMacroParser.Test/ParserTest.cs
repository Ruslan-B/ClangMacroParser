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
            Assert.IsInstanceOfType(a, typeof(BinaryExpression));

            var b = Parser.Parse(@" AV_VERSION_INT(LIBAVCODEC_VERSION_MAJOR, \
                                               LIBAVCODEC_VERSION_MINOR, \
                                               LIBAVCODEC_VERSION_MICRO)");
            var c = Parser.Parse(@"(LIBAVCODEC_VERSION_MAJOR < 58)");

            var d = Parser.Parse(@"MKBETAG('N','O','N','E')");

            var e = Parser.Parse(@"MKBETAG(a, b, c, d)((d) | ((c) << 8) | ((b) << 16) | ((unsigned)(a) << 24))");
            var f = Parser.Parse(@"((d) | ((c) << 8) | ((b) << 16) | ((unsigned)(a) << 24))");
        }
    }
}