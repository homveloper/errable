using System.Collections.Generic;

namespace Errable
{

/// <summary>
/// Interface for errors that provide contextual information.
/// This allows access to structured data associated with the error.
/// </summary>
public interface IErrorContextProvider
{
    /// <summary>
    /// Gets the context information associated with this error.
    /// </summary>
    IReadOnlyDictionary<string, object> Context { get; }
}
}