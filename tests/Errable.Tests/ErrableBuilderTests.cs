namespace Errable.Tests;

public class ErrableBuilderTests
{
    [Fact]
    public void ErrableBuilder_Code_ShouldSetErrorCode()
    {
        // Arrange & Act
        var error = Errable.Code("TEST_CODE").Error("Test message");

        // Assert
        Assert.IsAssignableFrom<IErrorCoder>(error);
        var coder = (IErrorCoder)error;
        Assert.Equal("TEST_CODE", coder.Code.ToString());
    }

    [Fact]
    public void ErrableBuilder_With_ShouldAddContext()
    {
        // Arrange & Act
        var error = Errable.Code("TEST")
            .With("key1", "value1")
            .With("key2", 42)
            .Error("Test message");

        // Assert
        Assert.IsAssignableFrom<IErrorContextProvider>(error);
        var contextProvider = (IErrorContextProvider)error;
        Assert.Equal("value1", contextProvider.Context["key1"]);
        Assert.Equal(42, contextProvider.Context["key2"]);
    }

    [Fact]
    public void ErrableBuilder_WithMultiple_ShouldAddAllPairs()
    {
        // Arrange & Act
        var error = Errable.Code("TEST")
            .With(("key1", "value1"), ("key2", 42), ("key3", true))
            .Error("Test message");

        // Assert
        Assert.IsAssignableFrom<IErrorContextProvider>(error);
        var contextProvider = (IErrorContextProvider)error;
        Assert.Equal("value1", contextProvider.Context["key1"]);
        Assert.Equal(42, contextProvider.Context["key2"]);
        Assert.Equal(true, contextProvider.Context["key3"]);
    }

    [Fact]
    public void ErrableBuilder_WithLazy_ShouldEvaluateOnCreation()
    {
        // Arrange
        var callCount = 0;
        object LazyValue()
        {
            callCount++;
            return "lazy-value";
        }

        // Act
        var error = Errable.Code("TEST")
            .WithLazy("lazy", LazyValue)
            .Error("Test message");

        // Assert
        Assert.Equal(1, callCount);
        Assert.IsAssignableFrom<IErrorContextProvider>(error);
        var contextProvider = (IErrorContextProvider)error;
        Assert.Equal("lazy-value", contextProvider.Context["lazy"]);
    }

    [Fact]
    public void ErrableBuilder_Tags_ShouldAddTags()
    {
        // Arrange & Act
        var error = Errable.Code("TEST")
            .Tags("tag1", "tag2", "tag3")
            .Error("Test message") as Erratic;

        // Assert
        Assert.NotNull(error);
        Assert.NotNull(error.Tags);
        Assert.Contains("tag1", error.Tags);
        Assert.Contains("tag2", error.Tags);
        Assert.Contains("tag3", error.Tags);
    }

    [Fact]
    public void ErrableBuilder_In_ShouldSetDomain()
    {
        // Arrange & Act
        var error = Errable.Code("TEST")
            .In("authentication")
            .Error("Test message") as Erratic;

        // Assert
        Assert.NotNull(error);
        Assert.Equal("authentication", error.Domain);
    }

    [Fact]
    public void ErrableBuilder_Public_ShouldSetPublicMessage()
    {
        // Arrange & Act
        var error = Errable.Code("TEST")
            .Public("Public error message")
            .Error("Internal error message") as Erratic;

        // Assert
        Assert.NotNull(error);
        Assert.Equal("Public error message", error.PublicMessage);
        Assert.Equal("Internal error message", error.Error());
    }

    [Fact]
    public void ErrableBuilder_Hint_ShouldSetHint()
    {
        // Arrange & Act
        var error = Errable.Code("TEST")
            .Hint("Try checking your configuration")
            .Error("Configuration error") as Erratic;

        // Assert
        Assert.NotNull(error);
        Assert.Equal("Try checking your configuration", error.Hint);
    }

    [Fact]
    public void ErrableBuilder_Owner_ShouldSetOwner()
    {
        // Arrange & Act
        var error = Errable.Code("TEST")
            .Owner("auth-service")
            .Error("Authentication failed") as Erratic;

        // Assert
        Assert.NotNull(error);
        Assert.Equal("auth-service", error.Owner);
    }

    [Fact]
    public void ErrableBuilder_Trace_ShouldSetTraceId()
    {
        // Arrange & Act
        var error = Errable.Code("TEST")
            .Trace("trace-12345")
            .Error("Test message") as Erratic;

        // Assert
        Assert.NotNull(error);
        Assert.Equal("trace-12345", error.TraceId);
    }

    [Fact]
    public void ErrableBuilder_Span_ShouldSetSpanId()
    {
        // Arrange & Act
        var error = Errable.Code("TEST")
            .Span("span-67890")
            .Error("Test message") as Erratic;

        // Assert
        Assert.NotNull(error);
        Assert.Equal("span-67890", error.SpanId);
    }

