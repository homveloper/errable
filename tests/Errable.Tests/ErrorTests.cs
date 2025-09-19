namespace Errable.Tests;

public class ErrorTests
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
    public void Error_ShouldReturnMessage()
    {
        // Arrange
        const string expectedMessage = "Test error message";
        var error = new TestError(expectedMessage);

        // Act
        var actualMessage = error.Error();

        // Assert
        Assert.Equal(expectedMessage, actualMessage);
    }

    [Fact]
    public void Error_Interface_CanBeUsedPolymorphically()
    {
        // Arrange
        const string message1 = "First error";
        const string message2 = "Second error";
        IError[] errors =
        {
            new TestError(message1),
            new TestError(message2)
        };

        // Act & Assert
        Assert.Equal(message1, errors[0].Error());
        Assert.Equal(message2, errors[1].Error());
    }

    [Fact]
    public void Error_Interface_SupportsInheritance()
    {
        // Arrange
        IError error = new TestError("Inherited error");

        // Act
        var message = error.Error();

        // Assert
        Assert.Equal("Inherited error", message);
        Assert.IsAssignableFrom<IError>(error);
    }
}