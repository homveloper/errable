namespace Errable.Tests;

public class ErrableFormattingTests
{
    [Fact]
    public void Errable_ToString_ShouldReturnMessage()
    {
        // Arrange
        var error = Errable.Error("Simple error message");

        // Act
        var result = error.ToString();

        // Assert
        Assert.Equal("Simple error message", result);
    }

    [Fact]
    public void Errable_ToString_FullFormat_ShouldIncludeAllDetails()
    {
        // Arrange
        var error = Errable.Code("TEST_CODE")
            .In("test-domain")
            .Tags("tag1", "tag2")
            .With("key1", "value1")
            .With("key2", 42)
            .Error("Test message") as IFormattable;

        // Act
        var result = error!.ToString("F", null);

        // Assert
        Assert.Contains("[TEST_CODE] Test message", result);
        Assert.Contains("Domain: test-domain", result);
        Assert.Contains("Tags: tag1, tag2", result);
        Assert.Contains("Context: key1=value1, key2=42", result);
    }

    [Fact]
    public void Errable_ToString_CodeFormat_ShouldShowCodeAndMessage()
    {
        // Arrange
        var error = Errable.Code("ERROR_123")
            .With("context", "data")
            .Error("Error message") as IFormattable;

        // Act
        var result = error!.ToString("C", null);

        // Assert
        Assert.Equal("[ERROR_123] Error message", result);
        Assert.DoesNotContain("context", result);
    }

    [Fact]
    public void Errable_ToString_DebugFormat_ShouldIncludeStackTrace()
    {
        // Arrange
        var error = Errable.Code("DEBUG_ERROR")
            .Error("Debug message") as IFormattable;

        // Act
        var result = error!.ToString("D", null);

        // Assert
        Assert.Contains("[DEBUG_ERROR] Debug message", result);
        Assert.Contains("Stack:", result);
        Assert.Contains("ErrableFormattingTests", result); // Should contain calling method

        // Verify that the stack trace is filtered (should not contain test framework noise)
        Assert.DoesNotContain("Xunit.", result);
        Assert.DoesNotContain("System.Runtime.", result);
        Assert.DoesNotContain("Microsoft.TestPlatform.", result);
    }

    [Fact]
    public void Errable_ToString_JsonFormat_ShouldReturnValidJson()
    {
        // Arrange
        var error = Errable.Code("JSON_TEST")
            .In("json-domain")
            .Tags("json", "test")
            .With("data", "value")
            .Error("JSON test message") as IFormattable;

        // Act
        var result = error!.ToString("J", null);

        // Assert
        Assert.Contains("\"code\":\"JSON_TEST\"", result);
        Assert.Contains("\"message\":\"JSON test message\"", result);
        Assert.Contains("\"domain\":\"json-domain\"", result);
        Assert.Contains("\"tags\":[\"json\",\"test\"]", result);

        // Should be valid JSON
        var parsed = System.Text.Json.JsonDocument.Parse(result);
        Assert.NotNull(parsed);
    }

    [Fact]
    public void Errable_ToString_PublicFormat_ShouldShowPublicMessage()
    {
        // Arrange
        var error = Errable.Code("INTERNAL_ERROR")
            .Public("Something went wrong")
            .Error("Internal database connection failed") as IFormattable;

        // Act
        var result = error!.ToString("P", null);

        // Assert
        Assert.Equal("Something went wrong", result);
    }

    [Fact]
    public void Errable_ToString_PublicFormat_NoPublicMessage_ShouldFallbackToMessage()
    {
        // Arrange
        var error = Errable.Code("ERROR")
            .Error("Regular message") as IFormattable;

        // Act
        var result = error!.ToString("P", null);

        // Assert
        Assert.Equal("Regular message", result);
    }

    [Fact]
    public void Errable_ToString_LogFormat_ShouldIncludeTimestamp()
    {
        // Arrange
        var error = Errable.Code("LOG_ERROR")
            .Error("Log message") as IFormattable;

        // Act
        var result = error!.ToString("L", null);

        // Assert
        // Should match pattern: yyyy-MM-dd HH:mm:ss.fff [LOG_ERROR] Log message
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} \[LOG_ERROR\] Log message", result);
    }

    [Fact]
    public void Errable_ToString_UnknownFormat_ShouldReturnMessage()
    {
        // Arrange
        var error = Errable.Code("TEST")
            .Error("Test message") as IFormattable;

        // Act
        var result = error!.ToString("X", null);

        // Assert
        Assert.Equal("Test message", result);
    }

    [Fact]
    public void Errable_ToString_LowercaseFormat_ShouldWork()
    {
        // Arrange
        var error = Errable.Code("TEST")
            .Error("Test message") as IFormattable;

        // Act
        var fullResult = error!.ToString("full", null);
        var codeResult = error!.ToString("code", null);

        // Assert
        Assert.Contains("[TEST] Test message", fullResult);
        Assert.Equal("[TEST] Test message", codeResult);
    }

    [Fact]
    public void Errable_StringInterpolation_ShouldUseDefaultFormat()
    {
        // Arrange
        var error = Errable.Code("INTERPOLATION")
            .Error("Interpolation test");

        // Act
        var result = $"Error occurred: {error}";

        // Assert
        Assert.Equal("Error occurred: Interpolation test", result);
    }

    [Fact]
    public void Errable_StringInterpolation_WithFormat_ShouldUseSpecifiedFormat()
    {
        // Arrange
        var error = Errable.Code("FORMAT_TEST")
            .Error("Format test message");

        // Act
        var result = $"Error: {error:C}";

        // Assert
        Assert.Equal("Error: [FORMAT_TEST] Format test message", result);
    }

    [Fact]
    public void Errable_StringFormat_WithFormat_ShouldWork()
    {
        // Arrange
        var error = Errable.Code("STRING_FORMAT")
            .With("userId", 123)
            .Error("String format test");

        // Act
        var result = string.Format("Error details: {0:F}", error);

        // Assert
        Assert.Contains("Error details: [STRING_FORMAT] String format test", result);
        Assert.Contains("Context: userId=123", result);
    }
}