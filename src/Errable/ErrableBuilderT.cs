using System.Diagnostics;

namespace Errable;

/// <summary>
/// A fluent builder for creating Errable&lt;T&gt; instances with contextual information.
/// This generic version provides complete type inference throughout the builder chain.
/// </summary>
/// <typeparam name="T">The type for the final Errable result</typeparam>
public sealed class ErrableBuilder<T>
{
    private Code _code;
    private readonly Dictionary<string, object> _context = new();
    private IError? _cause;
    private string? _domain;
    private readonly List<string> _tags = new();
    private string? _publicMessage;
    private string? _hint;
    private string? _owner;
    private string? _traceId;
    private string? _spanId;
    private TimeSpan? _duration;
    private (string Id, IReadOnlyDictionary<string, object> Attributes)? _user;
    private (string Id, IReadOnlyDictionary<string, object> Attributes)? _tenant;

    /// <summary>
    /// Initializes a new instance of the ErrableBuilder&lt;T&gt; class.
    /// </summary>
    internal ErrableBuilder()
    {
        _code = new Code("UNKNOWN");
    }

    /// <summary>
    /// Initializes a new instance of the ErrableBuilder&lt;T&gt; class with the specified error code.
    /// </summary>
    /// <param name="code">The error code</param>
    internal ErrableBuilder(Code code)
    {
        _code = code;
    }

    #region Chaining Methods

    /// <summary>
    /// Sets the error code.
    /// </summary>
    /// <param name="code">The error code</param>
    /// <returns>This builder instance for method chaining</returns>
    public ErrableBuilder<T> Code(Code code)
    {
        _code = code;
        return this;
    }

    /// <summary>
    /// Sets the error code.
    /// </summary>
    /// <param name="code">The error code as string</param>
    /// <returns>This builder instance for method chaining</returns>
    public ErrableBuilder<T> Code(string code)
    {
        _code = new Code(code);
        return this;
    }

