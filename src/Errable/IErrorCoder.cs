namespace Errable;

/// <summary>
/// Interface for errors that provide a code property.
/// This allows type-safe access to error codes without casting.
/// </summary>
public interface IErrorCoder
{
    /// <summary>
    /// Gets the error code for this error.
    /// </summary>
    Code Code { get; }
}