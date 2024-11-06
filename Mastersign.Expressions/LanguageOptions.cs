using System;
using System.Collections.Generic;

namespace Mastersign.Expressions
{
    public class LanguageOptions
    {
        public bool MemberRead { get; }

        public bool IgnoreOperatorCase { get; }

        public bool IgnoreBooleanLiteralCase { get; }

        public bool IgnoreNullLiteralCase { get; }

        public bool IgnoreNullTestCase { get; }

        public bool IgnoreConditionalCase { get; }

        public bool IgnoreVariableNameCase { get; }

        public bool IgnoreParameterNameCase { get; }

        public bool IgnoreFunctionNameCase { get; }

        public QuoteStyle QuoteCharacter { get; }

        public string OperatorAndName { get; }

        public string OperatorOrName { get; }

        public string OperatorXorName { get; }

        public string LiteralTrueName { get; }

        public string LiteralFalseName { get; }

        public string LiteralNullName { get; }

        public string ConditionalName { get; }

        public string NullTestName { get; }

        public char StartQuoteChar => QuoteCharacter switch
        {
            QuoteStyle.SingleQuote => '\'',
            QuoteStyle.Backtick => '`',
            _ => '"',
        };

        public char EndQuoteChar => QuoteCharacter switch
        {
            QuoteStyle.SingleQuote => '\'',
            QuoteStyle.Backtick => '`',
            _ => '"',
        };

        public LanguageOptions(
            bool memberRead,
            bool ignoreOperatorCase,
            bool ignoreBooleanLiteralCase,
            bool ignoreNullLiteralCase,
            bool ignoreNullTestCase,
            bool ignoreConditionalCase,
            bool ignoreVariableNameCase,
            bool ignoreParameterNameCase,
            bool ignoreFunctionNameCase,
            QuoteStyle quoteCharacter,
            string operatorAndName,
            string operatorOrName,
            string operatorXorName,
            string literalTrueName,
            string literalFalseName,
            string literalNullName,
            string conditionalName,
            string nullTestName)
        {
            MemberRead = memberRead;
            IgnoreOperatorCase = ignoreOperatorCase;
            IgnoreBooleanLiteralCase = ignoreBooleanLiteralCase;
            IgnoreNullLiteralCase = ignoreNullLiteralCase;
            IgnoreNullTestCase = ignoreNullTestCase;
            IgnoreConditionalCase = ignoreConditionalCase;
            IgnoreVariableNameCase = ignoreVariableNameCase;
            IgnoreParameterNameCase = ignoreParameterNameCase;
            IgnoreFunctionNameCase = ignoreFunctionNameCase;
            QuoteCharacter = quoteCharacter;
            OperatorAndName = operatorAndName;
            OperatorOrName = operatorOrName;
            OperatorXorName = operatorXorName;
            LiteralTrueName = literalTrueName;
            LiteralFalseName = literalFalseName;
            LiteralNullName = literalNullName;
            ConditionalName = conditionalName;
            NullTestName = nullTestName;
        }

        public StringComparison GetStringComparison(bool ignoreCase)
            => ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;

        public StringComparer GetStringComparer(bool ignoreCase)
            => ignoreCase ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture;

        public static LanguageOptions Default { get; } = new LanguageOptions(
            memberRead: false,
            ignoreOperatorCase: false,
            ignoreBooleanLiteralCase: false,
            ignoreNullLiteralCase: false,
            ignoreNullTestCase: false,
            ignoreConditionalCase: false,
            ignoreVariableNameCase: false,
            ignoreParameterNameCase: false,
            ignoreFunctionNameCase: false,
            quoteCharacter: QuoteStyle.DoubleQuote,
            operatorAndName: "and",
            operatorOrName: "or",
            operatorXorName: "xor",
            literalTrueName: "true",
            literalFalseName: "false",
            literalNullName: "null",
            conditionalName: "if",
            nullTestName: "isnull"
        );

        public IEnumerable<string> Keywords
        {
            get
            {
                yield return OperatorAndName;
                yield return OperatorOrName;
                yield return OperatorXorName;
                yield return LiteralNullName;
                yield return LiteralTrueName;
                yield return LiteralFalseName;
                yield return ConditionalName;
                yield return NullTestName;
            }
        }

        public bool IsKeyword(string value)
        {
            return
                string.Equals(OperatorAndName, value, GetStringComparison(IgnoreOperatorCase)) ||
                string.Equals(OperatorOrName, value, GetStringComparison(IgnoreOperatorCase)) ||
                string.Equals(OperatorXorName, value, GetStringComparison(IgnoreOperatorCase)) ||
                string.Equals(LiteralNullName, value, GetStringComparison(IgnoreNullLiteralCase)) ||
                string.Equals(LiteralTrueName, value, GetStringComparison(IgnoreBooleanLiteralCase)) ||
                string.Equals(LiteralFalseName, value, GetStringComparison(IgnoreBooleanLiteralCase)) ||
                string.Equals(ConditionalName, value, GetStringComparison(IgnoreConditionalCase)) ||
                string.Equals(NullTestName, value, GetStringComparison(IgnoreNullTestCase));
        }

        public LanguageOptionsBuilder Derive() => new(this);
    }

    public enum QuoteStyle
    {
        DoubleQuote,
        SingleQuote,
        Backtick,
    }

    public class LanguageOptionsBuilder
    {
        private bool memberRead;

        private bool ignoreOperatorCase;

        private bool ignoreBooleanLiteralCase;

        private bool ignoreNullLiteralCase;

        private bool ignoreNullTestCase;

        private bool ignoreConditionalCase;

        private bool ignoreVariableNameCase;

