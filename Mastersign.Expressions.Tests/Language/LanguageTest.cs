﻿using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Mastersign.Expressions.Functions;
using Mastersign.Expressions.Language;
using NUnit.Framework;
using Sprache;

namespace Mastersign.Expressions.Tests.Language
{
    public class LanguageTest
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

        private static void ExpectResult(LanguageOptions options, string input, Type expectedType, object expectedValue)
        {
            ExpectResult(new EvaluationContext { Options = options }, input, expectedType, expectedValue);
        }

        private static void ExpectResult(EvaluationContext context, string input, Type expectedType, object expectedValue)
        {
            var res = Parse(context.Grammar.Expression, input);
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
            if (expr.Type != typeof(object))
            {
                expr = Expression.Convert(expr, typeof(object));
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
            var res = Parse(context.Grammar.Expression, input);
            var sb = new StringBuilder();
            Assert.IsFalse(res.CheckSemantic(context, sb));
            Assert.IsTrue(sb.Length > 0);
        }

        public static T If<T>(bool condition, T trueCase, T falseCase)
        {
            return condition ? trueCase : falseCase;
        }

        #endregion

        [Test]
        public void OperatorTest()
        {
            var grammar = new Grammar();
            ExpectReject(grammar.AnyOperator,
                " ", "abc", "#", "==", "< >");

            ExpectAccept(grammar.AnyOperator,
                "+", "-", " +", "+ ", "  +  ", "<>");

            Assert.AreEqual(Operator.NumAddition, Parse(grammar.AnyOperator, "+"));
            Assert.AreEqual(Operator.NumSubtraction, Parse(grammar.AnyOperator, "-"));
            Assert.AreEqual(Operator.NumMultiplication, Parse(grammar.AnyOperator, "*"));
            Assert.AreEqual(Operator.NumDivision, Parse(grammar.AnyOperator, "/"));
            Assert.AreEqual(Operator.NumPower, Parse(grammar.AnyOperator, "^"));

            Assert.AreEqual(Operator.BoolAnd, Parse(grammar.AnyOperator, "and"));
            Assert.AreEqual(Operator.BoolOr, Parse(grammar.AnyOperator, "or"));
            Assert.AreEqual(Operator.BoolXor, Parse(grammar.AnyOperator, "xor"));

            Assert.AreEqual(Operator.StringConcat, Parse(grammar.AnyOperator, "&"));

            Assert.AreEqual(Operator.RelationLess, Parse(grammar.AnyOperator, "<"));
            Assert.AreEqual(Operator.RelationLessOrEqual, Parse(grammar.AnyOperator, "<="));
            Assert.AreEqual(Operator.RelationEqual, Parse(grammar.AnyOperator, "="));
            Assert.AreEqual(Operator.RelationUnequal, Parse(grammar.AnyOperator, "<>"));
            Assert.AreEqual(Operator.RelationGreaterOrEqual, Parse(grammar.AnyOperator, ">="));
            Assert.AreEqual(Operator.RelationGreater, Parse(grammar.AnyOperator, ">"));
        }

        [Test]
        public void OperatorCasingTest()
        {
            var grammar = new Grammar();
            ExpectAccept(grammar.AnyOperator, "and", "or", "xor");
            ExpectReject(grammar.AnyOperator, "And", "Or", "Xor");
            ExpectReject(grammar.AnyOperator, "AND", "OR", "XOR");
        }

        [Test]
        public void OperatorIgnoreCasingTest()
        {
            var grammar = new Grammar
            {
                Options = new LanguageOptionsBuilder()
                    .IgnoreOperatorCase()
                    .Build(),
            };
            ExpectAccept(grammar.AnyOperator, "and", "or", "xor");
            ExpectAccept(grammar.AnyOperator, "And", "Or", "Xor");
            ExpectAccept(grammar.AnyOperator, "AND", "OR", "XOR");
        }

        [Test]
        public void NullLiteralTest()
        {
            var grammar = new Grammar();
            ExpectReject(grammar.NullLiteral,
                " ", "n", "0", "NULL");

            ExpectAccept(grammar.NullLiteral,
                "null", " null", "null ", "\tnull ");

            ExpectResult("null", typeof(object), null);
        }

        [Test]
        public void NullLiteralCasingTest()
        {
            var grammar = new Grammar();
            ExpectAccept(grammar.NullLiteral, "null");
            ExpectReject(grammar.NullLiteral, "Null");
            ExpectReject(grammar.NullLiteral, "NULL");
        }

        [Test]
        public void NullLiteralIgnoreCasingTest()
        {
            var grammar = new Grammar
            {
                Options = new LanguageOptionsBuilder()
                    .IgnoreNullLiteralCase()
                    .Build(),
            };
            ExpectAccept(grammar.NullLiteral, "null");
            ExpectAccept(grammar.NullLiteral, "Null");
            ExpectAccept(grammar.NullLiteral, "NULL");
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
            var grammar = new Grammar();
            ExpectReject(grammar.BooleanLiteral,
                "", " ", "0", "1", "t", "f");

            ExpectAccept(grammar.BooleanLiteral,
                "true", "false");

            var context = new EvaluationContext();

            Assert.AreEqual(true, Parse(grammar.BooleanLiteral, "true").GetValue(context));
            Assert.AreEqual(false, Parse(grammar.BooleanLiteral, "false").GetValue(context));

            var res = Parse(grammar.BooleanLiteral, "true");
            Assert.IsTrue(res.CheckSemantic(context, new StringBuilder()));
        }

        [Test]
        public void BooleanCasingTest()
        {
            var grammar = new Grammar();
            ExpectAccept(grammar.BooleanLiteral, "true", "false");
            ExpectReject(grammar.BooleanLiteral, "True", "False");
            ExpectReject(grammar.BooleanLiteral, "TRUE", "FALSE");
        }

        [Test]
        public void BooleanIgnoreCasingTest()
        {
            var grammar = new Grammar
            {
                Options = new LanguageOptionsBuilder()
                    .IgnoreBooleanLiteralCase()
                    .Build(),
            };
            ExpectAccept(grammar.BooleanLiteral, "true", "false");
            ExpectAccept(grammar.BooleanLiteral, "True", "False");
            ExpectAccept(grammar.BooleanLiteral, "TRUE", "FALSE");
        }

        [Test]
        public void StringTest()
        {
            var grammar = new Grammar();
            ExpectReject(grammar.StringLiteral,
                "", " ", "\"", "'", "`", "''", "' '", "``", "` `", "\"abc", "\"a\" \"b\"");

            ExpectAccept(grammar.StringLiteral, "\"\""); // |""|
            ExpectAccept(grammar.StringLiteral, " \"\" "); // | "" |
            ExpectAccept(grammar.StringLiteral, "\" \""); // |" "|
            ExpectAccept(grammar.StringLiteral, "\"abc def\""); // |"abc def"|
            ExpectAccept(grammar.StringLiteral, " \"abc def\" "); // | "abc def" |
            ExpectAccept(grammar.StringLiteral, "\"\\\"\""); // |"\""|
            ExpectAccept(grammar.StringLiteral, "\"\\t\\\"abc\\\"\""); // |"\t\"abc\""|

            ExpectResult("\"abc\"", typeof(string), "abc");
            ExpectResult("\" 123\\t \\n\\r\\\\ ende\"", typeof(string), " 123\t \n\r\\ ende");
            ExpectResult("\"''``\"", typeof(string), "''``");
        }

        [Test]
        public void SingleQuoteStringTest()
        {
            var options = new LanguageOptionsBuilder()
                .WithQuoteCharacter(QuoteStyle.SingleQuote)
                .Build();
            var grammar = new Grammar { Options = options };
            ExpectReject(grammar.StringLiteral,
                "", " ", "\"", "'", "`", "\"\"", "\" \"", "``", "` `", "'abc", "'a' 'b'");

            ExpectAccept(grammar.StringLiteral, "''");
            ExpectAccept(grammar.StringLiteral, " '' ");
            ExpectAccept(grammar.StringLiteral, "' '");
            ExpectAccept(grammar.StringLiteral, "'abc def'");
            ExpectAccept(grammar.StringLiteral, " 'abc def' ");
            ExpectAccept(grammar.StringLiteral, "'\\''");
            ExpectAccept(grammar.StringLiteral, "'\\t\\'abc\\''"); // |'\t\'abc\''|

            ExpectResult(options, "'abc'", typeof(string), "abc");
            ExpectResult(options, "' 123\\t \\n\\r\\\\ ende'", typeof(string), " 123\t \n\r\\ ende");
            ExpectResult(options, "'\"\"``'", typeof(string), "\"\"``");
        }

        [Test]
        public void BacktickStringTest()
        {
            var options = new LanguageOptionsBuilder()
                .WithQuoteCharacter(QuoteStyle.Backtick)
                .Build();
            var grammar = new Grammar { Options = options };
            ExpectReject(grammar.StringLiteral,
                "", " ", "\"", "'", "`", "\"\"", "\" \"", "''", "' '", "`abc", "`a` `b`");

            ExpectAccept(grammar.StringLiteral, "``");
            ExpectAccept(grammar.StringLiteral, " `` ");
            ExpectAccept(grammar.StringLiteral, "` `");
            ExpectAccept(grammar.StringLiteral, "`abc def`");
            ExpectAccept(grammar.StringLiteral, " `abc def` ");
            ExpectAccept(grammar.StringLiteral, "`\\``");
            ExpectAccept(grammar.StringLiteral, "`\\t\\`abc\\``"); // |`\t\`abc\``|

            ExpectResult(options, "`abc`", typeof(string), "abc");
            ExpectResult(options, "` 123\\t \\n\\r\\\\ ende`", typeof(string), " 123\t \n\r\\ ende");
            ExpectResult(options, "`\"\"''`", typeof(string), "\"\"''");
        }

        [Test]
        public void GroupTest()
        {

            ExpectReject(new Grammar().Group,
                "", " ", "(", "()", "( )", ")", "12");

            ExpectAccept(new Grammar().Group,
                "(1)", "(1+2)", "(((1)))");

            var context = new EvaluationContext();
            Assert.AreEqual(1, Parse(context.Grammar.Group, "((1))").GetValue(context));
        }

        [Test]
        public void TermBaseTest()
        {
            var grammar = new Grammar();

            ExpectReject(grammar.TermBase,
                "", " ", "1 + 2");

            ExpectAccept(grammar.TermBase,
                "1", "(1+2)", "(true and false)");
        }

        [Test]
        public void TermWithMemberReadTest()
        {
            var grammar = new Grammar();

            ExpectReject(grammar.TermWithMemberRead,
                "", " ", "1 + 2", ".", ".a", "().a");

            ExpectAccept(grammar.TermWithMemberRead,
                "1", "(1+2)", "(true and false)",
                "a.b.c", "a().b", "(a + b).c");
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

            var astX = context.Grammar.Expression.End().Parse("x");
            Assert.IsTrue(astX.CheckSemantic(context, new StringBuilder()));
            var exprX = astX.GetExpression(context);
            var lambdaX = Expression.Lambda<Func<int>>(exprX);
            var x = lambdaX.Compile();

            var astY = context.Grammar.Expression.End().Parse("y");
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
            ExpectError("null < null");

            ExpectError("true = null");
            ExpectError("true <> null");
            ExpectError("null = true");
            ExpectError("null <> true");

            ExpectError("1 = null");
            ExpectError("null = 1");
            ExpectError("1 <> null");
            ExpectError("null <> 1");

            ExpectResult("null = null", typeof(bool), true);
            ExpectResult("null <> null", typeof(bool), false);

            ExpectResult("null = \"a\"", typeof(bool), false);
            ExpectResult("null <> \"a\"", typeof(bool), true);
            ExpectResult("\"a\" = null", typeof(bool), false);
            ExpectResult("\"a\" <> null", typeof(bool), true);

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

            ExpectResult("\"a\" = \"a\"", typeof(bool), true);
            ExpectResult("\"a\" <> \"a\"", typeof(bool), false);
            ExpectResult("\"a\" = \"b\"", typeof(bool), false);
            ExpectResult("\"a\" <> \"b\"", typeof(bool), true);
            ExpectResult("\"a\" < \"a\"", typeof(bool), false);
            ExpectResult("\"a\" < \"A\"", typeof(bool), true);
            ExpectResult("\"a\" < \"b\"", typeof(bool), true);

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
        public void ParameterCasingTest()
        {
            var context = new EvaluationContext();
            context.SetParameters(
                new ParameterInfo("a", typeof(int)),
                new ParameterInfo("B", typeof(string)));

            var fA = context.CompileExpression<int, string, int>("a");
            Assert.AreEqual(42, fA(42, "test"));

            var fB = context.CompileExpression<int, string, string>("B");
            Assert.AreEqual("test", fB(42, "test"));

            Assert.Throws(typeof(SemanticErrorException), () =>
            {
                context.CompileExpression("A");
            });
            Assert.Throws(typeof(SemanticErrorException), () =>
            {
                context.CompileExpression("b");
            });
        }

        [Test]
        public void ParameterIgnoreCasingText()
        {
            var context = new EvaluationContext
            {
                Options = new LanguageOptionsBuilder()
                    .IgnoreParameterNameCase()
                    .Build(),
            };
            context.SetParameters(
                new ParameterInfo("a", typeof(int)),
                new ParameterInfo("B", typeof(string)));

            var fA1 = context.CompileExpression<int, string, int>("a");
            Assert.AreEqual(42, fA1(42, "test"));
            var fA2 = context.CompileExpression<int, string, int>("A");
            Assert.AreEqual(42, fA2(42, "test"));

            var fB1 = context.CompileExpression<int, string, string>("B");
            Assert.AreEqual("test", fB1(42, "test"));
            var fB2 = context.CompileExpression<int, string, string>("b");
            Assert.AreEqual("test", fB2(42, "test"));
        }

        [Test]
        public void ExpressionTest()
        {
            var context = new EvaluationContext();
            context.SetVariable("quest", 42);
            context.SetVariable("pi", Math.PI);
            context.SetVariable("hello", "Hello");
            context.SetVariable("yes", true);
            context.SetVariable("no", false);

            ExpectReject(context.Grammar.Expression,
                "", " ", "1+", "--4", "(", "(1", "2(3)");

            ExpectAccept(context.Grammar.Expression,
                "1+1", "1+2+3", "2*(3+4)", "a + 4", "xyz and abc");

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

            context.Options = new LanguageOptionsBuilder()
                .WithMemberRead()
                .Build();

            ExpectResult(context, "(\"abc\" & \"def\").Length", typeof(int), 6);
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
            ExpectAccept(new Grammar().ExpressionList,
                "1", " 1 ", " 1 + 2 ", "1,2", "1, 2", " 1 + 2 , 3 + 4 ",
                "a", "a, b", "a, b, c",
                " \"hello\", \"world\" ");
        }

        [Test]
        public void FunctionCallTest()
        {
            ExpectReject(new Grammar().FunctionCall,
                "(", "()", "()a", "(a)", "a(", "a)", "a(1 + )");

            ExpectAccept(new Grammar().FunctionCall,
                "a()", "a ()", "a( )", "a(b)", " a (b) ", " a ( b ) ", "a(b, c)", "sinus(ld(a))");

            var context = new EvaluationContext();
            context.SetVariable("π", Math.PI);
            //context.SetVariable("wrappedString", new WrappedString("Hello!"));
            context.AddFunction("sin", new FunctionHandle((Func<double, double>)Math.Sin));
            context.AddFunction("min", new FunctionHandle((Func<int, int, int>)Math.Min));
            context.AddFunction("min", new FunctionHandle((Func<string, string, string>)Strings.Min));
            //context.AddFunction("x2", new FunctionHandle((Func<decimal, decimal>)(v => v * 2m)));
            //context.AddFunction("rev", new FunctionHandle((Func<string, string>)(s => new string(s.Reverse().ToArray()))));
            //context.AddFunction("test_if", new FunctionHandle(GetType().GetMethod("If")));

            ExpectResult(context, "sin(π)", typeof(double), Math.Sin(Math.PI)); // complete match
            ExpectResult(context, "min(100, 42)", typeof(int), Math.Min(100, 42));
            ExpectResult(context, "min(\"xyz\", \"abc\")", typeof(string), "abc");
            ExpectResult(context, "sin(1)", typeof(double), Math.Sin(1)); // implicit numeric cast from int to double

            //ExpectResult(context, "x2(4)", typeof(decimal), 4 * 2m); // implicit numeric cast from int to decimal
            //ExpectResult(context, "rev(wrappedString)", typeof(string), new string("implicit(Hello!)".Reverse().ToArray())); // overloaded implicit cast operator
            //ExpectResult(context, "test_if(4 < 5, \"Yes\", \"No\")", typeof (string), "Yes"); // generic method binding

            ExpectError(context, "foo(π)"); // not existing function

            ExpectError(context, "sin(true)"); // wrong parameter type
            ExpectError(context, "sin()"); // too few parameter
            ExpectError(context, "sin(pi, 2)"); // too many parameter
        }

        [Test]
        public void FunctionCasingTest()
        {
            var context = new EvaluationContext();
            context.AddFunction("test", new FunctionHandle((Func<int, int>)(a => a * 2)));
            context.AddFunction("Test", new FunctionHandle((Func<int, int>)(a => a * 3)));
            context.AddFunction("TEST", new FunctionHandle((Func<int, int>)(a => a * 4)));

            ExpectResult(context, "test(10)", typeof(int), 20);
            ExpectResult(context, "Test(10)", typeof(int), 30);
            ExpectResult(context, "TEST(10)", typeof(int), 40);
            ExpectError(context, "tESt(10)");
        }

        [Test]
        public void FunctionIgnoreCasingTest()
        {
            var context = new EvaluationContext
            {
                Options = new LanguageOptionsBuilder()
                    .IgnoreFunctionNameCase()
                    .Build(),
            };
            context.AddFunction("test", new FunctionHandle((Func<int, int>)(a => a * 2)));

            ExpectResult(context, "test(10)", typeof(int), 20);
            ExpectResult(context, "Test(10)", typeof(int), 20);
            ExpectResult(context, "TEST(10)", typeof(int), 20);
            ExpectResult(context, "tESt(10)", typeof(int), 20);
        }

        [Test]
        public void FunctionLateIgnoreCasingTest()
        {
            var context = new EvaluationContext();
            context.AddFunction("test", new FunctionHandle((Func<int, int>)(a => a * 2)));

            context.Options = new LanguageOptionsBuilder()
                    .IgnoreFunctionNameCase()
                    .Build();

            ExpectResult(context, "test(10)", typeof(int), 20);
            ExpectResult(context, "Test(10)", typeof(int), 20);
            ExpectResult(context, "TEST(10)", typeof(int), 20);
            ExpectResult(context, "tESt(10)", typeof(int), 20);
        }

        [Test]
        public void AmbigousFunctionTest()
        {
            var context = new EvaluationContext();
            context.AddFunction("twice", new FunctionHandle((Func<string>)(() => "One")));
            context.AddFunction("twice", new FunctionHandle((Func<string>)(() => "Two")));

            ExpectError("twice()");
        }

        [Test]
        public void NullTestTest()
        {
            var context = new EvaluationContext();
            context.SetVariable("v1", null);
            context.SetVariable("v2", true);
            context.SetVariable("v3", 1);
            context.SetVariable("v4", "A");
            context.SetVariable("v5", new object());

            ExpectResult(context, context.Options.NullTestName + "(null)", typeof(bool), true);
            ExpectResult(context, context.Options.NullTestName + "(v1)", typeof(bool), true);
            ExpectResult(context, context.Options.NullTestName + "(v2)", typeof(bool), false);
            ExpectResult(context, context.Options.NullTestName + "(v3)", typeof(bool), false);
            ExpectResult(context, context.Options.NullTestName + "(v4)", typeof(bool), false);
            ExpectResult(context, context.Options.NullTestName + "(v5)", typeof(bool), false);

            // wrong number of arguments
            ExpectError(context, context.Options.NullTestName + "()");
            ExpectError(context, context.Options.NullTestName + "(null, null)");

            // wrapped semantic errors
            ExpectError(context, context.Options.NullTestName + "(not_exist())"); // invalid function call as parameter
        }

        [Test]
        public void NullTestCasingTest()
        {
            var context = new EvaluationContext();
            ExpectResult(context, "isnull(null)", typeof(bool), true);
            ExpectError(context, "ISNULL(null)");
        }

        [Test]
        public void NullTestNamingTest()
        {
            var context = new EvaluationContext
            {
                Options = new LanguageOptionsBuilder()
                    .WithNullTestName("nul")
                    .Build(),
            };
            ExpectResult(context, "nul(null)", typeof(bool), true);
            ExpectError(context, "isnull(null)");
        }

        [Test]
        public void NullTestIgnoreCasingTest()
        {
            var context = new EvaluationContext
            {
                Options = new LanguageOptionsBuilder()
                    .IgnoreNullTestCase()
                    .Build(),
            };
            ExpectResult(context, "isnull(null)", typeof(bool), true);
            ExpectResult(context, "ISNULL(null)", typeof(bool), true);
        }

        [Test]
        public void ConditionalTest()
        {
            var context = new EvaluationContext();
            context.SetVariable("a", 42);
            context.SetVariable("b", 12);
            context.SetVariable("c", Math.E);
            context.SetVariable("x", "abc");
            context.SetVariable("y", "xyz");

            ExpectResult(context, context.Options.ConditionalName + "(true, a, b)", typeof(int), 42);
            ExpectResult(context, context.Options.ConditionalName + "(false, a, b)", typeof(int), 12);

            ExpectResult(context, context.Options.ConditionalName + "(a > b, false, true)", typeof(bool), false);
            ExpectResult(context, context.Options.ConditionalName + "(100 = a, x, y)", typeof(string), "xyz");

            // wrong number of arguments
            ExpectError(context, context.Options.ConditionalName + "()");
            ExpectError(context, context.Options.ConditionalName + "(true, 1)");
            ExpectError(context, context.Options.ConditionalName + "(true, 1, 2, 3)");
            ExpectError(context, context.Options.ConditionalName + "(true, 1, 2, 3, 4)");

            // wrong argument type
            ExpectError(context, context.Options.ConditionalName + "(100, a, b)"); // wrong type for condition
            ExpectError(context, context.Options.ConditionalName + "(true, a, x)"); // different types for true and false part

            // wrapped semantic errors
            ExpectError(context, context.Options.ConditionalName + "(not_exist(), a, b)"); // invalid function call as parameter
            ExpectError(context, context.Options.ConditionalName + "(true, not_exist(), x)"); // invalid function call as parameter
        }

        [Test]
        public void ConditionalCasingTest()
        {
            var context = new EvaluationContext();
            ExpectResult(context, "if(true, 1, 2)", typeof(int), 1);
            ExpectError(context, "IF(true, 1, 2)");
        }

        [Test]
        public void ConditionalNamingTest()
        {
            var context = new EvaluationContext
            {
                Options = new LanguageOptionsBuilder()
                    .WithConditionalName("when")
                    .Build(),
            };
            ExpectResult(context, "when(true, 1, 2)", typeof(int), 1);
            ExpectError(context, "if(true, 1, 2)");
        }

        [Test]
        public void ConditionalIgnoreCasingTest()
        {
            var context = new EvaluationContext
            {
                Options = new LanguageOptionsBuilder()
                    .IgnoreConditionalCase()
                    .Build(),
            };
            ExpectResult(context, "if(true, 1, 2)", typeof(int), 1);
            ExpectResult(context, "IF(true, 1, 2)", typeof(int), 1);
        }

        [Test]
        public void ConditionalTestWithUpCast()
        {
            var context = new EvaluationContext();
            context.LoadConversionPackage();
            context.SetVariable("a", 42);
            context.SetVariable("b", Math.E);

            ExpectResult(context, context.Options.ConditionalName + "(true, a, b)", typeof(double), 42.0);
            ExpectResult(context, context.Options.ConditionalName + "(false, a, b)", typeof(double), Math.E);
            ExpectResult(context, context.Options.ConditionalName + "(true, b, a)", typeof(double), Math.E);
            ExpectResult(context, context.Options.ConditionalName + "(false, b, a)", typeof(double), 42.0);
        }

        private static string TestString() { return "Test String"; }

        [Test]
        public void MemberReadTest()
        {
            ExpectReject(Grammar.MemberReadRight,
                ":Length", "-Length", "Length",
                ".test()", ".123",
                ".", " .", " . ", ".()");

            ExpectAccept(Grammar.MemberReadRight,
                ".b", " .b", ". b", " . b", "._b");

            var context = new EvaluationContext();
            var str = TestString();
            context.SetVariable("strA", str);
            context.SetVariable("strB", str, true);
            context.SetVariable("intA", 42, true);
            context.SetVariable("ex", new MemberReadExample(str), true);
            context.AddFunction("f", (Func<string>)TestString);

            context.Options = context.Options.Derive()
                .WithMemberRead()
                .Build();

            ExpectResult(context, "strA.Length", typeof(int), str.Length);
            ExpectResult(context, "strB.Length", typeof(int), str.Length);
            ExpectResult(context, "ex.PubF", typeof(string), str);
            ExpectResult(context, "ex.PubC", typeof(string), str);
            ExpectResult(context, "ex.PubP", typeof(string), str);
            ExpectResult(context, "ex.PubRoP", typeof(string), str);

            ExpectResult(context, "f().Length", typeof(int), str.Length);
            ExpectResult(context, "ex.PubF.Length", typeof(int), str.Length);

            ExpectError(context, "strA.ToString");
            ExpectError(context, "ex.NotExist");
            ExpectError(context, "ex.pubf");
            ExpectError(context, "ex.prvF");
            ExpectError(context, "ex.PubWoP");
        }

        [Test]
        public void GrammarCapabilityMemberReadTest()
        {
            var context = new EvaluationContext();
            Assert.IsFalse(context.Options.MemberRead);

            ExpectReject(context.Grammar.Expression.End(), "a.b");

            context.Options = context.Options.Derive()
                .WithMemberRead()
                .Build();

            ExpectAccept(context.Grammar.Expression.End(), "a.b");
        }

        [Test]
        public void LoadPackagesTest()
        {
            var context = new EvaluationContext();
            context.LoadAllPackages();
        }

        #region Test Types

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

        private class MemberReadExample
        {
            public MemberReadExample(string value)
            {
                PubF = value;
                PubC = value;
                prvF = value;
                PubP = value;
                PubRoP = value;
                PubWoP = value;
                PrvP = value;
            }

            public string PubF;
            public readonly string PubC;

            private string prvF;

            public string PubP { get; set; }
            public string PubRoP { get; private set; }
            public string PubWoP { private get; set; }

            private string PrvP { get; set; }
        }

        #endregion

    }
}
