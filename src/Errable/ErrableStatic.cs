namespace Errable;

/// <summary>
/// Static factory class for creating Error instances and starting builder chains.
/// This provides the main API entry point matching the package name (Errable.Error, Errable.Code, etc.)
/// </summary>
public static class Errable
{
    #region Direct Error Creation

    /// <summary>
    /// Creates a simple error with the specified message.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <returns>A new Error instance</returns>
    public static IError Error(string message) => ErrableFactory.Error(message);

    /// <summary>
    /// Creates an error with a formatted message.
    /// </summary>
    /// <param name="format">The format string</param>
    /// <param name="args">The format arguments</param>
    /// <returns>A new Error instance</returns>
    public static IError Errorf(string format, params object[] args) => ErrableFactory.Errorf(format, args);

    /// <summary>
    /// Creates an error by wrapping an exception.
    /// </summary>
    /// <param name="ex">The exception to wrap</param>
    /// <param name="message">Optional additional message</param>
    /// <returns>A new Error instance</returns>
    public static IError Wrap(Exception ex, string? message = null) => ErrableFactory.Wrap(ex, message);

    /// <summary>
    /// Creates an error by wrapping an exception with a formatted message.
    /// </summary>
    /// <param name="ex">The exception to wrap</param>
    /// <param name="format">The format string</param>
    /// <param name="args">The format arguments</param>
    /// <returns>A new Error instance</returns>
    public static IError Wrapf(Exception ex, string format, params object[] args) => ErrableFactory.Wrapf(ex, format, args);

    /// <summary>
    /// Creates an error by wrapping another Errable result.
    /// </summary>
    /// <typeparam name="T">The type of the Errable</typeparam>
    /// <param name="result">The Errable to wrap</param>
    /// <returns>A new Error instance</returns>
    public static IError Wrap<T>(Errable<T> result) => ErrableFactory.Wrap(result);

    /// <summary>
    /// Creates an error by wrapping another Errable result with a formatted message.
    /// </summary>
    /// <typeparam name="T">The type of the Errable</typeparam>
    /// <param name="result">The Errable to wrap</param>
    /// <param name="format">The format string</param>
    /// <param name="args">The format arguments</param>
    /// <returns>A new Error instance</returns>
    public static IError Wrapf<T>(Errable<T> result, string format, params object[] args) => ErrableFactory.Wrapf(result, format, args);

    /// <summary>
    /// Creates an error from an exception without additional wrapping.
    /// </summary>
    /// <param name="ex">The exception</param>
    /// <returns>A new Error instance</returns>
    public static IError Except(Exception ex) => ErrableFactory.Except(ex);

    /// <summary>
    /// Creates an error from an exception with a formatted message.
    /// </summary>
    /// <param name="ex">The exception</param>
    /// <param name="format">The format string</param>
    /// <param name="args">The format arguments</param>
    /// <returns>A new Error instance</returns>
    public static IError Exceptf(Exception ex, string format, params object[] args) => ErrableFactory.Exceptf(ex, format, args);

    #endregion

    #region Type Inference Builder Entry Points

    /// <summary>
    /// Creates a type-specific fluent builder for building Errable&lt;T&gt; instances.
    /// This provides complete type inference throughout the builder chain.
    /// </summary>
    /// <typeparam name="T">The type for the final Errable result</typeparam>
    /// <returns>A new ErrableBuilder&lt;T&gt; instance for method chaining</returns>
    public static ErrableBuilder<T> For<T>() => new();

    /// <summary>
    /// Creates a type-specific fluent builder for building Errable&lt;T&gt; instances with an initial error code.
    /// This provides complete type inference throughout the builder chain.
    /// </summary>
    /// <typeparam name="T">The type for the final Errable result</typeparam>
    /// <param name="code">The error code</param>
    /// <returns>A new ErrableBuilder&lt;T&gt; instance for method chaining</returns>
    public static ErrableBuilder<T> For<T>(Code code) => new(code);

