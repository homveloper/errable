namespace Errable.Tests;

public class FluentApiFactoryTests
{
    private class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class Order
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    [Fact]
    public void ErrorFor_ShouldCreateTypedErrableWithCodeAndMessage()
    {
        // Act
        var result = Errable.ErrorFor<User>("USER_NOT_FOUND", "User does not exist");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<User>>(result);

        var error = result.Error;
        Assert.IsAssignableFrom<IErrorCoder>(error);
        var coder = (IErrorCoder)error;
        Assert.Equal("USER_NOT_FOUND", coder.Code.ToString());
        Assert.Equal("User does not exist", error.Error());
    }

    [Fact]
    public void ErrorfFor_ShouldCreateTypedErrableWithFormattedMessage()
    {
        // Act
        var result = Errable.ErrorfFor<User>("USER_NOT_FOUND", "User with ID {0} does not exist in {1}", 123, "database");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<User>>(result);

        var error = result.Error;
        Assert.IsAssignableFrom<IErrorCoder>(error);
        var coder = (IErrorCoder)error;
        Assert.Equal("USER_NOT_FOUND", coder.Code.ToString());
        Assert.Equal("User with ID 123 does not exist in database", error.Error());
    }

    [Fact]
    public void WrapFor_ShouldCreateTypedErrableFromException()
    {
        // Arrange
        var exception = new InvalidOperationException("Database connection failed");

        // Act
        var result = Errable.WrapFor<User>(exception);

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<User>>(result);
        Assert.Equal("Database connection failed", result.Error.Error());
    }

    [Fact]
    public void WrapFor_WithCustomMessage_ShouldCreateTypedErrableWithCustomMessage()
    {
        // Arrange
        var exception = new ArgumentException("Invalid parameter");

        // Act
        var result = Errable.WrapFor<User>(exception, "Failed to validate user input");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<User>>(result);
        Assert.Equal("Failed to validate user input", result.Error.Error());
    }

    [Fact]
    public void WrapfFor_ShouldCreateTypedErrableWithFormattedExceptionMessage()
    {
        // Arrange
        var exception = new TimeoutException("Operation timed out");

        // Act
        var result = Errable.WrapfFor<Order>(exception, "Order processing timed out for order {0} after {1} seconds", "ORD-123", 30);

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<Order>>(result);
        Assert.Equal("Order processing timed out for order ORD-123 after 30 seconds", result.Error.Error());
    }

    [Fact]
    public void EAlias_ErrorFor_ShouldWorkIdenticallyToErrable()
    {
        // Act
        var result1 = E.ErrorFor<User>("TEST_ERROR", "Test message");
        var result2 = Errable.ErrorFor<User>("TEST_ERROR", "Test message");

        // Assert
        Assert.True(result1.IsError);
        Assert.True(result2.IsError);
        Assert.IsType<Errable<User>>(result1);
        Assert.IsType<Errable<User>>(result2);
        Assert.Equal(result1.Error.Error(), result2.Error.Error());

        var error1 = result1.Error as IErrorCoder;
        var error2 = result2.Error as IErrorCoder;
        Assert.NotNull(error1);
        Assert.NotNull(error2);
        Assert.Equal(error1.Code.ToString(), error2.Code.ToString());
    }

    [Fact]
    public void EAlias_ErrorfFor_ShouldWorkIdenticallyToErrable()
    {
        // Act
        var result1 = E.ErrorfFor<User>("TEST_ERROR", "Test message for {0}", "john");
        var result2 = Errable.ErrorfFor<User>("TEST_ERROR", "Test message for {0}", "john");

        // Assert
        Assert.True(result1.IsError);
        Assert.True(result2.IsError);
        Assert.Equal(result1.Error.Error(), result2.Error.Error());
    }

    [Fact]
    public void EAlias_WrapFor_ShouldWorkIdenticallyToErrable()
    {
        // Arrange
        var exception = new Exception("Test exception");

        // Act
        var result1 = E.WrapFor<User>(exception, "Wrapped message");
        var result2 = Errable.WrapFor<User>(exception, "Wrapped message");

        // Assert
        Assert.True(result1.IsError);
        Assert.True(result2.IsError);
        Assert.Equal(result1.Error.Error(), result2.Error.Error());
    }

