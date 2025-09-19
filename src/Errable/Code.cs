using System;

namespace Errable
{

/// <summary>
/// A value type that can hold either a string or int error code.
/// Provides implicit conversions and consistent string representation.
/// </summary>
public readonly struct Code : IEquatable<Code>
{
    private readonly object? _value;

    /// <summary>
    /// Initializes a new instance of the Code struct with a string value.
    /// </summary>
    /// <param name="code">The string error code</param>
    public Code(string code)
    {
        _value = code ?? throw new ArgumentNullException(nameof(code));
    }

    /// <summary>
    /// Initializes a new instance of the Code struct with an integer value.
    /// </summary>
    /// <param name="code">The integer error code</param>
    public Code(int code)
    {
        _value = code;
    }

    /// <summary>
    /// Implicitly converts a string to a Code.
    /// </summary>
    /// <param name="code">The string error code</param>
    public static implicit operator Code(string code) => new Code(code);

    /// <summary>
    /// Implicitly converts an int to a Code.
    /// </summary>
    /// <param name="code">The integer error code</param>
    public static implicit operator Code(int code) => new Code(code);

    /// <summary>
    /// Checks if two Code instances are equal.
    /// </summary>
    /// <param name="left">The first Code to compare</param>
    /// <param name="right">The second Code to compare</param>
    /// <returns>True if the codes are equal, false otherwise</returns>
    public static bool operator ==(Code left, Code right) => left.Equals(right);

    /// <summary>
    /// Checks if two Code instances are not equal.
    /// </summary>
    /// <param name="left">The first Code to compare</param>
    /// <param name="right">The second Code to compare</param>
    /// <returns>True if the codes are not equal, false otherwise</returns>
    public static bool operator !=(Code left, Code right) => !left.Equals(right);

    /// <summary>
    /// Returns the string representation of the code.
    /// </summary>
    /// <returns>The code as a string</returns>
    public override string ToString() => _value?.ToString() ?? string.Empty;

    /// <summary>
    /// Determines whether the specified object is equal to the current Code.
    /// </summary>
    /// <param name="obj">The object to compare with the current Code</param>
    /// <returns>True if the objects are equal, false otherwise</returns>
    public override bool Equals(object? obj) => obj is Code code && Equals(code);

    /// <summary>
    /// Indicates whether the current Code is equal to another Code.
    /// </summary>
    /// <param name="other">A Code to compare with this Code</param>
    /// <returns>True if the current Code is equal to the other parameter, false otherwise</returns>
    public bool Equals(Code other) => Equals(_value, other._value);

    /// <summary>
    /// Returns the hash code for this Code.
    /// </summary>
    /// <returns>A hash code for the current Code</returns>
    public override int GetHashCode() => _value?.GetHashCode() ?? 0;
}
}