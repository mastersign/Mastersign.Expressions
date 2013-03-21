using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace de.mastersign.expressions.language
{
    internal class FunctionCall : ExpressionNode
    {
        public string Identifier { get; private set; }

        public FunctionCall(string identifier)
        {
            Name = identifier + "()";
            Identifier = identifier;
        }

        public FunctionCall AddParameter(ExpressionElement parameter)
        {
            parameters.Add(parameter);
            return this;
        }

        public FunctionCall WithParameters(IEnumerable<ExpressionElement> parameter)
        {
            parameters.AddRange(parameter);
            return this;
        }

        private readonly List<ExpressionElement> parameters = new List<ExpressionElement>();

        public override string Source
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append(Identifier);
                sb.Append("(");
                sb.Append(string.Join(", ", parameters.Select(p => p.Source)));
                sb.Append(")");
                return sb.ToString();
            }
        }

        public override bool CheckSemantic(EvaluationContext context, StringBuilder errMessages)
        {
            var res = true;
            var functionGroup = context.GetFunctionGroup(Identifier);
            if (functionGroup == null)
            {
                errMessages.AppendLine(string.Format(
                    "Function '{0}' not found.",
                    Identifier));
                res = false;
            }

            foreach (var p in parameters)
            {
                if (!p.CheckSemantic(context, errMessages)) res = false;
            }

            if (!res) return false;

            var types = (from p in parameters select p.GetValueType(context)).ToArray();
            var function = functionGroup.FindMatch(types);
            if (function == null)
            {
                errMessages.AppendLine(string.Format(
                    "The function '{0}' does not match the given parameters.",
                    Identifier));
                res = false;
            }

            return res;
        }

        private FunctionHandle FindHandle(EvaluationContext context)
        {
            var functionGroup = context.GetFunctionGroup(Identifier);
            if (functionGroup == null) throw new InvalidOperationException("The function does not exist.");
            var types = (from p in parameters select p.GetValueType(context)).ToArray();
            var function = functionGroup.FindMatch(types);
            if (function == null) throw new InvalidOperationException("The function does not match the parameters.");
            return function;
        }

        public override Type GetValueType(EvaluationContext context)
        {
            return FindHandle(context).ReturnType;
        }

        public override object GetValue(EvaluationContext context, object[] parameterValues)
        {
            var funcParameterValues = parameters.Select(p => p.GetValue(context, parameterValues)).ToArray();
            var function = FindHandle(context);
            return function.Call(funcParameterValues);
        }

        public override Expression GetExpression(EvaluationContext context)
        {
            var function = FindHandle(context);
            if (function == null) throw new InvalidOperationException("Function not found.");
            var pi = function.Method.GetParameters();
            var p = parameters.Select(e => e.GetExpression(context)).ToArray();
            for (var i = 0; i < p.Length; i++)
            {
                if (p[i].Type == pi[i].ParameterType) continue;

                if (NumericHelper.IsNumeric(pi[i].ParameterType) && 
                    NumericHelper.IsNumeric(p[i].Type))
                {
                    p[i] = Expression.ConvertChecked(p[i], pi[i].ParameterType);
                }
            }
            return Expression.Call(function.Method, p);
        }

        public override IEnumerator<ExpressionElement> GetEnumerator()
        {
            return parameters.GetEnumerator();
        }
    }
}
