namespace Errable.Tests;

public class ErrableBuilderTTests
{
    private class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class Product
    {
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    [Fact]
    public void ErrableFor_WithoutCode_ShouldCreateTypedBuilder()
    {
        // Act
        var result = Errable.For<User>()
            .Code("USER_ERROR")
            .Error("User operation failed");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<User>>(result);

        var error = result.Error;
        Assert.IsAssignableFrom<IErrorCoder>(error);
        var coder = (IErrorCoder)error;
        Assert.Equal("USER_ERROR", coder.Code.ToString());
        Assert.Equal("User operation failed", error.Error());
    }

    [Fact]
    public void ErrableFor_WithCodeObject_ShouldCreateTypedBuilderWithCode()
    {
        // Arrange
        var code = new Code("PRODUCT_ERROR");

        // Act
        var result = Errable.For<Product>(code)
            .With("sku", "ABC123")
            .Error("Product not found");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<Product>>(result);

        var error = result.Error;
        Assert.IsAssignableFrom<IErrorCoder>(error);
        var coder = (IErrorCoder)error;
        Assert.Equal("PRODUCT_ERROR", coder.Code.ToString());
        Assert.Equal("Product not found", error.Error());
    }

    [Fact]
    public void ErrableFor_WithCodeString_ShouldCreateTypedBuilderWithCode()
    {
        // Act
        var result = Errable.For<User>("USER_NOT_FOUND")
            .With("userId", 123)
            .Error("User not found");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<User>>(result);

        var error = result.Error;
        Assert.IsAssignableFrom<IErrorCoder>(error);
        var coder = (IErrorCoder)error;
        Assert.Equal("USER_NOT_FOUND", coder.Code.ToString());
    }

    [Fact]
    public void ErrableBuilderT_AllChainingMethods_ShouldReturnSameType()
    {
        // Arrange
        var builder = Errable.For<User>();

        // Act & Assert - All chaining methods should return ErrableBuilder<User>
        var builder1 = builder.Code("TEST");
        Assert.IsType<ErrableBuilder<User>>(builder1);

        var builder2 = builder1.Code(new Code("TEST2"));
        Assert.IsType<ErrableBuilder<User>>(builder2);

        var builder3 = builder2.With("key", "value");
        Assert.IsType<ErrableBuilder<User>>(builder3);

        var builder4 = builder3.With(("k1", "v1"), ("k2", "v2"));
        Assert.IsType<ErrableBuilder<User>>(builder4);

        var builder5 = builder4.WithLazy("lazy", () => "lazy-value");
        Assert.IsType<ErrableBuilder<User>>(builder5);

        var builder6 = builder5.Tags("tag1", "tag2");
        Assert.IsType<ErrableBuilder<User>>(builder6);

        var builder7 = builder6.In("domain");
        Assert.IsType<ErrableBuilder<User>>(builder7);

        var builder8 = builder7.Public("Public message");
        Assert.IsType<ErrableBuilder<User>>(builder8);

        var builder9 = builder8.Hint("Hint message");
        Assert.IsType<ErrableBuilder<User>>(builder9);

        var builder10 = builder9.Owner("owner");
        Assert.IsType<ErrableBuilder<User>>(builder10);

        var builder11 = builder10.Trace("trace-id");
        Assert.IsType<ErrableBuilder<User>>(builder11);

        var builder12 = builder11.Span("span-id");
        Assert.IsType<ErrableBuilder<User>>(builder12);

        var builder13 = builder12.Since(DateTime.UtcNow.AddMinutes(-1));
        Assert.IsType<ErrableBuilder<User>>(builder13);

        var builder14 = builder13.Duration(TimeSpan.FromMinutes(1));
        Assert.IsType<ErrableBuilder<User>>(builder14);

        var builder15 = builder14.User("user123", ("role", "admin"));
        Assert.IsType<ErrableBuilder<User>>(builder15);

        var builder16 = builder15.Tenant("tenant456", ("plan", "pro"));
        Assert.IsType<ErrableBuilder<User>>(builder16);
    }

    [Fact]
    public void ErrableBuilderT_Error_ShouldReturnTypedErrable()
    {
        // Act
        var result = Errable.For<User>()
            .Code("USER_ERROR")
            .Error("User operation failed");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<User>>(result);
        Assert.Equal("User operation failed", result.Error.Error());
    }

    [Fact]
    public void ErrableBuilderT_Errorf_ShouldReturnTypedErrableWithFormattedMessage()
    {
        // Act
        var result = Errable.For<User>()
            .Code("USER_ERROR")
            .Errorf("User {0} failed operation {1}", "john.doe", "login");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<User>>(result);
        Assert.Equal("User john.doe failed operation login", result.Error.Error());
    }

    [Fact]
    public void ErrableBuilderT_Wrap_ShouldReturnTypedErrableWithWrappedException()
    {
        // Arrange
        var exception = new InvalidOperationException("Database error");

        // Act
        var result = Errable.For<User>()
            .Code("DATABASE_ERROR")
            .Wrap(exception, "Failed to fetch user");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<User>>(result);
        Assert.Equal("Failed to fetch user", result.Error.Error());
    }