    /// <summary>
    /// Creates a type-specific fluent builder for building Errable&lt;T&gt; instances with an initial error code.
    /// This provides complete type inference throughout the builder chain.
    /// </summary>
    /// <typeparam name="T">The type for the final Errable result</typeparam>
    /// <param name="code">The error code as string</param>
    /// <returns>A new ErrableBuilder&lt;T&gt; instance for method chaining</returns>
    public static ErrableBuilder<T> For<T>(string code) => new(new Code(code));

    #endregion

    #region Convenience Factory Methods

    /// <summary>
    /// Creates a failed Errable&lt;T&gt; with the specified code and message.
    /// This is a convenience method for simple error creation.
    /// </summary>
    /// <typeparam name="T">The type for the Errable result</typeparam>
    /// <param name="code">The error code</param>
    /// <param name="message">The error message</param>
    /// <returns>A failed Errable&lt;T&gt; instance</returns>
    public static Errable<T> ErrorFor<T>(string code, string message) => Code(code).Error<T>(message);

    /// <summary>
    /// Creates a failed Errable&lt;T&gt; with the specified code and formatted message.
    /// This is a convenience method for simple error creation.
    /// </summary>
    /// <typeparam name="T">The type for the Errable result</typeparam>
    /// <param name="code">The error code</param>
    /// <param name="format">The format string</param>
    /// <param name="args">The format arguments</param>
    /// <returns>A failed Errable&lt;T&gt; instance</returns>
    public static Errable<T> ErrorfFor<T>(string code, string format, params object[] args) => Code(code).Errorf<T>(format, args);

    /// <summary>
    /// Creates a failed Errable&lt;T&gt; by wrapping an exception.
    /// This is a convenience method for simple exception wrapping.
    /// </summary>
    /// <typeparam name="T">The type for the Errable result</typeparam>
    /// <param name="ex">The exception to wrap</param>
    /// <param name="message">Optional additional message</param>
    /// <returns>A failed Errable&lt;T&gt; instance</returns>
    public static Errable<T> WrapFor<T>(Exception ex, string? message = null)
    {
        var error = Wrap(ex, message);
        return Errable<T>.Wrap(error);
    }

    /// <summary>
    /// Creates a failed Errable&lt;T&gt; by wrapping an exception with a formatted message.
    /// This is a convenience method for simple exception wrapping.
    /// </summary>
    /// <typeparam name="T">The type for the Errable result</typeparam>
    /// <param name="ex">The exception to wrap</param>
    /// <param name="format">The format string</param>
    /// <param name="args">The format arguments</param>
    /// <returns>A failed Errable&lt;T&gt; instance</returns>
    public static Errable<T> WrapfFor<T>(Exception ex, string format, params object[] args)
    {
        var error = Wrapf(ex, format, args);
        return Errable<T>.Wrap(error);
    }

    #endregion

    #region Builder Entry Points

    /// <summary>
    /// Starts a fluent builder chain with the specified error code.
    /// </summary>
    /// <param name="code">The error code</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Code(Code code) => ErrableFactory.Code(code);

    /// <summary>
    /// Starts a fluent builder chain with the specified error code.
    /// </summary>
    /// <param name="code">The error code as string</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Code(string code) => ErrableFactory.Code(new Code(code));

    /// <summary>
    /// Starts a fluent builder chain with context data.
    /// </summary>
    /// <param name="key">The context key</param>
    /// <param name="value">The context value</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder With(string key, object value) => ErrableFactory.With(key, value);

    /// <summary>
    /// Starts a fluent builder chain with multiple context data pairs.
    /// </summary>
    /// <param name="pairs">The key-value pairs to add</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder With(params (string Key, object Value)[] pairs) => ErrableFactory.With(pairs);

    /// <summary>
    /// Starts a fluent builder chain with lazy-evaluated context data.
    /// </summary>
    /// <param name="key">The context key</param>
    /// <param name="valueFactory">Function to evaluate the value</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder WithLazy(string key, Func<object> valueFactory) => ErrableFactory.WithLazy(key, valueFactory);

    /// <summary>
    /// Starts a fluent builder chain with tags.
    /// </summary>
    /// <param name="tags">The tags to add</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Tags(params string[] tags) => ErrableFactory.Tags(tags);

