using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemTest.Core.Numerics;

/// <summary>
/// Tests of the <see cref="FloatRep"/> struct.
/// </summary>
[TestClass]
public class FloatRepTest
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
            var testRep = new FloatRep(test);
            Assert.AreEqual(test, testRep.ToFloat(), $"Float value {test} could not be reproduced.");
        }
    }

    /// <summary>
    /// Tests categorization of various <see cref="float"/> values via the <see cref="FloatRep"/> type.
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
            var testRep = new FloatRep(Value);

            Assert.AreEqual(IsNegative, testRep.IsNegative, $"{Value} {nameof(FloatRep.IsNegative)} mismatch.");
            Assert.AreEqual(IsNaN, testRep.IsNaN, $"{Value} {nameof(FloatRep.IsNaN)} mismatch.");
            Assert.AreEqual(IsInfinity, testRep.IsInfinity, $"{Value} {nameof(FloatRep.IsInfinity)} mismatch.");
            Assert.AreEqual(IsFinite, testRep.IsFinite, $"{Value} {nameof(FloatRep.IsFinite)} mismatch.");
            Assert.AreEqual(IsSubnormal, testRep.IsSubnormal, $"{Value} {nameof(FloatRep.IsSubnormal)} mismatch.");
            Assert.AreEqual(IsZero, testRep.IsZero, $"{Value} {nameof(FloatRep.IsZero)} mismatch.");
        }
    }

    /// <summary>
    /// Tests the getters for the logical components of <see cref="FloatRep"/> instances.
    /// </summary>
    [TestMethod]
    public void TestLogicalComponents()
    {
        var tests = new (float Value, int Sign, int Exponent, ulong Mantissa)[]
        {
            (Value: float.NaN,
             Sign: -1,
             Exponent: FloatRep.MaxLogicalExponent,
             Mantissa: FloatRep.ImplicitMantissaBit | (1uL << (FloatRep.MantissaBitLength - 1))),
            (Value: float.PositiveInfinity,
             Sign: 1,
             Exponent: FloatRep.MaxLogicalExponent,
             Mantissa: FloatRep.ImplicitMantissaBit),
            (Value: float.NegativeInfinity,
             Sign: -1,
             Exponent: FloatRep.MaxLogicalExponent,
             Mantissa: FloatRep.ImplicitMantissaBit),
            (Value: float.MaxValue,
             Sign: 1,
             Exponent: FloatRep.MaxFiniteLogicalExponent,
             Mantissa: FloatRep.MaxLogicalMantissa),
            (Value: float.MinValue,
             Sign: -1,
             Exponent: FloatRep.MaxFiniteLogicalExponent,
             Mantissa: FloatRep.MaxLogicalMantissa),
            (Value: 0.0f,
             Sign: 1,
             Exponent: FloatRep.MinLogicalExponent,
             Mantissa: 0),
            (Value: -0.0f,
             Sign: -1,
             Exponent: FloatRep.MinLogicalExponent,
             Mantissa: 0),
            (Value: MinPositiveValue,
             Sign: 1,
             Exponent: FloatRep.MinLogicalExponent,
             Mantissa: 1),
            (Value: MaxNegativeValue,
             Sign: -1,
             Exponent: FloatRep.MinLogicalExponent,
             Mantissa: 1),
        };

        foreach (var (Value, Sign, Exponent, Mantissa) in tests)
        {
            var testRep = new FloatRep(Value);

            Assert.AreEqual(Sign, testRep.LogicalSign, $"{nameof(FloatRep.LogicalSign)} mismatch for {Value}.");
            Assert.AreEqual(
                Exponent, testRep.LogicalExponent, $"{nameof(FloatRep.LogicalExponent)} mismatch for {Value}.");
            Assert.AreEqual(
                Mantissa, testRep.LogicalMantissa,
                $"{nameof(FloatRep.LogicalMantissa)} mismatch for {Value} "
                    + $"(expected: {Mantissa:X}, actual: {testRep.LogicalMantissa:X}).");
        }
    }

    /// <summary>
    /// Tests the getters for the normalized logical components of <see cref="FloatRep"/> instances.
    /// </summary>
    [TestMethod]
    public void TestNormalizedLogicalComponents()
    {
        var tests = new (float Value, int Sign, int Exponent, ulong Mantissa)[]
        {
            (Value: float.NaN,
             Sign: -1,
             Exponent: FloatRep.MaxLogicalExponent + FloatRep.MantissaBitLength - 1,
             Mantissa: 3),
            (Value: float.PositiveInfinity,
             Sign: 1,
             Exponent: FloatRep.MaxLogicalExponent + FloatRep.MantissaBitLength,
             Mantissa: 1),
            (Value: float.NegativeInfinity,
             Sign: -1,
             Exponent: FloatRep.MaxLogicalExponent + FloatRep.MantissaBitLength,
             Mantissa: 1),
            (Value: float.MaxValue,
             Sign: 1,
             Exponent: FloatRep.MaxFiniteLogicalExponent,
             Mantissa: FloatRep.MaxLogicalMantissa),
            (Value: float.MinValue,
             Sign: -1,
             Exponent: FloatRep.MaxFiniteLogicalExponent,
             Mantissa: FloatRep.MaxLogicalMantissa),
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
             Exponent: FloatRep.MinLogicalExponent,
             Mantissa: 1),
            (Value: MaxNegativeValue,
             Sign: -1,
             Exponent: FloatRep.MinLogicalExponent,
             Mantissa: 1),
        };

        foreach (var (Value, Sign, Exponent, Mantissa) in tests)
        {
            var testRep = new FloatRep(Value);

            Assert.AreEqual(Sign, testRep.LogicalSign, $"{nameof(FloatRep.LogicalSign)} mismatch for {Value}.");
            Assert.AreEqual(
                Exponent, testRep.NormalizedLogicalExponent,
                $"{nameof(FloatRep.NormalizedLogicalExponent)} mismatch for {Value}.");
            Assert.AreEqual(
                Mantissa, testRep.NormalizedLogicalMantissa,
                $"{nameof(FloatRep.NormalizedLogicalMantissa)} mismatch for {Value} "
                    + $"(expected: {Mantissa:X}, actual: {testRep.NormalizedLogicalMantissa:X}).");
        }
    }
}
