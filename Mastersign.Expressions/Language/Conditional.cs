using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Mastersign.Expressions.Language
{
    internal class Conditional : FunctionCall
    {
        public Conditional(string conditionalName)
            : base(conditionalName)
        {
            Name = "Condition";
        }

        public override bool CheckSemantic(EvaluationContext context, StringBuilder errMessages)
        {
            if (Parameters.Count != 3)
            {
                errMessages.AppendLine("The conditional must have three arguments.");
                return false;
            }

            var res = true;
            foreach (var p in Parameters)
            {
                if (!p.CheckSemantic(context, errMessages)) res = false;
            }
            if (!res) return false;

            if (Parameters[0].GetValueType(context) != typeof(bool))
            {
                errMessages.AppendLine("The first parameter of the conditional must be a boolean value (true or false).");
                res = false;
            }
            var pt1 = Parameters[1].GetValueType(context);
            var pt2 = Parameters[2].GetValueType(context);
            if (pt1 != pt2)
            {
                if (NumericHelper.IsNumeric(pt1) && NumericHelper.IsNumeric(pt2))
                {
                    if (!NumericHelper.TryHarmonizeTypes(pt1, pt2, out var _))
                    {
                        errMessages.AppendLine("The numeric types of the true and false part in the conditional are incompatible.");
                        res = false;
                    }
                }
                else
                {
                    errMessages.AppendLine("The true part and the false part in the conditional must have the same type or compatible numeric types.");
                    res = false;
                }
            }
            return res;
        }

        public override Type GetValueType(EvaluationContext context)
        {
            var pt1 = Parameters[1].GetValueType(context);
            var pt2 = Parameters[2].GetValueType(context);
            return pt1 == pt2
                ? pt1
                : NumericHelper.IsNumeric(pt1) &&
                  NumericHelper.IsNumeric(pt2) &&
                  NumericHelper.TryHarmonizeTypes(pt1, pt2, out var rt)
                    ? rt
                    : Parameters[1].GetValueType(context);
        }

        public override object GetValue(EvaluationContext context, object[] parameterValues)
        {
            var funcParameterValues = Parameters.Select(p => p.GetValue(context, parameterValues)).ToArray();
            var predicate = funcParameterValues[0];
            var resultTrue = funcParameterValues[1];
            var resultFalse = funcParameterValues[2];
            var pt1 = Parameters[1].GetValueType(context);
            var pt2 = Parameters[2].GetValueType(context);
            if (pt1 == pt2)
            {
                return (bool)predicate ? resultTrue : resultFalse;
            }
            if (!NumericHelper.TryHarmonizeTypes(pt1, pt2, out var rt))
            {
                throw new InvalidOperationException("The conditional has invalid semantics. The argument types are incompatible with each other.");
            }
            if (pt1 == rt)
            {
                return (bool)predicate ? resultTrue : NumericHelper.Upgrade(resultFalse, rt);
            }
            else
            {
                return (bool)predicate ? NumericHelper.Upgrade(resultTrue, rt) : resultFalse;
            }
        }

        public override Expression GetExpression(EvaluationContext context)
        {
            var pt1 = Parameters[1].GetValueType(context);
            var pt2 = Parameters[2].GetValueType(context);
            if (pt1 == pt2)
            {
                return Expression.Condition(
                    Parameters[0].GetExpression(context),
                    Parameters[1].GetExpression(context),
                    Parameters[2].GetExpression(context));
            }
            if (!NumericHelper.TryHarmonizeTypes(pt1, pt2, out var rt))
            {
                throw new InvalidOperationException("The conditional has invalid semantics. The argument types are incompatible with each other.");
            }
            if (pt1 == rt)
            {
                return Expression.Condition(
                    Parameters[0].GetExpression(context),
                    Parameters[1].GetExpression(context),
                    Expression.ConvertChecked(Parameters[2].GetExpression(context), rt));
            }
            else
            {
                return Expression.Condition(
                    Parameters[0].GetExpression(context),
                    Expression.ConvertChecked(Parameters[1].GetExpression(context), rt),
                    Parameters[2].GetExpression(context));
            }
        }
    }
}
