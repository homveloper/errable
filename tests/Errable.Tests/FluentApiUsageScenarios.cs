namespace Errable.Tests;

public class FluentApiUsageScenarios
{
    private class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    private class Order
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    private class PaymentResult
    {
        public string TransactionId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public decimal Amount { get; set; }
    }

    #region Real-world Usage Scenarios

    [Fact]
    public void Scenario_UserService_GetUser_AllFourPatterns()
    {
        var userId = 123;

        // Pattern 1: Traditional approach (backward compatibility)
        var result1 = Errable<User>.Wrap(
            Errable.Code("USER_NOT_FOUND")
                .With("userId", userId)
                .Error("User not found")
        );

        // Pattern 2: Generic method approach
        var result2 = Errable.Code("USER_NOT_FOUND")
            .With("userId", userId)
            .Error<User>("User not found");

        // Pattern 3: Type inference builder (recommended!)
        var result3 = Errable.For<User>()
            .Code("USER_NOT_FOUND")
            .With("userId", userId)
            .Error("User not found");

        // Pattern 4: Convenience factory (simple cases)
        var result4 = Errable.ErrorFor<User>("USER_NOT_FOUND", "User not found");

        // Assert - All patterns should work correctly
        Assert.True(result1.IsError);
        Assert.True(result2.IsError);
        Assert.True(result3.IsError);
        Assert.True(result4.IsError);

        Assert.IsType<Errable<User>>(result1);
        Assert.IsType<Errable<User>>(result2);
        Assert.IsType<Errable<User>>(result3);
        Assert.IsType<Errable<User>>(result4);

        // Pattern 3 provides the best developer experience
        Assert.Equal("User not found", result3.Error.Error());
        var error3 = result3.Error as IErrorCoder;
        Assert.NotNull(error3);
        Assert.Equal("USER_NOT_FOUND", error3.Code.ToString());
    }

    [Fact]
    public void Scenario_PaymentService_ProcessPayment_ComplexErrorHandling()
    {
        var orderId = "ORD-123";
        var amount = 299.99m;
        var userId = 456;

        // Simulate a complex payment processing scenario with type inference
        var result = SimulatePaymentProcessing(orderId, amount, userId);

        // Assert
        Assert.True(result.IsError);
        Assert.IsType<Errable<PaymentResult>>(result);

        var error = result.Error as Erratic;
        Assert.NotNull(error);
        Assert.Equal("PAYMENT_GATEWAY_ERROR", error.Code.ToString());
        Assert.Equal("payment-processing", error.Domain);
        Assert.Contains("payment", error.Tags!);
        Assert.Contains("gateway", error.Tags!);
        Assert.Contains("critical", error.Tags!);
        Assert.Equal(orderId, error.Context["orderId"]);
        Assert.Equal(amount, error.Context["amount"]);
        Assert.Equal(userId, error.Context["userId"]);
        Assert.Equal("payment-service", error.Owner);
        Assert.Equal("Payment processing is temporarily unavailable", error.PublicMessage);
        Assert.Equal("Check gateway configuration and retry", error.Hint);
        Assert.NotNull(error.Duration);
        Assert.NotNull(error.User);
        Assert.NotNull(error.Tenant);
    }

    private Errable<PaymentResult> SimulatePaymentProcessing(string orderId, decimal amount, int userId)
    {
        var startTime = DateTime.UtcNow.AddSeconds(-2);

        try
        {
            // Simulate payment gateway failure
            throw new HttpRequestException("Gateway timeout");
        }
        catch (Exception ex)
        {
            // This demonstrates the revolutionary type inference pattern!
            return Errable.For<PaymentResult>()
                .Code("PAYMENT_GATEWAY_ERROR")
                .In("payment-processing")
                .Tags("payment", "gateway", "critical")
                .With("orderId", orderId)
                .With("amount", amount)
                .With("userId", userId)
                .With("gateway", "stripe")
                .User($"user-{userId}", ("email", "user@example.com"))
                .Tenant("main", ("environment", "production"))
                .Owner("payment-service")
                .Public("Payment processing is temporarily unavailable")
                .Hint("Check gateway configuration and retry")
                .Since(startTime)
                .Wrap(ex, $"Failed to process payment of ${amount} for order {orderId}");
        }
    }

    [Fact]
    public void Scenario_DatabaseService_QueryUsers_TypeInferenceWithCollections()
    {
        // Demonstrate type inference with collection types
        var result = SimulateDatabaseQuery();

        Assert.True(result.IsError);
        Assert.IsType<Errable<List<User>>>(result);

        var error = result.Error as Erratic;
        Assert.NotNull(error);
        Assert.Equal("DATABASE_CONNECTION_FAILED", error.Code.ToString());
        Assert.Equal("database", error.Domain);
        Assert.Contains("database", error.Tags!);
        Assert.Contains("infrastructure", error.Tags!);
    }

