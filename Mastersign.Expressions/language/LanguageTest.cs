using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sprache;
using System.Linq.Expressions;

namespace de.mastersign.expressions.language
{
    internal class LanguageTest
    {
        #region Helper

        static T Parse<T>(Parser<T> parser, string input)
        {
            return parser.End().Parse(input);
        }

        static void ExpectAccept<T>(Parser<T> parser, params string[] input)
        {
            foreach (var err in
                from text in input
                let res = parser.End().TryParse(text)
                where !res.WasSuccessful
                select Tuple.Create(text, res.Message))
            {
                throw new Exception(err.Item2 + " (input: '" + err.Item1 + "')");
            }
        }

        static void ExpectReject<T>(Parser<T> parser, params string[] input)
        {
            foreach (var text in
                from text in input
                let res = parser.End().TryParse(text)
                where res.WasSuccessful
                select text)
            {
                throw new Exception("Input succeeded: '" + text + "'");
            }
        }

        private static void ExpectResult(string input, Type expectedType, object expectedValue)
        {
            ExpectResult(new EvaluationContext(), input, expectedType, expectedValue);
        }

        private static void ExpectResult(EvaluationContext context, string input, Type expectedType, object expectedValue)
        {
            var res = Parse(Grammar.Expression, input);
            var sb = new StringBuilder();
            var semanticSuccess = res.CheckSemantic(context, sb);
            var resType = semanticSuccess ? res.GetValueType(context) : null;
            var resValue = semanticSuccess ? res.GetValue(context) : null;
            var expr = semanticSuccess ? res.GetExpression(context) : null;
            var exprType = semanticSuccess ? expr.Type : null;
            var typesMatch1 = expectedType == resType;
            var typesMatch2 = expectedType == exprType;
            var exprValue = semanticSuccess ? EvaluateExpression(expr) : null;
            var typeConsistance1 = resValue == null || resType == resValue.GetType();
            var typeConsistance2 = exprValue == null || exprType == exprValue.GetType();
            var valuesMatch1 = expectedValue == null
                ? resValue == null : expectedValue.Equals(resValue);
            var valuesMatch2 = expectedValue == null
                ? exprValue == null : expectedValue.Equals(exprValue);

            if (!semanticSuccess)
            {
                Console.WriteLine("\nUnexpected semantic error:");
                Console.WriteLine(sb.ToString());
                Console.WriteLine("From expression:");
                Console.WriteLine(res.Source);
            }
            else if (!typesMatch1)
            {
                Console.WriteLine("Unexpected type from interpreted expression:");
                Console.WriteLine(res.Source);
                Console.WriteLine("Expected: " + expectedType);
                Console.WriteLine("Returned: " + resType);
            }
            else if (!typesMatch2)
            {
                Console.WriteLine("Unexpected type from compiled expression:");
                Console.WriteLine(res.Source);
                Console.WriteLine("Expected: " + expectedType);
                Console.WriteLine("Returned: " + exprType);
            }
            else if (!typeConsistance1)
            {
                Console.WriteLine("Inconsistent types from interpreted expression:");
                Console.WriteLine(res.Source);
                Console.WriteLine("Announced: " + resType);
                Console.WriteLine("Delivered: " + resValue.GetType());
            }
            else if (!typeConsistance2)
            {
                Console.WriteLine("Inconsistent types from compiled expression:");
                Console.WriteLine(res.Source);
                Console.WriteLine("Announced: " + resType);
                Console.WriteLine("Delivered: " + exprValue.GetType());
            }
            else if (!valuesMatch1)
            {
                Console.WriteLine("Unexpected result value from interpreted expression:");
                Console.WriteLine(res.Source);
                Console.WriteLine("Expected: '{0}'", expectedValue);
                Console.WriteLine("Returned: '{0}'", resValue);
            }
            else if (!valuesMatch2)
            {
                Console.WriteLine("Unexpected result value from compiled expression:");
                Console.WriteLine(res.Source);
                Console.WriteLine("Expected: '{0}'", expectedValue);
                Console.WriteLine("Returned: '{0}'", exprValue);
            }

            if (!semanticSuccess) Assert.Fail("Unexpected semantic error.");
            Assert.IsFalse(sb.Length > 0, "An error message got written.");
            if (!typesMatch1) Assert.Fail("Unexpected result type from interpretation.");
            if (!typesMatch2) Assert.Fail("Unexpected result type from compiled expression.");
            if (!typeConsistance1) Assert.Fail("Inconsistent types from interpretation.");
            if (!typeConsistance2) Assert.Fail("Inconsistent types from compiled expression.");
            if (!valuesMatch1) Assert.Fail("Unexpected result value from interpretation.");
            if (!valuesMatch2) Assert.Fail("Unexpected result value from compiled expression.");
        }

