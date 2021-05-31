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

        public static string[] LiteralKeywords = new[] { "true", "false", "null" };

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

        private static readonly Parser<StringLiteral> emptyString =
            from text in Parse.String("\"\"").Text()
            select new StringLiteral(String.Empty);

        private static string Unescape(char escapeSymbol)
        {
            switch (escapeSymbol)
            {
                case '\\': return "\\";
                case '\"': return "\"";
                case 'a': return "\a";
                case 'b': return "\b";
                case 'f': return "\f";
                case 'n': return "\n";
                case 'r': return "\r";
                case 't': return "\t";
                case 'v': return "\v";
            }
            throw new ArgumentException(
                String.Format("The escape symbol '{0}' is unknown.", escapeSymbol),
                "escapeSymbol");
        }

        private static readonly Predicate<char> specialStringChar = c => c == '"' || c == '\\';

        private static readonly Parser<string> stringTokens =
            from token in
                (
                    Parse.CharExcept(specialStringChar, "Quotes and the escape character.")
                    .Many().Text())
                    .Or(
                    from escape in Parse.Char('\\')
                    from escapeSymbol in Parse.AnyChar
                    select Unescape(escapeSymbol))
            select token;

        private static readonly Parser<StringLiteral> nonEmptyString =
            from leadingQuotes in Parse.Char('"').Once().Text()
            from content in stringTokens.Until(Parse.Char('"'))
            select new StringLiteral(String.Concat(content));

        public static readonly Parser<StringLiteral> StringLiteral =
            from literal in emptyString.Or(nonEmptyString).Token()
            select literal;

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

        public LanguageCapabilities Capabilities { get; set; }

        private bool ignoreOperatorCase;
        public bool IgnoreOperatorCase
        {
            get { return ignoreOperatorCase; }
            set
            {
                if (ignoreOperatorCase == value) return;
                ignoreOperatorCase = value;
                UpdateConfigurationDependentTokens();
            }
        }

        public bool ignoreLiteralCase;
        public bool IgnoreLiteralCase
        {
            get { return ignoreLiteralCase; }
            set
            {
                if (ignoreLiteralCase == value) return;
                ignoreLiteralCase = value;
                UpdateConfigurationDependentTokens();
            }
        }

        public bool IgnoreFunctionCase { get; set; }

        #endregion

        #region configuration dependend tokens

        public Parser<BooleanLiteral> BooleanLiteral;
        public Parser<NullLiteral> NullLiteral;

        public Parser<Operator> BoolAndOp;
        public Parser<Operator> BoolOrOp;
        public Parser<Operator> BoolXorOp;

        public Parser<Operator> AnyOperator;

        private void UpdateConfigurationDependentTokens()
        {
            BooleanLiteral = IgnoreLiteralCase
                ? from value in Parse.IgnoreCase("true").Or(Parse.IgnoreCase("false")).Token().Text()
                  select new BooleanLiteral(value.ToLowerInvariant())
                : from value in Parse.String("true").Or(Parse.String("false")).Token().Text()
                  select new BooleanLiteral(value);

            NullLiteral = IgnoreLiteralCase
                ? from src in Parse.IgnoreCase("null").Token().Text()
                  select new NullLiteral(src.ToLowerInvariant())
                : from src in Parse.String("null").Token().Text()
                  select new NullLiteral(src);

            BoolAndOp = BuildOpParser("and", Operator.BoolAnd, IgnoreOperatorCase);
            BoolOrOp = BuildOpParser("or", Operator.BoolOr, IgnoreOperatorCase);
            BoolXorOp = BuildOpParser("xor", Operator.BoolXor, IgnoreOperatorCase);
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
        }

        public bool IsLiteralKeyword(string word)
        {
            return LiteralKeywords.Contains(word, IgnoreLiteralCase
                ? StringComparer.InvariantCultureIgnoreCase
                : StringComparer.InvariantCulture);
        }

        public bool IsOperatorKeyword(string word)
        {
            return OperatorKeywords.Contains(word, IgnoreOperatorCase
                ? StringComparer.InvariantCultureIgnoreCase
                : StringComparer.InvariantCulture);
        }

        #endregion

        public Grammar()
        {
            Capabilities = LanguageCapabilities.Basic;
            UpdateConfigurationDependentTokens();
        }

        public Parser<Group> Group
        {
            get
            {
                return from lPar in Parse.Char('(')
                       from expr in Parse.Ref(() => Expression)
                       from rPar in Parse.Char(')')
                       select new Group(expr);
            }
        }

        public Parser<IEnumerable<ExpressionElement>> ExpressionList
        {
            get
            {
                return
                    from chain in
                        Parse.ChainOperator(
                            ListSeparator,
                            from expression in Parse.Ref(() => Expression)
                            select new Chain<ExpressionElement>(expression),
                            (sep, c1, c2) => c2.Append(c1))
                    select chain.Reverse();
            }
        }

        public Parser<FunctionCall> FunctionCall
        {
            get
            {
                return
                    from call in
                        (from identifier in Identifier.Token()
                         select Language.FunctionCall.CreateInstance(identifier, IgnoreFunctionCase))
                    from lPar in Parse.Char('(')
                    from parameters in ExpressionList
                        .Or(Parse.WhiteSpace.Many().Return(Chain<ExpressionElement>.Empty))
                    from rPar in Parse.Char(')')
                    from white in Parse.WhiteSpace.Many()
                    select call.WithParameters(parameters);
            }
        }

        public Parser<ExpressionElement> TermBase
        {
            get
            {
                return
                    from term in NullLiteral
                        .Or<ExpressionElement>(DecimalLiteral)
                        .Or<ExpressionElement>(FloatingPointLiteral)
                        .Or<ExpressionElement>(IntegerLiteral)
                        .Or<ExpressionElement>(BooleanLiteral)
                        .Or<ExpressionElement>(StringLiteral)
                        .Or<ExpressionElement>(FunctionCall)
                        .Or<ExpressionElement>(Variable)
                        .Or<ExpressionElement>(Group)
                    select term;
            }
        }

        public Parser<ExpressionElement> TermWithMemberRead
        {
            get
            {
                return
                    from termBase in TermBase
                    from rightParts in RightParts
                    select termBase.TransformWithRightParts(rightParts);
            }
        }


        public Parser<ExpressionElement> Term
        {
            get
            {
                switch (Capabilities)
                {
                    case LanguageCapabilities.Basic:
                        return TermBase;
                    case LanguageCapabilities.MemberRead:
                        return TermWithMemberRead;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public Parser<ExpressionElement> Expression
        {
            get
            {
                return
                    from expression in Parse.ChainOperator(AnyOperator, Term, OperationBuilder)
                    select expression;
            }
        }
    }
}
