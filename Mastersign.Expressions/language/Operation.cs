using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System;

namespace de.mastersign.expressions.language
{
    internal enum OperatorType
    {
        Numeric,
        Boolean,
        String,
        Relation,
    }

    internal class Operator
    {
        public string Name { get; private set; }
        public string Source { get; private set; }
        public OperatorType OpType { get; private set; }
        public int PriorityLevel { get; private set; }
        public Func<object, object, object> Function { get; private set; }

        private Operator(string name, string source, OperatorType opType, int priority, Func<object, object, object> func)
        {
            Name = name;
            Source = source;
            OpType = opType;
            PriorityLevel = priority;
            Function = func;
        }

        public override string ToString()
        {
            return Source;
        }

        public static readonly Operator NumPower
            = new Operator("power", "^", OperatorType.Numeric, 0, NumPowerFunc);
        private static object NumPowerFunc(object l, object r)
        {
            return Math.Pow(Convert.ToDouble(l), Convert.ToDouble(r));
        }

        public static readonly Operator NumMultiplication
            = new Operator("multiplication", " * ", OperatorType.Numeric, 1, NumMultiplicationFunc);
        private static object NumMultiplicationFunc(object l, object r)
        {
            object l2, r2;
            var type = NumericHelper.HarmonizeValues(
                NumericHelper.AutoUpgradeNumericValue(l),
                NumericHelper.AutoUpgradeNumericValue(r),
                out l2, out r2);
            checked
            {
                if (type == typeof(Int32)) return (Int32)l2 * (Int32)r2;
                if (type == typeof(UInt32)) return (UInt32)l2 * (UInt32)r2;
                if (type == typeof(Int64)) return (Int64)l2 * (Int64)r2;
                if (type == typeof(UInt64)) return (UInt64)l2 * (UInt64)r2;
                if (type == typeof(Single)) return (Single)l2 * (Single)r2;
                if (type == typeof(Double)) return (Double)l2 * (Double)r2;
                if (type == typeof(Decimal)) return (Decimal)l2 * (Decimal)r2;
            }
            throw new NotSupportedException();
        }

        public static readonly Operator NumDivision
            = new Operator("division", " / ", OperatorType.Numeric, 1, NumDivisionFunc);
        private static object NumDivisionFunc(object l, object r)
        {
            object l2, r2;
            var type = NumericHelper.HarmonizeValues(
                NumericHelper.AutoUpgradeNumericValue(l),
                NumericHelper.AutoUpgradeNumericValue(r),
                out l2, out r2);
            checked
            {
                if (type == typeof(Int32)) return (Int32)l2 / (Int32)r2;
                if (type == typeof(UInt32)) return (UInt32)l2 / (UInt32)r2;
                if (type == typeof(Int64)) return (Int64)l2 / (Int64)r2;
                if (type == typeof(UInt64)) return (UInt64)l2 / (UInt64)r2;
                if (type == typeof(Single)) return (Single)l2 / (Single)r2;
                if (type == typeof(Double)) return (Double)l2 / (Double)r2;
                if (type == typeof(Decimal)) return (Decimal)l2 / (Decimal)r2;
            }
            throw new NotSupportedException();
        }

        public static readonly Operator NumAddition
            = new Operator("addition", " + ", OperatorType.Numeric, 2, NumAdditionFunc);
        private static object NumAdditionFunc(object l, object r)
        {
            object l2, r2;
            var type = NumericHelper.HarmonizeValues(
                NumericHelper.AutoUpgradeNumericValue(l),
                NumericHelper.AutoUpgradeNumericValue(r),
                out l2, out r2);
            checked
            {
                if (type == typeof(Int32)) return (Int32)l2 + (Int32)r2;
                if (type == typeof(UInt32)) return (UInt32)l2 + (UInt32)r2;
                if (type == typeof(Int64)) return (Int64)l2 + (Int64)r2;
                if (type == typeof(UInt64)) return (UInt64)l2 + (UInt64)r2;
                if (type == typeof(Single)) return (Single)l2 + (Single)r2;
                if (type == typeof(Double)) return (Double)l2 + (Double)r2;
                if (type == typeof(Decimal)) return (Decimal)l2 + (Decimal)r2;
            }
            throw new NotSupportedException();
        }