        private static object EvaluateExpression(Expression expr)
        {
            if (expr.Type != typeof (object))
            {
                expr = Expression.Convert(expr, typeof (object));
            }
            var lambda = Expression.Lambda<Func<object>>(expr);
            var function = lambda.Compile();
            return function();
        }

        private static void ExpectError(string input)
        {
            ExpectError(new EvaluationContext(), input);
        }

        private static void ExpectError(EvaluationContext context, string input)
        {
            var res = Parse(Grammar.Expression, input);
            var sb = new StringBuilder();
            Assert.IsFalse(res.CheckSemantic(context, sb));
            Assert.IsTrue(sb.Length > 0);
        }

        #endregion

        [Test]
        public void OperatorTest()
        {
            ExpectReject(Grammar.AnyOperator,
                " ", "abc", "#", "==", "< >");

            ExpectAccept(Grammar.AnyOperator,
                "+", "-", " +", "+ ", "  +  ", "<>");

            Assert.AreEqual(Operator.NumAddition, Parse(Grammar.AnyOperator, "+"));
            Assert.AreEqual(Operator.NumSubtraction, Parse(Grammar.AnyOperator, "-"));
            Assert.AreEqual(Operator.NumMultiplication, Parse(Grammar.AnyOperator, "*"));
            Assert.AreEqual(Operator.NumDivision, Parse(Grammar.AnyOperator, "/"));
            Assert.AreEqual(Operator.NumPower, Parse(Grammar.AnyOperator, "^"));

            Assert.AreEqual(Operator.BoolAnd, Parse(Grammar.AnyOperator, "and"));
            Assert.AreEqual(Operator.BoolOr, Parse(Grammar.AnyOperator, "or"));
            Assert.AreEqual(Operator.BoolXor, Parse(Grammar.AnyOperator, "xor"));

            Assert.AreEqual(Operator.StringConcat, Parse(Grammar.AnyOperator, "&"));

            Assert.AreEqual(Operator.RelationLess, Parse(Grammar.AnyOperator, "<"));
            Assert.AreEqual(Operator.RelationLessOrEqual, Parse(Grammar.AnyOperator, "<="));
            Assert.AreEqual(Operator.RelationEqual, Parse(Grammar.AnyOperator, "="));
            Assert.AreEqual(Operator.RelationUnequal, Parse(Grammar.AnyOperator, "<>"));
            Assert.AreEqual(Operator.RelationGreaterOrEqual, Parse(Grammar.AnyOperator, ">="));
            Assert.AreEqual(Operator.RelationGreater, Parse(Grammar.AnyOperator, ">"));
        }

        [Test]
        public void NullLiteralTest()
        {
            ExpectReject(Grammar.NullLiteral,
                " ", "n", "0", "NULL");

            ExpectAccept(Grammar.NullLiteral,
                "null", " null", "null ", "\tnull ");

            ExpectResult("null", typeof(object), null);
        }

