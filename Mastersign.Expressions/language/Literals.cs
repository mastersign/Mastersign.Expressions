using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace de.mastersign.expressions.language
{
    internal abstract class Literal : ExpressionElement
    {
        private readonly string source;

        protected Literal(string source)
        {
            this.source = source;
        }

        public sealed override string Source
        {
            get { return source; }
        }
    }

    internal class NullLiteral : Literal
    {
        public NullLiteral(string source) 
            : base(source)
        {}

        public override bool CheckSemantic(EvaluationContext context, StringBuilder errMessages)
        {
            return true;
        }

        public override Type GetValueType(EvaluationContext context)
        {
            return typeof(object);
        }

        public override object GetValue(EvaluationContext context, object[] parameters)
        {
            return null;
        }

        public override Expression GetExpression(EvaluationContext context)
        {
            return Expression.Constant(null);
        }
    }

    internal class IntegerLiteral : Literal
    {
        private readonly int value32;
        private readonly long value64;
        private readonly bool isLong;
        private readonly bool success;

        public IntegerLiteral(string source)
            : base(source)
        {
            Name = "Integer Number";

            if (source.EndsWith("i", StringComparison.InvariantCultureIgnoreCase))
            {
                isLong = false;
                if (int.TryParse(
                    source.Substring(0, source.Length - 1),
                    NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out value32))
                {
                    success = true;
                }
            }
            else if (source.EndsWith("l", StringComparison.InvariantCultureIgnoreCase))
            {
                isLong = true;
                if (long.TryParse(
                    source.Substring(0, source.Length - 1),
                    NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out value64))
                {
                    success = true;
                }
            }
            else
            {
                long tmp;
                if (long.TryParse(
                    source,
                    NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out tmp))
                {
                    success = true;
                    if ((int)tmp == tmp)
                    {
                        value32 = (int)tmp;
                        isLong = false;
                    }
                    else
                    {
                        value64 = tmp;
                        isLong = true;
                    }
                }
            }
        }

        public long Value64 { get { return value64; } }
        public int Value32 { get { return value32; } }

        public override bool CheckSemantic(EvaluationContext context, StringBuilder errMessages)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (errMessages == null) throw new ArgumentNullException("errMessages");
            if (!success)
            {
                errMessages.AppendLine(string.Format("Error parsing '{0}' as integer literal.", Source));
            }
            return success;
        }

        public override Type GetValueType(EvaluationContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            return isLong ? typeof(long) : typeof(int);
        }

        public override object GetValue(EvaluationContext context, object[] parameters)
        {
            if (context == null) throw new ArgumentNullException("context");
            return isLong ? (object)value64 : (object)value32;
        }

        public override Expression GetExpression(EvaluationContext context)
        {
            return isLong 
                ? Expression.Constant(value64, typeof(long))
                : Expression.Constant(value32, typeof(int));
        }
    }

    internal class FloatingPointLiteral : Literal
    {
        private readonly float value32;
        private readonly double value64;
        private readonly bool isDouble;
        private readonly bool success;

        public FloatingPointLiteral(string source)
            : base(source)
        {
            Name = "Real Number";

            if (source.EndsWith("f", StringComparison.InvariantCultureIgnoreCase))
            {
                isDouble = false;
                if (float.TryParse(
                    source.Substring(0, source.Length - 1),
                    NumberStyles.Float, CultureInfo.InvariantCulture,
                    out value32))
                {
                    success = true;
                }
            }
            else if (source.EndsWith("d", StringComparison.InvariantCultureIgnoreCase))
            {
                isDouble = true;
                if (double.TryParse(
                    source.Substring(0, source.Length - 1),
                    NumberStyles.Float, CultureInfo.InvariantCulture,
                    out value64))
                {
                    success = true;
                }
            }
            else
            {
                double tmp;
                if (double.TryParse(
                    source,
                    NumberStyles.Float, CultureInfo.InvariantCulture,
                    out tmp))
                {
                    success = true;
                    if (Math.Abs((float)tmp - tmp) < double.Epsilon)
                    {
                        value32 = (float)tmp;
                        isDouble = false;
                    }
                    else
                    {
                        value64 = tmp;
                        isDouble = true;
                    }
                }
            }
        }

        public float Value32 { get { return value32; } }
        public double Value64 { get { return value64; } }

        public override bool CheckSemantic(EvaluationContext context, StringBuilder errMessages)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (errMessages == null) throw new ArgumentNullException("errMessages");
            if (!success)
            {
                errMessages.AppendLine(string.Format("Error parsing '{0}' as real number literal.", Source));
            }
            return success;
        }

        public override Type GetValueType(EvaluationContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            return isDouble ? typeof(double) : typeof(float);
        }

        public override object GetValue(EvaluationContext context, object[] parameters)
        {
            if (context == null) throw new ArgumentNullException("context");
            return isDouble ? (object)value64 : (object)value32;
        }

        public override Expression GetExpression(EvaluationContext context)
        {
            return isDouble
                ? Expression.Constant(value64, typeof(double))
                : Expression.Constant(value32, typeof(float));
        }
    }

    internal class DecimalLiteral : Literal
    {
        private readonly decimal value;
        private readonly bool success;

        public DecimalLiteral(string source)
            : base(source)
        {
            Name = "Decimal Number";

            if (decimal.TryParse(source.Substring(0, source.Length - 1),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value))
            {
                success = true;
            }
        }

        public decimal Value { get { return value; } }

        public override bool CheckSemantic(EvaluationContext context, StringBuilder errMessages)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (errMessages == null) throw new ArgumentNullException("errMessages");
            if (!success)
            {
                errMessages.AppendLine(string.Format("Error parsing '{0}' as decimal number literal.", Source));
            }
            return success;
        }

        public override Type GetValueType(EvaluationContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            return typeof(decimal);
        }

        public override object GetValue(EvaluationContext context, object[] parameters)
        {
            if (context == null) throw new ArgumentNullException("context");
            return Value;
        }

        public override Expression GetExpression(EvaluationContext context)
        {
            return Expression.Constant(value, typeof(decimal));
        }
    }

    internal class BooleanLiteral : Literal
    {
        private readonly bool value;
        private readonly bool success;

        public BooleanLiteral(string source)
            : base(source)
        {
            Name = "Boolean";

            if (string.Equals("true", source, StringComparison.InvariantCultureIgnoreCase))
            {
                value = true;
                success = true;
            }
            else if (string.Equals("false", source, StringComparison.InvariantCultureIgnoreCase))
            {
                value = false;
                success = true;
            }
        }

        public bool Value { get { return value; } }

        public override bool CheckSemantic(EvaluationContext context, StringBuilder errMessages)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (errMessages == null) throw new ArgumentNullException("errMessages");
            if (!success)
            {
                errMessages.AppendLine(string.Format("Error parsing '{0}' as boolean value.", Source));
            }
            return success;
        }

        public override Type GetValueType(EvaluationContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            return typeof(bool);
        }

        public override object GetValue(EvaluationContext context, object[] parameters)
        {
            if (context == null) throw new ArgumentNullException("context");
            return value;
        }

        public override Expression GetExpression(EvaluationContext context)
        {
            return Expression.Constant(value, typeof(bool));
        }
    }

    internal class StringLiteral : Literal
    {
        private readonly string value;

        public StringLiteral(string source)
            : base("'" + source + "'")
        {
            Name = "String";
            value = source;
        }

        public string Value { get { return value; } }

        public override bool CheckSemantic(EvaluationContext context, StringBuilder errMessages)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (errMessages == null) throw new ArgumentNullException("errMessages");
            return true;
        }

        public override Type GetValueType(EvaluationContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            return typeof(string);
        }

        public override object GetValue(EvaluationContext context, object[] parameters)
        {
            if (context == null) throw new ArgumentNullException("context");
            return value;
        }

        public override Expression GetExpression(EvaluationContext context)
        {
            return Expression.Constant(value, typeof(string));
        }
    }

}
