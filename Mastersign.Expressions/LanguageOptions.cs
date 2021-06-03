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
            string conditionalName)
        {
            MemberRead = memberRead;
            IgnoreOperatorCase = ignoreOperatorCase;
            IgnoreBooleanLiteralCase = ignoreBooleanLiteralCase;
            IgnoreNullLiteralCase = ignoreNullLiteralCase;
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
            conditionalName: "if"
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
                string.Equals(ConditionalName, value, GetStringComparison(IgnoreConditionalCase));
        }
    }

    public enum QuoteStyle
    {
        DoubleQuote,
        SingleQuote,
        Backtick,
    }

    public class LanguageOptionsBuilder
    {
        private bool memberRead = LanguageOptions.Default.MemberRead;

        private bool ignoreOperatorCase = LanguageOptions.Default.IgnoreOperatorCase;

        private bool ignoreBooleanLiteralCase = LanguageOptions.Default.IgnoreBooleanLiteralCase;

        private bool ignoreNullLiteralCase = LanguageOptions.Default.IgnoreNullLiteralCase;

        private bool ignoreConditionalCase = LanguageOptions.Default.IgnoreConditionalCase;

        private bool ignoreVariableNameCase = LanguageOptions.Default.IgnoreVariableNameCase;

        private bool ignoreParameterNameCase = LanguageOptions.Default.IgnoreParameterNameCase;

        private bool ignoreFunctionNameCase = LanguageOptions.Default.IgnoreFunctionNameCase;

        private QuoteStyle quoteCharacter = LanguageOptions.Default.QuoteCharacter;

        private string operatorAndName = LanguageOptions.Default.OperatorAndName;

        private string operatorOrName = LanguageOptions.Default.OperatorOrName;

        private string operatorXorName = LanguageOptions.Default.OperatorXorName;

        private string literalTrueName = LanguageOptions.Default.LiteralTrueName;

        private string literalFalseName = LanguageOptions.Default.LiteralFalseName;

        private string literalNullName = LanguageOptions.Default.LiteralNullName;

        private string conditionalName = LanguageOptions.Default.ConditionalName;

        public LanguageOptions Build()
        {
            return new LanguageOptions(
                memberRead,
                ignoreOperatorCase,
                ignoreBooleanLiteralCase,
                ignoreNullLiteralCase,
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
                conditionalName);
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

        public LanguageOptionsBuilder IgnoreNullLiteralCase()
        {
            ignoreNullLiteralCase = true;
            return this;
        }

        public LanguageOptionsBuilder IgnoreBooleanLiteralCase()
        {
            ignoreBooleanLiteralCase = true;
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
        public LanguageOptionsBuilder IgnoreCase()
        {
            ignoreOperatorCase = true;
            ignoreNullLiteralCase = true;
            ignoreBooleanLiteralCase = true;
            ignoreConditionalCase = true;
            ignoreVariableNameCase = true;
            ignoreParameterNameCase = true;
            ignoreFunctionNameCase = true;
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

        public LanguageOptionsBuilder WithConditionalName(string name)
        {
            conditionalName = name;
            return this;
        }
    }
}
