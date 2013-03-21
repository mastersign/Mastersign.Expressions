using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System;

namespace de.mastersign.expressions.language
{
    internal class Variable : ExpressionElement
    {
        public Variable(string name)
        {
            Name = name;
        }

        public override string Source
        {
            get { return Name; }
        }

        public override bool CheckSemantic(EvaluationContext context, StringBuilder errMessages)
        {
            var varExists = context.ParameterExists(Name) || context.VariableExists(Name);
            if (!varExists)
            {
                errMessages.AppendLine(string.Format("No parameter or variable with name '{0}' found.", Name));
            }
            return varExists;
        }

        public override Type GetValueType(EvaluationContext context)
        {
            if (context.ParameterExists(Name))
            {
                return context.GetParameter(Name).Type;
            }
            var value = context.ReadVariable(Name);
            return value != null ? value.GetType() : null;
        }

        public override object GetValue(EvaluationContext context, object[] parameters)
        {
            if (context.ParameterExists(Name))
            {
                return parameters[context.GetParameterPosition(Name)];
            }
            return context.ReadVariable(Name);
        }

        public override Expression GetExpression(EvaluationContext context)
        {
            if (context.ParameterExists(Name))
            {
                return context.GetParameter(Name).Expression;
            }
            if (context.IsVariableConstant(Name))
            {
                return Expression.Constant(GetValue(context), GetValueType(context));
            }
            return Expression.Convert(
                Expression.Call(
                    Expression.Constant(context, typeof(EvaluationContext)),
                    typeof(EvaluationContext).GetMethod("ReadVariable", new[] { typeof(string) }),
                    Expression.Constant(Name, typeof(string))),
                GetValueType(context));
        }
    }
}