        [Test]
        public void IntegerLiteralTest()
        {
            ExpectReject(Grammar.IntegerLiteral,
                " ", "a", "0x", "x0", "0.0", " 0. ", " .0 ", "1 2", "++1");

            ExpectAccept(Grammar.IntegerLiteral,
                "1", " 1", "1 ", "1234", "0123", "9354", "-1", "+1", " + 1 ", " +\t1 ",
                "1i", "1I", "1l", "1L");

            var context = new EvaluationContext();

            Assert.AreEqual(12345, Parse(Grammar.IntegerLiteral, " 12345").GetValue(context));
            Assert.AreEqual(0, Parse(Grammar.IntegerLiteral, "0").GetValue(context));

            Assert.AreEqual(typeof(int), Parse(Grammar.IntegerLiteral, "1").GetValueType(context));
            Assert.AreEqual(typeof(int), Parse(Grammar.IntegerLiteral, "0").GetValueType(context));
            Assert.AreEqual(typeof(int), Parse(Grammar.IntegerLiteral, "-1").GetValueType(context));
            Assert.AreEqual(typeof(int), Parse(Grammar.IntegerLiteral, int.MinValue.ToString(CultureInfo.InvariantCulture)).GetValueType(context));
            Assert.AreEqual(typeof(int), Parse(Grammar.IntegerLiteral, int.MaxValue.ToString(CultureInfo.InvariantCulture)).GetValueType(context));
            Assert.AreEqual(typeof(long), Parse(Grammar.IntegerLiteral, (int.MinValue - 1L).ToString(CultureInfo.InvariantCulture)).GetValueType(context));
            Assert.AreEqual(typeof(long), Parse(Grammar.IntegerLiteral, (int.MaxValue + 1L).ToString(CultureInfo.InvariantCulture)).GetValueType(context));
            Assert.AreEqual(typeof(long), Parse(Grammar.IntegerLiteral, long.MinValue.ToString(CultureInfo.InvariantCulture)).GetValueType(context));
            Assert.AreEqual(typeof(long), Parse(Grammar.IntegerLiteral, long.MaxValue.ToString(CultureInfo.InvariantCulture)).GetValueType(context));

            Assert.AreEqual(typeof(int), Parse(Grammar.IntegerLiteral, "1").GetValue(context).GetType());
            Assert.AreEqual(typeof(long), Parse(Grammar.IntegerLiteral, long.MaxValue.ToString(CultureInfo.InvariantCulture)).GetValue(context).GetType());

            Assert.AreEqual(typeof(long), Parse(Grammar.IntegerLiteral, "1l").GetValueType(context));
            Assert.AreEqual(typeof(long), Parse(Grammar.IntegerLiteral, "1l").GetValue(context).GetType());
            Assert.AreEqual(typeof(long), Parse(Grammar.IntegerLiteral, "1L").GetValueType(context));
            Assert.AreEqual(typeof(long), Parse(Grammar.IntegerLiteral, "1L").GetValue(context).GetType());

            Assert.AreEqual(typeof(int), Parse(Grammar.IntegerLiteral, "1i").GetValueType(context));
            Assert.AreEqual(typeof(int), Parse(Grammar.IntegerLiteral, "1i").GetValue(context).GetType());
            Assert.AreEqual(typeof(int), Parse(Grammar.IntegerLiteral, "1I").GetValueType(context));
            Assert.AreEqual(typeof(int), Parse(Grammar.IntegerLiteral, "1I").GetValue(context).GetType());

            var res = Parse(Grammar.IntegerLiteral, "1234");
            Assert.IsTrue(res.CheckSemantic(context, new StringBuilder()));
        }

