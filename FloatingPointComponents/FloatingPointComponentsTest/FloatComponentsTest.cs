using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemTest.Core.Numerics;

/// <summary>
/// Tests of the <see cref="FloatComponents"/> struct.
/// </summary>
[TestClass]
public class FloatComponentsTest
{
    private const float MaxNegativeValue = -MinPositiveValue;
    private const float MinPositiveValue = float.Epsilon;

    /// <summary>
    /// Performs a series of round robin tests to ensure that the representations can be converted back to their
    /// original values.
    /// </summary>
    [TestMethod]
    public void TestFloatConversion()
    {
        var tests = new[]
        {
            float.NaN,
            float.PositiveInfinity, float.NegativeInfinity,
            float.MaxValue, float.MinValue,
            0.0f, -0.0f,
            1.0f, -1.0f,
            0.5f, -0.5f,
            MinPositiveValue, MaxNegativeValue,
        };

        foreach (var test in tests)
        {
            var testRep = new FloatComponents(test);
            Assert.AreEqual(test, testRep.ToFloat(), $"Float value {test} could not be reproduced.");
        }
    }

    /// <summary>
    /// Ensures that the exponent bias of the <see cref="float"/> type is also the literal exponent of the constant
    /// 1, as is stated in the documentation comments on <see cref="FloatComponents.ExponentBias"/>.
    /// </summary>
    [TestMethod]
    public void TestLiteralUnaryExponent()
    {
        var one = new FloatComponents(1);
        Assert.AreEqual(FloatComponents.ExponentBias, one.LiteralExponent);
    }

    /// <summary>
    /// Tests categorization of various <see cref="float"/> values via the <see cref="FloatComponents"/> type.
    /// </summary>
    [TestMethod]
    public void TestCategorization()
    {
        var tests = new (float Value, bool IsNegative, bool IsNaN, bool IsInfinity, bool IsFinite, bool IsSubnormal, bool IsZero)[]
        {
            (float.NaN, true, true, false, false, false, false),
            (float.PositiveInfinity, false, false, true, false, false, false),
            (float.NegativeInfinity, true, false, true, false, false, false),
            (float.MaxValue, false, false, false, true, false, false),
            (float.MinValue, true, false, false, true, false, false),
            (0.0f, false, false, false, true, false, true),
            (-0.0f, true, false, false, true, false, true),
            (1.0f, false, false, false, true, false, false),
            (-1.0f, true, false, false, true, false, false),
            (MinPositiveValue, false, false, false, true, true, false),
            (MaxNegativeValue, true, false, false, true, true, false),
        };

        foreach (var (Value, IsNegative, IsNaN, IsInfinity, IsFinite, IsSubnormal, IsZero) in tests)
        {
            var testRep = new FloatComponents(Value);

            Assert.AreEqual(IsNegative, testRep.IsNegative, $"{Value} {nameof(FloatComponents.IsNegative)} mismatch.");
            Assert.AreEqual(IsNaN, testRep.IsNaN, $"{Value} {nameof(FloatComponents.IsNaN)} mismatch.");
            Assert.AreEqual(IsInfinity, testRep.IsInfinity, $"{Value} {nameof(FloatComponents.IsInfinity)} mismatch.");
            Assert.AreEqual(IsFinite, testRep.IsFinite, $"{Value} {nameof(FloatComponents.IsFinite)} mismatch.");
            Assert.AreEqual(IsSubnormal, testRep.IsSubnormal, $"{Value} {nameof(FloatComponents.IsSubnormal)} mismatch.");
            Assert.AreEqual(IsZero, testRep.IsZero, $"{Value} {nameof(FloatComponents.IsZero)} mismatch.");
        }
    }