    [Fact]
    public void ErrableBuilderT_Wrapf_ShouldReturnTypedErrableWithFormattedWrappedException()
    {
        // Arrange
        var exception = new ArgumentException("Invalid input");

        // Act
        var result = Errable.For<User>()
            .Code("VALIDATION_ERROR")
            .Wrapf(exception, "Validation failed for field {0}", "email");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<User>>(result);
        Assert.Equal("Validation failed for field email", result.Error.Error());
    }

    [Fact]
    public void ErrableBuilderT_CompleteTypeInferenceExample_ShouldWorkSeamlessly()
    {
        // This demonstrates the complete type inference pattern from the plan
        var startTime = DateTime.UtcNow.AddSeconds(-30);

        // Act - This is the revolutionary usage pattern!
        var result = Errable.For<User>()
            .Code("USER_CREATION_FAILED")
            .With("userId", 789)
            .With("operation", "CreateUser")
            .Tags("user", "creation", "validation")
            .In("user-management")
            .Public("User registration failed")
            .Hint("Please check your input and try again")
            .Owner("user-service")
            .Trace("trace-create-user-789")
            .Span("span-validation-check")
            .Since(startTime)
            .User("admin", ("role", "system"))
            .Tenant("main", ("environment", "production"))
            .Error("Failed to create user due to validation errors"); // 🚀 Complete type inference!

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<User>>(result);

        var error = result.Error as Erratic;
        Assert.NotNull(error);
        Assert.Equal("USER_CREATION_FAILED", error.Code.ToString());
        Assert.Equal("user-management", error.Domain);
        Assert.Contains("user", error.Tags!);
        Assert.Contains("creation", error.Tags!);
        Assert.Contains("validation", error.Tags!);
        Assert.Equal(789, error.Context["userId"]);
        Assert.Equal("CreateUser", error.Context["operation"]);
        Assert.Equal("User registration failed", error.PublicMessage);
        Assert.Equal("Please check your input and try again", error.Hint);
        Assert.Equal("user-service", error.Owner);
        Assert.Equal("trace-create-user-789", error.TraceId);
        Assert.Equal("span-validation-check", error.SpanId);
        Assert.NotNull(error.Duration);
        Assert.Equal("admin", error.User!.Value.Id);
        Assert.Equal("main", error.Tenant!.Value.Id);
        Assert.Equal("Failed to create user due to validation errors", error.Error());
    }

    [Fact]
    public void EAlias_For_ShouldWorkIdenticallyToErrable()
    {
        // Act - Using the E alias
        var result1 = E.For<User>()
            .Code("TEST_ERROR")
            .Error("Test message");

        var result2 = Errable.For<User>()
            .Code("TEST_ERROR")
            .Error("Test message");

        // Assert
        Assert.True(result1.IsError);
        Assert.True(result2.IsError);
        Assert.IsType<Errable<User>>(result1);
        Assert.IsType<Errable<User>>(result2);
        Assert.Equal(result1.Error.Error(), result2.Error.Error());
    }

    [Fact]
    public void ErrableBuilderT_WithValueTypes_ShouldWorkCorrectly()
    {
        // Act
        var result = Errable.For<int>()
            .Code("PARSE_ERROR")
            .Error("Failed to parse integer");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<int>>(result);
        Assert.Equal("Failed to parse integer", result.Error.Error());
    }

    [Fact]
    public void ErrableBuilderT_WithNullableTypes_ShouldWorkCorrectly()
    {
        // Act
        var result = Errable.For<DateTime?>()
            .Code("DATE_PARSE_ERROR")
            .Error("Failed to parse date");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<DateTime?>>(result);
        Assert.Equal("Failed to parse date", result.Error.Error());
    }

    [Fact]
    public void ErrableBuilderT_WithCollectionTypes_ShouldWorkCorrectly()
    {
        // Act
        var result = Errable.For<List<Product>>()
            .Code("EMPTY_PRODUCTS")
            .Error("No products found");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<List<Product>>>(result);
        Assert.Equal("No products found", result.Error.Error());
    }

    [Fact]
    public void ErrableBuilderT_WithGenericTypes_ShouldWorkCorrectly()
    {
        // Act
        var result = Errable.For<Dictionary<string, User>>()
            .Code("CACHE_ERROR")
            .Error("Failed to load user cache");

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<Dictionary<string, User>>>(result);
        Assert.Equal("Failed to load user cache", result.Error.Error());
    }

    [Fact]
    public void ErrableBuilderT_WithLazyEvaluation_ShouldWorkCorrectly()
    {
        // Arrange
        var callCount = 0;
        object ExpensiveOperation()
        {
            callCount++;
            return $"expensive-result-{callCount}";
        }

        // Act
        var result = Errable.For<User>()
            .Code("OPERATION_FAILED")
            .WithLazy("expensiveData", ExpensiveOperation)
            .Error("Operation failed");

        // Assert
        Assert.Equal(1, callCount);
        Assert.True(result.IsError);

        var error = result.Error;
        Assert.IsAssignableFrom<IErrorContextProvider>(error);
        var contextProvider = (IErrorContextProvider)error;
        Assert.Equal("expensive-result-1", contextProvider.Context["expensiveData"]);
    }
}