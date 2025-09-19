using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Errable;

/// <summary>
/// The concrete implementation of Error that provides rich error information and framework integration.
/// This class implements all component interfaces and provides automatic framework integration.
/// Erratic represents unpredictable, irregular errors in the system.
/// </summary>
[Serializable]
public sealed class Erratic : IError, IErrorCoder, IErrorCauser, IErrorContextProvider,
    IFormattable, ISerializable, IEnumerable<KeyValuePair<string, object>>
{
    private readonly Code _code;
    private readonly string _message;
    private readonly IReadOnlyDictionary<string, object> _context;
    private readonly IError? _cause;
    private readonly StackTrace? _stackTrace;
    private readonly DateTime _timestamp;
    private readonly string? _domain;
    private readonly IReadOnlyList<string>? _tags;
    private readonly string? _publicMessage;
    private readonly string? _hint;
    private readonly string? _owner;
    private readonly string? _traceId;
    private readonly string? _spanId;
    private readonly TimeSpan? _duration;
    private readonly (string Id, IReadOnlyDictionary<string, object> Attributes)? _user;
    private readonly (string Id, IReadOnlyDictionary<string, object> Attributes)? _tenant;

    /// <summary>
    /// Initializes a new instance of the Erratic class.
    /// </summary>
    internal Erratic(
        Code code,
        string message,
        IReadOnlyDictionary<string, object>? context = null,
        IError? cause = null,
        StackTrace? stackTrace = null,
        string? domain = null,
        IReadOnlyList<string>? tags = null,
        string? publicMessage = null,
        string? hint = null,
        string? owner = null,
        string? traceId = null,
        string? spanId = null,
        TimeSpan? duration = null,
        (string Id, IReadOnlyDictionary<string, object> Attributes)? user = null,
        (string Id, IReadOnlyDictionary<string, object> Attributes)? tenant = null)
    {
        _code = code;
        _message = message ?? throw new ArgumentNullException(nameof(message));
        _context = context ?? new Dictionary<string, object>();
        _cause = cause;
        _stackTrace = stackTrace ?? new StackTrace(skipFrames: 2, fNeedFileInfo: true);
        _timestamp = DateTime.UtcNow;
        _domain = domain;
        _tags = tags;
        _publicMessage = publicMessage;
        _hint = hint;
        _owner = owner;
        _traceId = traceId;
        _spanId = spanId;
        _duration = duration;
        _user = user;
        _tenant = tenant;
    }

    /// <summary>
    /// Deserialization constructor.
    /// </summary>
    private Erratic(SerializationInfo info, StreamingContext context)
    {
        _code = (Code)(info.GetValue(nameof(_code), typeof(Code)) ?? default(Code));
        _message = info.GetString(nameof(_message)) ?? string.Empty;
        _context = (IReadOnlyDictionary<string, object>)(info.GetValue(nameof(_context), typeof(IReadOnlyDictionary<string, object>)) ?? new Dictionary<string, object>());
        _cause = (IError?)info.GetValue(nameof(_cause), typeof(IError));
        _stackTrace = (StackTrace?)info.GetValue(nameof(_stackTrace), typeof(StackTrace));
        _timestamp = info.GetDateTime(nameof(_timestamp));
        _domain = info.GetString(nameof(_domain));
        _tags = (IReadOnlyList<string>?)info.GetValue(nameof(_tags), typeof(IReadOnlyList<string>));
        _publicMessage = info.GetString(nameof(_publicMessage));
        _hint = info.GetString(nameof(_hint));
        _owner = info.GetString(nameof(_owner));
        _traceId = info.GetString(nameof(_traceId));
        _spanId = info.GetString(nameof(_spanId));
        _duration = (TimeSpan?)info.GetValue(nameof(_duration), typeof(TimeSpan?));
        // User and tenant are complex types, simplified for serialization
        _user = null;
        _tenant = null;
    }

    #region Error Interface

    /// <summary>
    /// Returns the error message as required by the Error interface.
    /// </summary>
    /// <returns>The error message</returns>
    public string Error() => _message;

    #endregion

    #region Component Interfaces

    /// <summary>
    /// Gets the error code.
    /// </summary>
    public Code Code => _code;

    /// <summary>
    /// Gets the underlying cause of this error, if any.
    /// </summary>
    public IError? Cause => _cause;

    /// <summary>
    /// Gets the context information associated with this error.
    /// </summary>
    public IReadOnlyDictionary<string, object> Context => _context;

    #endregion

    #region Additional Properties

    /// <summary>
    /// Gets the timestamp when this error was created.
    /// </summary>
    public DateTime Timestamp => _timestamp;

    /// <summary>
    /// Gets the stack trace captured when this error was created.
    /// </summary>
    public StackTrace? StackTrace => _stackTrace;

    /// <summary>
    /// Gets the domain this error belongs to.
    /// </summary>
    public string? Domain => _domain;

    /// <summary>
    /// Gets the tags associated with this error.
    /// </summary>
    public IReadOnlyList<string>? Tags => _tags;

    /// <summary>
    /// Gets the public-facing message for this error.
    /// </summary>
    public string? PublicMessage => _publicMessage;

    /// <summary>
    /// Gets the hint for resolving this error.
    /// </summary>
    public string? Hint => _hint;

    /// <summary>
    /// Gets the owner responsible for this error.
    /// </summary>
    public string? Owner => _owner;

    /// <summary>
    /// Gets the trace ID for correlation.
    /// </summary>
    public string? TraceId => _traceId;

    /// <summary>
    /// Gets the span ID for tracking.
    /// </summary>
    public string? SpanId => _spanId;

    /// <summary>
    /// Gets the duration associated with this error.
    /// </summary>
    public TimeSpan? Duration => _duration;

    /// <summary>
    /// Gets the user information associated with this error.
    /// </summary>
    public (string Id, IReadOnlyDictionary<string, object> Attributes)? User => _user;

    /// <summary>
    /// Gets the tenant information associated with this error.
    /// </summary>
    public (string Id, IReadOnlyDictionary<string, object> Attributes)? Tenant => _tenant;

    #endregion

    #region IFormattable Implementation

    /// <summary>
    /// Formats the error according to the specified format string.
    /// </summary>
    /// <param name="format">The format string (F=Full, C=Code+Message, D=Debug, J=JSON, P=Public, L=Log)</param>
    /// <param name="formatProvider">The format provider</param>
    /// <returns>The formatted error string</returns>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return format?.ToUpperInvariant() switch
        {
            "F" or "FULL" => FormatFull(),
            "C" or "CODE" => FormatCodeAndMessage(),
            "D" or "DEBUG" => FormatDebug(),
            "J" or "JSON" => FormatJson(),
            "P" or "PUBLIC" => FormatPublic(),
            "L" or "LOG" => FormatLog(),
            _ => _message
        };
    }

    /// <summary>
    /// Returns the default string representation (message).
    /// </summary>
    /// <returns>The error message</returns>
    public override string ToString() => _message;

    #endregion

    #region ISerializable Implementation

    /// <summary>
    /// Populates a SerializationInfo with the data needed to serialize the error.
    /// </summary>
    /// <param name="info">The SerializationInfo to populate</param>
    /// <param name="context">The destination for this serialization</param>
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(_code), _code);
        info.AddValue(nameof(_message), _message);
        info.AddValue(nameof(_context), _context);
        info.AddValue(nameof(_cause), _cause);
        info.AddValue(nameof(_stackTrace), _stackTrace);
        info.AddValue(nameof(_timestamp), _timestamp);
        info.AddValue(nameof(_domain), _domain);
        info.AddValue(nameof(_tags), _tags);
        info.AddValue(nameof(_publicMessage), _publicMessage);
        info.AddValue(nameof(_hint), _hint);
        info.AddValue(nameof(_owner), _owner);
        info.AddValue(nameof(_traceId), _traceId);
        info.AddValue(nameof(_spanId), _spanId);
        info.AddValue(nameof(_duration), _duration);
    }

    #endregion

    #region IEnumerable Implementation

    /// <summary>
    /// Returns an enumerator that iterates through the error context.
    /// </summary>
    /// <returns>An enumerator for the error context</returns>
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        // Start with context
        foreach (var kvp in _context)
        {
            yield return kvp;
        }

        // Add built-in properties
        yield return new KeyValuePair<string, object>("code", _code.ToString());
        yield return new KeyValuePair<string, object>("message", _message);
        yield return new KeyValuePair<string, object>("timestamp", _timestamp);

        if (_domain != null)
            yield return new KeyValuePair<string, object>("domain", _domain);

        if (_tags != null && _tags.Count > 0)
            yield return new KeyValuePair<string, object>("tags", _tags);

        if (_publicMessage != null)
            yield return new KeyValuePair<string, object>("publicMessage", _publicMessage);

        if (_hint != null)
            yield return new KeyValuePair<string, object>("hint", _hint);

        if (_owner != null)
            yield return new KeyValuePair<string, object>("owner", _owner);

        if (_traceId != null)
            yield return new KeyValuePair<string, object>("traceId", _traceId);

        if (_spanId != null)
            yield return new KeyValuePair<string, object>("spanId", _spanId);

        if (_duration.HasValue)
            yield return new KeyValuePair<string, object>("duration", _duration.Value);

        if (_cause != null)
            yield return new KeyValuePair<string, object>("cause", _cause);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the error context.
    /// </summary>
    /// <returns>An enumerator for the error context</returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    #region Private Formatting Methods

    private string FormatFull()
    {
        var parts = new List<string> { $"[{_code}] {_message}" };

        if (_domain != null)
            parts.Add($"Domain: {_domain}");

        if (_tags != null && _tags.Count > 0)
            parts.Add($"Tags: {string.Join(", ", _tags)}");

        if (_context.Count > 0)
            parts.Add($"Context: {string.Join(", ", _context.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");

        if (_cause != null)
            parts.Add($"Caused by: {_cause.Error()}");

        return string.Join(" | ", parts);
    }

    private string FormatCodeAndMessage() => $"[{_code}] {_message}";

    private string FormatDebug()
    {
        var parts = new List<string> { FormatFull() };

        if (_stackTrace != null)
            parts.Add($"Stack: {_stackTrace}");

        return string.Join(Environment.NewLine, parts);
    }

    private string FormatJson()
    {
        var obj = new Dictionary<string, object>
        {
            ["code"] = _code.ToString(),
            ["message"] = _message,
            ["timestamp"] = _timestamp
        };

        if (_domain != null) obj["domain"] = _domain;
        if (_tags != null && _tags.Count > 0) obj["tags"] = _tags;
        if (_context.Count > 0) obj["context"] = _context;
        if (_cause != null) obj["cause"] = _cause.Error();

        return System.Text.Json.JsonSerializer.Serialize(obj);
    }

    private string FormatPublic() => _publicMessage ?? _message;

    private string FormatLog() => $"{_timestamp:yyyy-MM-dd HH:mm:ss.fff} [{_code}] {_message}";

    #endregion
}