    [Fact]
    public void EAlias_WrapfFor_ShouldWorkIdenticallyToErrable()
    {
        // Arrange
        var exception = new Exception("Test exception");

        // Act
        var result1 = E.WrapfFor<User>(exception, "Wrapped message for {0}", "john");
        var result2 = Errable.WrapfFor<User>(exception, "Wrapped message for {0}", "john");

        // Assert
        Assert.True(result1.IsError);
        Assert.True(result2.IsError);
        Assert.Equal(result1.Error.Error(), result2.Error.Error());
    }

    [Fact]
    public void FactoryMethods_WithValueTypes_ShouldWorkCorrectly()
    {
        // Act
        var result = Errable.ErrorFor<int>("PARSE_ERROR", "Failed to parse integer");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<int>>(result);
        Assert.Equal("Failed to parse integer", result.Error.Error());
    }

    [Fact]
    public void FactoryMethods_WithNullableTypes_ShouldWorkCorrectly()
    {
        // Act
        var result = Errable.ErrorFor<DateTime?>("DATE_ERROR", "Invalid date format");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<DateTime?>>(result);
        Assert.Equal("Invalid date format", result.Error.Error());
    }

    [Fact]
    public void FactoryMethods_WithCollectionTypes_ShouldWorkCorrectly()
    {
        // Act
        var result = Errable.ErrorFor<List<User>>("EMPTY_LIST", "User list is empty");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<List<User>>>(result);
        Assert.Equal("User list is empty", result.Error.Error());
    }

    [Fact]
    public void FactoryMethods_WithGenericTypes_ShouldWorkCorrectly()
    {
        // Act
        var result = Errable.ErrorFor<Dictionary<string, Order>>("CACHE_MISS", "Order not found in cache");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<Dictionary<string, Order>>>(result);
        Assert.Equal("Order not found in cache", result.Error.Error());
    }

    [Fact]
    public void FactoryMethods_PerformanceComparison_ShouldBeEfficient()
    {
        // This test demonstrates that factory methods should have similar performance
        // to the traditional builder pattern for simple cases

        var iterations = 1000;

        // Measure factory method
        var start1 = DateTime.UtcNow;
        for (int i = 0; i < iterations; i++)
        {
            var result = Errable.ErrorFor<User>("TEST", "Test message");
            Assert.True(result.IsError);
        }
        var factoryTime = DateTime.UtcNow - start1;

        // Measure builder pattern
        var start2 = DateTime.UtcNow;
        for (int i = 0; i < iterations; i++)
        {
            var result = Errable.Code("TEST").Error<User>("Test message");
            Assert.True(result.IsError);
        }
        var builderTime = DateTime.UtcNow - start2;

        // Factory method should be reasonably efficient
        // We allow for some overhead but not more than 10x slower
        Assert.True(factoryTime.TotalMilliseconds < builderTime.TotalMilliseconds * 10);
    }

    [Fact]
    public void WrapFor_WithInnerException_ShouldPreserveCause()
    {
        // Arrange
        var innerException = new ArgumentNullException("param");
        var outerException = new InvalidOperationException("Outer error", innerException);

        // Act
        var result = Errable.WrapFor<User>(outerException);

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<User>>(result);

        var error = result.Error as Erratic;
        Assert.NotNull(error);
        Assert.NotNull(error.Cause);
        Assert.Equal("Outer error", error.Error());
    }

    [Fact]
    public void FactoryMethods_ShouldProduceEquivalentResultsToBuilderMethods()
    {
        // Act - Create errors using factory methods and builder methods
        var factoryResult = Errable.ErrorFor<User>("USER_ERROR", "User operation failed");
        var builderResult = Errable.Code("USER_ERROR").Error<User>("User operation failed");

        // Assert - Both should produce equivalent errors
        Assert.True(factoryResult.IsError);
        Assert.True(builderResult.IsError);
        Assert.IsType<Errable<User>>(factoryResult);
        Assert.IsType<Errable<User>>(builderResult);
        Assert.Equal(factoryResult.Error.Error(), builderResult.Error.Error());

        var factoryError = factoryResult.Error as IErrorCoder;
        var builderError = builderResult.Error as IErrorCoder;
        Assert.NotNull(factoryError);
        Assert.NotNull(builderError);
        Assert.Equal(factoryError.Code.ToString(), builderError.Code.ToString());
    }
}