        [Test]
        public void FloatingPointTest()
        {
            ExpectReject(Grammar.FloatingPointLiteral,
                " ", "a", "0x", "x0", "0", ".0", "0.", " . ", "1 .2");

            ExpectAccept(Grammar.FloatingPointLiteral,
                "0.0", "123.456", "-1.0", "+1.0", " -\t1.0 ", " +\t1.0 ",
                "0.0f", "0.0F", "0.0d", "0.0D",
                "0f", "0F", "0d", "0D");

            var context = new EvaluationContext();

            Assert.AreEqual(0.0, Parse(Grammar.FloatingPointLiteral, "0.0").GetValue(context));
            Assert.AreEqual(-1.0, Parse(Grammar.FloatingPointLiteral, "-1.0").GetValue(context));
            Assert.AreEqual(1.0, Parse(Grammar.FloatingPointLiteral, "+1.0").GetValue(context));
            Assert.AreEqual(123.0, Parse(Grammar.FloatingPointLiteral, "123.0").GetValue(context));
            Assert.AreEqual(-123.0, Parse(Grammar.FloatingPointLiteral, "-123.000").GetValue(context));
            Assert.AreEqual(123.456, Parse(Grammar.FloatingPointLiteral, "123.456").GetValue(context));

            Assert.AreEqual(typeof(float), Parse(Grammar.FloatingPointLiteral, "1.0f").GetValueType(context));
            Assert.AreEqual(typeof(float), Parse(Grammar.FloatingPointLiteral, "1.0f").GetValue(context).GetType());
            Assert.AreEqual(typeof(float), Parse(Grammar.FloatingPointLiteral, "1.0F").GetValueType(context));
            Assert.AreEqual(typeof(float), Parse(Grammar.FloatingPointLiteral, "1.0F").GetValue(context).GetType());
            Assert.AreEqual(typeof(double), Parse(Grammar.FloatingPointLiteral, "1.0d").GetValueType(context));
            Assert.AreEqual(typeof(double), Parse(Grammar.FloatingPointLiteral, "1.0d").GetValue(context).GetType());
            Assert.AreEqual(typeof(double), Parse(Grammar.FloatingPointLiteral, "1.0D").GetValueType(context));
            Assert.AreEqual(typeof(double), Parse(Grammar.FloatingPointLiteral, "1.0D").GetValue(context).GetType());

            var res = Parse(Grammar.FloatingPointLiteral, "12.5");
            Assert.IsTrue(res.CheckSemantic(context, new StringBuilder()));
            Assert.AreEqual(typeof(float), res.GetValueType(context));
            Assert.IsTrue((float)res.GetValue(context) == 12.5f);

            res = Parse(Grammar.FloatingPointLiteral, "125F");
            Assert.IsTrue(res.CheckSemantic(context, new StringBuilder()));
            Assert.AreEqual(typeof(float), res.GetValueType(context));
            Assert.IsTrue((float)res.GetValue(context) == 125f);
        }

        [Test]
        public void DecimalTest()
        {
            ExpectReject(Grammar.DecimalLiteral,
                " ", "M", "m", "0x", "x0", "0", ".0", "0.", ".0M", "0.M", ".M", " . ", "1 .2M", "0.0", "0");

            ExpectAccept(Grammar.DecimalLiteral,
                "0.0M", "0.0m", "123.456M", "-1.0M", "+1.0M", " -\t1.0M ", " +\t1.0M ");

            var context = new EvaluationContext();

            Assert.AreEqual(0.0M, Parse(Grammar.DecimalLiteral, "0.0M").GetValue(context));
            Assert.AreEqual(-1.0M, Parse(Grammar.DecimalLiteral, "-1.0M").GetValue(context));
            Assert.AreEqual(1.0M, Parse(Grammar.DecimalLiteral, "+1.0M").GetValue(context));
            Assert.AreEqual(123.0M, Parse(Grammar.DecimalLiteral, "123.0M").GetValue(context));
            Assert.AreEqual(-123.0M, Parse(Grammar.DecimalLiteral, "-123.000M").GetValue(context));
            Assert.AreEqual(123.456M, Parse(Grammar.DecimalLiteral, "123.456M").GetValue(context));

            var res = Parse(Grammar.DecimalLiteral, "12.34M");
            Assert.IsTrue(res.CheckSemantic(context, new StringBuilder()));
        }

        [Test]
        public void BooleanTest()
        {
            ExpectReject(Grammar.BooleanLiteral,
                "", " ", "0", "1", "t", "f");

            ExpectAccept(Grammar.BooleanLiteral,
                "true", "false");

            var context = new EvaluationContext();

            Assert.AreEqual(true, Parse(Grammar.BooleanLiteral, "true").GetValue(context));
            Assert.AreEqual(false, Parse(Grammar.BooleanLiteral, "false").GetValue(context));

            var res = Parse(Grammar.BooleanLiteral, "true");
            Assert.IsTrue(res.CheckSemantic(context, new StringBuilder()));
        }

