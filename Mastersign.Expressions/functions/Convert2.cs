using System;
using System.Globalization;

namespace Mastersign.Expressions.Functions
{
    internal static class Convert2
    {
        public static Byte ParseByte(string str) { return Byte.Parse(str, CultureInfo.InvariantCulture); }
        public static SByte ParseSByte(string str) { return SByte.Parse(str, CultureInfo.InvariantCulture); }
        public static Int16 ParseInt16(string str) { return Int16.Parse(str, CultureInfo.InvariantCulture); }
        public static UInt16 ParseUInt16(string str) { return UInt16.Parse(str, CultureInfo.InvariantCulture); }
        public static Int32 ParseInt32(string str) { return Int32.Parse(str, CultureInfo.InvariantCulture); }
        public static UInt32 ParseUInt32(string str) { return UInt32.Parse(str, CultureInfo.InvariantCulture); }
        public static Int64 ParseInt64(string str) { return Int64.Parse(str, CultureInfo.InvariantCulture); }
        public static UInt64 ParseUInt64(string str) { return UInt64.Parse(str, CultureInfo.InvariantCulture); }
        public static Single ParseSingle(string str) { return Single.Parse(str, CultureInfo.InvariantCulture); }
        public static Double ParseDouble(string str) { return Double.Parse(str, CultureInfo.InvariantCulture); }
        public static Decimal ParseDecimal(string str) { return Decimal.Parse(str, CultureInfo.InvariantCulture); }

        public static string ByteToString(Byte v) { return v.ToString(CultureInfo.InvariantCulture); }
        public static string SByteToString(SByte v) { return v.ToString(CultureInfo.InvariantCulture); }
        public static string Int16ToString(Int16 v) { return v.ToString(CultureInfo.InvariantCulture); }
        public static string UInt16ToString(UInt16 v) { return v.ToString(CultureInfo.InvariantCulture); }
        public static string Int32ToString(Int32 v) { return v.ToString(CultureInfo.InvariantCulture); }
        public static string UInt32ToString(UInt32 v) { return v.ToString(CultureInfo.InvariantCulture); }
        public static string Int64ToString(Int64 v) { return v.ToString(CultureInfo.InvariantCulture); }
        public static string UInt64ToString(UInt64 v) { return v.ToString(CultureInfo.InvariantCulture); }
        public static string SingleToString(Single v) { return v.ToString(CultureInfo.InvariantCulture); }
        public static string DoubleToString(Double v) { return v.ToString(CultureInfo.InvariantCulture); }
        public static string DecimalToString(Decimal v) { return v.ToString(CultureInfo.InvariantCulture); }
        public static string BooleanToString(bool v) { return v.ToString(CultureInfo.InvariantCulture); }
        public static string DateTimeToString(DateTime v) { return v.ToString(CultureInfo.InvariantCulture); }
        public static string StringToString(string v) { return v; }
        public static string ObjectToString(object v) { return v.ToString(); }
    }
}
