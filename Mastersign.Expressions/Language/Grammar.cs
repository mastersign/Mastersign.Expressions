using System;
using System.Collections.Generic;
using System.Linq;
using Mastersign.MiniMods.Chain;
using Sprache;

namespace Mastersign.Expressions.Language
{
    internal class Grammar
    {
        #region static parts

        public static string[] BooleanKeywords = new[] { "true", "false" };

        public static string[] OperatorKeywords = new[] { "and", "or", "xor" };

        private static Parser<string> Suffix(char c)
        {
            return Parse.Char(Char.ToLowerInvariant(c))
                .Or(Parse.Char(Char.ToUpperInvariant(c)))
                .Once().Text();
        }

        public static readonly Parser<string> NumericSign =
            from sign in Parse.Char('-').Or(Parse.Char('+')).Once().Text()
            select sign;

        public static readonly Parser<string> SignedNumber =
            from sign in NumericSign
            from white in Parse.WhiteSpace.Many()
            from number in Parse.Number
            select sign + number;

        public static readonly Parser<string> Integer =
            from number in SignedNumber.Or(Parse.Number)
            select number;

        public static readonly Parser<IntegerLiteral> IntegerLiteral =
            from number in
                Integer.Concat(Suffix('i')).Text()
                .Or(Integer.Concat(Suffix('l')).Text())
                .Or(Integer)
                .Token()
            select new IntegerLiteral(number);

        public static readonly Parser<string> FloatingPoint =
            from fullnumber in
                (
                    from number in SignedNumber.Or(Parse.Number)
                    from decPnt in Parse.Char('.')
                    from decPlaces in Parse.Number
                    select number + '.' + decPlaces)
            select fullnumber;

        public static readonly Parser<FloatingPointLiteral> FloatingPointLiteral =
            from number in
                FloatingPoint.Concat(Suffix('f')).Text()
                .Or(FloatingPoint.Concat(Suffix('d'))).Text()
                .Or(FloatingPoint)
                .Or(Integer.Concat(Suffix('f'))).Text()
                .Or(Integer.Concat(Suffix('d'))).Text()
                .Token()
            select new FloatingPointLiteral(number);

        public static readonly Parser<DecimalLiteral> DecimalLiteral =
            from number in FloatingPoint.Concat(Suffix('m')).Text().Token()
            select new DecimalLiteral(number);

        public static readonly Parser<string> Identifier =
            from leadingWhite in Parse.WhiteSpace.Many()
            from head in Parse.Letter.Or(Parse.Char('_')).Once().Text()
            from rest in Parse.LetterOrDigit.Or(Parse.Char('_')).Many().Text()
            from trailingWhite in Parse.WhiteSpace.Many()
            select head + rest;

        public static readonly Parser<Variable> Variable =
            from name in Identifier
            select new Variable(name);

        public static readonly Parser<IRightPart> MemberReadRight =
            from period in Parse.Char('.').Token()
            from memberIdentifier in Identifier.Text()
            select new MemberRead.RightPart(memberIdentifier);

        public static readonly Parser<string> ListSeparator =
            from seperator in Parse.Char(',').Once().Token().Text()
            select seperator;


        public static readonly Parser<IEnumerable<IRightPart>> RightParts =
            from rightParts in MemberReadRight.Many()
            select rightParts;

        private static Parser<Operator> BuildOpParser(string text, Operator op, bool ignoreCase = false)
        {
            return ignoreCase
                ? Parse.IgnoreCase(text).Token().Return(op)
                : Parse.String(text).Token().Return(op);
        }

        public static readonly Parser<Operator> NumAdditionOp = BuildOpParser("+", Operator.NumAddition);
        public static readonly Parser<Operator> NumSubtractionOp = BuildOpParser("-", Operator.NumSubtraction);
        public static readonly Parser<Operator> NumMultiplicationOp = BuildOpParser("*", Operator.NumMultiplication);
        public static readonly Parser<Operator> NumDivisionOp = BuildOpParser("/", Operator.NumDivision);
        public static readonly Parser<Operator> NumPowerOp = BuildOpParser("^", Operator.NumPower);