        [Test]
        public void StringTest()
        {
            ExpectReject(Grammar.StringLiteral,
                "", " ", "\"", "'", "''", "' '", "\"abc", "\"a\" \"b\"");

            ExpectAccept(Grammar.StringLiteral, "\"\""); // '""'
            ExpectAccept(Grammar.StringLiteral, " \"\" "); // ' "" '
            ExpectAccept(Grammar.StringLiteral, "\" \""); // '" "'
            ExpectAccept(Grammar.StringLiteral, "\"abc def\""); // '"abc def"'
            ExpectAccept(Grammar.StringLiteral, " \"abc def\" "); // ' "abc def" '
            ExpectAccept(Grammar.StringLiteral, "\"\\\"\""); // '"\""'
            ExpectAccept(Grammar.StringLiteral, "\"\\t\\\"abc\\\"\""); // '"\t\"abc\""'

            ExpectResult("\"abc\"", typeof(string), "abc");
            ExpectResult("\" 123\\t \\n\\r\\\\ ende\"", typeof(string), " 123\t \n\r\\ ende");
        }

        [Test]
        public void GroupTest()
        {
            ExpectReject(Grammar.Group,
                "", " ", "(", "()", "( )", ")", "12");

            ExpectAccept(Grammar.Group,
                "(1)", "(1+2)", "(((1)))");

            var context = new EvaluationContext();

            Assert.AreEqual(1, Parse(Grammar.Group, "((1))").GetValue(context));
        }

        [Test]
        public void TermTest()
        {
            ExpectReject(Grammar.Term,
                "", " ", "1 + 2");

            ExpectAccept(Grammar.Term,
                "1", "(1+2)", "(true and false)");
        }

        [Test]
        public void VariableTest()
        {
            ExpectReject(Grammar.Variable,
                "", " ", "1", "123", "1a", "12abc", "1_");

            ExpectAccept(Grammar.Variable,
                "a", "A", " a ", "abc", " abc ", "a123", "a_123", "a123_", "_a");

            var context = new EvaluationContext();
            context.SetVariable("a", 1.0);
            context.SetVariable("b", true);
            context.SetVariable("c", "Hello");

            context.SetVariable("Byte", (byte)100, false);
            context.SetVariable("Int16", (short)100, false);
            context.SetVariable("Int32", (int)100, false);
            context.SetVariable("Int64", (long)100, false);
            context.SetVariable("Single", (float)100, false);
            context.SetVariable("Double", (double)100, false);

            context.SetVariable("ByteC", (byte)100, true);
            context.SetVariable("Int16C", (short)100, true);
            context.SetVariable("Int32C", (int)100, true);
            context.SetVariable("Int64C", (long)100, true);
            context.SetVariable("SingleC", (float)100, true);
            context.SetVariable("DoubleC", (double)100, true);

            ExpectError(context, "x");
            ExpectResult(context, "a", typeof(double), 1.0);
            ExpectResult(context, "b", typeof(bool), true);
            ExpectResult(context, "c", typeof(string), "Hello");

            ExpectResult(context, "Byte", typeof(byte), (byte)100);
            ExpectResult(context, "Int16", typeof(short), (short)100);
            ExpectResult(context, "Int32", typeof(int), 100);
            ExpectResult(context, "Int64", typeof(long), 100L);
            ExpectResult(context, "Single", typeof(float), 100F);
            ExpectResult(context, "Double", typeof(double), 100.0);

            ExpectResult(context, "ByteC", typeof(byte), (byte)100);
            ExpectResult(context, "Int16C", typeof(short), (short)100);
            ExpectResult(context, "Int32C", typeof(int), 100);
            ExpectResult(context, "Int64C", typeof(long), 100L);
            ExpectResult(context, "SingleC", typeof(float), 100F);
            ExpectResult(context, "DoubleC", typeof(double), 100.0);
        }

        [Test]
        public void ConstantVariableTest()
        {
            var context = new EvaluationContext();
            context.SetVariable("x", 1, true);
            context.SetVariable("y", 1, false);

            var astX = Grammar.Expression.End().Parse("x");
            Assert.IsTrue(astX.CheckSemantic(context, new StringBuilder()));
            var exprX = astX.GetExpression(context);
            var lambdaX = Expression.Lambda<Func<int>>(exprX);
            var x = lambdaX.Compile();

            var astY = Grammar.Expression.End().Parse("y");
            Assert.IsTrue(astY.CheckSemantic(context, new StringBuilder()));
            var exprY = astY.GetExpression(context);
            var lambdaY = Expression.Lambda<Func<int>>(exprY);
            var y = lambdaY.Compile();

            // Precondition
            Assert.AreEqual(1, x());
            Assert.AreEqual(1, y());

            // Change variable values
            context.SetVariable("x", 2, true);
            context.SetVariable("y", 2, false);

            // Check constant behavior
            Assert.AreEqual(1, x(), "The constant value was updated from the context.");
            Assert.AreEqual(2, y(), "The variable value was not updated from the context.");
        }

