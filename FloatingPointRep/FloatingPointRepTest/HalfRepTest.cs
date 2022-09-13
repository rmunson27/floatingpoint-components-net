using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemTest.Core.Numerics;

#if NET5_0_OR_GREATER
/// <summary>
/// Tests of the <see cref="HalfRep"/> struct.
/// </summary>
[TestClass]
public class HalfRepTest
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
            var testRep = new HalfRep(test);
            Assert.AreEqual(test, testRep.ToHalf(), $"Half value {test} could not be reproduced.");
        }
    }

    /// <summary>
    /// Tests categorization of various <see cref="Half"/> values via the <see cref="HalfRep"/> type.
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
            var testRep = new HalfRep(Value);

            Assert.AreEqual(IsNegative, testRep.IsNegative, $"{Value} {nameof(HalfRep.IsNegative)} mismatch.");
            Assert.AreEqual(IsNaN, testRep.IsNaN, $"{Value} {nameof(HalfRep.IsNaN)} mismatch.");
            Assert.AreEqual(IsInfinity, testRep.IsInfinity, $"{Value} {nameof(HalfRep.IsInfinity)} mismatch.");
            Assert.AreEqual(IsFinite, testRep.IsFinite, $"{Value} {nameof(HalfRep.IsFinite)} mismatch.");
            Assert.AreEqual(IsSubnormal, testRep.IsSubnormal, $"{Value} {nameof(HalfRep.IsSubnormal)} mismatch.");
            Assert.AreEqual(IsZero, testRep.IsZero, $"{Value} {nameof(HalfRep.IsZero)} mismatch.");
        }
    }

    /// <summary>
    /// Tests the getters for the logical components of <see cref="HalfRep"/> instances.
    /// </summary>
    [TestMethod]
    public void TestLogicalComponents()
    {
        var tests = new (Half Value, int Sign, int Exponent, ulong Mantissa)[]
        {
            (Value: Half.NaN,
             Sign: -1,
             Exponent: HalfRep.MaxLogicalExponent,
             Mantissa: HalfRep.ImplicitMantissaBit | (1uL << (HalfRep.MantissaBitLength - 1))),
            (Value: Half.PositiveInfinity,
             Sign: 1,
             Exponent: HalfRep.MaxLogicalExponent,
             Mantissa: HalfRep.ImplicitMantissaBit),
            (Value: Half.NegativeInfinity,
             Sign: -1,
             Exponent: HalfRep.MaxLogicalExponent,
             Mantissa: HalfRep.ImplicitMantissaBit),
            (Value: Half.MaxValue,
             Sign: 1,
             Exponent: HalfRep.MaxFiniteLogicalExponent,
             Mantissa: HalfRep.MaxLogicalMantissa),
            (Value: Half.MinValue,
             Sign: -1,
             Exponent: HalfRep.MaxFiniteLogicalExponent,
             Mantissa: HalfRep.MaxLogicalMantissa),
            (Value: (Half)0.0,
             Sign: 1,
             Exponent: HalfRep.MinLogicalExponent,
             Mantissa: 0),
            (Value: (Half)(-0.0),
             Sign: -1,
             Exponent: HalfRep.MinLogicalExponent,
             Mantissa: 0),
            (Value: MinPositiveValue,
             Sign: 1,
             Exponent: HalfRep.MinLogicalExponent,
             Mantissa: 1),
            (Value: MaxNegativeValue,
             Sign: -1,
             Exponent: HalfRep.MinLogicalExponent,
             Mantissa: 1),
        };

        foreach (var (Value, Sign, Exponent, Mantissa) in tests)
        {
            var testRep = new HalfRep(Value);

            Assert.AreEqual(Sign, testRep.LogicalSign, $"{nameof(HalfRep.LogicalSign)} mismatch for {Value}.");
            Assert.AreEqual(
                Exponent, testRep.LogicalExponent, $"{nameof(HalfRep.LogicalExponent)} mismatch for {Value}.");
            Assert.AreEqual(
                Mantissa, testRep.LogicalMantissa,
                $"{nameof(HalfRep.LogicalMantissa)} mismatch for {Value} "
                    + $"(expected: {Mantissa:X}, actual: {testRep.LogicalMantissa:X}).");
        }
    }

    /// <summary>
    /// Tests the getters for the normalized logical components of <see cref="HalfRep"/> instances.
    /// </summary>
    [TestMethod]
    public void TestNormalizedLogicalComponents()
    {
        var tests = new (Half Value, int Sign, int Exponent, ulong Mantissa)[]
        {
            (Value: Half.NaN,
             Sign: -1,
             Exponent: HalfRep.MaxLogicalExponent + HalfRep.MantissaBitLength - 1,
             Mantissa: 3),
            (Value: Half.PositiveInfinity,
             Sign: 1,
             Exponent: HalfRep.MaxLogicalExponent + HalfRep.MantissaBitLength,
             Mantissa: 1),
            (Value: Half.NegativeInfinity,
             Sign: -1,
             Exponent: HalfRep.MaxLogicalExponent + HalfRep.MantissaBitLength,
             Mantissa: 1),
            (Value: Half.MaxValue,
             Sign: 1,
             Exponent: HalfRep.MaxFiniteLogicalExponent,
             Mantissa: HalfRep.MaxLogicalMantissa),
            (Value: Half.MinValue,
             Sign: -1,
             Exponent: HalfRep.MaxFiniteLogicalExponent,
             Mantissa: HalfRep.MaxLogicalMantissa),
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
             Exponent: HalfRep.MinLogicalExponent,
             Mantissa: 1),
            (Value: MaxNegativeValue,
             Sign: -1,
             Exponent: HalfRep.MinLogicalExponent,
             Mantissa: 1),
        };

        foreach (var (Value, Sign, Exponent, Mantissa) in tests)
        {
            var testRep = new HalfRep(Value);

            Assert.AreEqual(Sign, testRep.LogicalSign, $"{nameof(HalfRep.LogicalSign)} mismatch for {Value}.");
            Assert.AreEqual(
                Exponent, testRep.NormalizedLogicalExponent,
                $"{nameof(HalfRep.NormalizedLogicalExponent)} mismatch for {Value}.");
            Assert.AreEqual(
                Mantissa, testRep.NormalizedLogicalMantissa,
                $"{nameof(HalfRep.NormalizedLogicalMantissa)} mismatch for {Value} "
                    + $"(expected: {Mantissa:X}, actual: {testRep.NormalizedLogicalMantissa:X}).");
        }
    }
}
#endif
