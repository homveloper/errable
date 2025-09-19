namespace Errable
{

/// <summary>
/// Interface for errors that wrap or are caused by other errors.
/// This provides access to the underlying cause in error chains.
/// </summary>
public interface IErrorCauser
{
    /// <summary>
    /// Gets the underlying error that caused this error, if any.
    /// </summary>
    IError? Cause { get; }
}
}