        [Test]
        public void NumericOperatorTest()
        {
            ExpectError("1 + \"abc\"");
            ExpectError("\"abc\" + 1");
            ExpectError("1 + true");
            ExpectError("true + 1");
            ExpectError("1 + null");
            ExpectError("null + 1");

            ExpectResult("2+3", typeof(int), 2 + 3);
            ExpectResult("2.0+3.0", typeof(float), 2.0f + 3.0f);
            ExpectResult("2-3", typeof(int), 2 - 3);
            ExpectResult("2.0-3.0", typeof(float), 2.0f - 3.0f);
            ExpectResult("2*3", typeof(int), 2 * 3);
            ExpectResult("2.0*3.0", typeof(float), 2.0f * 3.0f);
            ExpectResult("2/3", typeof(int), 2 / 3);
            ExpectResult("2.0/3.0", typeof(float), 2.0f / 3.0f);
            ExpectResult("2^3", typeof(double), Math.Pow(2, 3));
            ExpectResult("2.0^3.0", typeof(double), Math.Pow(2, 3));
        }

        [Test]
        public void BooleanOperatorTest()
        {
            ExpectError("true and 1");
            ExpectError("1 and true");
            ExpectError("true and 1.0");
            ExpectError("1.0 and true");
            ExpectError("true and \"abc\"");
            ExpectError("\"abc\" and true");
            ExpectError("true and null");
            ExpectError("null and true");

            ExpectResult("false and false", typeof(bool), false);
            ExpectResult("false and true", typeof(bool), false);
            ExpectResult("true and false", typeof(bool), false);
            ExpectResult("true and true", typeof(bool), true);

            ExpectResult("false or false", typeof(bool), false);
            ExpectResult("false or true", typeof(bool), true);
            ExpectResult("true or false", typeof(bool), true);
            ExpectResult("true or true", typeof(bool), true);

            ExpectResult("false xor false", typeof(bool), false);
            ExpectResult("false xor true", typeof(bool), true);
            ExpectResult("true xor false", typeof(bool), true);
            ExpectResult("true xor true", typeof(bool), false);
        }