    /// <summary>
    /// Starts a fluent builder chain with a domain.
    /// </summary>
    /// <param name="domain">The domain name</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder In(string domain) => ErrableFactory.In(domain);

    /// <summary>
    /// Starts a fluent builder chain with a public message.
    /// </summary>
    /// <param name="message">The public-facing error message</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Public(string message) => ErrableFactory.Public(message);

    /// <summary>
    /// Starts a fluent builder chain with a hint.
    /// </summary>
    /// <param name="hint">The hint for resolving the error</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Hint(string hint) => ErrableFactory.Hint(hint);

    /// <summary>
    /// Starts a fluent builder chain with an owner.
    /// </summary>
    /// <param name="owner">The owner of the error</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Owner(string owner) => ErrableFactory.Owner(owner);

    /// <summary>
    /// Starts a fluent builder chain with a trace ID.
    /// </summary>
    /// <param name="traceId">The trace ID</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Trace(string? traceId) => ErrableFactory.Trace(traceId);

    /// <summary>
    /// Starts a fluent builder chain with a span ID.
    /// </summary>
    /// <param name="spanId">The span ID</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Span(string? spanId) => ErrableFactory.Span(spanId);

    /// <summary>
    /// Starts a fluent builder chain with duration calculation from start time.
    /// </summary>
    /// <param name="start">The start time</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Since(DateTime start) => ErrableFactory.Since(start);

    /// <summary>
    /// Starts a fluent builder chain with a specific duration.
    /// </summary>
    /// <param name="duration">The duration</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Duration(TimeSpan duration) => ErrableFactory.Duration(duration);

    /// <summary>
    /// Starts a fluent builder chain with user information.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="attributes">The user attributes</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder User(string id, params (string Key, object Value)[] attributes) => ErrableFactory.User(id, attributes);

    /// <summary>
    /// Starts a fluent builder chain with tenant information.
    /// </summary>
    /// <param name="id">The tenant ID</param>
    /// <param name="attributes">The tenant attributes</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Tenant(string id, params (string Key, object Value)[] attributes) => ErrableFactory.Tenant(id, attributes);

    #endregion
}

/// <summary>
/// Backward compatibility alias for the Errable static class.
/// Provides the same short, convenient methods (E.Error, E.Code, etc.)
/// </summary>
public static class E
{
    #region Direct Error Creation

    /// <summary>
    /// Creates a simple error with the specified message.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <returns>A new Error instance</returns>
    public static IError Error(string message) => Errable.Error(message);

    /// <summary>
    /// Creates an error with a formatted message.
    /// </summary>
    /// <param name="format">The format string</param>
    /// <param name="args">The format arguments</param>
    /// <returns>A new Error instance</returns>
    public static IError Errorf(string format, params object[] args) => Errable.Errorf(format, args);

    /// <summary>
    /// Creates an error by wrapping an exception.
    /// </summary>
    /// <param name="ex">The exception to wrap</param>
    /// <param name="message">Optional additional message</param>
    /// <returns>A new Error instance</returns>
    public static IError Wrap(Exception ex, string? message = null) => Errable.Wrap(ex, message);

    /// <summary>
    /// Creates an error by wrapping an exception with a formatted message.
    /// </summary>
    /// <param name="ex">The exception to wrap</param>
    /// <param name="format">The format string</param>
    /// <param name="args">The format arguments</param>
    /// <returns>A new Error instance</returns>
    public static IError Wrapf(Exception ex, string format, params object[] args) => Errable.Wrapf(ex, format, args);

    /// <summary>
    /// Creates an error by wrapping another Errable result.
    /// </summary>
    /// <typeparam name="T">The type of the Errable</typeparam>
    /// <param name="result">The Errable to wrap</param>
    /// <returns>A new Error instance</returns>
    public static IError Wrap<T>(Errable<T> result) => Errable.Wrap(result);

