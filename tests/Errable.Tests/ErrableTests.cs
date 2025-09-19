namespace Errable.Tests;

public class ErrableTests
{
    /// <summary>
    /// Test implementation of Error interface for testing purposes
    /// </summary>
    private class TestError : IError
    {
        private readonly string _message;

        public TestError(string message)
        {
            _message = message;
        }

        public string Error() => _message;
    }

    [Fact]
    public void Errable_ImplicitFromValue_ShouldBeSuccess()
    {
        // Arrange
        const int value = 42;

        // Act
        Errable<int> result = value;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsError);
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void Errable_Wrap_ShouldBeError()
    {
        // Arrange
        var error = new TestError("Test error");

        // Act
        var result = Errable<int>.Wrap(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsError);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Errable_SuccessValue_AccessingError_ShouldThrow()
    {
        // Arrange
        Errable<string> result = "success";

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => result.Error);
    }

    [Fact]
    public void Errable_ErrorResult_AccessingValue_ShouldThrow()
    {
        // Arrange
        var error = new TestError("Test error");
        var result = Errable<string>.Wrap(error);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Errable_GetValueOrDefault_Success_ShouldReturnValue()
    {
        // Arrange
        const string value = "success";
        Errable<string> result = value;

        // Act
        var actualValue = result.GetValueOrDefault("default");

        // Assert
        Assert.Equal(value, actualValue);
    }

    [Fact]
    public void Errable_GetValueOrDefault_Error_ShouldReturnDefault()
    {
        // Arrange
        const string defaultValue = "default";
        var error = new TestError("Test error");
        var result = Errable<string>.Wrap(error);

        // Act
        var actualValue = result.GetValueOrDefault(defaultValue);

        // Assert
        Assert.Equal(defaultValue, actualValue);
    }

    [Fact]
    public void Errable_Match_Success_ShouldExecuteSuccessFunction()
    {
        // Arrange
        const int value = 42;
        Errable<int> result = value;
        var executed = false;

        // Act
        var returnValue = result.Match(
            onSuccess: v => { executed = true; return v * 2; },
            onError: _ => 0
        );

        // Assert
        Assert.True(executed);
        Assert.Equal(84, returnValue);
    }

    [Fact]
    public void Errable_Match_Error_ShouldExecuteErrorFunction()
    {
        // Arrange
        var error = new TestError("Test error");
        var result = Errable<int>.Wrap(error);
        var executed = false;

        // Act
        var returnValue = result.Match(
            onSuccess: v => v,
            onError: e => { executed = true; return -1; }
        );

        // Assert
        Assert.True(executed);
        Assert.Equal(-1, returnValue);
    }

    [Fact]
    public void Errable_MatchAction_Success_ShouldExecuteSuccessAction()
    {
        // Arrange
        const string value = "success";
        Errable<string> result = value;
        var successExecuted = false;
        var errorExecuted = false;

        // Act
        result.Match(
            onSuccess: v => successExecuted = true,
            onError: e => errorExecuted = true
        );

        // Assert
        Assert.True(successExecuted);
        Assert.False(errorExecuted);
    }

    [Fact]
    public void Errable_MatchAction_Error_ShouldExecuteErrorAction()
    {
        // Arrange
        var error = new TestError("Test error");
        var result = Errable<string>.Wrap(error);
        var successExecuted = false;
        var errorExecuted = false;

        // Act
        result.Match(
            onSuccess: v => successExecuted = true,
            onError: e => errorExecuted = true
        );

        // Assert
        Assert.False(successExecuted);
        Assert.True(errorExecuted);
    }

    [Fact]
    public void Errable_Wrap_NullError_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Errable<int>.Wrap(null!));
    }

    [Fact]
    public void Errable_ReferenceType_NullValue_ShouldBeValid()
    {
        // Act
        Errable<string> result = (string)null!;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }
}