        [Test]
        public void RelationOperatorTest()
        {
            ExpectError("1 < true");
            ExpectError("true < 1");
            ExpectError("1.0 < true");
            ExpectError("true < 1.0");
            ExpectError("1.0m < true");
            ExpectError("true < 1.0m");
            ExpectError("\"abc\" < 1");
            ExpectError("1 < \"abc\"");
            ExpectError("1 < null");
            ExpectError("null < 1");
            ExpectError("null < \"a\"");
            ExpectError("1 <> null");

            ExpectResult("1 < 2", typeof(bool), true);
            ExpectResult("2 < 2", typeof(bool), false);
            ExpectResult("3 < 2", typeof(bool), false);

            ExpectResult("1 <= 2", typeof(bool), true);
            ExpectResult("2 <= 2", typeof(bool), true);
            ExpectResult("3 <= 2", typeof(bool), false);

            ExpectResult("1 = 2", typeof(bool), false);
            ExpectResult("2 = 2", typeof(bool), true);
            ExpectResult("3 = 2", typeof(bool), false);

            ExpectResult("1 <> 2", typeof(bool), true);
            ExpectResult("2 <> 2", typeof(bool), false);
            ExpectResult("3 <> 2", typeof(bool), true);

            ExpectResult("1 >= 2", typeof(bool), false);
            ExpectResult("2 >= 2", typeof(bool), true);
            ExpectResult("3 >= 2", typeof(bool), true);

            ExpectResult("1 > 2", typeof(bool), false);
            ExpectResult("2 > 2", typeof(bool), false);
            ExpectResult("3 > 2", typeof(bool), true);

            ExpectResult("1.0F < 2.0F", typeof(bool), true);
            ExpectResult("1.0D < 2.0D", typeof(bool), true);
            ExpectResult("1.0M < 2.0M", typeof(bool), true);
            ExpectResult("1I < 2I", typeof(bool), true);
            ExpectResult("1L < 2L", typeof(bool), true);
            
            ExpectResult("100I = 100I", typeof(bool), true);
            ExpectResult("100I = 100L", typeof(bool), true);
            ExpectResult("100I = 100.0F", typeof(bool), true);
            ExpectResult("100I = 100.0D", typeof(bool), true);
            ExpectResult("100I = 100.0M", typeof(bool), true);

            ExpectResult("100L = 100I", typeof(bool), true);
            ExpectResult("100L = 100L", typeof(bool), true);
            ExpectResult("100L = 100.0F", typeof(bool), true);
            ExpectResult("100L = 100.0D", typeof(bool), true);
            ExpectResult("100L = 100.0M", typeof(bool), true);

            ExpectResult("100.0F = 100I", typeof(bool), true);
            ExpectResult("100.0F = 100L", typeof(bool), true);
            ExpectResult("100.0F = 100.0F", typeof(bool), true);
            ExpectResult("100.0F = 100.0D", typeof(bool), true);

            ExpectResult("100.0D = 100I", typeof(bool), true);
            ExpectResult("100.0D = 100L", typeof(bool), true);
            ExpectResult("100.0D = 100.0F", typeof(bool), true);
            ExpectResult("100.0D = 100.0D", typeof(bool), true);

            ExpectResult("100.0M = 100I", typeof(bool), true);
            ExpectResult("100.0M = 100L", typeof(bool), true);
            ExpectResult("100.0M = 100.0M", typeof(bool), true);

            ExpectResult("\"a\" < \"b\"", typeof(bool), true);
            ExpectResult("\"a\" = \"A\"", typeof(bool), false);
            ExpectResult("null <> \"a\"", typeof(bool), true);
        }

        [Test]
        public void StringOperatorTest()
        {
            ExpectResult("true & false", typeof(string), "TrueFalse");
            ExpectResult("80 & 4.2 & true", typeof(string), "80" + 4.2 + "True");
            ExpectResult("\"abc\" & \"def\"", typeof(string), "abcdef");
            ExpectResult("null & \"abc\"", typeof(string), "abc");
        }

        [Test]
        public void ExpressionTest()
        {
            ExpectReject(Grammar.Expression,
                "", " ", "1+", "--4", "(", "(1", "2(3)");

            ExpectAccept(Grammar.Expression,
                "1+1", "1+2+3", "2*(3+4)", "a + 4", "xyz and abc");

            var context = new EvaluationContext();
            context.SetVariable("quest", 42);
            context.SetVariable("pi", Math.PI);
            context.SetVariable("hello", "Hello");
            context.SetVariable("yes", true);
            context.SetVariable("no", false);

            ExpectError("true + 1");
            ExpectError("true = 2");
            ExpectError("abc + 1");

            ExpectResult("true <> false", typeof(bool), true);
            ExpectResult("2 > 4.0", typeof(bool), false);
            ExpectResult("400 * 300", typeof(int), 400 * 300);
            ExpectResult("2^10", typeof(double), Math.Pow(2, 10));
            ExpectResult("(3 > 2.99) and (40.0 = (2*20.0))", typeof(bool), true);

            ExpectResult(context, "(yes and no) <> true", typeof(bool), true);
            ExpectResult(context, "hello & quest", typeof(string), "Hello42");
        }