        public static readonly Operator NumSubtraction
            = new Operator("subtraction", " - ", OperatorType.Numeric, 2, NumSubtractionFunc);
        private static object NumSubtractionFunc(object l, object r)
        {
            object l2, r2;
            var type = NumericHelper.HarmonizeValues(
                NumericHelper.AutoUpgradeNumericValue(l),
                NumericHelper.AutoUpgradeNumericValue(r),
                out l2, out r2);
            checked
            {
                if (type == typeof(Int32)) return (Int32)l2 - (Int32)r2;
                if (type == typeof(UInt32)) return (UInt32)l2 - (UInt32)r2;
                if (type == typeof(Int64)) return (Int64)l2 - (Int64)r2;
                if (type == typeof(UInt64)) return (UInt64)l2 - (UInt64)r2;
                if (type == typeof(Single)) return (Single)l2 - (Single)r2;
                if (type == typeof(Double)) return (Double)l2 - (Double)r2;
                if (type == typeof(Decimal)) return (Decimal)l2 - (Decimal)r2;
            }
            throw new NotSupportedException();
        }

        public static readonly Operator StringConcat
            = new Operator("concatenation", " & ", OperatorType.String, 3, StringConcatFunc);
        private static object StringConcatFunc(object l, object r)
        {
            return string.Concat(l, r);
        }

        public static readonly Operator RelationLess
            = new Operator("less", " < ", OperatorType.Relation, 4, RelationLessFunc);
        private static object RelationLessFunc(object l, object r)
        {
            return Compare(l, r) < 0;
        }

        public static readonly Operator RelationLessOrEqual
            = new Operator("less or equal", " <= ", OperatorType.Relation, 4, RelationLessOrEqualFunc);
        private static object RelationLessOrEqualFunc(object l, object r)
        {
            return Compare(l, r) <= 0;
        }

        public static readonly Operator RelationEqual
            = new Operator("equal", " = ", OperatorType.Relation, 4, RelationEqualFunc);
        private static object RelationEqualFunc(object l, object r)
        {
            return AreEqual(l, r);
        }

        public static readonly Operator RelationUnequal
            = new Operator("unequal", " <> ", OperatorType.Relation, 4, RelationUnequalFunc);
        private static object RelationUnequalFunc(object l, object r)
        {
            return !AreEqual(l, r);
        }

        public static readonly Operator RelationGreaterOrEqual
            = new Operator("greater or equal", " >= ", OperatorType.Relation, 4, RelationGreaterOrEqualFunc);
        private static object RelationGreaterOrEqualFunc(object l, object r)
        {
            return Compare(l, r) >= 0;
        }

        public static readonly Operator RelationGreater
            = new Operator("greater", " > ", OperatorType.Relation, 4, RelationGreaterFunc);
        private static object RelationGreaterFunc(object l, object r)
        {
            return Compare(l, r) > 0;
        }

        public static readonly Operator BoolAnd
            = new Operator("and", " and ", OperatorType.Boolean, 5, BoolAndFunc);
        private static object BoolAndFunc(object l, object r)
        {
            return (bool)l && (bool)r;
        }

        public static readonly Operator BoolOr
            = new Operator("or", " or ", OperatorType.Boolean, 6, BoolOrFunc);
        private static object BoolOrFunc(object l, object r)
        {
            return (bool)l || (bool)r;
        }

        public static readonly Operator BoolXor
            = new Operator("xor", " xor ", OperatorType.Boolean, 6, BoolXorFunc);
        private static object BoolXorFunc(object l, object r)
        {
            return (bool)l ^ (bool)r;
        }

        private static object AutoUpgradeIfNumeric(object value)
        {
            return value != null
                       ? (NumericHelper.IsNumeric(value.GetType())
                              ? NumericHelper.AutoUpgradeNumericValue(value)
                              : value)
                       : null;
        }

        private static bool AreEqual(object l, object r)
        {
            if (l == null && r == null) return true;
            if (l == null || r == null) return false;

            var lt = l.GetType();
            var rt = r.GetType();

            if (NumericHelper.IsNumeric(lt) && NumericHelper.IsNumeric(rt))
            {
                l = NumericHelper.AutoUpgradeNumericValue(l);
                r = NumericHelper.AutoUpgradeNumericValue(r);
                Type t;
                if (NumericHelper.TryHarmonizeTypes(lt, rt, out t))
                {
                    var l2 = Convert.ChangeType(l, t);
                    var r2 = Convert.ChangeType(r, t);
                    return l2.Equals(r2);
                }
                throw new ArgumentException("Error comparing numeric values: The types are incompatible.");
            }

            return l.Equals(r);
        }

