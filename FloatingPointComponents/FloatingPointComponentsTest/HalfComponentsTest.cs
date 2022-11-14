using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemTest.Core.Numerics;

#if NET5_0_OR_GREATER
/// <summary>
/// Tests of the <see cref="HalfComponents"/> struct.
/// </summary>
[TestClass]
public class HalfComponentsTest
{
    private static readonly Half MinPositiveValue = Half.Epsilon;
    private static readonly Half MaxNegativeValue = (Half)(-(float)MinPositiveValue);

    /// <summary>
    /// Performs a series of round robin tests to ensure that the representations can be converted back to their
    /// original values.
    /// </summary>
    [TestMethod]
    public void TestHalfConversion()
    {
        var tests = new[]
        {
            Half.NaN,
            Half.PositiveInfinity, Half.NegativeInfinity,
            Half.MaxValue, Half.MinValue,
            (Half)0.0, (Half)(-0.0),
            (Half)1.0, (Half)(-1.0),
            (Half)0.5, (Half)(-0.5),
            MinPositiveValue, MaxNegativeValue,
        };

        foreach (var test in tests)
        {
            var testRep = new HalfComponents(test);
            Assert.AreEqual(test, testRep.ToHalf(), $"Half value {test} could not be reproduced.");
        }
    }

    /// <summary>
    /// Ensures that the exponent bias of the <see cref="Half"/> type is also the literal exponent of the constant
    /// 1, as is stated in the documentation comments on <see cref="HalfComponents.ExponentBias"/>.
    /// </summary>
    [TestMethod]
    public void TestLiteralUnaryExponent()
    {
        var one = new HalfComponents((Half)1);
        Assert.AreEqual(HalfComponents.ExponentBias, one.LiteralExponent);
    }

    /// <summary>
    /// Tests categorization of various <see cref="Half"/> values via the <see cref="HalfComponents"/> type.
    /// </summary>
    [TestMethod]
    public void TestCategorization()
    {
        var tests = new (Half Value, bool IsNegative, bool IsNaN, bool IsInfinity, bool IsFinite, bool IsSubnormal, bool IsZero)[]
        {
            (Half.NaN, true, true, false, false, false, false),
            (Half.PositiveInfinity, false, false, true, false, false, false),
            (Half.NegativeInfinity, true, false, true, false, false, false),
            (Half.MaxValue, false, false, false, true, false, false),
            (Half.MinValue, true, false, false, true, false, false),
            ((Half)0.0, false, false, false, true, false, true),
            ((Half)(-0.0), true, false, false, true, false, true),
            ((Half)1.0, false, false, false, true, false, false),
            ((Half)(-1.0), true, false, false, true, false, false),
            (MinPositiveValue, false, false, false, true, true, false),
            (MaxNegativeValue, true, false, false, true, true, false),
        };

        foreach (var (Value, IsNegative, IsNaN, IsInfinity, IsFinite, IsSubnormal, IsZero) in tests)
        {
            var testRep = new HalfComponents(Value);

            Assert.AreEqual(IsNegative, testRep.IsNegative, $"{Value} {nameof(HalfComponents.IsNegative)} mismatch.");
            Assert.AreEqual(IsNaN, testRep.IsNaN, $"{Value} {nameof(HalfComponents.IsNaN)} mismatch.");
            Assert.AreEqual(IsInfinity, testRep.IsInfinity, $"{Value} {nameof(HalfComponents.IsInfinity)} mismatch.");
            Assert.AreEqual(IsFinite, testRep.IsFinite, $"{Value} {nameof(HalfComponents.IsFinite)} mismatch.");
            Assert.AreEqual(IsSubnormal, testRep.IsSubnormal, $"{Value} {nameof(HalfComponents.IsSubnormal)} mismatch.");
            Assert.AreEqual(IsZero, testRep.IsZero, $"{Value} {nameof(HalfComponents.IsZero)} mismatch.");
        }
    }