        [Test]
        public void OperatorPriorityCheck()
        {
            // 0 vs. 1
            ExpectResult("1*2^3", typeof(double), 1 * Math.Pow(2, 3));
            // 1 vs. 2
            ExpectResult("1+2*3", typeof(int), 1 + 2 * 3);
            // 2 vs. 3
            ExpectResult("1&2+3", typeof(string), "15");
            // 3 vs. 4
            ExpectResult("\"12\"<1&3", typeof(bool), true);
            // 4 vs. 5
            ExpectResult("true and 1 < 2", typeof(bool), true);
            // 5 vs. 6
            ExpectResult("false xor true and true", typeof(bool), true); // real test?

            // 2 chain
            ExpectResult("1+2*3^4", typeof(double), 1 + 2 * Math.Pow(3, 4));
            // 3 chain
            ExpectResult("0&1+2*3^4", typeof(string), "0" + (1 + 2 * Math.Pow(3, 4)));
            // 4 chain
            ExpectResult("\"12\">0&1+2*3^4", typeof(bool), "0".CompareTo("0" + (1 + 2 * Math.Pow(2, 4))) < 0);
            // 5 chain
            ExpectResult("false or \"12\"<0&1+2*3^4", typeof(bool), false || ("12".CompareTo("0" + (1 + 2 * Math.Pow(2, 4))) < 0));

            // complex tests
            ExpectResult("true and 1&0>0&1+2*3^4", typeof(bool), true);
            ExpectResult("2+3*4>2^2 and 1&2<1&3", typeof(bool), true);
        }

        [Test]
        public void ExpressionListTest()
        {
            ExpectAccept(Grammar.ExpressionList,
                "1", " 1 ", " 1 + 2 ", "1,2", "1, 2", " 1 + 2 , 3 + 4 ",
                "a", "a, b", "a, b, c",
                " \"hello\", \"world\" ");
        }

        [Test]
        public void FunctionCallTest()
        {
            ExpectReject(Grammar.FunctionCall,
                "(", "()", "()a", "(a)", "a(", "a)");

            ExpectAccept(Grammar.FunctionCall,
                "a()", "a ()", "a( )", "a(b)", " a (b) ", " a ( b ) ", "a(b, c)", "sinus(ld(a))");

            var context = new EvaluationContext();
            context.SetVariable("π", Math.PI);
            context.SetVariable("wrappedString", new WrappedString("Hello!"));
            context.AddFunction("sin", new FunctionHandle((Func<double, double>)Math.Sin));
            context.AddFunction("x2", new FunctionHandle((Func<decimal, decimal>)(v => v * 2m)));
            context.AddFunction("min", new FunctionHandle((Func<int, int, int>)Math.Min));
            context.AddFunction("min", new FunctionHandle((Func<string, string, string>)((a, b) => string.Compare(a, b) > 0 ? b : a)));
            //context.AddFunction("rev", new FunctionHandle((Func<string, string>)(s => new string(s.Reverse().ToArray()))));
            //context.AddFunction("if", new FunctionHandle(GetType().GetMethod("If")));

            ExpectResult(context, "sin(π)", typeof(double), Math.Sin(Math.PI)); // complete match
            ExpectResult(context, "min(100, 42)", typeof(int), Math.Min(100, 42));
            ExpectResult(context, "min(\"xyz\", \"abc\")", typeof(string), "abc");
            ExpectResult(context, "sin(1)", typeof(double), Math.Sin(1)); // implicit numeric cast from int to double

            //ExpectResult(context, "x2(4)", typeof(decimal), 4 * 2m); // implicit numeric cast from int to decimal
            //ExpectResult(context, "rev(wrappedString)", typeof(string), new string("implicit(Hello!)".Reverse().ToArray())); // overloaded implicit cast operator
            //ExpectResult(context, "if(4 < 5, \"Yes\", \"No\")", typeof (string), "Yes"); // generic method binding

            ExpectError(context, "foo(π)"); // not existing function

            ExpectError(context, "sin(true)"); // wrong parameter type
            ExpectError(context, "sin()"); // too few parameter
            ExpectError(context, "sin(pi, 2)"); // too many parameter
        }

        public static T If<T>(bool condition, T trueCase, T falseCase)
        {
            return condition ? trueCase : falseCase;
        }

        private class WrappedString
        {
            public string Value { get; private set; }

            public WrappedString(string value)
            {
                Value = value;
            }

            public override string ToString()
            {
                return string.Format("str({0})", Value);
            }

            public static implicit operator string(WrappedString ws)
            {
                return string.Format("implicit({0})", ws.Value);
            }

            public static implicit operator WrappedString(string value)
            {
                return new WrappedString(string.Format("wrapped({0})", value));
            }
        }

        [Test]
        public void LoadPackagesTest()
        {
            var context = new EvaluationContext();
            context.LoadAllPackages();
        }
    }
}
