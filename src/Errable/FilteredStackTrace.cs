using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Errable
{

/// <summary>
/// A filtered version of StackTrace that excludes test framework and system noise,
/// showing only relevant user code and library code.
/// </summary>
[Serializable]
public sealed class FilteredStackTrace : ISerializable
{
    private readonly StackFrame[] _frames;

    /// <summary>
    /// Namespaces to exclude from the stack trace to reduce noise.
    /// </summary>
    private static readonly string[] ExcludedNamespaces =
    {
        "Xunit.",
        "System.Runtime.",
        "System.Threading.",
        "Microsoft.TestPlatform.",
        "Microsoft.VisualStudio.",
        "System.Reflection.",
        "System.RuntimeMethodHandle",
        "System.Reflection.MethodBaseInvoker"
    };

    /// <summary>
    /// Errable library namespaces to exclude from stack trace to show only user code.
    /// </summary>
    private static readonly string[] ExcludedLibraryNamespaces =
    {
        "Errable.ErrableFactory",
        "Errable.ErrableBuilder",
        "Errable.Erratic"
    };

    /// <summary>
    /// Additional method names to exclude from the stack trace.
    /// </summary>
    private static readonly string[] ExcludedMethods =
    {
        "InvokeMethod",
        "InvokeWithNoArgs",
        "CallTestMethod",
        "StartCallback"
    };

    /// <summary>
    /// Initializes a new instance of FilteredStackTrace by capturing and filtering the current stack.
    /// </summary>
    /// <param name="skipFrames">The number of frames to skip from the beginning of the stack</param>
    public FilteredStackTrace(int skipFrames = 2)
    {
        var original = new StackTrace(skipFrames, fNeedFileInfo: true);
        _frames = FilterFrames(original.GetFrames());
    }

    /// <summary>
    /// Initializes a new instance from an existing StackTrace by filtering its frames.
    /// </summary>
    /// <param name="original">The original StackTrace to filter</param>
    public FilteredStackTrace(StackTrace original)
    {
        _frames = FilterFrames(original.GetFrames());
    }

    /// <summary>
    /// Deserialization constructor.
    /// </summary>
    private FilteredStackTrace(SerializationInfo info, StreamingContext context)
    {
        _frames = (StackFrame[])(info.GetValue(nameof(_frames), typeof(StackFrame[])) ?? Array.Empty<StackFrame>());
    }

    /// <summary>
    /// Gets the filtered stack frames.
    /// </summary>
    /// <returns>Array of filtered StackFrame objects</returns>
    public StackFrame[] GetFrames() => _frames;

    /// <summary>
    /// Gets the number of frames in the filtered stack trace.
    /// </summary>
    public int FrameCount => _frames.Length;

    /// <summary>
    /// Filters stack frames to exclude test framework and system noise.
    /// </summary>
    /// <param name="frames">The original stack frames</param>
    /// <returns>Filtered array of stack frames</returns>
    private static StackFrame[] FilterFrames(StackFrame[]? frames)
    {
        if (frames == null || frames.Length == 0)
            return Array.Empty<StackFrame>();

        var filteredFrames = new List<StackFrame>();

        foreach (var frame in frames)
        {
            var method = frame.GetMethod();
            if (method?.DeclaringType == null)
                continue;

            var typeName = method.DeclaringType.FullName ?? "";
            var methodName = method.Name;

            // Skip if the type is in an excluded namespace
            if (ExcludedNamespaces.Any(ns => typeName.StartsWith(ns, StringComparison.OrdinalIgnoreCase)))
                continue;

            // Skip Errable library internal code (but keep test code)
            if (ExcludedLibraryNamespaces.Any(ns => typeName.StartsWith(ns, StringComparison.OrdinalIgnoreCase)) &&
                !typeName.Contains(".Tests."))
                continue;

            // Skip if the method name is in the excluded list
            if (ExcludedMethods.Any(excluded => methodName.Contains(excluded, StringComparison.OrdinalIgnoreCase)))
                continue;

            // Skip compiler-generated methods
            if (methodName.Contains("<") || methodName.Contains("$"))
                continue;

            // Skip async state machine methods
            if (typeName.Contains("__DisplayClass") || typeName.Contains("StateMachine"))
                continue;

            filteredFrames.Add(frame);
        }

        return filteredFrames.ToArray();
    }

    /// <summary>
    /// Returns a formatted string representation of the filtered stack trace.
    /// </summary>
    /// <returns>Formatted stack trace string</returns>
    public override string ToString()
    {
        if (_frames.Length == 0)
            return "";

        var sb = new StringBuilder();

        foreach (var frame in _frames)
        {
            var method = frame.GetMethod();
            if (method?.DeclaringType == null)
                continue;

            var fileName = frame.GetFileName();
            var lineNumber = frame.GetFileLineNumber();

            sb.Append("   at ");
            sb.Append(method.DeclaringType.FullName);
            sb.Append('.');
            sb.Append(method.Name);
            sb.Append("()");

            if (!string.IsNullOrEmpty(fileName))
            {
                sb.Append(" in ");
                sb.Append(fileName);
                if (lineNumber > 0)
                {
                    sb.Append(":line ");
                    sb.Append(lineNumber);
                }
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Determines whether the filtered stack trace is empty.
    /// </summary>
    public bool IsEmpty => _frames.Length == 0;

    /// <summary>
    /// Implements ISerializable for custom serialization.
    /// </summary>
    /// <param name="info">Serialization info</param>
    /// <param name="context">Streaming context</param>
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(_frames), _frames);
    }

    /// <summary>
    /// Gets a specific frame by index.
    /// </summary>
    /// <param name="index">The frame index</param>
    /// <returns>The StackFrame at the specified index</returns>
    public StackFrame? GetFrame(int index)
    {
        if (index < 0 || index >= _frames.Length)
            return null;

        return _frames[index];
    }
}
}