    /// <summary>
    /// Adds a key-value pair to the error context.
    /// </summary>
    /// <param name="key">The context key</param>
    /// <param name="value">The context value</param>
    /// <returns>This builder instance for method chaining</returns>
    public ErrableBuilder<T> With(string key, object value)
    {
        _context[key] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple key-value pairs to the error context.
    /// </summary>
    /// <param name="pairs">The key-value pairs to add</param>
    /// <returns>This builder instance for method chaining</returns>
    public ErrableBuilder<T> With(params (string Key, object Value)[] pairs)
    {
        foreach (var (key, value) in pairs)
        {
            _context[key] = value;
        }
        return this;
    }

    /// <summary>
    /// Adds a lazily-evaluated value to the error context.
    /// The function will be called only when the error is created.
    /// </summary>
    /// <param name="key">The context key</param>
    /// <param name="valueFactory">A function that returns the context value</param>
    /// <returns>This builder instance for method chaining</returns>
    public ErrableBuilder<T> WithLazy(string key, Func<object> valueFactory)
    {
        _context[key] = new Lazy<object>(valueFactory);
        return this;
    }

    /// <summary>
    /// Adds tags to the error for categorization.
    /// </summary>
    /// <param name="tags">The tags to add</param>
    /// <returns>This builder instance for method chaining</returns>
    public ErrableBuilder<T> Tags(params string[] tags)
    {
        _tags.AddRange(tags);
        return this;
    }

    /// <summary>
    /// Sets the domain this error belongs to.
    /// </summary>
    /// <param name="domain">The domain name</param>
    /// <returns>This builder instance for method chaining</returns>
    public ErrableBuilder<T> In(string domain)
    {
        _domain = domain;
        return this;
    }

    /// <summary>
    /// Sets a public-facing error message that can be shown to end users.
    /// </summary>
    /// <param name="message">The public message</param>
    /// <returns>This builder instance for method chaining</returns>
    public ErrableBuilder<T> Public(string message)
    {
        _publicMessage = message;
        return this;
    }

    /// <summary>
    /// Sets a hint for resolving this error.
    /// </summary>
    /// <param name="hint">The resolution hint</param>
    /// <returns>This builder instance for method chaining</returns>
    public ErrableBuilder<T> Hint(string hint)
    {
        _hint = hint;
        return this;
    }

    /// <summary>
    /// Sets the owner responsible for this error.
    /// </summary>
    /// <param name="owner">The owner identifier</param>
    /// <returns>This builder instance for method chaining</returns>
    public ErrableBuilder<T> Owner(string owner)
    {
        _owner = owner;
        return this;
    }

    /// <summary>
    /// Sets the trace ID for request correlation.
    /// </summary>
    /// <param name="traceId">The trace ID</param>
    /// <returns>This builder instance for method chaining</returns>
    public ErrableBuilder<T> Trace(string? traceId)
    {
        _traceId = traceId;
        return this;
    }

    /// <summary>
    /// Sets the span ID for operation tracking.
    /// </summary>
    /// <param name="spanId">The span ID</param>
    /// <returns>This builder instance for method chaining</returns>
    public ErrableBuilder<T> Span(string? spanId)
    {
        _spanId = spanId;
        return this;
    }

    /// <summary>
    /// Sets the duration from a start time to now.
    /// </summary>
    /// <param name="start">The start time</param>
    /// <returns>This builder instance for method chaining</returns>
    public ErrableBuilder<T> Since(DateTime start)
    {
        _duration = DateTime.UtcNow - start;
        return this;
    }

    /// <summary>
    /// Sets the duration explicitly.
    /// </summary>
    /// <param name="duration">The duration</param>
    /// <returns>This builder instance for method chaining</returns>
    public ErrableBuilder<T> Duration(TimeSpan duration)
    {
        _duration = duration;
        return this;
    }

    /// <summary>
    /// Sets user information associated with this error.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="attributes">Additional user attributes</param>
    /// <returns>This builder instance for method chaining</returns>
    public ErrableBuilder<T> User(string id, params (string Key, object Value)[] attributes)
    {
        var attributeDict = attributes.ToDictionary(a => a.Key, a => a.Value);
        _user = (id, attributeDict);
        return this;
    }

    /// <summary>
    /// Sets tenant information associated with this error.
    /// </summary>
    /// <param name="id">The tenant ID</param>
    /// <param name="attributes">Additional tenant attributes</param>
    /// <returns>This builder instance for method chaining</returns>
    public ErrableBuilder<T> Tenant(string id, params (string Key, object Value)[] attributes)
    {
        var attributeDict = attributes.ToDictionary(a => a.Key, a => a.Value);
        _tenant = (id, attributeDict);
        return this;
    }

    /// <summary>
    /// Sets the underlying cause of this error.
    /// </summary>
    /// <param name="cause">The cause error</param>
    /// <returns>This builder instance for method chaining</returns>
    public ErrableBuilder<T> Cause(IError cause)
    {
        _cause = cause;
        return this;
    }

    #endregion

    #region Termination Methods

    /// <summary>
    /// Creates a failed Errable&lt;T&gt; with the specified message.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <returns>A failed Errable&lt;T&gt; instance</returns>
    public Errable<T> Error(string message)
    {
        return Errable<T>.Wrap(CreateError(message));
    }

    /// <summary>
    /// Creates a failed Errable&lt;T&gt; with a formatted message.
    /// </summary>
    /// <param name="format">The format string</param>
    /// <param name="args">The format arguments</param>
    /// <returns>A failed Errable&lt;T&gt; instance</returns>
    public Errable<T> Errorf(string format, params object[] args)
    {
        var message = string.Format(format, args);
        return Errable<T>.Wrap(CreateError(message));
    }

    /// <summary>
    /// Creates a failed Errable&lt;T&gt; by wrapping an exception.
    /// </summary>
    /// <param name="ex">The exception to wrap</param>
    /// <param name="message">Optional additional message</param>
    /// <returns>A failed Errable&lt;T&gt; instance</returns>
    public Errable<T> Wrap(Exception ex, string? message = null)
    {
        var errorMessage = message ?? ex.Message;

        // If there's an inner exception, create a cause
        if (ex.InnerException != null && _cause == null)
        {
            _cause = ErrableFactory.Wrap(ex.InnerException);
        }

        return Errable<T>.Wrap(CreateError(errorMessage));
    }

    /// <summary>
    /// Creates a failed Errable&lt;T&gt; by wrapping an exception with a formatted message.
    /// </summary>
    /// <param name="ex">The exception to wrap</param>
    /// <param name="format">The format string</param>
    /// <param name="args">The format arguments</param>
    /// <returns>A failed Errable&lt;T&gt; instance</returns>
    public Errable<T> Wrapf(Exception ex, string format, params object[] args)
    {
        var message = string.Format(format, args);
        return Wrap(ex, message);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Creates the final Erratic instance with all configured properties.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <returns>A new Erratic instance</returns>
    private Erratic CreateError(string message)
    {
        // Evaluate any lazy values in the context
        var finalContext = new Dictionary<string, object>();
        foreach (var kvp in _context)
        {
            if (kvp.Value is Lazy<object> lazy)
            {
                finalContext[kvp.Key] = lazy.Value;
            }
            else
            {
                finalContext[kvp.Key] = kvp.Value;
            }
        }

        return new Erratic(
            code: _code,
            message: message,
            context: finalContext,
            cause: _cause,
            stackTrace: new StackTrace(skipFrames: 1, fNeedFileInfo: true),
            domain: _domain,
            tags: _tags.Count > 0 ? _tags.AsReadOnly() : null,
            publicMessage: _publicMessage,
            hint: _hint,
            owner: _owner,
            traceId: _traceId,
            spanId: _spanId,
            duration: _duration,
            user: _user,
            tenant: _tenant
        );
    }

    #endregion
}