using System;
using System.Diagnostics;
using CodeType = Errable.Code;

namespace Errable
{

/// <summary>
/// Static factory class for creating Error instances and starting builder chains.
/// Provides both direct error creation methods and fluent builder entry points.
/// </summary>
public static class ErrableFactory
{
    #region Direct Error Creation

    /// <summary>
    /// Creates a simple error with the specified message.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <returns>A new Error instance</returns>
    public static IError Error(string message)
    {
        return new Erratic(
            code: CodeType.Empty,
            message: message,
            stackTrace: new FilteredStackTrace(skipFrames: 1)
        );
    }

    /// <summary>
    /// Creates an error with a formatted message.
    /// </summary>
    /// <param name="format">The format string</param>
    /// <param name="args">The format arguments</param>
    /// <returns>A new Error instance</returns>
    public static IError Errorf(string format, params object[] args)
    {
        var message = string.Format(format, args);
        return Error(message);
    }

    /// <summary>
    /// Creates an error by wrapping an exception.
    /// </summary>
    /// <param name="ex">The exception to wrap</param>
    /// <param name="message">Optional additional message</param>
    /// <returns>A new Error instance</returns>
    public static IError Wrap(Exception ex, string? message = null)
    {
        var errorMessage = message ?? ex.Message;

        IError? cause = null;
        if (ex.InnerException != null)
        {
            cause = Wrap(ex.InnerException);
        }

        return new Erratic(
            code: CodeType.Empty,
            message: errorMessage,
            cause: cause,
            stackTrace: new FilteredStackTrace(skipFrames: 1)
        );
    }

    /// <summary>
    /// Creates an error by wrapping an exception with a formatted message.
    /// </summary>
    /// <param name="ex">The exception to wrap</param>
    /// <param name="format">The format string</param>
    /// <param name="args">The format arguments</param>
    /// <returns>A new Error instance</returns>
    public static IError Wrapf(Exception ex, string format, params object[] args)
    {
        var message = string.Format(format, args);
        return Wrap(ex, message);
    }

    /// <summary>
    /// Creates an error by wrapping another Errable result.
    /// Returns a zero-value error if the result is successful.
    /// Preserves the original error message and code.
    /// </summary>
    /// <typeparam name="T">The type of the Errable</typeparam>
    /// <param name="result">The Errable to wrap</param>
    /// <returns>A new Error instance, or zero-value error if result is successful</returns>
    public static IError Wrap<T>(Errable<T> result)
    {
        if (result.IsSuccess)
        {
            // Return zero-value error for successful case
            return new Erratic(
                code: CodeType.Empty,
                message: "",
                stackTrace: new FilteredStackTrace(skipFrames: 1)
            );
        }

        return new Erratic(
            code: CodeType.Empty,
            message: "",
            cause: result.Error,
            stackTrace: new FilteredStackTrace(skipFrames: 1)
        );
    }

    /// <summary>
    /// Creates an error by wrapping another Errable result with a formatted message.
    /// Returns a zero-value error if the result is successful.
    /// </summary>
    /// <typeparam name="T">The type of the Errable</typeparam>
    /// <param name="result">The Errable to wrap</param>
    /// <param name="format">The format string</param>
    /// <param name="args">The format arguments</param>
    /// <returns>A new Error instance, or zero-value error if result is successful</returns>
    public static IError Wrapf<T>(Errable<T> result, string format, params object[] args)
    {
        if (result.IsSuccess)
        {
            // Return zero-value error for successful case
            return new Erratic(
                code: CodeType.Empty,
                message: "",
                stackTrace: new FilteredStackTrace(skipFrames: 1)
            );
        }

        // Use formatted message instead of original
        var message = string.Format(format, args);

        return new Erratic(
            code: CodeType.Empty,
            message: message,
            cause: result.Error,
            stackTrace: new FilteredStackTrace(skipFrames: 1)
        );
    }

    /// <summary>
    /// Creates an error from an exception without additional wrapping.
    /// </summary>
    /// <param name="ex">The exception</param>
    /// <returns>A new Error instance</returns>
    public static IError Except(Exception ex)
    {
        return new Erratic(
            code: CodeType.Empty,
            message: ex.Message,
            stackTrace: new FilteredStackTrace(skipFrames: 1)
        );
    }

    /// <summary>
    /// Creates an error from an exception with a formatted message.
    /// </summary>
    /// <param name="ex">The exception</param>
    /// <param name="format">The format string</param>
    /// <param name="args">The format arguments</param>
    /// <returns>A new Error instance</returns>
    public static IError Exceptf(Exception ex, string format, params object[] args)
    {
        string message = string.Format(format, args);
        return new Erratic(
            code: CodeType.Empty,
            message: message,
            stackTrace: new FilteredStackTrace(skipFrames: 1)
        );
    }

