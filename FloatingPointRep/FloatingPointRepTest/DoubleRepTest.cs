using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemTest.Core.Numerics;

/// <summary>
/// Tests of the <see cref="DoubleRep"/> struct.
/// </summary>
[TestClass]
public class DoubleRepTest
{
    private const double MaxNegativeValue = -MinPositiveValue;
    private const double MinPositiveValue = double.Epsilon;

    /// <summary>
    /// Performs a series of round robin tests to ensure that the representations can be converted back to their
    /// original values.
    /// </summary>
    [TestMethod]
    public void TestDoubleConversion()
    {
        var tests = new[]
        {
            double.NaN,
            double.PositiveInfinity, double.NegativeInfinity,
            double.MaxValue, double.MinValue,
            0.0, -0.0,
            1.0, -1.0,
            0.5, -0.5,
            MinPositiveValue, MaxNegativeValue,
        };

        foreach (var test in tests)
        {
            var testRep = new DoubleRep(test);
            Assert.AreEqual(test, testRep.ToDouble(), $"Double value {test} could not be reproduced.");
        }
    }

    /// <summary>
    /// Tests categorization of various <see cref="double"/> values via the <see cref="DoubleRep"/> type.
    /// </summary>
    [TestMethod]
    public void TestCategorization()
    {
        var tests = new (double Value, bool IsNegative, bool IsNaN, bool IsInfinity, bool IsFinite, bool IsSubnormal, bool IsZero)[]
        {
            (double.NaN, true, true, false, false, false, false),
            (double.PositiveInfinity, false, false, true, false, false, false),
            (double.NegativeInfinity, true, false, true, false, false, false),
            (double.MaxValue, false, false, false, true, false, false),
            (double.MinValue, true, false, false, true, false, false),
            (0.0, false, false, false, true, false, true),
            (-0.0, true, false, false, true, false, true),
            (1.0, false, false, false, true, false, false),
            (-1.0, true, false, false, true, false, false),
            (MinPositiveValue, false, false, false, true, true, false),
            (MaxNegativeValue, true, false, false, true, true, false),
        };

        foreach (var (Value, IsNegative, IsNaN, IsInfinity, IsFinite, IsSubnormal, IsZero) in tests)
        {
            var testRep = new DoubleRep(Value);

            Assert.AreEqual(IsNegative, testRep.IsNegative, $"{Value} {nameof(DoubleRep.IsNegative)} mismatch.");
            Assert.AreEqual(IsNaN, testRep.IsNaN, $"{Value} {nameof(DoubleRep.IsNaN)} mismatch.");
            Assert.AreEqual(IsInfinity, testRep.IsInfinity, $"{Value} {nameof(DoubleRep.IsInfinity)} mismatch.");
            Assert.AreEqual(IsFinite, testRep.IsFinite, $"{Value} {nameof(DoubleRep.IsFinite)} mismatch.");
            Assert.AreEqual(IsSubnormal, testRep.IsSubnormal, $"{Value} {nameof(DoubleRep.IsSubnormal)} mismatch.");
            Assert.AreEqual(IsZero, testRep.IsZero, $"{Value} {nameof(DoubleRep.IsZero)} mismatch.");
        }
    }

