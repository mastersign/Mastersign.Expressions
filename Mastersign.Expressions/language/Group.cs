using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace de.mastersign.expressions.language
{
    internal class Group : ExpressionNode
    {
        private readonly ExpressionElement child;

        public Group(ExpressionElement child)
        {
            Name = "Group";
            this.child = child;
        }

        public override string Source
        {
            get { return "(" + child.Source + ")"; }
        }

        public override bool CheckSemantic(EvaluationContext context, StringBuilder errMessages)
        {
            return child.CheckSemantic(context, errMessages);
        }

        public override Type GetValueType(EvaluationContext context)
        {
            return child.GetValueType(context);
        }

        public override object GetValue(EvaluationContext context, object[] parameters)
        {
            return child.GetValue(context, parameters);
        }

        public override System.Linq.Expressions.Expression GetExpression(EvaluationContext context)
        {
            return child.GetExpression(context);
        }

        public override IEnumerator<ExpressionElement> GetEnumerator()
        {
            yield return child;
        }
    }
}