    /// <summary>
    /// Tests the getters for the logical components of <see cref="HalfComponents"/> instances.
    /// </summary>
    [TestMethod]
    public void TestLogicalComponents()
    {
        var tests = new (Half Value, int Sign, int Exponent, ulong Mantissa)[]
        {
            (Value: Half.NaN,
             Sign: -1,
             Exponent: HalfComponents.MaxLogicalExponent,
             Mantissa: HalfComponents.ImplicitMantissaBit | (1uL << (HalfComponents.MantissaBitLength - 1))),
            (Value: Half.PositiveInfinity,
             Sign: 1,
             Exponent: HalfComponents.MaxLogicalExponent,
             Mantissa: HalfComponents.ImplicitMantissaBit),
            (Value: Half.NegativeInfinity,
             Sign: -1,
             Exponent: HalfComponents.MaxLogicalExponent,
             Mantissa: HalfComponents.ImplicitMantissaBit),
            (Value: Half.MaxValue,
             Sign: 1,
             Exponent: HalfComponents.MaxFiniteLogicalExponent,
             Mantissa: HalfComponents.MaxLogicalMantissa),
            (Value: Half.MinValue,
             Sign: -1,
             Exponent: HalfComponents.MaxFiniteLogicalExponent,
             Mantissa: HalfComponents.MaxLogicalMantissa),
            (Value: (Half)0.0,
             Sign: 1,
             Exponent: HalfComponents.MinLogicalExponent,
             Mantissa: 0),
            (Value: (Half)(-0.0),
             Sign: -1,
             Exponent: HalfComponents.MinLogicalExponent,
             Mantissa: 0),
            (Value: MinPositiveValue,
             Sign: 1,
             Exponent: HalfComponents.MinLogicalExponent,
             Mantissa: 1),
            (Value: MaxNegativeValue,
             Sign: -1,
             Exponent: HalfComponents.MinLogicalExponent,
             Mantissa: 1),
        };

        foreach (var (Value, Sign, Exponent, Mantissa) in tests)
        {
            var testRep = new HalfComponents(Value);

            Assert.AreEqual(Sign, testRep.LogicalSign, $"{nameof(HalfComponents.LogicalSign)} mismatch for {Value}.");
            Assert.AreEqual(
                Exponent, testRep.LogicalExponent, $"{nameof(HalfComponents.LogicalExponent)} mismatch for {Value}.");
            Assert.AreEqual(
                Mantissa, testRep.LogicalMantissa,
                $"{nameof(HalfComponents.LogicalMantissa)} mismatch for {Value} "
                    + $"(expected: {Mantissa:X}, actual: {testRep.LogicalMantissa:X}).");
        }
    }

    /// <summary>
    /// Tests the getters for the normalized logical components of <see cref="HalfComponents"/> instances.
    /// </summary>
    [TestMethod]
    public void TestNormalizedLogicalComponents()
    {
        var tests = new (Half Value, int Sign, int Exponent, ulong Mantissa)[]
        {
            (Value: Half.NaN,
             Sign: -1,
             Exponent: HalfComponents.MaxLogicalExponent + HalfComponents.MantissaBitLength - 1,
             Mantissa: 3),
            (Value: Half.PositiveInfinity,
             Sign: 1,
             Exponent: HalfComponents.MaxLogicalExponent + HalfComponents.MantissaBitLength,
             Mantissa: 1),
            (Value: Half.NegativeInfinity,
             Sign: -1,
             Exponent: HalfComponents.MaxLogicalExponent + HalfComponents.MantissaBitLength,
             Mantissa: 1),
            (Value: Half.MaxValue,
             Sign: 1,
             Exponent: HalfComponents.MaxFiniteLogicalExponent,
             Mantissa: HalfComponents.MaxLogicalMantissa),
            (Value: Half.MinValue,
             Sign: -1,
             Exponent: HalfComponents.MaxFiniteLogicalExponent,
             Mantissa: HalfComponents.MaxLogicalMantissa),
            (Value: (Half)0.0,
             Sign: 1,
             Exponent: 0, // Should be simplified to 0
             Mantissa: 0),
            (Value: (Half)(-0.0),
             Sign: -1,
             Exponent: 0, // Should be simplified to 0
             Mantissa: 0),
            (Value: MinPositiveValue,
             Sign: 1,
             Exponent: HalfComponents.MinLogicalExponent,
             Mantissa: 1),
            (Value: MaxNegativeValue,
             Sign: -1,
             Exponent: HalfComponents.MinLogicalExponent,
             Mantissa: 1),
        };

        foreach (var (Value, Sign, Exponent, Mantissa) in tests)
        {
            var testRep = new HalfComponents(Value);

            Assert.AreEqual(Sign, testRep.LogicalSign, $"{nameof(HalfComponents.LogicalSign)} mismatch for {Value}.");
            Assert.AreEqual(
                Exponent, testRep.NormalizedLogicalExponent,
                $"{nameof(HalfComponents.NormalizedLogicalExponent)} mismatch for {Value}.");
            Assert.AreEqual(
                Mantissa, testRep.NormalizedLogicalMantissa,
                $"{nameof(HalfComponents.NormalizedLogicalMantissa)} mismatch for {Value} "
                    + $"(expected: {Mantissa:X}, actual: {testRep.NormalizedLogicalMantissa:X}).");
        }
    }
}
#endif
