namespace Errable.Tests;

public class ErrableFactoryTests
{
    [Fact]
    public void Errable_Error_ShouldCreateSimpleError()
    {
        // Arrange & Act
        var error = Errable.Error("Simple error message");

        // Assert
        Assert.Equal("Simple error message", error.Error());
        Assert.IsAssignableFrom<IErrorCoder>(error);
        var coder = (IErrorCoder)error;
        Assert.Equal("", coder.Code.ToString());
    }

    [Fact]
    public void Errable_Errorf_ShouldCreateFormattedError()
    {
        // Arrange & Act
        var error = Errable.Errorf("Error occurred for user {0} at {1}", "john", DateTime.Today);

        // Assert
        var expectedMessage = string.Format("Error occurred for user {0} at {1}", "john", DateTime.Today);
        Assert.Equal(expectedMessage, error.Error());
    }

    [Fact]
    public void Errable_Wrap_ShouldWrapException()
    {
        // Arrange
        var exception = new ArgumentNullException("parameter", "Value cannot be null");

        // Act
        var error = Errable.Wrap(exception) as Erratic;

        // Assert
        Assert.NotNull(error);
        Assert.Equal("Value cannot be null (Parameter 'parameter')", error.Error());
        Assert.IsAssignableFrom<IErrorCoder>(error);
        var coder = (IErrorCoder)error;
        Assert.Equal("", coder.Code.ToString());
        Assert.Empty(error.Context);
    }

    [Fact]
    public void Errable_Wrap_WithCustomMessage_ShouldUseCustomMessage()
    {
        // Arrange
        var exception = new InvalidOperationException("Original message");

        // Act
        var error = Errable.Wrap(exception, "Custom wrapper message");

        // Assert
        Assert.Equal("Custom wrapper message", error.Error());
    }

    [Fact]
    public void Errable_Wrapf_ShouldWrapWithFormattedMessage()
    {
        // Arrange
        var exception = new FileNotFoundException("File not found");

        // Act
        var error = Errable.Wrapf(exception, "Failed to load config file: {0}", "app.config");

        // Assert
        Assert.Equal("Failed to load config file: app.config", error.Error());
    }

    [Fact]
    public void Errable_WrapErrable_ShouldWrapFailedResult()
    {
        // Arrange
        var originalError = Errable.Error("Original error");
        var failedResult = Errable<int>.Wrap(originalError);

        // Act
        var wrappedError = Errable.Wrapf(failedResult, "Wrapped error");

        // Assert
        Assert.Equal("Wrapped error", wrappedError.Error());
        Assert.IsAssignableFrom<IErrorCauser>(wrappedError);
        var causer = (IErrorCauser)wrappedError;
        Assert.Equal(originalError, causer.Cause);
    }

    [Fact]
    public void Errable_WrapErrable_SuccessfulResult_ShouldReturnZeroValueError()
    {
        // Arrange
        Errable<int> successResult = 42;

        // Act
        var error = Errable.Wrap(successResult);

        // Assert - Should return zero-value error, not throw
        Assert.Equal("", error.Error());
        Assert.IsAssignableFrom<IErrorCoder>(error);
        var coder = (IErrorCoder)error;
        Assert.Equal("", coder.Code.ToString());
    }

    [Fact]
    public void Errable_WrapfErrable_ShouldWrapWithFormattedMessage()
    {
        // Arrange
        var originalError = Errable.Error("Database connection failed");
        var failedResult = Errable<string>.Wrap(originalError);

        // Act
        var wrappedError = Errable.Wrapf(failedResult, "Service {0} failed to retrieve data", "UserService");

        // Assert
        Assert.Equal("Service UserService failed to retrieve data", wrappedError.Error());
        Assert.IsAssignableFrom<IErrorCauser>(wrappedError);
        var causer = (IErrorCauser)wrappedError;
        Assert.Equal(originalError, causer.Cause);
    }

    [Fact]
    public void Errable_Except_ShouldCreateErrorFromException()
    {
        // Arrange
        var exception = new DivideByZeroException("Cannot divide by zero");

        // Act
        var error = Errable.Except(exception) as Erratic;

        // Assert
        Assert.NotNull(error);
        Assert.Equal("Cannot divide by zero", error.Error());
        Assert.IsAssignableFrom<IErrorCoder>(error);
        var coder = (IErrorCoder)error;
        Assert.Equal("", coder.Code.ToString());
        Assert.Empty(error.Context);
    }

    [Fact]
    public void Errable_Exceptf_ShouldCreateErrorWithFormattedMessage()
    {
        // Arrange
        var exception = new TimeoutException("Operation timed out");

        // Act
        var error = Errable.Exceptf(exception, "Operation {0} failed after {1} seconds", "GetUser", 30) as Erratic;

        // Assert
        Assert.NotNull(error);
        Assert.Equal("Operation GetUser failed after 30 seconds", error.Error());
        Assert.Empty(error.Context);
    }


    [Fact]
    public void Errable_Wrap_WithNestedExceptions_ShouldCreateCauseChain()
    {
        // Arrange
        var level1 = new ArgumentException("Level 1");
        var level2 = new InvalidOperationException("Level 2", level1);
        var level3 = new ApplicationException("Level 3", level2);

        // Act
        var error = Errable.Wrap(level3) as Erratic;

        // Assert
        Assert.NotNull(error);
        Assert.Equal("Level 3", error.Error());

        // Should have a cause for level2
        Assert.NotNull(error.Cause);
        Assert.Equal("Level 2", error.Cause.Error());

        // The cause should be an Erratic with its own cause
        if (error.Cause is Erratic causeImpl)
        {
            Assert.NotNull(causeImpl.Cause);
            Assert.Equal("Level 1", causeImpl.Cause.Error());
        }
    }

    [Fact]
    public void Errable_Code_ShouldStartBuilderChain()
    {
        // Arrange & Act
        var builder = Errable.Code("TEST_CODE");

        // Assert
        Assert.IsType<ErrableBuilder>(builder);

        // Verify the builder works
        var error = builder.Error("Test message");
        Assert.IsAssignableFrom<IErrorCoder>(error);
        var coder = (IErrorCoder)error;
        Assert.Equal("TEST_CODE", coder.Code.ToString());
    }
}