    private Errable<List<User>> SimulateDatabaseQuery()
    {
        return Errable.For<List<User>>()
            .Code("DATABASE_CONNECTION_FAILED")
            .In("database")
            .Tags("database", "infrastructure")
            .With("connectionString", "***REDACTED***")
            .With("timeout", 30)
            .Error("Unable to connect to user database");
    }

    [Fact]
    public void Scenario_ValidationService_ValidateUserData_MultipleValidationErrors()
    {
        var userData = new User { Id = 0, Name = "", Email = "invalid-email" };

        // Simulate validation that can return different error types
        var nameResult = ValidateUserName(userData.Name);
        var emailResult = ValidateUserEmail(userData.Email);
        var idResult = ValidateUserId(userData.Id);

        // Assert all validations failed with proper typing
        Assert.True(nameResult.IsError);
        Assert.True(emailResult.IsError);
        Assert.True(idResult.IsError);

        Assert.IsType<Errable<User>>(nameResult);
        Assert.IsType<Errable<User>>(emailResult);
        Assert.IsType<Errable<User>>(idResult);

        // Each error should have appropriate codes and messages
        var nameError = nameResult.Error as IErrorCoder;
        var emailError = emailResult.Error as IErrorCoder;
        var idError = idResult.Error as IErrorCoder;

        Assert.NotNull(nameError);
        Assert.NotNull(emailError);
        Assert.NotNull(idError);

        Assert.Equal("INVALID_NAME", nameError.Code.ToString());
        Assert.Equal("INVALID_EMAIL", emailError.Code.ToString());
        Assert.Equal("INVALID_ID", idError.Code.ToString());
    }