    /// <summary>
    /// Tests the getters for the logical components of <see cref="DoubleRep"/> instances.
    /// </summary>
    [TestMethod]
    public void TestLogicalComponents()
    {
        var tests = new (double Value, int Sign, int Exponent, ulong Mantissa)[]
        {
            (Value: double.NaN,
             Sign: -1,
             Exponent: DoubleRep.MaxLogicalExponent,
             Mantissa: DoubleRep.ImplicitMantissaBit | (1uL << (DoubleRep.MantissaBitLength - 1))),
            (Value: double.PositiveInfinity,
             Sign: 1,
             Exponent: DoubleRep.MaxLogicalExponent,
             Mantissa: DoubleRep.ImplicitMantissaBit),
            (Value: double.NegativeInfinity,
             Sign: -1,
             Exponent: DoubleRep.MaxLogicalExponent,
             Mantissa: DoubleRep.ImplicitMantissaBit),
            (Value: double.MaxValue,
             Sign: 1,
             Exponent: DoubleRep.MaxFiniteLogicalExponent,
             Mantissa: DoubleRep.MaxLogicalMantissa),
            (Value: double.MinValue,
             Sign: -1,
             Exponent: DoubleRep.MaxFiniteLogicalExponent,
             Mantissa: DoubleRep.MaxLogicalMantissa),
            (Value: 0.0,
             Sign: 1,
             Exponent: DoubleRep.MinLogicalExponent,
             Mantissa: 0),
            (Value: -0.0,
             Sign: -1,
             Exponent: DoubleRep.MinLogicalExponent,
             Mantissa: 0),
            (Value: MinPositiveValue,
             Sign: 1,
             Exponent: DoubleRep.MinLogicalExponent,
             Mantissa: 1),
            (Value: MaxNegativeValue,
             Sign: -1,
             Exponent: DoubleRep.MinLogicalExponent,
             Mantissa: 1),
        };

        foreach (var (Value, Sign, Exponent, Mantissa) in tests)
        {
            var testRep = new DoubleRep(Value);

            Assert.AreEqual(Sign, testRep.LogicalSign, $"{nameof(DoubleRep.LogicalSign)} mismatch for {Value}.");
            Assert.AreEqual(
                Exponent, testRep.LogicalExponent, $"{nameof(DoubleRep.LogicalExponent)} mismatch for {Value}.");
            Assert.AreEqual(
                Mantissa, testRep.LogicalMantissa,
                $"{nameof(DoubleRep.LogicalMantissa)} mismatch for {Value} "
                    + $"(expected: {Mantissa:X}, actual: {testRep.LogicalMantissa:X}).");
        }
    }

    /// <summary>
    /// Tests the getters for the normalized logical components of <see cref="DoubleRep"/> instances.
    /// </summary>
    [TestMethod]
    public void TestNormalizedLogicalComponents()
    {
        var tests = new (double Value, int Sign, int Exponent, ulong Mantissa)[]
        {
            (Value: double.NaN,
             Sign: -1,
             Exponent: DoubleRep.MaxLogicalExponent + DoubleRep.MantissaBitLength - 1,
             Mantissa: 3),
            (Value: double.PositiveInfinity,
             Sign: 1,
             Exponent: DoubleRep.MaxLogicalExponent + DoubleRep.MantissaBitLength,
             Mantissa: 1),
            (Value: double.NegativeInfinity,
             Sign: -1,
             Exponent: DoubleRep.MaxLogicalExponent + DoubleRep.MantissaBitLength,
             Mantissa: 1),
            (Value: double.MaxValue,
             Sign: 1,
             Exponent: DoubleRep.MaxFiniteLogicalExponent,
             Mantissa: DoubleRep.MaxLogicalMantissa),
            (Value: double.MinValue,
             Sign: -1,
             Exponent: DoubleRep.MaxFiniteLogicalExponent,
             Mantissa: DoubleRep.MaxLogicalMantissa),
            (Value: 0.0,
             Sign: 1,
             Exponent: 0, // Should be simplified to 0
             Mantissa: 0),
            (Value: -0.0,
             Sign: -1,
             Exponent: 0, // Should be simplified to 0
             Mantissa: 0),
            (Value: MinPositiveValue,
             Sign: 1,
             Exponent: DoubleRep.MinLogicalExponent,
             Mantissa: 1),
            (Value: MaxNegativeValue,
             Sign: -1,
             Exponent: DoubleRep.MinLogicalExponent,
             Mantissa: 1),
        };

        foreach (var (Value, Sign, Exponent, Mantissa) in tests)
        {
            var testRep = new DoubleRep(Value);

            Assert.AreEqual(Sign, testRep.LogicalSign, $"{nameof(DoubleRep.LogicalSign)} mismatch for {Value}.");
            Assert.AreEqual(
                Exponent, testRep.NormalizedLogicalExponent,
                $"{nameof(DoubleRep.NormalizedLogicalExponent)} mismatch for {Value}.");
            Assert.AreEqual(
                Mantissa, testRep.NormalizedLogicalMantissa,
                $"{nameof(DoubleRep.NormalizedLogicalMantissa)} mismatch for {Value} "
                    + $"(expected: {Mantissa:X}, actual: {testRep.NormalizedLogicalMantissa:X}).");
        }
    }
}
