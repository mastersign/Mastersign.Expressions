using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Mastersign.Expressions.Language
{
    internal class MemberRead : ExpressionNode
    {
        public class RightPart : IRightPart
        {
            public string MemberIdentifier { get; private set; }

            public RightPart(string memberIdentifier)
            {
                MemberIdentifier = memberIdentifier;
            }

            public ExpressionElement BuildExpressionElement(ExpressionElement leftPart)
            {
                return new MemberRead(leftPart, MemberIdentifier);
            }
        }

        public ExpressionElement Target { get; private set; }

        public string MemberIdentifier { get; private set; }

        public MemberRead(ExpressionElement target, string memberIdentifier)
        {
            Target = target;
            MemberIdentifier = memberIdentifier;
        }

        public override string Source
        {
            get { return Target.Source + "." + MemberIdentifier; }
        }

        public override bool CheckSemantic(EvaluationContext context, StringBuilder errMessages)
        {
            var res = Target.CheckSemantic(context, errMessages);

            var targetType = Target.GetSafeValueType(context);
            var members = GetMatchingMembers(context);
            if (members.Length > 1)
            {
                errMessages.AppendLine(string.Format(
                    "Member identifier '{0}' is ambigues for target type '{1}'.",
                    MemberIdentifier, targetType));
                res = false;
            }
            if (members.Length == 0)
            {
                errMessages.AppendLine(string.Format(
                    "Could not find member '{0}' in target type '{1}'.",
                    MemberIdentifier, targetType));
                res = false;
            }

            return res;
        }

        private MemberInfo[] GetMatchingMembers(EvaluationContext context)
        {
            var targetType = Target.GetSafeValueType(context);
            return targetType
                .GetMember(MemberIdentifier)
                .Where(m => (m is PropertyInfo && ((PropertyInfo)m).GetGetMethod() != null)
                            || m is FieldInfo)
                .ToArray();
        }

        private MemberInfo GetMember(EvaluationContext context)
        {
            return GetMatchingMembers(context)[0];
        }

        public override Type GetValueType(EvaluationContext context)
        {
            var m = GetMember(context);
            
            var p = m as PropertyInfo;
            if (p != null) return p.PropertyType;
            
            var f = m as FieldInfo;
            if (f != null) return f.FieldType;
            
            throw new InvalidOperationException("No matching property or field found.");
        }

        public override object GetValue(EvaluationContext context, object[] parameters)
        {
            var m = GetMember(context);

            var t = Target.GetValue(context);

            var p = m as PropertyInfo;
            if (p != null) return p.GetValue(t, new object[0]);

            var f = m as FieldInfo;
            if (f != null) return f.GetValue(t);

            throw new InvalidOperationException("No matching property or field found.");
        }

        public override Expression GetExpression(EvaluationContext context)
        {
            var m = GetMember(context);

            var t = Target.GetExpression(context);

            var p = m as PropertyInfo;
            if (p != null)
            {
                return Expression.Call(t, p.GetGetMethod());
            }

            var f = m as FieldInfo;
            if (f != null)
            {
                return Expression.Field(t, f);
            }

            throw new InvalidOperationException("No matching property or field found.");
        }

        public override IEnumerator<ExpressionElement> GetEnumerator()
        {
            yield return Target;
        }
    }
}
