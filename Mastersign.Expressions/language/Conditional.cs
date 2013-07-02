using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace de.mastersign.expressions.language
{
    internal class Conditional : FunctionCall
    {
        public const string FUNCTION_NAME = "if";

        public Conditional()
            : base(FUNCTION_NAME)
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

            if (Parameters[0].GetValueType(context) != typeof (bool))
            {
                errMessages.AppendLine("The first parameter of the conditional must be a boolean value (true or false).");
                res = false;
            }
            if (Parameters[1].GetValueType(context) != Parameters[2].GetValueType(context))
            {
                errMessages.AppendLine("The true part and the false part in the conditional must have the same type.");
                res = false;
            }
            return res;
        }

        public override Type GetValueType(EvaluationContext context)
        {
            return Parameters[1].GetValueType(context);
        }

        public override object GetValue(EvaluationContext context, object[] parameterValues)
        {
            var funcParameterValues = Parameters.Select(p => p.GetValue(context, parameterValues)).ToArray();
            return (bool)funcParameterValues[0] ? funcParameterValues[1] : funcParameterValues[2];
        }

        public override Expression GetExpression(EvaluationContext context)
        {
            return Expression.Condition(
                Parameters[0].GetExpression(context),
                Parameters[1].GetExpression(context),
                Parameters[2].GetExpression(context));
        }
    }
}
