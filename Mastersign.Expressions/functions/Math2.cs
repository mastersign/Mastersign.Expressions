using System;

namespace de.mastersign.expressions.functions
{
    internal static class Math2
    {
        public static Int32 Mod(Int32 a, Int32 b) { return a % b; }
        public static UInt32 Mod(UInt32 a, UInt32 b) { return a % b; }
        public static Int64 Mod(Int64 a, Int64 b) { return a % b; }
        public static UInt64 Mod(UInt64 a, UInt64 b) { return a % b; }
        public static Single Mod(Single a, Single b) { return a % b; }
        public static Double Mod(Double a, Double b) { return a % b; }
        public static Decimal Mod(Decimal a, Decimal b) { return a % b; }
    }
}
