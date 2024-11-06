using System;

namespace Mastersign.Expressions.Language
{
    internal static class ExpressionElementExtensions
    {
        public static Type GetSafeValueType(this ExpressionElement element, EvaluationContext context)
        {
            return element.GetValueType(context) ?? typeof(object);
        }
    }
}
