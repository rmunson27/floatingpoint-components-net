using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemTest.Core.Numerics;

/// <summary>
/// Tests of the <see cref="DoubleComponents"/> struct.
/// </summary>
[TestClass]
public class DoubleComponentsTest
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
            var testRep = new DoubleComponents(test);
            Assert.AreEqual(test, testRep.ToDouble(), $"Double value {test} could not be reproduced.");
        }
    }

    /// <summary>
    /// Ensures that the exponent bias of the <see cref="double"/> type is also the literal exponent of the constant
    /// 1, as is stated in the documentation comments on <see cref="DoubleComponents.ExponentBias"/>.
    /// </summary>
    [TestMethod]
    public void TestLiteralUnaryExponent()
    {
        var one = new DoubleComponents(1);
        Assert.AreEqual(DoubleComponents.ExponentBias, one.LiteralExponent);
    }

    /// <summary>
    /// Tests categorization of various <see cref="double"/> values via the <see cref="DoubleComponents"/> type.
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
            var testRep = new DoubleComponents(Value);

            Assert.AreEqual(IsNegative, testRep.IsNegative, $"{Value} {nameof(DoubleComponents.IsNegative)} mismatch.");
            Assert.AreEqual(IsNaN, testRep.IsNaN, $"{Value} {nameof(DoubleComponents.IsNaN)} mismatch.");
            Assert.AreEqual(IsInfinity, testRep.IsInfinity, $"{Value} {nameof(DoubleComponents.IsInfinity)} mismatch.");
            Assert.AreEqual(IsFinite, testRep.IsFinite, $"{Value} {nameof(DoubleComponents.IsFinite)} mismatch.");
            Assert.AreEqual(IsSubnormal, testRep.IsSubnormal, $"{Value} {nameof(DoubleComponents.IsSubnormal)} mismatch.");
            Assert.AreEqual(IsZero, testRep.IsZero, $"{Value} {nameof(DoubleComponents.IsZero)} mismatch.");
        }
    }

    /// <summary>
    /// Tests the getters for the logical components of <see cref="DoubleComponents"/> instances.
    /// </summary>
    [TestMethod]
    public void TestLogicalComponents()
    {
        var tests = new (double Value, int Sign, int Exponent, ulong Mantissa)[]
        {
            (Value: double.NaN,
             Sign: -1,
             Exponent: DoubleComponents.MaxLogicalExponent,
             Mantissa: DoubleComponents.ImplicitMantissaBit | (1uL << (DoubleComponents.MantissaBitLength - 1))),
            (Value: double.PositiveInfinity,
             Sign: 1,
             Exponent: DoubleComponents.MaxLogicalExponent,
             Mantissa: DoubleComponents.ImplicitMantissaBit),
            (Value: double.NegativeInfinity,
             Sign: -1,
             Exponent: DoubleComponents.MaxLogicalExponent,
             Mantissa: DoubleComponents.ImplicitMantissaBit),
            (Value: double.MaxValue,
             Sign: 1,
             Exponent: DoubleComponents.MaxFiniteLogicalExponent,
             Mantissa: DoubleComponents.MaxLogicalMantissa),
            (Value: double.MinValue,
             Sign: -1,
             Exponent: DoubleComponents.MaxFiniteLogicalExponent,
             Mantissa: DoubleComponents.MaxLogicalMantissa),
            (Value: 0.0,
             Sign: 1,
             Exponent: DoubleComponents.MinLogicalExponent,
             Mantissa: 0),
            (Value: -0.0,
             Sign: -1,
             Exponent: DoubleComponents.MinLogicalExponent,
             Mantissa: 0),
            (Value: MinPositiveValue,
             Sign: 1,
             Exponent: DoubleComponents.MinLogicalExponent,
             Mantissa: 1),
            (Value: MaxNegativeValue,
             Sign: -1,
             Exponent: DoubleComponents.MinLogicalExponent,
             Mantissa: 1),
        };

        foreach (var (Value, Sign, Exponent, Mantissa) in tests)
        {
            var testRep = new DoubleComponents(Value);

            Assert.AreEqual(Sign, testRep.LogicalSign, $"{nameof(DoubleComponents.LogicalSign)} mismatch for {Value}.");
            Assert.AreEqual(
                Exponent, testRep.LogicalExponent, $"{nameof(DoubleComponents.LogicalExponent)} mismatch for {Value}.");
            Assert.AreEqual(
                Mantissa, testRep.LogicalMantissa,
                $"{nameof(DoubleComponents.LogicalMantissa)} mismatch for {Value} "
                    + $"(expected: {Mantissa:X}, actual: {testRep.LogicalMantissa:X}).");
        }
    }

    /// <summary>
    /// Tests the getters for the normalized logical components of <see cref="DoubleComponents"/> instances.
    /// </summary>
    [TestMethod]
    public void TestNormalizedLogicalComponents()
    {
        var tests = new (double Value, int Sign, int Exponent, ulong Mantissa)[]
        {
            (Value: double.NaN,
             Sign: -1,
             Exponent: DoubleComponents.MaxLogicalExponent + DoubleComponents.MantissaBitLength - 1,
             Mantissa: 3),
            (Value: double.PositiveInfinity,
             Sign: 1,
             Exponent: DoubleComponents.MaxLogicalExponent + DoubleComponents.MantissaBitLength,
             Mantissa: 1),
            (Value: double.NegativeInfinity,
             Sign: -1,
             Exponent: DoubleComponents.MaxLogicalExponent + DoubleComponents.MantissaBitLength,
             Mantissa: 1),
            (Value: double.MaxValue,
             Sign: 1,
             Exponent: DoubleComponents.MaxFiniteLogicalExponent,
             Mantissa: DoubleComponents.MaxLogicalMantissa),
            (Value: double.MinValue,
             Sign: -1,
             Exponent: DoubleComponents.MaxFiniteLogicalExponent,
             Mantissa: DoubleComponents.MaxLogicalMantissa),
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
             Exponent: DoubleComponents.MinLogicalExponent,
             Mantissa: 1),
            (Value: MaxNegativeValue,
             Sign: -1,
             Exponent: DoubleComponents.MinLogicalExponent,
             Mantissa: 1),
        };

        foreach (var (Value, Sign, Exponent, Mantissa) in tests)
        {
            var testRep = new DoubleComponents(Value);

            Assert.AreEqual(Sign, testRep.LogicalSign, $"{nameof(DoubleComponents.LogicalSign)} mismatch for {Value}.");
            Assert.AreEqual(
                Exponent, testRep.NormalizedLogicalExponent,
                $"{nameof(DoubleComponents.NormalizedLogicalExponent)} mismatch for {Value}.");
            Assert.AreEqual(
                Mantissa, testRep.NormalizedLogicalMantissa,
                $"{nameof(DoubleComponents.NormalizedLogicalMantissa)} mismatch for {Value} "
                    + $"(expected: {Mantissa:X}, actual: {testRep.NormalizedLogicalMantissa:X}).");
        }
    }
}