        private bool ignoreParameterNameCase;

        private bool ignoreFunctionNameCase;

        private QuoteStyle quoteCharacter;

        private string operatorAndName;

        private string operatorOrName;

        private string operatorXorName;

        private string literalTrueName;

        private string literalFalseName;

        private string literalNullName;

        private string conditionalName;

        private string nullTestName;

        public LanguageOptionsBuilder(LanguageOptions options)
        {
            memberRead = options.MemberRead;
            ignoreOperatorCase = options.IgnoreOperatorCase;
            ignoreBooleanLiteralCase = options.IgnoreBooleanLiteralCase;
            ignoreNullLiteralCase = options.IgnoreNullLiteralCase;
            ignoreNullTestCase = options.IgnoreNullTestCase;
            ignoreConditionalCase = options.IgnoreConditionalCase;
            ignoreVariableNameCase = options.IgnoreVariableNameCase;
            ignoreParameterNameCase = options.IgnoreParameterNameCase;
            ignoreFunctionNameCase = options.IgnoreFunctionNameCase;
            quoteCharacter = options.QuoteCharacter;
            operatorAndName = options.OperatorAndName;
            operatorOrName = options.OperatorOrName;
            operatorXorName = options.OperatorXorName;
            literalTrueName = options.LiteralTrueName;
            literalFalseName = options.LiteralFalseName;
            literalNullName = options.LiteralNullName;
            conditionalName = options.ConditionalName;
            nullTestName = options.NullTestName;
        }

        public LanguageOptionsBuilder()
            : this(LanguageOptions.Default)
        { }

        public LanguageOptions Build()
        {
            return new LanguageOptions(
                memberRead,
                ignoreOperatorCase,
                ignoreBooleanLiteralCase,
                ignoreNullLiteralCase,
                ignoreNullTestCase,
                ignoreConditionalCase,
                ignoreVariableNameCase,
                ignoreParameterNameCase,
                ignoreFunctionNameCase,
                quoteCharacter,
                operatorAndName,
                operatorOrName,
                operatorXorName,
                literalTrueName,
                literalFalseName,
                literalNullName,
                conditionalName,
                nullTestName);
        }

        public LanguageOptionsBuilder WithMemberRead()
        {
            memberRead = true;
            return this;
        }

        public LanguageOptionsBuilder IgnoreOperatorCase()
        {
            ignoreOperatorCase = true;
            return this;
        }

        public LanguageOptionsBuilder IgnoreBooleanLiteralCase()
        {
            ignoreBooleanLiteralCase = true;
            return this;
        }
        public LanguageOptionsBuilder IgnoreNullLiteralCase()
        {
            ignoreNullLiteralCase = true;
            return this;
        }
        public LanguageOptionsBuilder IgnoreNullTestCase()
        {
            ignoreNullTestCase = true;
            return this;
        }
        public LanguageOptionsBuilder IgnoreConditionalCase()
        {
            ignoreConditionalCase = true;
            return this;
        }
        public LanguageOptionsBuilder IgnoreVariableNameCase()
        {
            ignoreVariableNameCase = true;
            return this;
        }
        public LanguageOptionsBuilder IgnoreParameterNameCase()
        {
            ignoreParameterNameCase = true;
            return this;
        }
        public LanguageOptionsBuilder IgnoreFunctionNameCase()
        {
            ignoreFunctionNameCase = true;
            return this;
        }

        public LanguageOptionsBuilder IgnoreCase() => IgnoreCase(all: true);

        public LanguageOptionsBuilder IgnoreCase(bool all)
        {
            ignoreOperatorCase = all;
            ignoreNullLiteralCase = all;
            ignoreNullTestCase = all;
            ignoreBooleanLiteralCase = all;
            ignoreConditionalCase = all;
            ignoreVariableNameCase = all;
            ignoreParameterNameCase = all;
            ignoreFunctionNameCase = all;
            return this;
        }

        public LanguageOptionsBuilder IgnoreCase(
            bool operatorCase = false,
            bool nullLiteralCase = false,
            bool nullTestCase = false,
            bool booleanLiteralCase = false,
            bool conditionalCase = false,
            bool variableNameCase = false,
            bool parameterNameCase = false,
            bool functionNameCase = false)
        {
            ignoreOperatorCase = operatorCase;
            ignoreNullLiteralCase = nullLiteralCase;
            ignoreNullTestCase = nullTestCase;
            ignoreBooleanLiteralCase = booleanLiteralCase;
            ignoreConditionalCase = conditionalCase;
            ignoreVariableNameCase = variableNameCase;
            ignoreParameterNameCase = parameterNameCase;
            ignoreFunctionNameCase = functionNameCase;
            return this;
        }


        public LanguageOptionsBuilder WithQuoteCharacter(QuoteStyle character)
        {
            quoteCharacter = character;
            return this;
        }

        public LanguageOptionsBuilder WithNullLiteralName(string nullName)
        {
            literalNullName = nullName;
            return this;
        }

        public LanguageOptionsBuilder WithBooleanLiteralNames(string trueName, string falseName)
        {
            literalTrueName = trueName;
            literalFalseName = falseName;
            return this;
        }

        public LanguageOptionsBuilder WithBooleanOperatorNames(string andName, string orName, string xorName)
        {
            operatorAndName = andName;
            operatorOrName = orName;
            operatorXorName = xorName;
            return this;
        }

        public LanguageOptionsBuilder WithNullTestName(string name)
        {
            nullTestName = name;
            return this;
        }

        public LanguageOptionsBuilder WithConditionalName(string name)
        {
            conditionalName = name;
            return this;
        }
    }
}