    #endregion

    #region Builder Entry Point

    /// <summary>
    /// Starts a fluent builder chain with the specified error code.
    /// </summary>
    /// <param name="code">The error code</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Code(CodeType code)
    {
        return new ErrableBuilder(code);
    }

    /// <summary>
    /// Starts a fluent builder chain with the specified error code string.
    /// </summary>
    /// <param name="code">The error code string</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Code(string code)
    {
        return new ErrableBuilder(new CodeType(code));
    }

    /// <summary>
    /// Starts a fluent builder chain with the specified error code integer.
    /// </summary>
    /// <param name="code">The error code integer</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Code(int code)
    {
        return new ErrableBuilder(new CodeType(code));
    }

    /// <summary>
    /// Starts a fluent builder chain with context data.
    /// </summary>
    /// <param name="key">The context key</param>
    /// <param name="value">The context value</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder With(string key, object value)
    {
        return new ErrableBuilder(CodeType.Empty).With(key, value);
    }

    /// <summary>
    /// Starts a fluent builder chain with multiple context data pairs.
    /// </summary>
    /// <param name="pairs">The key-value pairs to add</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder With(params (string Key, object Value)[] pairs)
    {
        return new ErrableBuilder(new Code("")).With(pairs);
    }

    /// <summary>
    /// Starts a fluent builder chain with lazy-evaluated context data.
    /// </summary>
    /// <param name="key">The context key</param>
    /// <param name="valueFactory">Function to evaluate the value</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder WithLazy(string key, Func<object> valueFactory)
    {
        return new ErrableBuilder(new Code("")).WithLazy(key, valueFactory);
    }

    /// <summary>
    /// Starts a fluent builder chain with tags.
    /// </summary>
    /// <param name="tags">The tags to add</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Tags(params string[] tags)
    {
        return new ErrableBuilder(new Code("")).Tags(tags);
    }

    /// <summary>
    /// Starts a fluent builder chain with a domain.
    /// </summary>
    /// <param name="domain">The domain name</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder In(string domain)
    {
        return new ErrableBuilder(new Code("")).In(domain);
    }

    /// <summary>
    /// Starts a fluent builder chain with a public message.
    /// </summary>
    /// <param name="message">The public-facing error message</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Public(string message)
    {
        return new ErrableBuilder(new Code("")).Public(message);
    }

    /// <summary>
    /// Starts a fluent builder chain with a hint.
    /// </summary>
    /// <param name="hint">The hint for resolving the error</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Hint(string hint)
    {
        return new ErrableBuilder(new Code("")).Hint(hint);
    }

    /// <summary>
    /// Starts a fluent builder chain with an owner.
    /// </summary>
    /// <param name="owner">The owner of the error</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Owner(string owner)
    {
        return new ErrableBuilder(new Code("")).Owner(owner);
    }

    /// <summary>
    /// Starts a fluent builder chain with a trace ID.
    /// </summary>
    /// <param name="traceId">The trace ID</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Trace(string? traceId)
    {
        return new ErrableBuilder(new Code("")).Trace(traceId);
    }

    /// <summary>
    /// Starts a fluent builder chain with a span ID.
    /// </summary>
    /// <param name="spanId">The span ID</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Span(string? spanId)
    {
        return new ErrableBuilder(new Code("")).Span(spanId);
    }

    /// <summary>
    /// Starts a fluent builder chain with duration calculation from start time.
    /// </summary>
    /// <param name="start">The start time</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Since(DateTime start)
    {
        return new ErrableBuilder(new Code("")).Since(start);
    }

    /// <summary>
    /// Starts a fluent builder chain with a specific duration.
    /// </summary>
    /// <param name="duration">The duration</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Duration(TimeSpan duration)
    {
        return new ErrableBuilder(new Code("")).Duration(duration);
    }

    /// <summary>
    /// Starts a fluent builder chain with user information.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="attributes">The user attributes</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder User(string id, params (string Key, object Value)[] attributes)
    {
        return new ErrableBuilder(new Code("")).User(id, attributes);
    }

    /// <summary>
    /// Starts a fluent builder chain with tenant information.
    /// </summary>
    /// <param name="id">The tenant ID</param>
    /// <param name="attributes">The tenant attributes</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Tenant(string id, params (string Key, object Value)[] attributes)
    {
        return new ErrableBuilder(new Code("")).Tenant(id, attributes);
    }


    #endregion
}
}