    [Fact]
    public void ErrableBuilder_Since_ShouldCalculateDuration()
    {
        // Arrange
        var start = DateTime.UtcNow.AddSeconds(-5);

        // Act
        var error = Errable.Code("TEST")
            .Since(start)
            .Error("Test message") as Erratic;

        // Assert
        Assert.NotNull(error);
        Assert.NotNull(error.Duration);
        Assert.True(error.Duration.Value.TotalSeconds >= 4); // Allow for some timing variance
    }

    [Fact]
    public void ErrableBuilder_Duration_ShouldSetDuration()
    {
        // Arrange
        var duration = TimeSpan.FromMinutes(5);

        // Act
        var error = Errable.Code("TEST")
            .Duration(duration)
            .Error("Test message") as Erratic;

        // Assert
        Assert.NotNull(error);
        Assert.Equal(duration, error.Duration);
    }

    [Fact]
    public void ErrableBuilder_User_ShouldSetUserInfo()
    {
        // Arrange & Act
        var error = Errable.Code("TEST")
            .User("user123", ("name", "John Doe"), ("role", "admin"))
            .Error("Test message") as Erratic;

        // Assert
        Assert.NotNull(error);
        Assert.NotNull(error.User);
        Assert.Equal("user123", error.User.Value.Id);
        Assert.Equal("John Doe", error.User.Value.Attributes["name"]);
        Assert.Equal("admin", error.User.Value.Attributes["role"]);
    }

    [Fact]
    public void ErrableBuilder_Tenant_ShouldSetTenantInfo()
    {
        // Arrange & Act
        var error = Errable.Code("TEST")
            .Tenant("tenant456", ("name", "Acme Corp"), ("plan", "enterprise"))
            .Error("Test message") as Erratic;

        // Assert
        Assert.NotNull(error);
        Assert.NotNull(error.Tenant);
        Assert.Equal("tenant456", error.Tenant.Value.Id);
        Assert.Equal("Acme Corp", error.Tenant.Value.Attributes["name"]);
        Assert.Equal("enterprise", error.Tenant.Value.Attributes["plan"]);
    }

    [Fact]
    public void ErrableBuilder_Errorf_ShouldFormatMessage()
    {
        // Arrange & Act
        var error = Errable.Code("TEST")
            .Errorf("User {0} failed to login with reason: {1}", "john", "invalid password");

        // Assert
        Assert.Equal("User john failed to login with reason: invalid password", error.Error());
    }

    [Fact]
    public void ErrableBuilder_Wrap_ShouldWrapException()
    {
        // Arrange
        var exception = new InvalidOperationException("Original exception");

        // Act
        var error = Errable.Code("TEST")
            .Wrap(exception, "Wrapped exception") as Erratic;

        // Assert
        Assert.NotNull(error);
        Assert.Equal("Wrapped exception", error.Error());
        // Context should only contain user-defined data, not hardcoded exception info
        // Check that context doesn't contain library-added exception data
        Assert.DoesNotContain("exception", error.Context.Keys);
    }

    [Fact]
    public void ErrableBuilder_Wrapf_ShouldWrapExceptionWithFormattedMessage()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");

        // Act
        var error = Errable.Code("VALIDATION")
            .Wrapf(exception, "Validation failed for parameter {0}", "username") as Erratic;

        // Assert
        Assert.NotNull(error);
        Assert.Equal("Validation failed for parameter username", error.Error());
        // Context should only contain user-defined data, not hardcoded exception info
        Assert.DoesNotContain("exception", error.Context.Keys);
    }


    [Fact]
    public void ErrableBuilder_FluentChaining_ShouldAllowComplexConfiguration()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddMinutes(-1);

        // Act
        var error = Errable.Code("COMPLEX_ERROR")
            .In("payment-processing")
            .Tags("payment", "critical")
            .With("transactionId", "tx-12345")
            .With("amount", 299.99m)
            .User("user789", ("email", "user@example.com"))
            .Tenant("tenant123", ("name", "Test Org"))
            .Trace("trace-abc123")
            .Span("span-def456")
            .Since(startTime)
            .Public("Payment processing failed")
            .Hint("Please try again or contact support")
            .Owner("payment-service")
            .Errorf("Failed to process payment of ${0} for transaction {1}", 299.99m, "tx-12345") as Erratic;

        // Assert
        Assert.NotNull(error);
        Assert.Equal("COMPLEX_ERROR", error.Code.ToString());
        Assert.Equal("payment-processing", error.Domain);
        Assert.Contains("payment", error.Tags!);
        Assert.Contains("critical", error.Tags!);
        Assert.Equal("tx-12345", error.Context["transactionId"]);
        Assert.Equal(299.99m, error.Context["amount"]);
        Assert.Equal("user789", error.User!.Value.Id);
        Assert.Equal("tenant123", error.Tenant!.Value.Id);
        Assert.Equal("trace-abc123", error.TraceId);
        Assert.Equal("span-def456", error.SpanId);
        Assert.NotNull(error.Duration);
        Assert.Equal("Payment processing failed", error.PublicMessage);
        Assert.Equal("Please try again or contact support", error.Hint);
        Assert.Equal("payment-service", error.Owner);
        Assert.Equal("Failed to process payment of $299.99 for transaction tx-12345", error.Error());
    }
}