        public static readonly Parser<Operator> StringConcatOp = BuildOpParser("&", Operator.StringConcat);

        public static readonly Parser<Operator> RelLessOp = BuildOpParser("<", Operator.RelationLess);
        public static readonly Parser<Operator> RelLessOrEqualOp = BuildOpParser("<=", Operator.RelationLessOrEqual);
        public static readonly Parser<Operator> RelEqualOp = BuildOpParser("=", Operator.RelationEqual);
        public static readonly Parser<Operator> RelUnequalOp = BuildOpParser("<>", Operator.RelationUnequal);
        public static readonly Parser<Operator> RelGreaterOrEqualOp = BuildOpParser(">=", Operator.RelationGreaterOrEqual);
        public static readonly Parser<Operator> RelGreaterOp = BuildOpParser(">", Operator.RelationGreater);

        private static Operation OperationBuilder(Operator op, ExpressionElement l, ExpressionElement r)
        {
            return l is Operation
                ? ((Operation)l).AppendOperand(op, r)
                : new Operation(l, op, r);
        }

        #endregion

        #region configuration

        private LanguageOptions options;
        public LanguageOptions Options
        {
            get { return options; }
            set
            {
                if (options == value) return;
                options = value;
                UpdateConfigurationDependentTokens();
            }
        }

        #endregion

        #region configuration dependend tokens

        public Parser<BooleanLiteral> BooleanLiteral;
        public Parser<NullLiteral> NullLiteral;

        public Parser<Operator> BoolAndOp;
        public Parser<Operator> BoolOrOp;
        public Parser<Operator> BoolXorOp;

        public Parser<Operator> AnyOperator;

        private Parser<StringLiteral> emptyString;
        private Parser<string> stringTokens;
        private Parser<StringLiteral> nonEmptyString;

        public Parser<StringLiteral> StringLiteral;

        private void UpdateConfigurationDependentTokens()
        {
            BooleanLiteral = options.IgnoreBooleanLiteralCase
                ? from value in Parse.IgnoreCase(options.LiteralTrueName).Or(Parse.IgnoreCase(options.LiteralFalseName)).Token().Text()
                  select new BooleanLiteral(value.ToLowerInvariant())
                : from value in Parse.String(options.LiteralTrueName).Or(Parse.String(options.LiteralFalseName)).Token().Text()
                  select new BooleanLiteral(value);

            NullLiteral = options.IgnoreNullLiteralCase
                ? from src in Parse.IgnoreCase(options.LiteralNullName).Token().Text()
                  select new NullLiteral(src.ToLowerInvariant())
                : from src in Parse.String(options.LiteralNullName).Token().Text()
                  select new NullLiteral(src);

            BoolAndOp = BuildOpParser(options.OperatorAndName, Operator.BoolAnd, options.IgnoreOperatorCase);
            BoolOrOp = BuildOpParser(options.OperatorOrName, Operator.BoolOr, options.IgnoreOperatorCase);
            BoolXorOp = BuildOpParser(options.OperatorXorName, Operator.BoolXor, options.IgnoreOperatorCase);
            AnyOperator =
                from op in NumAdditionOp
                    .Or(NumSubtractionOp)
                    .Or(NumMultiplicationOp)
                    .Or(NumDivisionOp)
                    .Or(NumPowerOp)
                    .Or(BoolAndOp)
                    .Or(BoolOrOp)
                    .Or(BoolXorOp)
                    .Or(StringConcatOp)
                    .Or(RelEqualOp)
                    .Or(RelUnequalOp)
                    .Or(RelLessOrEqualOp)
                    .Or(RelLessOp)
                    .Or(RelGreaterOrEqualOp)
                    .Or(RelGreaterOp)
                select op;

            emptyString =
                from text in Parse.String("" + options.StartQuoteChar + options.EndQuoteChar).Text()
                select new StringLiteral(String.Empty);
            stringTokens =
                    from token in
                        (Parse.CharExcept(IsSpecialStringChar, "Quotes and the escape character.")
                        .Many().Text())
                        .Or(from escape in Parse.Char('\\')
                            from escapeSymbol in Parse.AnyChar
                            select Unescape(escapeSymbol))
                    select token;
            nonEmptyString =
                from leadingQuotes in Parse.Char(options.StartQuoteChar).Once().Text()
                from content in stringTokens.Until(Parse.Char(options.EndQuoteChar))
                select new StringLiteral(String.Concat(content));
            StringLiteral =
                from literal in emptyString.Or(nonEmptyString).Token()
                select literal;
        }

