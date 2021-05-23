using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Mastersign.Expressions.Tests
{
    internal class EvaluationContextTest
    {

        [Test]
        public void ParseAndCheckTest()
        {
            var context = new EvaluationContext();
            context.LoadAllPackages();

            Assert.Throws<SyntaxErrorException>(() => context.ParseAndCheckExpression("1 2"));
            Assert.Throws<SemanticErrorException>(() => context.ParseAndCheckExpression("foo"));

            var res = context.ParseAndCheckExpression("1 + 2");
            Assert.NotNull(res);
            Assert.IsTrue(res.CheckSemantic(context, new StringBuilder()));
        }

        [Test]
        public void CompileExpressionTest()
        {
            var context = new EvaluationContext();
            context.LoadAllPackages();

            Assert.Throws<SyntaxErrorException>(() => context.CompileExpression("1 2"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression("foo"));

            var untyped = context.CompileExpression("1 + 2");
            Assert.NotNull(untyped);
            Assert.AreEqual(3, (int)untyped());
        }

        [Test]
        public void CompileExpressionTypedTest()
        {
            var context = new EvaluationContext();
            context.LoadAllPackages();

            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int>("\"abc\""));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<string>("1.0"));

            var typed = context.CompileExpression<int>("1 + 2");
            Assert.NotNull(typed);
            Assert.AreEqual(3, typed());

            var converted = context.CompileExpression<double>("1 + 2");
            Assert.NotNull(converted);
            Assert.AreEqual(3.0, converted());
        }

        [Test]
        public void CompileExpressionWithArgs1Test()
        {
            var context = new EvaluationContext();
            context.LoadAllPackages();
            context.SetParameters(new ParameterInfo("x", typeof(int)));

            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<string, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int>("y"));

            var lambda0 = context.CompileExpression<int, int>("1");
            Assert.NotNull(lambda0);
            Assert.AreEqual(1, lambda0(42));

            var lambda1 = context.CompileExpression<int, int>("x + 1");
            Assert.NotNull(lambda1);
            Assert.AreEqual(2 + 1, lambda1(2));

            var lambda2 = context.CompileExpression<int, string>("\"x = \" & x");
            Assert.NotNull(lambda2);
            Assert.AreEqual("x = 7", lambda2(7));
        }

        [Test]
        public void CompileExpressionWithArgs2Test()
        {
            var context = new EvaluationContext();
            context.LoadAllPackages();
            context.SetParameters(
                new ParameterInfo("x", typeof(int)),
                new ParameterInfo("y", typeof(int)));

            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<string, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, string, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int>("z"));

            var lambda = context.CompileExpression<int, int, int>("x + y");
            Assert.NotNull(lambda);
            Assert.AreEqual(2 + 1, lambda(2, 1));

            var lambda2 = context.CompileExpression<int, int, string>("\"x + y = \" & x + y");
            Assert.NotNull(lambda2);
            Assert.AreEqual("x + y = 5", lambda2(2, 3));
        }

        [Test]
        public void CompileExpressionWithArgs3Test()
        {
            var context = new EvaluationContext();
            context.LoadAllPackages();
            context.SetParameters(
                new ParameterInfo("p1", typeof(int)),
                new ParameterInfo("p2", typeof(int)),
                new ParameterInfo("p3", typeof(int)));

            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<string, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, string, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, string, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int>("p0"));

            var lambda = context.CompileExpression<int, int, int, int>("p1 + p2 + p3");
            Assert.NotNull(lambda);
            Assert.AreEqual(1 + 2 + 4, lambda(1, 2, 4));
        }

        [Test]
        public void CompileExpressionWithArgs4Test()
        {
            var context = new EvaluationContext();
            context.LoadAllPackages();
            context.SetParameters(
                new ParameterInfo("p1", typeof(int)),
                new ParameterInfo("p2", typeof(int)),
                new ParameterInfo("p3", typeof(int)),
                new ParameterInfo("p4", typeof(int)));

            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<string, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, string, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, string, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, string, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, int>("p0"));

            var lambda = context.CompileExpression<int, int, int, int, int>("p1 + p2 + p3 + p4");
            Assert.NotNull(lambda);
            Assert.AreEqual(1 + 2 + 4 + 8, lambda(1, 2, 4, 8));
        }

        [Test]
        public void CompileExpressionWithArgs5Test()
        {
            var context = new EvaluationContext();
            context.LoadAllPackages();
            context.SetParameters(
                new ParameterInfo("p1", typeof(int)),
                new ParameterInfo("p2", typeof(int)),
                new ParameterInfo("p3", typeof(int)),
                new ParameterInfo("p4", typeof(int)),
                new ParameterInfo("p5", typeof(int)));

            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<string, int, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, string, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, string, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, string, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, string, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, int, int>("p0"));

            var lambda = context.CompileExpression<int, int, int, int, int, int>("p1 + p2 + p3 + p4 + p5");
            Assert.NotNull(lambda);
            Assert.AreEqual(1 + 2 + 4 + 8 + 16, lambda(1, 2, 4, 8, 16));
        }

        [Test]
        public void CompileExpressionWithArgs6Test()
        {
            var context = new EvaluationContext();
            context.LoadAllPackages();
            context.SetParameters(
                new ParameterInfo("p1", typeof(int)),
                new ParameterInfo("p2", typeof(int)),
                new ParameterInfo("p3", typeof(int)),
                new ParameterInfo("p4", typeof(int)),
                new ParameterInfo("p5", typeof(int)),
                new ParameterInfo("p6", typeof(int)));

            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<string, int, int, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, string, int, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, string, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, string, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, string, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, int, string, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, int, int, int>("p0"));

            var lambda = context.CompileExpression<int, int, int, int, int, int, int>("p1 + p2 + p3 + p4 + p5 + p6");
            Assert.NotNull(lambda);
            Assert.AreEqual(1 + 2 + 4 + 8 + 16 + 32, lambda(1, 2, 4, 8, 16, 32));
        }

        [Test]
        public void CompileExpressionWithArgs7Test()
        {
            var context = new EvaluationContext();
            context.LoadAllPackages();
            context.SetParameters(
                new ParameterInfo("p1", typeof(int)),
                new ParameterInfo("p2", typeof(int)),
                new ParameterInfo("p3", typeof(int)),
                new ParameterInfo("p4", typeof(int)),
                new ParameterInfo("p5", typeof(int)),
                new ParameterInfo("p6", typeof(int)),
                new ParameterInfo("p7", typeof(int)));

            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, int, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<string, int, int, int, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, string, int, int, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, string, int, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, string, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, string, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, int, string, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, int, int, string, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, int, int, int, int>("p0"));

            var lambda = context.CompileExpression<int, int, int, int, int, int, int, int>("p1 + p2 + p3 + p4 + p5 + p6 + p7");
            Assert.NotNull(lambda);
            Assert.AreEqual(1 + 2 + 4 + 8 + 16 + 32 + 64, lambda(1, 2, 4, 8, 16, 32, 64));
        }

        [Test]
        public void CompileExpressionWithArgs8Test()
        {
            var context = new EvaluationContext();
            context.LoadAllPackages();
            context.SetParameters(
                new ParameterInfo("p1", typeof(int)),
                new ParameterInfo("p2", typeof(int)),
                new ParameterInfo("p3", typeof(int)),
                new ParameterInfo("p4", typeof(int)),
                new ParameterInfo("p5", typeof(int)),
                new ParameterInfo("p6", typeof(int)),
                new ParameterInfo("p7", typeof(int)),
                new ParameterInfo("p8", typeof(int)));

            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, int, int, int, int>("0"));
            //Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, int, int, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<string, int, int, int, int, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, string, int, int, int, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, string, int, int, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, string, int, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, string, int, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, int, string, int, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, int, int, string, int, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, int, int, int, string, int>("0"));
            Assert.Throws<SemanticErrorException>(() => context.CompileExpression<int, int, int, int, int, int, int, int, int>("p0"));

            var lambda = context.CompileExpression<int, int, int, int, int, int, int, int, int>("p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8");
            Assert.NotNull(lambda);
            Assert.AreEqual(1 + 2 + 4 + 8 + 16 + 32 + 64 + 128, lambda(1, 2, 4, 8, 16, 32, 64, 128));
        }

        [Test]
        public void EvaluateExpressionTest()
        {
            var context = new EvaluationContext();
            context.LoadAllPackages();

            Assert.Throws<SyntaxErrorException>(() => context.EvaluateExpression("1 2"));
            Assert.Throws<SemanticErrorException>(() => context.EvaluateExpression("foo"));

            Assert.AreEqual((int)context.EvaluateExpression("1 + 2"), 3);
        }

        [Test]
        public void StaticLambdaTest()
        {
            var context = new EvaluationContext();
            context.AddFunction("test", (Func<string, int>)(static s => s.Length));

            var evaluatedResult = context.EvaluateExpression("test(\"abc\")");
            Assert.AreEqual(3, evaluatedResult);
        }

        [Test]
        public void DynamicLambdaTest()
        {
            var closureVar = new string('X', 4);

            var context = new EvaluationContext();
            context.AddFunction("test", (Func<int>)(() => closureVar.Length));

            var evaluatedResult = context.EvaluateExpression("test()");
            Assert.AreEqual(4, evaluatedResult);
        }

    }
}
