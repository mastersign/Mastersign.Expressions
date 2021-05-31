using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Mastersign.Expressions.Language
{
    internal class FunctionCall : ExpressionNode
    {
        public static FunctionCall CreateInstance(string identifier, bool ignoreCase)
        {
            if (ignoreCase) identifier = identifier.ToLowerInvariant();
            switch (identifier)
            {
                case Conditional.FUNCTION_NAME:
                    return new Conditional();
                default:
                    return new FunctionCall(identifier);
            }
        }

        public string Identifier { get; private set; }

        public FunctionCall(string identifier)
        {
            Name = identifier + "()";
            Identifier = identifier;
        }

        public FunctionCall AddParameter(ExpressionElement parameter)
        {
            Parameters.Add(parameter);
            return this;
        }

        public FunctionCall WithParameters(IEnumerable<ExpressionElement> parameter)
        {
            Parameters.AddRange(parameter);
            return this;
        }

        protected readonly List<ExpressionElement> Parameters = new List<ExpressionElement>();

        public override string Source
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append(Identifier);
                sb.Append("(");
                sb.Append(string.Join(", ", Parameters.Select(p => p.Source)));
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

            foreach (var p in Parameters)
            {
                if (!p.CheckSemantic(context, errMessages)) res = false;
            }
            if (!res) return false;

            var types = (from p in Parameters select p.GetValueType(context)).ToArray();
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
            var types = (from p in Parameters select p.GetValueType(context)).ToArray();
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
            var funcParameterValues = Parameters.Select(p => p.GetValue(context, parameterValues)).ToArray();
            var function = FindHandle(context);
            return function.Call(funcParameterValues);
        }

        public override Expression GetExpression(EvaluationContext context)
        {
            var function = FindHandle(context);
            if (function == null) throw new InvalidOperationException("Function not found.");
            var pi = function.Method.GetParameters();
            var p = Parameters.Select(e => e.GetExpression(context)).ToArray();
            for (var i = 0; i < p.Length; i++)
            {
                if (p[i].Type == pi[i].ParameterType) continue;

                if (NumericHelper.IsNumeric(pi[i].ParameterType) &&
                    NumericHelper.IsNumeric(p[i].Type))
                {
                    p[i] = Expression.ConvertChecked(p[i], pi[i].ParameterType);
                }
            }
            return function.Method.IsStatic
                ? Expression.Call(function.Method, p)
                : Expression.Call(Expression.Constant(function.Target), function.Method, p);
        }

        public override IEnumerator<ExpressionElement> GetEnumerator()
        {
            return Parameters.GetEnumerator();
        }
    }
}