    /// <summary>
    /// Creates an error by wrapping another Errable result with a formatted message.
    /// </summary>
    /// <typeparam name="T">The type of the Errable</typeparam>
    /// <param name="result">The Errable to wrap</param>
    /// <param name="format">The format string</param>
    /// <param name="args">The format arguments</param>
    /// <returns>A new Error instance</returns>
    public static IError Wrapf<T>(Errable<T> result, string format, params object[] args) => Errable.Wrapf(result, format, args);

    /// <summary>
    /// Creates an error from an exception without additional wrapping.
    /// </summary>
    /// <param name="ex">The exception</param>
    /// <returns>A new Error instance</returns>
    public static IError Except(Exception ex) => Errable.Except(ex);

    /// <summary>
    /// Creates an error from an exception with a formatted message.
    /// </summary>
    /// <param name="ex">The exception</param>
    /// <param name="format">The format string</param>
    /// <param name="args">The format arguments</param>
    /// <returns>A new Error instance</returns>
    public static IError Exceptf(Exception ex, string format, params object[] args) => Errable.Exceptf(ex, format, args);

    #endregion

    #region Type Inference Builder Entry Points

    /// <summary>
    /// Creates a type-specific fluent builder for building Errable&lt;T&gt; instances.
    /// This provides complete type inference throughout the builder chain.
    /// </summary>
    /// <typeparam name="T">The type for the final Errable result</typeparam>
    /// <returns>A new ErrableBuilder&lt;T&gt; instance for method chaining</returns>
    public static ErrableBuilder<T> For<T>() => Errable.For<T>();

    /// <summary>
    /// Creates a type-specific fluent builder for building Errable&lt;T&gt; instances with an initial error code.
    /// This provides complete type inference throughout the builder chain.
    /// </summary>
    /// <typeparam name="T">The type for the final Errable result</typeparam>
    /// <param name="code">The error code</param>
    /// <returns>A new ErrableBuilder&lt;T&gt; instance for method chaining</returns>
    public static ErrableBuilder<T> For<T>(Code code) => Errable.For<T>(code);

    /// <summary>
    /// Creates a type-specific fluent builder for building Errable&lt;T&gt; instances with an initial error code.
    /// This provides complete type inference throughout the builder chain.
    /// </summary>
    /// <typeparam name="T">The type for the final Errable result</typeparam>
    /// <param name="code">The error code as string</param>
    /// <returns>A new ErrableBuilder&lt;T&gt; instance for method chaining</returns>
    public static ErrableBuilder<T> For<T>(string code) => Errable.For<T>(code);

    #endregion

    #region Convenience Factory Methods

    /// <summary>
    /// Creates a failed Errable&lt;T&gt; with the specified code and message.
    /// This is a convenience method for simple error creation.
    /// </summary>
    /// <typeparam name="T">The type for the Errable result</typeparam>
    /// <param name="code">The error code</param>
    /// <param name="message">The error message</param>
    /// <returns>A failed Errable&lt;T&gt; instance</returns>
    public static Errable<T> ErrorFor<T>(string code, string message) => Errable.ErrorFor<T>(code, message);

    /// <summary>
    /// Creates a failed Errable&lt;T&gt; with the specified code and formatted message.
    /// This is a convenience method for simple error creation.
    /// </summary>
    /// <typeparam name="T">The type for the Errable result</typeparam>
    /// <param name="code">The error code</param>
    /// <param name="format">The format string</param>
    /// <param name="args">The format arguments</param>
    /// <returns>A failed Errable&lt;T&gt; instance</returns>
    public static Errable<T> ErrorfFor<T>(string code, string format, params object[] args) => Errable.ErrorfFor<T>(code, format, args);

    /// <summary>
    /// Creates a failed Errable&lt;T&gt; by wrapping an exception.
    /// This is a convenience method for simple exception wrapping.
    /// </summary>
    /// <typeparam name="T">The type for the Errable result</typeparam>
    /// <param name="ex">The exception to wrap</param>
    /// <param name="message">Optional additional message</param>
    /// <returns>A failed Errable&lt;T&gt; instance</returns>
    public static Errable<T> WrapFor<T>(Exception ex, string? message = null) => Errable.WrapFor<T>(ex, message);