    /// <summary>
    /// Tests the getters for the logical components of <see cref="FloatComponents"/> instances.
    /// </summary>
    [TestMethod]
    public void TestLogicalComponents()
    {
        var tests = new (float Value, int Sign, int Exponent, ulong Mantissa)[]
        {
            (Value: float.NaN,
             Sign: -1,
             Exponent: FloatComponents.MaxLogicalExponent,
             Mantissa: FloatComponents.ImplicitMantissaBit | (1uL << (FloatComponents.MantissaBitLength - 1))),
            (Value: float.PositiveInfinity,
             Sign: 1,
             Exponent: FloatComponents.MaxLogicalExponent,
             Mantissa: FloatComponents.ImplicitMantissaBit),
            (Value: float.NegativeInfinity,
             Sign: -1,
             Exponent: FloatComponents.MaxLogicalExponent,
             Mantissa: FloatComponents.ImplicitMantissaBit),
            (Value: float.MaxValue,
             Sign: 1,
             Exponent: FloatComponents.MaxFiniteLogicalExponent,
             Mantissa: FloatComponents.MaxLogicalMantissa),
            (Value: float.MinValue,
             Sign: -1,
             Exponent: FloatComponents.MaxFiniteLogicalExponent,
             Mantissa: FloatComponents.MaxLogicalMantissa),
            (Value: 0.0f,
             Sign: 1,
             Exponent: FloatComponents.MinLogicalExponent,
             Mantissa: 0),
            (Value: -0.0f,
             Sign: -1,
             Exponent: FloatComponents.MinLogicalExponent,
             Mantissa: 0),
            (Value: MinPositiveValue,
             Sign: 1,
             Exponent: FloatComponents.MinLogicalExponent,
             Mantissa: 1),
            (Value: MaxNegativeValue,
             Sign: -1,
             Exponent: FloatComponents.MinLogicalExponent,
             Mantissa: 1),
        };

        foreach (var (Value, Sign, Exponent, Mantissa) in tests)
        {
            var testRep = new FloatComponents(Value);

            Assert.AreEqual(Sign, testRep.LogicalSign, $"{nameof(FloatComponents.LogicalSign)} mismatch for {Value}.");
            Assert.AreEqual(
                Exponent, testRep.LogicalExponent, $"{nameof(FloatComponents.LogicalExponent)} mismatch for {Value}.");
            Assert.AreEqual(
                Mantissa, testRep.LogicalMantissa,
                $"{nameof(FloatComponents.LogicalMantissa)} mismatch for {Value} "
                    + $"(expected: {Mantissa:X}, actual: {testRep.LogicalMantissa:X}).");
        }
    }

    /// <summary>
    /// Tests the getters for the normalized logical components of <see cref="FloatComponents"/> instances.
    /// </summary>
    [TestMethod]
    public void TestNormalizedLogicalComponents()
    {
        var tests = new (float Value, int Sign, int Exponent, ulong Mantissa)[]
        {
            (Value: float.NaN,
             Sign: -1,
             Exponent: FloatComponents.MaxLogicalExponent + FloatComponents.MantissaBitLength - 1,
             Mantissa: 3),
            (Value: float.PositiveInfinity,
             Sign: 1,
             Exponent: FloatComponents.MaxLogicalExponent + FloatComponents.MantissaBitLength,
             Mantissa: 1),
            (Value: float.NegativeInfinity,
             Sign: -1,
             Exponent: FloatComponents.MaxLogicalExponent + FloatComponents.MantissaBitLength,
             Mantissa: 1),
            (Value: float.MaxValue,
             Sign: 1,
             Exponent: FloatComponents.MaxFiniteLogicalExponent,
             Mantissa: FloatComponents.MaxLogicalMantissa),
            (Value: float.MinValue,
             Sign: -1,
             Exponent: FloatComponents.MaxFiniteLogicalExponent,
             Mantissa: FloatComponents.MaxLogicalMantissa),
            (Value: 0.0f,
             Sign: 1,
             Exponent: 0, // Should be simplified to 0
             Mantissa: 0),
            (Value: -0.0f,
             Sign: -1,
             Exponent: 0, // Should be simplified to 0
             Mantissa: 0),
            (Value: MinPositiveValue,
             Sign: 1,
             Exponent: FloatComponents.MinLogicalExponent,
             Mantissa: 1),
            (Value: MaxNegativeValue,
             Sign: -1,
             Exponent: FloatComponents.MinLogicalExponent,
             Mantissa: 1),
        };

        foreach (var (Value, Sign, Exponent, Mantissa) in tests)
        {
            var testRep = new FloatComponents(Value);

            Assert.AreEqual(Sign, testRep.LogicalSign, $"{nameof(FloatComponents.LogicalSign)} mismatch for {Value}.");
            Assert.AreEqual(
                Exponent, testRep.NormalizedLogicalExponent,
                $"{nameof(FloatComponents.NormalizedLogicalExponent)} mismatch for {Value}.");
            Assert.AreEqual(
                Mantissa, testRep.NormalizedLogicalMantissa,
                $"{nameof(FloatComponents.NormalizedLogicalMantissa)} mismatch for {Value} "
                    + $"(expected: {Mantissa:X}, actual: {testRep.NormalizedLogicalMantissa:X}).");
        }
    }
}