        private static int Compare(object l, object r)
        {
            if (l == null && r == null) return 0;
            if (l == null) return int.MinValue;
            if (r == null) return int.MaxValue;

            var lt = l.GetType();
            var rt = r.GetType();

            if (NumericHelper.IsNumeric(lt) && NumericHelper.IsNumeric(rt))
            {
                l = NumericHelper.AutoUpgradeNumericValue(l);
                r = NumericHelper.AutoUpgradeNumericValue(r);
                Type t;
                if (NumericHelper.TryHarmonizeTypes(lt, rt, out t))
                {
                    var l2 = Convert.ChangeType(l, t);
                    var r2 = Convert.ChangeType(r, t);
                    return ((IComparable)l2).CompareTo(r2);
                }
                throw new ArgumentException("Error comparing numeric values: The types are incompatible.");
            }

            if (lt != rt)
            {
                throw new ArgumentException("Comparing values with different types is impossible.");
            }

            if (lt == typeof(string))
            {
                return string.Compare((string)l, (string)r, StringComparison.InvariantCulture);
            }

            if (!(l is IComparable))
            {
                throw new ArgumentException("The given values are not comparable.");
            }
            return ((IComparable)l).CompareTo(r);
        }

        public Expression GetExpression(EvaluationContext context, ExpressionElement left, ExpressionElement right)
        {
            var leftExpr = left.GetExpression(context);
            var rightExpr = right.GetExpression(context);

            if (OpType == OperatorType.Numeric)
            {
                HarmonizeNumericExpressions(ref leftExpr, ref rightExpr);
                if (this == NumAddition) return Expression.AddChecked(leftExpr, rightExpr);
                if (this == NumSubtraction) return Expression.Subtract(leftExpr, rightExpr);
                if (this == NumMultiplication) return Expression.Multiply(leftExpr, rightExpr);
                if (this == NumDivision) return Expression.Divide(leftExpr, rightExpr);
                if (this == NumPower) return Expression.Power(NumConvert(leftExpr, typeof(double)), NumConvert(rightExpr, typeof(double)));
            }

            if (this == BoolAnd) return Expression.And(leftExpr, rightExpr);
            if (this == BoolOr) return Expression.Or(leftExpr, rightExpr);
            if (this == BoolXor) return Expression.ExclusiveOr(leftExpr, rightExpr);

            if (this == StringConcat) return BuildConcatExpression(leftExpr, rightExpr);

            if (OpType == OperatorType.Relation)
            {
                if (NumericHelper.IsNumeric(leftExpr.Type))
                {
                    HarmonizeNumericExpressions(ref leftExpr, ref rightExpr);
                    if (this == RelationLess) return Expression.LessThan(leftExpr, rightExpr);
                    if (this == RelationLessOrEqual) return Expression.LessThanOrEqual(leftExpr, rightExpr);
                    if (this == RelationEqual) return Expression.Equal(leftExpr, rightExpr);
                    if (this == RelationUnequal) return Expression.NotEqual(leftExpr, rightExpr);
                    if (this == RelationGreaterOrEqual) return Expression.GreaterThanOrEqual(leftExpr, rightExpr);
                    if (this == RelationGreater) return Expression.GreaterThan(leftExpr, rightExpr);
                }
                else if (leftExpr.Type == typeof (bool) || rightExpr.Type == typeof(bool))
                {
                    if (this == RelationEqual) return Expression.Equal(leftExpr, rightExpr);
                    if (this == RelationUnequal) return Expression.NotEqual(leftExpr, rightExpr);
                }
                else if (typeof(IComparable).IsAssignableFrom(leftExpr.Type))
                {
                    var compareExpr = Expression.Call(
                        leftExpr,
                        typeof (IComparable).GetMethod("CompareTo", new[] {typeof (object)}),
                        typeof(object).IsAssignableFrom(rightExpr.Type) ? rightExpr : Expression.Convert(rightExpr, typeof(object)));
                    return RelationFromComparation(compareExpr);
                }
                else
                {
                    if (this == RelationEqual) return Expression.Equal(leftExpr, rightExpr);
                    if (this == RelationUnequal) return Expression.NotEqual(leftExpr, rightExpr);
                    throw new InvalidOperationException("The relation operation does not support the given operands.");
                }
            }

            throw new NotSupportedException();
        }