    /// <summary>
    /// Creates a failed Errable&lt;T&gt; by wrapping an exception with a formatted message.
    /// This is a convenience method for simple exception wrapping.
    /// </summary>
    /// <typeparam name="T">The type for the Errable result</typeparam>
    /// <param name="ex">The exception to wrap</param>
    /// <param name="format">The format string</param>
    /// <param name="args">The format arguments</param>
    /// <returns>A failed Errable&lt;T&gt; instance</returns>
    public static Errable<T> WrapfFor<T>(Exception ex, string format, params object[] args) => Errable.WrapfFor<T>(ex, format, args);

    #endregion

    #region Builder Entry Points

    /// <summary>
    /// Starts a fluent builder chain with the specified error code.
    /// </summary>
    /// <param name="code">The error code</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Code(Code code) => Errable.Code(code);

    /// <summary>
    /// Starts a fluent builder chain with the specified error code.
    /// </summary>
    /// <param name="code">The error code as string</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Code(string code) => Errable.Code(code);

    /// <summary>
    /// Starts a fluent builder chain with context data.
    /// </summary>
    /// <param name="key">The context key</param>
    /// <param name="value">The context value</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder With(string key, object value) => Errable.With(key, value);

    /// <summary>
    /// Starts a fluent builder chain with multiple context data pairs.
    /// </summary>
    /// <param name="pairs">The key-value pairs to add</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder With(params (string Key, object Value)[] pairs) => Errable.With(pairs);

    /// <summary>
    /// Starts a fluent builder chain with lazy-evaluated context data.
    /// </summary>
    /// <param name="key">The context key</param>
    /// <param name="valueFactory">Function to evaluate the value</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder WithLazy(string key, Func<object> valueFactory) => Errable.WithLazy(key, valueFactory);

    /// <summary>
    /// Starts a fluent builder chain with tags.
    /// </summary>
    /// <param name="tags">The tags to add</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Tags(params string[] tags) => Errable.Tags(tags);

    /// <summary>
    /// Starts a fluent builder chain with a domain.
    /// </summary>
    /// <param name="domain">The domain name</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder In(string domain) => Errable.In(domain);

    /// <summary>
    /// Starts a fluent builder chain with a public message.
    /// </summary>
    /// <param name="message">The public-facing error message</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Public(string message) => Errable.Public(message);

    /// <summary>
    /// Starts a fluent builder chain with a hint.
    /// </summary>
    /// <param name="hint">The hint for resolving the error</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Hint(string hint) => Errable.Hint(hint);

    /// <summary>
    /// Starts a fluent builder chain with an owner.
    /// </summary>
    /// <param name="owner">The owner of the error</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Owner(string owner) => Errable.Owner(owner);

    /// <summary>
    /// Starts a fluent builder chain with a trace ID.
    /// </summary>
    /// <param name="traceId">The trace ID</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Trace(string? traceId) => Errable.Trace(traceId);

    /// <summary>
    /// Starts a fluent builder chain with a span ID.
    /// </summary>
    /// <param name="spanId">The span ID</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Span(string? spanId) => Errable.Span(spanId);

    /// <summary>
    /// Starts a fluent builder chain with duration calculation from start time.
    /// </summary>
    /// <param name="start">The start time</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Since(DateTime start) => Errable.Since(start);

    /// <summary>
    /// Starts a fluent builder chain with a specific duration.
    /// </summary>
    /// <param name="duration">The duration</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Duration(TimeSpan duration) => Errable.Duration(duration);

    /// <summary>
    /// Starts a fluent builder chain with user information.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="attributes">The user attributes</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder User(string id, params (string Key, object Value)[] attributes) => Errable.User(id, attributes);

    /// <summary>
    /// Starts a fluent builder chain with tenant information.
    /// </summary>
    /// <param name="id">The tenant ID</param>
    /// <param name="attributes">The tenant attributes</param>
    /// <returns>A new ErrableBuilder instance for method chaining</returns>
    public static ErrableBuilder Tenant(string id, params (string Key, object Value)[] attributes) => Errable.Tenant(id, attributes);

    #endregion
}