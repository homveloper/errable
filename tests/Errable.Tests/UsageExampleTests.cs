namespace Errable.Tests;

/// <summary>
/// Demonstrates real-world usage scenarios of the Errable library
/// </summary>
public class UsageExampleTests
{
    /// <summary>
    /// Demonstrates basic error creation and usage patterns
    /// </summary>
    [Fact]
    public void BasicUsageExample()
    {
        // Simple error creation
        IError simpleError = Errable.Error("Something went wrong");
        Assert.Equal("Something went wrong", simpleError.Error());

        // Formatted error creation
        IError formattedError = Errable.Errorf("User {0} failed operation {1}", "john", "login");
        Assert.Equal("User john failed operation login", formattedError.Error());

        // Rich error with context
        IError richError = Errable.Code("AUTH_FAILED")
            .With("userId", 123)
            .With("attemptCount", 3)
            .Tags("security", "authentication")
            .In("auth-service")
            .Errorf("Authentication failed for user {0}", "john");

        Assert.Equal("Authentication failed for user john", richError.Error());

        // Type-safe access to error properties
        if (richError is IErrorCoder coder)
        {
            Assert.Equal("AUTH_FAILED", coder.Code.ToString());
        }

        if (richError is IErrorContextProvider contextProvider)
        {
            Assert.Equal(123, contextProvider.Context["userId"]);
            Assert.Equal(3, contextProvider.Context["attemptCount"]);
        }
    }

    /// <summary>
    /// Demonstrates the Result pattern with Errable<T>
    /// </summary>
    [Fact]
    public void ResultPatternExample()
    {
        // Functions returning Errable<T>
        Errable<int> GetUserAge(string username)
        {
            if (username == "john")
                return 25; // Implicit conversion from int to Errable<int>

            // Return error using Wrap
            return Errable<int>.Wrap(
                Errable.Code("USER_NOT_FOUND")
                    .With("username", username)
                    .Errorf("User '{0}' not found", username)
            );
        }

        Errable<string> GetUserEmail(string username)
        {
            if (username == "john")
                return "john@example.com";

            return Errable.Code("USER_NOT_FOUND")
                    .With("username", username)
                    .Errorf<string>("User '{0}' not found", username);
        }

        // Improved version using extension method
        Errable<int> GetUserAgeImproved(string username)
        {
            if (username == "john")
                return 25; // Implicit conversion from int to Errable<int>

            // Direct return using generic method - much cleaner!
            return Errable.Code("USER_NOT_FOUND")
                .With("username", username)
                .Errorf<int>("User '{0}' not found", username);
        }

        // Usage with successful results
        var ageResult = GetUserAge("john");
        Assert.True(ageResult.IsSuccess);
        Assert.Equal(25, ageResult.Value);

        var emailResult = GetUserEmail("john");
        Assert.True(emailResult.IsSuccess);
        Assert.Equal("john@example.com", emailResult.Value);

        // Usage with error results
        var failedAge = GetUserAge("unknown");
        Assert.True(failedAge.IsError);
        Assert.Equal("User 'unknown' not found", failedAge.Error.Error());

        var failedEmail = GetUserEmail("unknown");
        Assert.True(failedEmail.IsError);
        Assert.Equal("User 'unknown' not found", failedEmail.Error.Error());

        // Pattern matching with Match method
        var message = failedAge.Match(
            onSuccess: age => $"Age is {age}",
            onError: error => $"Failed: {error.Error()}"
        );
        Assert.Equal("Failed: User 'unknown' not found", message);
    }

    /// <summary>
    /// Demonstrates error chaining and wrapping
    /// </summary>
    [Fact]
    public void ErrorChainingExample()
    {
        // Simulate a layered application with error propagation
        Errable<string> DatabaseQuery(string sql)
        {
            // Simulate database error
            var dbException = new InvalidOperationException("Connection timeout");
            return Errable<string>.Wrap(
                Errable.Code("DB_TIMEOUT")
                    .With("sql", sql)
                    .With("timeout", TimeSpan.FromSeconds(30))
                    .Wrap(dbException, "Database query failed")
            );
        }

        Errable<string> ServiceCall(int userId)
        {
            var dbResult = DatabaseQuery($"SELECT * FROM Users WHERE Id = {userId}");
            if (dbResult.IsError)
            {
                // Wrap the database error with service context
                return Errable<string>.Wrap(
                    Errable.Wrapf(dbResult, "Failed to retrieve user {0}", userId)
                );
            }
            return dbResult.Value;
        }

        Errable<string> ControllerAction(int userId)
        {
            var serviceResult = ServiceCall(userId);
            if (serviceResult.IsError)
            {
                // Wrap again at controller level
                return Errable<string>.Wrap(
                    Errable.Wrapf(serviceResult, "API call failed for user {0}", userId)
                );
            }
            return serviceResult.Value;
        }

        // Execute the chain
        var result = ControllerAction(123);

        // Verify error chain
        Assert.True(result.IsError);
        Assert.Equal("API call failed for user 123", result.Error.Error());

        // Navigate the error chain
        if (result.Error is IErrorCauser causer)
        {
            Assert.Equal("Failed to retrieve user 123", causer.Cause!.Error());

            if (causer.Cause is IErrorCauser serviceCauser)
            {
                Assert.Equal("Database query failed", serviceCauser.Cause!.Error());
            }
        }
    }

