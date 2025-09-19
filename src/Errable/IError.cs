namespace Errable;

/// <summary>
/// The minimal error interface that all errors must implement.
/// This follows Go's simple error interface philosophy with just one method.
/// </summary>
public interface IError
{
    /// <summary>
    /// Returns the error message as a string.
    /// This is the only required method for all error implementations.
    /// </summary>
    /// <returns>A string describing the error</returns>
    string Error();
}