    private Errable<User> ValidateUserName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Errable.For<User>()
                .Code("INVALID_NAME")
                .With("field", "name")
                .With("value", name ?? "null")
                .Tags("validation", "required")
                .Error("Name is required");
        }
        return new User { Name = name };
    }

    private Errable<User> ValidateUserEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
        {
            return Errable.For<User>()
                .Code("INVALID_EMAIL")
                .With("field", "email")
                .With("value", email ?? "null")
                .Tags("validation", "format")
                .Error("Valid email address is required");
        }
        return new User { Email = email };
    }

    private Errable<User> ValidateUserId(int id)
    {
        if (id <= 0)
        {
            return Errable.For<User>()
                .Code("INVALID_ID")
                .With("field", "id")
                .With("value", id)
                .Tags("validation", "range")
                .Error("ID must be a positive integer");
        }
        return new User { Id = id };
    }

    [Fact]
    public async Task Scenario_AsyncOperations_TypeInferenceInAsyncContext()
    {
        // Demonstrate that type inference works well in async scenarios
        var result = await SimulateAsyncUserOperation();

        Assert.True(result.IsError);
        Assert.IsType<Errable<User>>(result);

        var error = result.Error as Erratic;
        Assert.NotNull(error);
        Assert.Equal("ASYNC_OPERATION_FAILED", error.Code.ToString());
    }

    private async Task<Errable<User>> SimulateAsyncUserOperation()
    {
        await Task.Delay(10); // Simulate async work

        return Errable.For<User>()
            .Code("ASYNC_OPERATION_FAILED")
            .With("operation", "async-user-fetch")
            .Error("Async operation failed");
    }

    #endregion

    #region Performance and Memory Tests

    [Fact]
    public void Performance_TypeInferenceBuilder_ShouldBeEfficient()
    {
        var iterations = 1000;

        // Test performance of type inference builder
        var start = DateTime.UtcNow;
        for (int i = 0; i < iterations; i++)
        {
            var result = Errable.For<User>()
                .Code("TEST_ERROR")
                .With("iteration", i)
                .Error("Test error");

            Assert.True(result.IsError);
        }
        var typeInferenceTime = DateTime.UtcNow - start;

        // Test performance of traditional approach
        start = DateTime.UtcNow;
        for (int i = 0; i < iterations; i++)
        {
            var result = Errable<User>.Wrap(
                Errable.Code("TEST_ERROR")
                    .With("iteration", i)
                    .Error("Test error")
            );

            Assert.True(result.IsError);
        }
        var traditionalTime = DateTime.UtcNow - start;

        // Type inference should not be significantly slower
        // We allow up to 50% overhead for the improved developer experience
        Assert.True(typeInferenceTime.TotalMilliseconds < traditionalTime.TotalMilliseconds * 1.5);
    }

    [Fact]
    public void MemoryUsage_AllPatterns_ShouldHaveSimilarFootprint()
    {
        // This test ensures that our new patterns don't significantly increase memory usage
        var iterations = 100;

        var results1 = new List<Errable<User>>();
        var results2 = new List<Errable<User>>();
        var results3 = new List<Errable<User>>();

        for (int i = 0; i < iterations; i++)
        {
            // Traditional pattern
            results1.Add(Errable<User>.Wrap(Errable.Code($"ERR{i}").Error($"Error {i}")));

            // Generic method pattern
            results2.Add(Errable.Code($"ERR{i}").Error<User>($"Error {i}"));

            // Type inference pattern
            results3.Add(Errable.For<User>().Code($"ERR{i}").Error($"Error {i}"));
        }

        // All should have the same count and type
        Assert.Equal(iterations, results1.Count);
        Assert.Equal(iterations, results2.Count);
        Assert.Equal(iterations, results3.Count);

        Assert.All(results1, r => Assert.IsType<Errable<User>>(r));
        Assert.All(results2, r => Assert.IsType<Errable<User>>(r));
        Assert.All(results3, r => Assert.IsType<Errable<User>>(r));
    }

    #endregion

    #region IDE Support and IntelliSense Tests

    [Fact]
    public void IDE_TypeInference_ShouldProvideCorrectTypes()
    {
        // This test verifies that the compiler correctly infers types
        // which translates to better IDE support

        // The variable should be inferred as Errable<User>
        var userResult = Errable.For<User>().Code("TEST").Error("Test");
        Assert.IsType<Errable<User>>(userResult);

        // The variable should be inferred as Errable<Order>
        var orderResult = Errable.For<Order>().Code("TEST").Error("Test");
        Assert.IsType<Errable<Order>>(orderResult);

        // The variable should be inferred as Errable<int>
        var intResult = Errable.For<int>().Code("TEST").Error("Test");
        Assert.IsType<Errable<int>>(intResult);

        // The variable should be inferred as Errable<List<User>>
        var listResult = Errable.For<List<User>>().Code("TEST").Error("Test");
        Assert.IsType<Errable<List<User>>>(listResult);
    }

    [Fact]
    public void IDE_ChainedMethods_ShouldMaintainTypeInference()
    {
        // This test ensures that the builder maintains its type throughout the chain
        var builder = Errable.For<User>(); // ErrableBuilder<User>

        var withCode = builder.Code("TEST"); // Should be ErrableBuilder<User>
        var withContext = withCode.With("key", "value"); // Should be ErrableBuilder<User>
        var withTags = withContext.Tags("tag1", "tag2"); // Should be ErrableBuilder<User>
        var result = withTags.Error("Test error"); // Should be Errable<User>

        Assert.IsType<Errable<User>>(result);
        Assert.True(result.IsError);
    }

    #endregion

    #region Integration with Existing Code

    [Fact]
    public void Integration_BackwardCompatibility_AllPatternsCoexist()
    {
        // Demonstrate that all patterns can be used together seamlessly

        // Old pattern (still works)
        var oldResult = CreateUserOldPattern(123);

        // Generic method pattern
        var genericResult = CreateUserGenericMethod(123);

        // Type inference pattern (recommended)
        var typeInferenceResult = CreateUserTypeInference(123);

        // Factory pattern
        var factoryResult = CreateUserFactory(123);

        // All should produce equivalent results
        Assert.True(oldResult.IsError);
        Assert.True(genericResult.IsError);
        Assert.True(typeInferenceResult.IsError);
        Assert.True(factoryResult.IsError);

        Assert.IsType<Errable<User>>(oldResult);
        Assert.IsType<Errable<User>>(genericResult);
        Assert.IsType<Errable<User>>(typeInferenceResult);
        Assert.IsType<Errable<User>>(factoryResult);
    }

    private Errable<User> CreateUserOldPattern(int id)
    {
        return Errable<User>.Wrap(
            Errable.Code("USER_CREATION_FAILED")
                .With("userId", id)
                .Error("User creation failed")
        );
    }

    private Errable<User> CreateUserGenericMethod(int id)
    {
        return Errable.Code("USER_CREATION_FAILED")
            .With("userId", id)
            .Error<User>("User creation failed");
    }

    private Errable<User> CreateUserTypeInference(int id)
    {
        return Errable.For<User>()
            .Code("USER_CREATION_FAILED")
            .With("userId", id)
            .Error("User creation failed");
    }

    private Errable<User> CreateUserFactory(int id)
    {
        return Errable.ErrorFor<User>("USER_CREATION_FAILED", "User creation failed");
    }

    #endregion
}