    /// <summary>
    /// Demonstrates error formatting for different audiences
    /// </summary>
    [Fact]
    public void ErrorFormattingExample()
    {
        var error = Errable.Code("PAYMENT_FAILED")
            .In("payment-service")
            .Tags("payment", "critical")
            .With("transactionId", "tx-12345")
            .With("amount", 99.99m)
            .With("currency", "USD")
            .User("user456", ("email", "user@example.com"), ("plan", "premium"))
            .Trace("trace-abc123")
            .Public("Payment processing failed. Please try again.")
            .Hint("Check your payment method or contact support")
            .Owner("payment-team")
            .Errorf("Payment of ${0} failed for transaction {1}", 99.99m, "tx-12345");

        // Different formatting for different purposes
        var publicMessage = $"{error:P}"; // For end users
        var logMessage = $"{error:L}"; // For logs
        var debugInfo = $"{error:D}"; // For debugging
        var fullInfo = $"{error:F}"; // For detailed analysis
        var codeAndMessage = $"{error:C}"; // For simple display

        Assert.Equal("Payment processing failed. Please try again.", publicMessage);
        Assert.Contains("PAYMENT_FAILED", logMessage);
        Assert.Contains("Stack:", debugInfo);
        Assert.Contains("Domain: payment-service", fullInfo);
        Assert.Equal("[PAYMENT_FAILED] Payment of $99.99 failed for transaction tx-12345", codeAndMessage);

        // JSON serialization for APIs
        var jsonFormat = $"{error:J}";
        Assert.Contains("\"code\":\"PAYMENT_FAILED\"", jsonFormat);
        Assert.Contains("\"transactionId\":\"tx-12345\"", jsonFormat);
    }

    /// <summary>
    /// Demonstrates advanced features like lazy evaluation and duration tracking
    /// </summary>
    [Fact]
    public void AdvancedFeaturesExample()
    {
        var startTime = DateTime.UtcNow;
        var callCount = 0;

        // Lazy evaluation - expensive computation only done when error is created
        string ExpensiveComputation()
        {
            callCount++;
            return "computed-value";
        }

        // Build error with lazy evaluation
        var error = Errable.Code("COMPUTATION_FAILED")
            .WithLazy("expensiveData", ExpensiveComputation)
            .Since(startTime) // Calculate duration from start time
            .Error("Computation failed") as Erratic;

        // Verify lazy evaluation happened
        Assert.Equal(1, callCount);
        Assert.NotNull(error);
        Assert.Contains("expensiveData", error.Context.Keys);
        Assert.Equal("computed-value", error.Context["expensiveData"]);
        Assert.NotNull(error.Duration);

        // Multiple context additions
        var multiContextError = Errable.Code("BATCH_ERROR")
            .With(
                ("batch1", "data1"),
                ("batch2", "data2"),
                ("batch3", "data3")
            )
            .Error("Batch processing failed") as Erratic;

        Assert.NotNull(multiContextError);
        Assert.Equal("data1", multiContextError.Context["batch1"]);
        Assert.Equal("data2", multiContextError.Context["batch2"]);
        Assert.Equal("data3", multiContextError.Context["batch3"]);
    }

    /// <summary>
    /// Demonstrates how the library integrates with existing .NET patterns
    /// </summary>
    [Fact]
    public void DotNetIntegrationExample()
    {
        var error = Errable.Code("INTEGRATION_TEST")
            .With("key1", "value1")
            .With("key2", 42)
            .Tags("integration", "test")
            .Error("Integration test error") as Erratic;

        Assert.NotNull(error);

        // IEnumerable<KeyValuePair> integration
        var kvpList = error.ToList();
        Assert.NotEmpty(kvpList);
        Assert.Contains(kvpList, kvp => kvp.Key == "key1" && kvp.Value.Equals("value1"));
        Assert.Contains(kvpList, kvp => kvp.Key == "key2" && kvp.Value.Equals(42));
        Assert.Contains(kvpList, kvp => kvp.Key == "code" && kvp.Value.Equals("INTEGRATION_TEST"));

        // IFormattable integration
        var formatted = string.Format("Error details: {0:F}", error);
        Assert.Contains("INTEGRATION_TEST", formatted);
        Assert.Contains("integration, test", formatted);
    }

    /// <summary>
    /// Demonstrates creating domain-specific error patterns using the Builder
    /// </summary>
    [Fact]
    public void DomainSpecificErrorsExample()
    {
        // Each domain can define their own error patterns

        // Validation errors - defined by the domain
        var validationError = Errable.Code("VALIDATION_ERROR")
            .Tags("validation")
            .With("field", "username")
            .Error("Username is required");
        Assert.Contains("validation", ((Erratic)validationError).Tags!);

        // Not found errors - customized per domain
        var notFoundError = Errable.Code("PRODUCT_NOT_FOUND")
            .With("productId", 123)
            .With("category", "electronics")
            .Tags("not-found", "product")
            .Errorf("Product {0} not found in category {1}", 123, "electronics");
        Assert.Contains("Product 123 not found in category electronics", notFoundError.Error());

        // Security errors - domain-specific codes and context
        var unauthorizedError = Errable.Code("API_KEY_INVALID")
            .Tags("security", "auth")
            .With("keyPrefix", "ak_123")
            .Error("Invalid API key");
        Assert.Equal("Invalid API key", unauthorizedError.Error());

        // Timeout errors - with specific operation context
        var timeoutError = Errable.Code("DATABASE_TIMEOUT")
            .With("operation", "getUserProfile")
            .With("timeout", TimeSpan.FromSeconds(30))
            .With("database", "postgres")
            .Tags("timeout", "database")
            .Errorf("Database operation '{0}' timed out after {1}", "getUserProfile", TimeSpan.FromSeconds(30));
        Assert.Contains("Database operation 'getUserProfile' timed out", timeoutError.Error());
    }
}