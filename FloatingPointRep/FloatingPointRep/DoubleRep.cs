using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rem.Core.Numerics;

/// <summary>
/// Represents a <see cref="double"/> as exponent, mantissa and sign bit.
/// </summary>
public readonly record struct DoubleRep
{
    #region Constants
    #region Components
    /// <summary>
    /// The maximum logical exponent value of finite <see cref="double"/> values.
    /// </summary>
    public const short MaxFiniteLogicalExponent = MaxFiniteLiteralExponent - ExponentBias - MantissaBitLength;

    /// <summary>
    /// The maximum logical exponent value.
    /// </summary>
    /// <remarks>
    /// This uniquely defines non-finite values.
    /// </remarks>
    public const short MaxLogicalExponent = MaxLiteralExponent - ExponentBias - MantissaBitLength;

    /// <summary>
    /// The minimum logical exponent value.
    /// </summary>
    /// <remarks>
    /// This value is both the minimum normal range logical exponent and the logical exponent for the subnormal range.
    /// </remarks>
    public const short MinLogicalExponent
        = 1 - ExponentBias - MantissaBitLength; // Subtract from 1 to adjust for subnormal range

    /// <summary>
    /// The bias of the exponent of a <see cref="double"/>.
    /// </summary>
    public const ushort ExponentBias = 1023;

    /// <summary>
    /// The maximum literal exponent value of finite <see cref="double"/> values.
    /// </summary>
    public const ushort MaxFiniteLiteralExponent = MaxLiteralExponent - 1;

    /// <summary>
    /// The maximum literal exponent value.
    /// </summary>
    /// <remarks>
    /// This uniquely identifies non-finite values.
    /// </remarks>
    public const ushort MaxLiteralExponent = 0x7FF;

    /// <summary>
    /// The maximum logical mantissa value.
    /// </summary>
    public const ulong MaxLogicalMantissa = MaxLiteralMantissa | ImplicitMantissaBit;

    /// <summary>
    /// The bit that is implicitly set in non-zero, non-subnormal <see cref="double"/> mantissas.
    /// </summary>
    public const ulong ImplicitMantissaBit = 1uL << MantissaBitLength;

    /// <summary>
    /// The maximum literal mantissa value.
    /// </summary>
    public const ulong MaxLiteralMantissa = 0xFFFFFFFFFFFFF;
    #endregion

    #region Bit Lengths
    /// <summary>
    /// The length of the mantissa of a <see cref="double"/> in bits.
    /// </summary>
    public const int MantissaBitLength = 52;

    /// <summary>
    /// The length of the exponent of a <see cref="double"/> in bits.
    /// </summary>
    public const int ExponentBitLength = 11;
    #endregion
    #endregion

    #region Properties
    #region Computed
    #region Logical Components
    /// <summary>
    /// Gets the logical sign of this instance.
    /// </summary>
    public int LogicalSign => IsNegative ? -1 : 1;

    /// <summary>
    /// Gets the normalized logical exponent of this instance (such that the normalized mantissa has no trailing
    /// 0 bits).
    /// </summary>
    /// <remarks>
    /// This property will return 0 if this instance represents 0.
    /// </remarks>
    public int NormalizedLogicalExponent => NormalizedLogicalExponentFromNormalizationShift(LogicalNormalizationShift);

    /// <summary>
    /// Gets the normalized logical mantissa of this instance (with no trailing 0 bits).
    /// </summary>
    public ulong NormalizedLogicalMantissa
        => NormalizedLogicalMantissaFromNormalizationShift(LogicalNormalizationShift);

    /// <summary>
    /// Gets the logical (biased) exponent of the <see cref="double"/> being represented.
    /// </summary>
    public int LogicalExponent => (LiteralExponent == 0 ? 1 : LiteralExponent) - (ExponentBias + MantissaBitLength);

    /// <summary>
    /// Gets the increase in the exponent and right shift in the mantissa required to normalize the exponent
    /// and mantissa.
    /// </summary>
    /// <remarks>
    /// This is used internally to normalize the logical mantissa and exponent.
    /// </remarks>
    private int LogicalNormalizationShift
    {
        get
        {
            var result = 0;
            var mantissa = LogicalMantissa;
            while (mantissa != 0 && (mantissa & 1) == 0)
            {
                mantissa >>= 1;
                result++;
            }
            return result;
        }
    }

    /// <summary>
    /// Gets the logical mantissa of the <see cref="double"/> being represented.
    /// </summary>
    /// <remarks>
    /// This is the same as <see cref="LiteralMantissa"/>, but with the implicit 1-bit added to the left if
    /// <see cref="LiteralExponent"/> is not 0 (in which case the number is in the subnormal range).
    /// </remarks>
    public ulong LogicalMantissa => LiteralExponent == 0 ? LiteralMantissa : LiteralMantissa | ImplicitMantissaBit;
    #endregion

    #region Characterization
    /// <summary>
    /// Gets whether or not this instance represents a <see cref="double"/> in the (nonzero) subnormal range.
    /// </summary>
    public bool IsSubnormal => LiteralExponent == 0 && LiteralMantissa != 0;

    /// <summary>
    /// Gets whether or not this instance represents a <see cref="double"/> that is an infinity value (either positive
    /// or negative).
    /// </summary>
    public bool IsInfinity => LiteralExponent == MaxLiteralExponent && LiteralMantissa == 0;

    /// <summary>
    /// Gets whether or not this instance represents a <see cref="double"/> that is a NaN value.
    /// </summary>
    public bool IsNaN => LiteralExponent == MaxLiteralExponent && LiteralMantissa != 0;

    /// <summary>
    /// Gets whether or not this instance represents a <see cref="double"/> that is finite.
    /// </summary>
    public bool IsFinite => LiteralExponent != MaxLiteralExponent;

    /// <summary>
    /// Gets whether or not this instance represents a <see cref="double"/> equal to zero.
    /// </summary>
    public bool IsZero => LiteralExponent == 0 && LiteralMantissa == 0;

    /// <summary>
    /// Gets whether or not the <see cref="double"/> being represented is positive.
    /// </summary>
    public bool IsPositive => !IsNegative;
    #endregion
    #endregion

    #region Stored
    /// <summary>
    /// Gets the literal exponent of the <see cref="double"/> being represented.
    /// </summary>
    public ushort LiteralExponent { get; }

    /// <summary>
    /// Gets the mantissa of the <see cref="double"/> being represented.
    /// </summary>
    public ulong LiteralMantissa { get; }

    /// <summary>
    /// Gets whether or not the <see cref="double"/> being represented is negative.
    /// </summary>
    public bool IsNegative { get; }
    #endregion
    #endregion

    #region Constructor
    /// <summary>
    /// Constructs a new instance of the <see cref="DoubleRep"/> struct representing the <see cref="double"/> value
    /// passed in.
    /// </summary>
    /// <param name="Double"></param>
    public DoubleRep(double Double)
    {
        // Translate the double into sign, exponent and mantissa.
        var bits = unchecked((ulong)BitConverter.DoubleToInt64Bits(Double));

        IsNegative = (bits & (1uL << 63)) != 0;
        LiteralExponent = unchecked((ushort)((bits >> MantissaBitLength) & MaxLiteralExponent));
        LiteralMantissa = bits & MaxLiteralMantissa;
    }
    #endregion

    #region Equality
    /// <summary>
    /// Determines whether or not this instance is equal to another object of the same type.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(DoubleRep other) => IsNegative == other.IsNegative
                                            && LiteralExponent == other.LiteralExponent
                                            && LiteralMantissa == other.LiteralMantissa;

    /// <summary>
    /// Gets a hash code for the current instance.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() => HashCode.Combine(IsNegative, LiteralExponent, LiteralMantissa);
    #endregion

    #region ToString
    /// <summary>
    /// Gets a string that represents the current instance.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
        => $"{nameof(DoubleRep)} {{ "
            + $"IsNegative = {IsNegative}, "
            + $"Exponent = {LiteralExponent.ToString($"X{ExponentBitLength}")}, "
            + $"Mantissa = {LiteralMantissa.ToString($"X{MantissaBitLength}")} }}";
    #endregion

    #region Deconstruction
    /// <summary>
    /// Deconstructs the current instance into its literal components.
    /// </summary>
    /// <param name="IsNegative"></param>
    /// <param name="LiteralExponent"></param>
    /// <param name="LiteralMantissa"></param>
    public void Deconstruct(out bool IsNegative, out ushort LiteralExponent, out ulong LiteralMantissa)
    {
        IsNegative = this.IsNegative;
        LiteralExponent = this.LiteralExponent;
        LiteralMantissa = this.LiteralMantissa;
    }

    /// <summary>
    /// Deconstructs the current instance into its logical components.
    /// </summary>
    /// <param name="IsNegative"></param>
    /// <param name="Exponent"></param>
    /// <param name="Mantissa"></param>
    /// <returns>
    /// Whether or not this instance represents a finite <see cref="double"/> value.
    /// </returns>
    /// <seealso cref="LogicalExponent"/>
    /// <seealso cref="LogicalMantissa"/>
    public bool TryGetLogical(out bool IsNegative, out int Exponent, out ulong Mantissa)
    {
        IsNegative = this.IsNegative;
        Exponent = LogicalExponent;
        Mantissa = LogicalMantissa;

        return IsFinite;
    }

    /// <summary>
    /// Deconstructs the current instance into its normalized logical components.
    /// </summary>
    /// <param name="IsNegative"></param>
    /// <param name="Exponent"></param>
    /// <param name="Mantissa"></param>
    /// <returns>
    /// Whether or not the instance represents a finite <see cref="double"/> value.
    /// </returns>
    public bool TryGetNormalizedLogical(out bool IsNegative, out int Exponent, out ulong Mantissa)
    {
        IsNegative = this.IsNegative;

        var shift = LogicalNormalizationShift;
        Exponent = NormalizedLogicalExponentFromNormalizationShift(shift);
        Mantissa = NormalizedLogicalMantissaFromNormalizationShift(shift);

        return IsFinite;
    }
    #endregion

    #region Conversion
    /// <summary>
    /// Gets the <see cref="double"/> value represented by this instance.
    /// </summary>
    /// <returns></returns>
    public double ToDouble()
        => BitConverter.Int64BitsToDouble(unchecked((long)(
                                            (IsNegative ? 1uL << 63 : 0) // Sign
                                                | (ulong)LiteralExponent << MantissaBitLength // Exponent
                                                | LiteralMantissa))); // Mantissa
    #endregion

    #region Helpers
    /// <summary>
    /// Computes the normalized logical mantissa from the precomputed normalization shift.
    /// </summary>
    /// <param name="shift"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong NormalizedLogicalMantissaFromNormalizationShift(int shift) => LogicalMantissa >> shift;

    /// <summary>
    /// Computes the normalized logical exponent from the precomputed normalization shift.
    /// </summary>
    /// <param name="shift"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int NormalizedLogicalExponentFromNormalizationShift(int shift)
        => LogicalMantissa == 0 ? 0 : LogicalExponent + shift;
    #endregion
}

