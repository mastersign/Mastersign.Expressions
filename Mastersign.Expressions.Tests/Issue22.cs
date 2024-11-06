using NUnit.Framework;

namespace Mastersign.Expressions.Tests
{
    public class Issue22
    {
        [Test]
        public void NullCheckDirectEquality()
        {
            var ctx = new EvaluationContext();
            ctx.SetVariable("x", null);
            var result = ctx.EvaluateExpression("if(x=null,\"0\", \"1\")");
            Assert.AreEqual("0", result);
        }

        [Test]
        public void NullCheckStringLength()
        {
            var ctx = new EvaluationContext();
            ctx.LoadStringPackage();
            ctx.SetVariable("x", null);
            // The (string) length of null is not defined.
            Assert.Throws(
                typeof(SemanticErrorException),
                () => ctx.EvaluateExpression("if(len(x)=0,\"0\", \"1\")"));
        }

        [Test]
        public void NullCheckEmptyString()
        {
            var ctx = new EvaluationContext();
            ctx.LoadConversionPackage();
            ctx.SetVariable("x", null);
            var result = ctx.EvaluateExpression("if(c_str(x)=\"\",\"0\", \"1\")");
            Assert.AreEqual("0", result);
        }

        [Test]
        public void NullCheckIntegratedFunction()
        {
            var ctx = new EvaluationContext();
            ctx.SetVariable("x", null);
            var result = ctx.EvaluateExpression("if(isnull(x),\"0\", \"1\")");
            Assert.AreEqual("0", result);
        }
    }
}
