using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mastersign.Expressions.Language
{
    static class NumericHelper
    {
        public static bool IsNumeric(Type type)
        {
            if (type == null) return false;
            if (!type.IsValueType) return false;
            return
                type == typeof(SByte) ||
                type == typeof(Byte) ||
                type == typeof(Int16) ||
                type == typeof(UInt16) ||
                type == typeof(Int32) ||
                type == typeof(UInt32) ||
                type == typeof(Int64) ||
                type == typeof(UInt64) ||
                type == typeof(Single) ||
                type == typeof(Double) ||
                type == typeof(Decimal);
        }

        public static bool NeedsAutoUpgradeToInt32(Type type)
        {
            return type == typeof (SByte) ||
                   type == typeof (Byte) ||
                   type == typeof (Int16) ||
                   type == typeof (UInt16);
        }

        public static Type AutoUpgradeNumericType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (!IsNumeric(type)) throw new ArgumentException("The given value is not numeric.", "value");
            return NeedsAutoUpgradeToInt32(type) ? typeof (Int32) : type;
        }

        public static object AutoUpgradeNumericValue(object value)
        {
            if (value == null)throw new ArgumentNullException("value");
            var type = value.GetType();
            if (!IsNumeric(type)) throw new ArgumentException("The given value is not numeric.", "value");
            return NeedsAutoUpgradeToInt32(type) ? Convert.ToInt32(value) : value;
        }

        public static bool IsUpgradable(Type sourceType, Type targetType)
        {
            if (sourceType == null) throw new ArgumentNullException("sourceType");
            if (targetType == null) throw new ArgumentNullException("targetType");
            if (!IsNumeric(sourceType)) throw new ArgumentException("The given type is not numeric.", "sourceType");
            if (!IsNumeric(targetType)) throw new ArgumentException("The given type is not numeric.", "targetType");
            if (targetType == typeof (Int16))
            {
                return sourceType == typeof (SByte) ||
                       sourceType == typeof (Byte) ||
                       sourceType == typeof(Int16);
            }
            if (targetType == typeof (UInt16))
            {
                return sourceType == typeof (Byte) ||
                       sourceType == typeof (UInt16);
            }
            if (targetType == typeof (Int32))
            {
                return sourceType == typeof (SByte) ||
                       sourceType == typeof (Byte) ||
                       sourceType == typeof (Int16) ||
                       sourceType == typeof (UInt16) ||
                       sourceType == typeof (Int32);
            }
            if (targetType == typeof (UInt32))
            {
                return sourceType == typeof (Byte) ||
                       sourceType == typeof (UInt16) ||
                       sourceType == typeof (UInt32);
            }
            if (targetType == typeof (Int64))
            {
                return sourceType == typeof(SByte) ||
                       sourceType == typeof(Byte) ||
                       sourceType == typeof(Int16) ||
                       sourceType == typeof(UInt16) ||
                       sourceType == typeof(Int32) ||
                       sourceType == typeof(UInt32) ||
                       sourceType == typeof(Int64);
            }
            if (targetType == typeof (UInt64))
            {
                return sourceType == typeof(Byte) ||
                       sourceType == typeof(UInt16) ||
                       sourceType == typeof(UInt32) ||
                       sourceType == typeof(UInt64);
            }
            if (targetType == typeof (Single))
            {
                return sourceType == typeof(SByte) ||
                       sourceType == typeof(Byte) ||
                       sourceType == typeof(Int16) ||
                       sourceType == typeof(UInt16) ||
                       sourceType == typeof(Int32) ||
                       sourceType == typeof(UInt32) ||
                       sourceType == typeof(Int64) ||
                       sourceType == typeof(UInt64);
            }
            if (targetType == typeof (Double))
            {
                return sourceType == typeof(SByte) ||
                       sourceType == typeof(Byte) ||
                       sourceType == typeof(Int16) ||
                       sourceType == typeof(UInt16) ||
                       sourceType == typeof(Int32) ||
                       sourceType == typeof(UInt32) ||
                       sourceType == typeof(Int64) ||
                       sourceType == typeof(UInt64) ||
                       sourceType == typeof(Single);
            }
            if (targetType == typeof (Decimal))
            {
                return sourceType == typeof(SByte) ||
                       sourceType == typeof(Byte) ||
                       sourceType == typeof(Int16) ||
                       sourceType == typeof(UInt16) ||
                       sourceType == typeof(Int32) ||
                       sourceType == typeof(UInt32) ||
                       sourceType == typeof(Int64) ||
                       sourceType == typeof(UInt64);
            }
            return false;
        }

        public static object Upgrade(object value, Type targetType)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (targetType == null) throw new ArgumentNullException("targetType");
            var from = value.GetType();
            if (!IsNumeric(from)) throw new ArgumentException("The given value is not numeric.", "value");
            if (!IsNumeric(targetType)) throw new ArgumentException("The given target type is not numeric.", "targetType");
            return Convert.ChangeType(value, targetType);
        }

        public static bool TryHarmonizeTypes(Type t1, Type t2, out Type t)
        {
            if (t1 == null) throw new ArgumentNullException("t1");
            if (t2 == null) throw new ArgumentNullException("t2");
            if (!IsNumeric(t1)) throw new ArgumentException("The given type is not numeric.", "t1");
            if (!IsNumeric(t2)) throw new ArgumentException("The given type is not numeric.", "t2");
            if (t1 == t2)
            {
                t = t1;
                return true;
            }
            if (IsUpgradable(t1, t2))
            {
                t = t2;
                return true;
            }
            if (IsUpgradable(t2, t1))
            {
                t = t1;
                return true;
            }
            t = null;
            return false;
        }

        public static Type HarmonizeValues(object src1, object src2, out object res1, out object res2)
        {
            if (src1 == null) throw new ArgumentNullException("src1");
            if (src2 == null) throw new ArgumentNullException("src2");
            var t1 = src1.GetType();
            var t2 = src2.GetType();
            if (!IsNumeric(t1)) throw new ArgumentException("The given value is not numeric.", "src1");
            if (!IsNumeric(t2)) throw new ArgumentException("The given value is not numeric.", "src2");
            Type t;
            if (TryHarmonizeTypes(t1, t2, out t))
            {
                res1 = t != t1 ? Convert.ChangeType(src1, t) : src1;
                res2 = t != t2 ? Convert.ChangeType(src2, t) : src2;
                return t;
            }
            throw new ArgumentException("The values are incompatible.");
        }
    }
}
