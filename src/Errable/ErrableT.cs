using System;
using System.Collections.Generic;

namespace Errable
{

/// <summary>
/// A Result-pattern struct that represents either a successful value or an error.
/// This is the generic version that carries a value of type T when successful.
/// </summary>
/// <typeparam name="T">The type of the successful value</typeparam>
public readonly struct Errable<T>
{
    private readonly T _value;
    private readonly IError? _error;
    private readonly bool _isSuccess;

    /// <summary>
    /// Initializes a successful Errable with a value.
    /// </summary>
    /// <param name="value">The successful value</param>
    public Errable(T value)
    {
        _value = value;
        _error = null;
        _isSuccess = true;
    }

    /// <summary>
    /// Initializes a failed Errable with an error.
    /// </summary>
    /// <param name="error">The error</param>
    public Errable(IError error)
    {
        _value = default;
        _error = error ?? throw new ArgumentNullException(nameof(error));
        _isSuccess = false;
    }

    /// <summary>
    /// Gets a value indicating whether this Errable represents a successful result.
    /// </summary>
    public bool IsSuccess => _isSuccess;

    /// <summary>
    /// Gets a value indicating whether this Errable represents an error.
    /// </summary>
    public bool IsError => !_isSuccess;

    /// <summary>
    /// Gets the successful value. Throws if this Errable represents an error.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing Value on an error Errable</exception>
    public T Value => _isSuccess ? _value : throw new InvalidOperationException("Cannot access Value on an error Errable");

    /// <summary>
    /// Gets the error. Throws if this Errable represents a successful value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing Error on a successful Errable</exception>
    public IError Error => !_isSuccess ? _error! : throw new InvalidOperationException("Cannot access Error on a successful Errable");

    /// <summary>
    /// Attempts to get the value if successful.
    /// </summary>
    /// <param name="value">The value if successful</param>
    /// <returns>True if successful, false if error</returns>
    public bool TryGetValue(out T value)
    {
        if (_isSuccess)
        {
            value = _value;
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>
    /// Attempts to get the error if failed.
    /// </summary>
    /// <param name="error">The error if failed</param>
    /// <returns>True if error, false if successful</returns>
    public bool TryGetError(out IError error)
    {
        if (!_isSuccess)
        {
            error = _error!;
            return true;
        }

        error = default!;
        return false;
    }

    /// <summary>
    /// Gets the value if successful, otherwise returns the default value.
    /// </summary>
    /// <param name="defaultValue">The default value to return if this is an error</param>
    /// <returns>The value or default value</returns>
    public T GetValueOrDefault(T defaultValue = default!) => _isSuccess ? _value : defaultValue;

    /// <summary>
    /// Implicit conversion from a value to a successful Errable.
    /// </summary>
    /// <param name="value">The value</param>
    public static implicit operator Errable<T>(T value) => new Errable<T>(value);

    /// <summary>
    /// Creates a failed Errable by wrapping an Error.
    /// </summary>
    /// <param name="error">The error to wrap</param>
    /// <returns>A failed Errable</returns>
    public static Errable<T> Wrap(IError error) => new Errable<T>(error);

    /// <summary>
    /// Pattern matches on the Errable, executing one function for success and another for error.
    /// </summary>
    /// <typeparam name="TResult">The type of the result</typeparam>
    /// <param name="onSuccess">Function to execute on success</param>
    /// <param name="onError">Function to execute on error</param>
    /// <returns>The result of the executed function</returns>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<IError, TResult> onError)
    {
        return _isSuccess ? onSuccess(_value) : onError(_error!);
    }

    /// <summary>
    /// Returns a string representation of this Errable.
    /// </summary>
    /// <returns>String representation</returns>
    public override string ToString()
    {
        return _isSuccess ? $"Success: {_value}" : $"Error: {_error!.Error()}";
    }

    /// <summary>
    /// Determines equality with another Errable.
    /// </summary>
    /// <param name="other">The other Errable</param>
    /// <returns>True if equal</returns>
    public bool Equals(Errable<T> other)
    {
        if (_isSuccess != other._isSuccess)
            return false;

        if (_isSuccess)
            return EqualityComparer<T>.Default.Equals(_value, other._value);

        return ReferenceEquals(_error, other._error) ||
               (_error != null && other._error != null && _error.Error() == other._error.Error());
    }

    /// <summary>
    /// Determines equality with another object.
    /// </summary>
    /// <param name="obj">The other object</param>
    /// <returns>True if equal</returns>
    public override bool Equals(object? obj) => obj is Errable<T> other && Equals(other);

    /// <summary>
    /// Gets the hash code for this Errable.
    /// </summary>
    /// <returns>Hash code</returns>
    public override int GetHashCode()
    {
        if (_isSuccess)
            return HashCode.Combine(_isSuccess, _value);

        return HashCode.Combine(_isSuccess, _error?.Error());
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    /// <returns>True if equal</returns>
    public static bool operator ==(Errable<T> left, Errable<T> right) => left.Equals(right);

    /// <summary>
    /// Inequality operator.
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    /// <returns>True if not equal</returns>
    public static bool operator !=(Errable<T> left, Errable<T> right) => !left.Equals(right);
}
}