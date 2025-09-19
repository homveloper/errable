namespace Errable.Tests;

public class ErrableBuilderGenericTests
{
    private class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void ErrableBuilder_Error_Generic_ShouldReturnTypedErrable()
    {
        // Arrange & Act
        var result = Errable.Code("USER_NOT_FOUND")
            .With("userId", 123)
            .Error<User>("User not found");

        // Assert
        Assert.True(result.IsError);
        Assert.False(result.IsSuccess);
        Assert.IsType<Errable<User>>(result);

        var error = result.Error;
        Assert.IsAssignableFrom<IErrorCoder>(error);
        var coder = (IErrorCoder)error;
        Assert.Equal("USER_NOT_FOUND", coder.Code.ToString());
        Assert.Equal("User not found", error.Error());
    }

    [Fact]
    public void ErrableBuilder_Errorf_Generic_ShouldReturnTypedErrableWithFormattedMessage()
    {
        // Arrange & Act
        var result = Errable.Code("VALIDATION_FAILED")
            .With("field", "email")
            .Errorf<User>("Validation failed for field {0}: {1}", "email", "invalid format");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<User>>(result);
        Assert.Equal("Validation failed for field email: invalid format", result.Error.Error());
    }

    [Fact]
    public void ErrableBuilder_Wrap_Generic_ShouldReturnTypedErrableWithWrappedException()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument provided");

        // Act
        var result = Errable.Code("ARGUMENT_ERROR")
            .With("parameter", "userId")
            .Wrap<User>(exception, "Failed to validate user ID");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<User>>(result);
        Assert.Equal("Failed to validate user ID", result.Error.Error());

        var error = result.Error;
        Assert.IsAssignableFrom<IErrorCoder>(error);
        var coder = (IErrorCoder)error;
        Assert.Equal("ARGUMENT_ERROR", coder.Code.ToString());
    }

    [Fact]
    public void ErrableBuilder_Wrapf_Generic_ShouldReturnTypedErrableWithFormattedWrappedException()
    {
        // Arrange
        var exception = new InvalidOperationException("Database connection failed");

        // Act
        var result = Errable.Code("DATABASE_ERROR")
            .With("operation", "GetUser")
            .Wrapf<User>(exception, "Database operation {0} failed for user {1}", "GetUser", 123);

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<User>>(result);
        Assert.Equal("Database operation GetUser failed for user 123", result.Error.Error());
    }

    [Fact]
    public void ErrableBuilder_Generic_ShouldPreserveAllBuilderFunctionality()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddMinutes(-2);

        // Act
        var result = Errable.Code("COMPLEX_USER_ERROR")
            .In("user-service")
            .Tags("user", "validation", "critical")
            .With("userId", 456)
            .With("operation", "CreateUser")
            .User("admin123", ("role", "administrator"))
            .Tenant("tenant456", ("plan", "premium"))
            .Trace("trace-xyz789")
            .Span("span-abc123")
            .Since(startTime)
            .Public("User creation failed")
            .Hint("Please check user data and try again")
            .Owner("user-management-service")
            .Error<User>("Failed to create user with invalid data");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<User>>(result);

        var error = result.Error as Erratic;
        Assert.NotNull(error);
        Assert.Equal("COMPLEX_USER_ERROR", error.Code.ToString());
        Assert.Equal("user-service", error.Domain);
        Assert.Contains("user", error.Tags!);
        Assert.Contains("validation", error.Tags!);
        Assert.Contains("critical", error.Tags!);
        Assert.Equal(456, error.Context["userId"]);
        Assert.Equal("CreateUser", error.Context["operation"]);
        Assert.Equal("admin123", error.User!.Value.Id);
        Assert.Equal("tenant456", error.Tenant!.Value.Id);
        Assert.Equal("trace-xyz789", error.TraceId);
        Assert.Equal("span-abc123", error.SpanId);
        Assert.NotNull(error.Duration);
        Assert.Equal("User creation failed", error.PublicMessage);
        Assert.Equal("Please check user data and try again", error.Hint);
        Assert.Equal("user-management-service", error.Owner);
        Assert.Equal("Failed to create user with invalid data", error.Error());
    }

    [Fact]
    public void ErrableBuilder_Generic_WithInnerException_ShouldCreateCause()
    {
        // Arrange
        var innerException = new ArgumentNullException("email");
        var outerException = new InvalidOperationException("User validation failed", innerException);

        // Act
        var result = Errable.Code("USER_VALIDATION_ERROR")
            .Wrap<User>(outerException);

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<User>>(result);

        var error = result.Error as Erratic;
        Assert.NotNull(error);
        Assert.NotNull(error.Cause);
        Assert.Equal("User validation failed", error.Error());
    }

    [Fact]
    public void ErrableBuilder_Generic_ShouldWorkWithValueTypes()
    {
        // Act
        var result = Errable.Code("PARSE_ERROR")
            .Error<int>("Failed to parse integer");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<int>>(result);
        Assert.Equal("Failed to parse integer", result.Error.Error());
    }

    [Fact]
    public void ErrableBuilder_Generic_ShouldWorkWithNullableTypes()
    {
        // Act
        var result = Errable.Code("NULL_VALUE")
            .Error<int?>("Value cannot be null");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<int?>>(result);
        Assert.Equal("Value cannot be null", result.Error.Error());
    }

    [Fact]
    public void ErrableBuilder_Generic_ShouldWorkWithCollectionTypes()
    {
        // Act
        var result = Errable.Code("EMPTY_COLLECTION")
            .Error<List<User>>("User collection is empty");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<List<User>>>(result);
        Assert.Equal("User collection is empty", result.Error.Error());
    }

    [Fact]
    public void ErrableBuilder_Generic_WithLazy_ShouldEvaluateOnCreation()
    {
        // Arrange
        var evaluationCount = 0;
        object LazyExpensiveComputation()
        {
            evaluationCount++;
            return $"computed-value-{evaluationCount}";
        }

        // Act
        var result = Errable.Code("COMPUTATION_ERROR")
            .WithLazy("computation", LazyExpensiveComputation)
            .Error<User>("Computation failed");

        // Assert
        Assert.Equal(1, evaluationCount);
        Assert.True(result.IsError);

        var error = result.Error;
        Assert.IsAssignableFrom<IErrorContextProvider>(error);
        var contextProvider = (IErrorContextProvider)error;
        Assert.Equal("computed-value-1", contextProvider.Context["computation"]);
    }
}