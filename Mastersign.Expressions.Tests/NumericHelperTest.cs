using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace de.mastersign.expressions.language
{
    internal class NumericHelperTest
    {
        [Test]
        public void AutoUpgradeNumericTypeTest()
        {
            Assert.AreEqual(typeof(Int32), NumericHelper.AutoUpgradeNumericType(typeof(SByte)));
            Assert.AreEqual(typeof(Int32), NumericHelper.AutoUpgradeNumericType(typeof(Byte)));
            Assert.AreEqual(typeof(Int32), NumericHelper.AutoUpgradeNumericType(typeof(Int16)));
            Assert.AreEqual(typeof(Int32), NumericHelper.AutoUpgradeNumericType(typeof(UInt16)));
            Assert.AreEqual(typeof(Int32), NumericHelper.AutoUpgradeNumericType(typeof(Int32)));
            Assert.AreEqual(typeof(UInt32), NumericHelper.AutoUpgradeNumericType(typeof(UInt32)));
            Assert.AreEqual(typeof(Int64), NumericHelper.AutoUpgradeNumericType(typeof(Int64)));
            Assert.AreEqual(typeof(UInt64), NumericHelper.AutoUpgradeNumericType(typeof(UInt64)));
            Assert.AreEqual(typeof(Single), NumericHelper.AutoUpgradeNumericType(typeof(Single)));
            Assert.AreEqual(typeof(Double), NumericHelper.AutoUpgradeNumericType(typeof(Double)));
            Assert.AreEqual(typeof(Decimal), NumericHelper.AutoUpgradeNumericType(typeof(Decimal)));
        }

        [Test]
        public void AutoUpgradeNumericValueTest()
        {
            Assert.AreEqual(100, NumericHelper.AutoUpgradeNumericValue((sbyte)100));
            Assert.AreEqual(100, NumericHelper.AutoUpgradeNumericValue((byte)100));
            Assert.AreEqual(100, NumericHelper.AutoUpgradeNumericValue((short)100));
            Assert.AreEqual(100, NumericHelper.AutoUpgradeNumericValue((ushort)100));
            Assert.AreEqual(100, NumericHelper.AutoUpgradeNumericValue(100));
            Assert.AreEqual(100u, NumericHelper.AutoUpgradeNumericValue((uint)100));
            Assert.AreEqual(100L, NumericHelper.AutoUpgradeNumericValue((long)100));
            Assert.AreEqual(100UL, NumericHelper.AutoUpgradeNumericValue((ulong)100));
            Assert.AreEqual(100f, NumericHelper.AutoUpgradeNumericValue(100f));
            Assert.AreEqual(100.0, NumericHelper.AutoUpgradeNumericValue(100.0));
            Assert.AreEqual(100D, NumericHelper.AutoUpgradeNumericValue(100D));
        }

        [Test]
        public void TryHarmonizeTypesTest()
        {
            Type resT;
            Assert.IsTrue(NumericHelper.TryHarmonizeTypes(typeof(int), typeof(long), out resT));
            Assert.AreEqual(typeof(long), resT);
            Assert.IsTrue(NumericHelper.TryHarmonizeTypes(typeof(long), typeof(int), out resT));
            Assert.AreEqual(typeof(long), resT);

            Assert.IsTrue(NumericHelper.TryHarmonizeTypes(typeof(int), typeof(float), out resT));
            Assert.AreEqual(typeof(float), resT);
            Assert.IsTrue(NumericHelper.TryHarmonizeTypes(typeof(float), typeof(int), out resT));
            Assert.AreEqual(typeof(float), resT);

            Assert.IsTrue(NumericHelper.TryHarmonizeTypes(typeof(int), typeof(double), out resT));
            Assert.AreEqual(typeof(double), resT);
            Assert.IsTrue(NumericHelper.TryHarmonizeTypes(typeof(double), typeof(int), out resT));
            Assert.AreEqual(typeof(double), resT);

            Assert.IsTrue(NumericHelper.TryHarmonizeTypes(typeof(byte), typeof(long), out resT));
            Assert.AreEqual(typeof(long), resT);
            Assert.IsTrue(NumericHelper.TryHarmonizeTypes(typeof(long), typeof(byte), out resT));
            Assert.AreEqual(typeof(long), resT);

            Assert.IsTrue(NumericHelper.TryHarmonizeTypes(typeof(byte), typeof(double), out resT));
            Assert.AreEqual(typeof(double), resT);
            Assert.IsTrue(NumericHelper.TryHarmonizeTypes(typeof(double), typeof(byte), out resT));
            Assert.AreEqual(typeof(double), resT);

            Assert.IsTrue(NumericHelper.TryHarmonizeTypes(typeof(int), typeof(decimal), out resT));
            Assert.AreEqual(typeof(decimal), resT);
            Assert.IsTrue(NumericHelper.TryHarmonizeTypes(typeof(decimal), typeof(int), out resT));
            Assert.AreEqual(typeof(decimal), resT);

            Assert.IsTrue(NumericHelper.TryHarmonizeTypes(typeof(long), typeof(decimal), out resT));
            Assert.AreEqual(typeof(decimal), resT);
            Assert.IsTrue(NumericHelper.TryHarmonizeTypes(typeof(decimal), typeof(long), out resT));
            Assert.AreEqual(typeof(decimal), resT);

            Assert.IsFalse(NumericHelper.TryHarmonizeTypes(typeof(float), typeof(decimal), out resT));
            Assert.IsNull(resT);
            Assert.IsFalse(NumericHelper.TryHarmonizeTypes(typeof(decimal), typeof(float), out resT));
            Assert.IsNull(resT);

            Assert.IsFalse(NumericHelper.TryHarmonizeTypes(typeof(double), typeof(decimal), out resT));
            Assert.IsNull(resT);
            Assert.IsFalse(NumericHelper.TryHarmonizeTypes(typeof(decimal), typeof(double), out resT));
            Assert.IsNull(resT);
        }

        [Test]
        public void HarmonizeValuesTest()
        {
            object res1, res2;
            Assert.AreEqual(typeof(int), NumericHelper.HarmonizeValues(2, 3, out res1, out res2));
            Assert.AreEqual(typeof(long), NumericHelper.HarmonizeValues(2L, 3L, out res1, out res2));
            Assert.AreEqual(typeof(float), NumericHelper.HarmonizeValues(2f, 3f, out res1, out res2));
            Assert.AreEqual(typeof(double), NumericHelper.HarmonizeValues(2.0, 3.0, out res1, out res2));
            Assert.AreEqual(typeof(decimal), NumericHelper.HarmonizeValues(2M, 3M, out res1, out res2));

            Assert.AreEqual(typeof(long), NumericHelper.HarmonizeValues((byte)100, 200L, out res1, out res2));
            Assert.AreEqual(typeof(long), res1.GetType());
            Assert.AreEqual(typeof(long), res2.GetType());
            Assert.AreEqual((long)res1, 100L);
            Assert.AreEqual((long)res2, 200L);
        }
    }
}
