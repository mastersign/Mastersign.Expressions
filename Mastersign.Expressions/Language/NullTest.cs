using System;
using System.Linq.Expressions;
using System.Text;

namespace Mastersign.Expressions.Language
{
    internal class NullTest : FunctionCall
    {
        public NullTest(string nullTestName)
            : base(nullTestName)
        {
            Name = "NullTest";
        }

        public override Type GetValueType(EvaluationContext context)
        {
            return typeof(bool);
        }

        public override bool CheckSemantic(EvaluationContext context, StringBuilder errMessages)
        {
            if (Parameters.Count != 1)
            {
                errMessages.AppendLine("The null test must have one argument.");
                return false;
            }

            var res = true;
            foreach (var p in Parameters)
            {
                if (!p.CheckSemantic(context, errMessages)) res = false;
            }
            return res;
        }

        public override object GetValue(EvaluationContext context, object[] parameterValues)
        {
            var input = Parameters[0].GetValue(context, parameterValues);
            return input == null;
        }

        public override Expression GetExpression(EvaluationContext context)
        {
            var pt = Parameters[0].GetValueType(context);
            if (pt == null) return Expression.Constant(true);
            if (pt.IsValueType) return Expression.Constant(false);
            return Expression.Equal(
                Parameters[0].GetExpression(context),
                Expression.Constant(null));
        }
    }
}
