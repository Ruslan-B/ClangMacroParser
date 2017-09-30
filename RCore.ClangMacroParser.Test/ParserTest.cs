using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RCore.ClangMacroParser.Expressions;

namespace RCore.ClangMacroParser.Test
{
    [TestClass]
    public class ParserTest
    {
        private void CastExpression<T>(IExpression e, params Action<T>[] tests) where T : IExpression
        {
            if (e is T) tests.ToList().ForEach(test => test((T)e));
            Assert.IsInstanceOfType(e, typeof(T));
        }

        [TestMethod]
        public void ExpressionPrecedence()
        {
            var e = Parser.Parse("1 + 2 * 3");
            CastExpression<BinaryExpression>(e, x =>
            {
                Assert.AreEqual(OperationType.Add, x.OperationType);
                CastExpression<ConstantExpression>(x.Left, y => Assert.AreEqual("1", y.Value));
                CastExpression<BinaryExpression>(x.Right, y =>
                {
                    Assert.AreEqual(OperationType.Multiply, y.OperationType);
                    CastExpression<ConstantExpression>(y.Left, z => Assert.AreEqual("2", z.Value));
                    CastExpression<ConstantExpression>(y.Right, z => Assert.AreEqual("3", z.Value));
                });
            });
        }

        [TestMethod]
        public void C()
        {
            var b = Parser.Parse(@" AV_VERSION_INT(LIBAVCODEC_VERSION_MAJOR, \
                                               LIBAVCODEC_VERSION_MINOR, \
                                               LIBAVCODEC_VERSION_MICRO)");
            var c = Parser.Parse(@"(LIBAVCODEC_VERSION_MAJOR < 58)");

            var d = Parser.Parse(@"MKBETAG('N','O','N','E')");

            var e = Parser.Parse(@"MKBETAG(a, b, c, d)((d) | ((c) << 8) | ((b) << 16) | ((unsigned)(a) << 24))");
        }

        [TestMethod]
        public void Cast()
        {
            var e = Parser.Parse(@"((unsigned)(1 - 2))");
            CastExpression<CastExpression>(e, x =>
            {
                Assert.AreEqual("unsigned", x.TargetType);
                Assert.IsInstanceOfType(x.Operand, typeof(BinaryExpression));
            });
            ;
        }
    }
}