        private string Unescape(char escapeSymbol)
        {
            if (escapeSymbol == options.StartQuoteChar) return new string(escapeSymbol, 1);
            if (escapeSymbol == options.EndQuoteChar) return new string(escapeSymbol, 1);
            switch (escapeSymbol)
            {
                case '\\': return "\\";
                case 'a': return "\a";
                case 'b': return "\b";
                case 'f': return "\f";
                case 'n': return "\n";
                case 'r': return "\r";
                case 't': return "\t";
                case 'v': return "\v";
            }
            throw new ArgumentException(
                string.Format("The escape symbol '{0}' is unknown.", escapeSymbol),
                nameof(escapeSymbol));
        }

        private bool IsSpecialStringChar(char c)
            => c == '\\' || c == options.StartQuoteChar || c == options.EndQuoteChar;

        #endregion

        public Grammar()
        {
            options = LanguageOptions.Default;
            UpdateConfigurationDependentTokens();
        }

        public Parser<Group> Group
            => from lPar in Parse.Char('(')
               from expr in Parse.Ref(() => Expression)
               from rPar in Parse.Char(')')
               select new Group(expr);

        public Parser<IEnumerable<ExpressionElement>> ExpressionList
            => from chain in
                   Parse.ChainOperator(
                       ListSeparator,
                       from expression in Parse.Ref(() => Expression)
                       select new Chain<ExpressionElement>(expression),
                       (sep, c1, c2) => c2.Append(c1))
               select chain.Reverse();

        public Parser<FunctionCall> FunctionCall
            => from call in
                   (from identifier in Identifier.Token()
                       select string.Equals(identifier, options.ConditionalName, options.GetStringComparison(options.IgnoreConditionalCase))
                        ? new Conditional(options.IgnoreConditionalCase
                            ? identifier.ToLowerInvariant()
                            : identifier)
                        : new FunctionCall(options.IgnoreFunctionNameCase
                            ? identifier.ToLowerInvariant()
                            : identifier))
               from lPar in Parse.Char('(')
               from parameters in ExpressionList
                   .Or(Parse.WhiteSpace.Many().Return(Chain<ExpressionElement>.Empty))
               from rPar in Parse.Char(')')
               from white in Parse.WhiteSpace.Many()
               select call.WithParameters(parameters);

        public Parser<ExpressionElement> TermBase
            => from term in NullLiteral
                   .Or<ExpressionElement>(DecimalLiteral)
                   .Or<ExpressionElement>(FloatingPointLiteral)
                   .Or<ExpressionElement>(IntegerLiteral)
                   .Or<ExpressionElement>(BooleanLiteral)
                   .Or<ExpressionElement>(StringLiteral)
                   .Or<ExpressionElement>(FunctionCall)
                   .Or<ExpressionElement>(Variable)
                   .Or<ExpressionElement>(Group)
               select term;

        public Parser<ExpressionElement> TermWithMemberRead
            => from termBase in TermBase
               from rightParts in RightParts
               select termBase.TransformWithRightParts(rightParts);


        public Parser<ExpressionElement> Term
            => options.MemberRead ? TermWithMemberRead : TermBase;

        public Parser<ExpressionElement> Expression
            => from expression in Parse.ChainOperator(AnyOperator, Term, OperationBuilder)
               select expression;
    }
}