        private Expression RelationFromComparation(Expression compareResult)
        {
            var zeroExpr = Expression.Constant(0, typeof (int));
            if (this == RelationLess) return Expression.LessThan(compareResult, zeroExpr);
            if (this == RelationLessOrEqual) return Expression.LessThanOrEqual(compareResult, zeroExpr);
            if (this == RelationEqual) return Expression.Equal(compareResult, zeroExpr);
            if (this == RelationUnequal) return Expression.NotEqual(compareResult, zeroExpr);
            if (this == RelationGreaterOrEqual) return Expression.GreaterThanOrEqual(compareResult, zeroExpr);
            if (this == RelationGreater) return Expression.GreaterThan(compareResult, zeroExpr);
            throw new NotSupportedException();
        }

        private static void HarmonizeNumericExpressions(ref Expression leftExpr, ref Expression rightExpr)
        {
            Type target;
            if (!NumericHelper.TryHarmonizeTypes(leftExpr.Type, rightExpr.Type, out target))
            {
                throw new InvalidOperationException("The types are not compatible.");
            }
            leftExpr = NumConvert(leftExpr, target);
            rightExpr = NumConvert(rightExpr, target);
        }

        private static Expression BuildConcatExpression(Expression leftExpr, Expression rightExpr)
        {
            return Expression.Call(
                typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }),
                MakeString(leftExpr), MakeString(rightExpr));
        }

        private static Expression MakeString(Expression expr)
        {
            return expr.Type == typeof(string)
                       ? expr
                       : expr.Type.IsValueType
                            ? (Expression)Expression.Call(expr, typeof(object).GetMethod("ToString", new Type[0]))
                            : (Expression)Expression.Condition(
                                Expression.Equal(Expression.Constant(null), expr),
                                Expression.Constant(null, typeof(string)),
                                Expression.Call(expr, typeof(object).GetMethod("ToString", new Type[0])));
        }

        private static Expression NumConvert(Expression expr, Type target)
        {
            return expr.Type == target
                       ? expr
                       : Expression.ConvertChecked(expr, target);
        }

    }

    internal class Operation : ExpressionNode
    {
        private readonly Operator op;
        private readonly ExpressionElement left;
        private readonly ExpressionElement right;

        public Operation(ExpressionElement left, Operator op, ExpressionElement right)
        {
            this.left = left;
            this.op = op;
            this.right = right;
            Name = op.Name;
        }

        public Operation AppendOperand(Operator newOp, ExpressionElement newTerm)
        {
            return newOp.PriorityLevel < op.PriorityLevel
                       ? new Operation(left, op,
                                       right is Operation
                                           ? ((Operation)right).AppendOperand(newOp, newTerm)
                                           : new Operation(right, newOp, newTerm))
                       : new Operation(this, newOp, newTerm);
        }

        public override string Source
        {
            get { return "[" + left.Source + op + right.Source + "]"; }
        }

        public override bool CheckSemantic(EvaluationContext context, StringBuilder errMessages)
        {
            var checkL = left.CheckSemantic(context, errMessages);
            var checkR = right.CheckSemantic(context, errMessages);

            var res = checkL && checkR;
            if (!res) return false;

            var leftType = left.GetValueType(context);
            var rightType = right.GetValueType(context);
            Type resultType;
            switch (op.OpType)
            {
                case OperatorType.Boolean:
                    if (leftType != typeof(bool))
                    {
                        errMessages.AppendLine(string.Format(
                            "The left operand of the boolean operation '{0}' is not a boolean value.",
                            op.Name));
                        res = false;
                    }
                    if (rightType != typeof(bool))
                    {
                        errMessages.AppendLine(string.Format(
                            "The right operand of the boolean operation '{0}' is not a boolean value.",
                            op.Name));
                        res = false;
                    }
                    break;
                case OperatorType.Numeric:
                    if (!NumericHelper.IsNumeric(leftType))
                    {
                        errMessages.AppendLine(string.Format(
                            "The left operand of the numeric operation '{0}' is not a numeric value.",
                            op.Name));
                        res = false;
                    }
                    if (!NumericHelper.IsNumeric(rightType))
                    {
                        errMessages.AppendLine(string.Format(
                            "The right operand of the numeric operation '{0}' is not a numeric value.",
                            op.Name));
                        res = false;
                    }
                    if (res && op != Operator.NumPower)
                    {
                        if (!NumericHelper.TryHarmonizeTypes(leftType, rightType, out resultType))
                        {
                            errMessages.AppendLine(string.Format(
                                "The operands of the numeric operation '{0}' are not compatible.",
                                op.Name));
                            res = false;
                        }
                    }
                    break;
                case OperatorType.String:
                    break;
                case OperatorType.Relation:
                    if (leftType == typeof(string))
                    {
                        if (rightType != typeof(string))
                        {
                            errMessages.AppendLine(string.Format(
                                "The left operand of the comparison '{0}' is a string but the right operand is not.",
                                op.Name));
                            res = false;
                        }
                    }
                    else if (NumericHelper.IsNumeric(leftType))
                    {
                        if (!NumericHelper.IsNumeric(rightType))
                        {
                            errMessages.AppendLine(string.Format(
                                "The left operand of the comparison '{0}' is a numeric value but the right operand is not.",
                                op.Name));
                            res = false;
                        }
                        else if (!NumericHelper.TryHarmonizeTypes(leftType, rightType, out resultType))
                        {
                            errMessages.AppendLine(string.Format(
                                "The numeric operands of the comparison '{0}' are not compatible.",
                                op.Name));
                            res = false;
                        }
                    }
                    else if ((op == Operator.RelationEqual || op == Operator.RelationUnequal))
                    {
                        if (leftType == typeof(bool) && rightType != typeof(bool))
                        {
                            errMessages.AppendLine(string.Format(
                                "The left operand of the comparison '{0}' is a boolean value but the right operand is not.",
                                op.Name));
                            res = false;
                        }
                        if (NumericHelper.IsNumeric(leftType) && !NumericHelper.IsNumeric(rightType))
                        {
                            errMessages.AppendLine(string.Format(
                                "The left operand of the comparison '{0}' is a numeric value but the right operand is not.",
                                op.Name));
                            res = false;
                        }
                        if (IsValueType(leftType) && !IsValueType(rightType))
                        {
                            errMessages.AppendLine(string.Format(
                                "The left operand of the comparison '{0}' is a value but the right operand is a reference.",
                                op.Name));
                            res = false;
                        }
                        if (!IsValueType(leftType) && IsValueType(rightType))
                        {
                            errMessages.AppendLine(string.Format(
                                "The left operand of the comparison '{0}' is a reference but the right operand is a value.",
                                op.Name));
                            res = false;
                        }
                    }
                    else
                    {
                        errMessages.AppendLine(string.Format(
                            "The operands are not supported by the comparison operator '{0}'.",
                            op.Name));
                        res = false;
                    }
                    break;
            }
            return res;
        }

        private static bool IsValueType(Type type)
        {
            return type != null && type.IsValueType;
        }

        public override Type GetValueType(EvaluationContext context)
        {
            switch (op.OpType)
            {
                case OperatorType.Numeric:
                    if (op == Operator.NumPower) return typeof(double);
                    var leftType = left.GetValueType(context);
                    var rightType = right.GetValueType(context);
                    Type resultType;
                    if (!NumericHelper.TryHarmonizeTypes(leftType, rightType, out resultType))
                    {
                        throw new InvalidOperationException("The operation is semantically incorrect. No type available.");
                    }
                    return resultType;
                case OperatorType.Relation:
                case OperatorType.Boolean:
                    return typeof(bool);
                case OperatorType.String:
                    return typeof(string);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override object GetValue(EvaluationContext context, object[] parameters)
        {
            var l = left.GetValue(context, parameters);
            var r = right.GetValue(context, parameters);
            var res = op.Function(l, r);
            return res;
        }

        public override Expression GetExpression(EvaluationContext context)
        {
            return op.GetExpression(context, left, right);
        }

        public override IEnumerator<ExpressionElement> GetEnumerator()
        {
            yield return left;
            yield return right;
        }
    }
}