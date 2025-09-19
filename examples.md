# Errable Non-Generic Type Usage Examples

This document demonstrates the new structure after the refactoring where:
- `Erratic` is now the pure error type (non-generic, representing unpredictable errors)
- `Errable<T>` is the Result pattern struct
- `Errable` is the main static API class matching the package name
- `E` is provided as a backward compatibility alias
- All interfaces follow C# conventions with `I` prefix: `IError`, `IErrorCoder`, `IErrorCauser`, `IErrorContextProvider`

## Pure Error Type Usage

```csharp
// Create a pure error using the Errable static factory
Erratic pureError = Errable.Error("Database connection failed");

// Alternative: using the E alias for shorter syntax
Erratic pureErrorShort = E.Error("Database connection failed");

// Create a rich error with context
Erratic contextualError = Errable.Code("DB_CONNECTION_FAILED")
    .With("host", "localhost")
    .With("port", 5432)
    .Tags("database", "connectivity")
    .In("user-service")
    .Error("Failed to connect to database");

// Use the pure error type
Console.WriteLine(pureError.Error()); // "Database connection failed"

// Access rich error properties
if (contextualError is IErrorCoder coder)
{
    Console.WriteLine($"Error Code: {coder.Code}"); // "DB_CONNECTION_FAILED"
}

if (contextualError is IErrorContextProvider provider)
{
    Console.WriteLine($"Host: {provider.Context["host"]}"); // "localhost"
}
```

## Result Pattern Usage

```csharp
// Methods can return either success values or errors
public Errable<User> GetUser(int id)
{
    if (id <= 0)
    {
        return Errable.Error("Invalid user ID");
    }

    // Simulate database lookup
    if (id == 999)
    {
        return Errable.Code("USER_NOT_FOUND")
            .With("userId", id)
            .Error("User not found");
    }

    return new User { Id = id, Name = $"User{id}" };
}

// Using the result
Errable<User> userResult = GetUser(123);

if (userResult.IsSuccess)
{
    User user = userResult.Value;
    Console.WriteLine($"Found user: {user.Name}");
}
else
{
    Erratic error = userResult.Error; // Pure error type (Erratic = unpredictable)
    Console.WriteLine($"Error: {error.Error()}");
}

// Pattern matching approach
string result = userResult.Match(
    onSuccess: user => $"Hello, {user.Name}!",
    onError: error => $"Error: {error.Error()}"
);
```

## Type Clarity

The refactoring achieves better type clarity:

```csharp
// Before refactoring (confusing)
// var error = Errable.Error("message");      // What type is this?
// var result = GetSomeValue();               // What type is this?

// After refactoring (clear)
Erratic pureError = Errable.Error("Database failed");           // Pure error type (Erratic = unpredictable)
Errable<User> userResult = userService.GetUser(123);            // Result with User
Errable<string> fileResult = fileService.ReadFile(path);        // Result with string

// Variable names can now clearly indicate their purpose
if (userResult.IsError)
{
    Erratic userError = userResult.Error;  // Pure error type extracted from result
    logger.LogError(userError.Error());
}
```

## Migration from Old API

```csharp
// New primary API (clean and intuitive):
Erratic error = Errable.Error("message");

// Alternative: using the E alias for shorter syntax (backward compatibility):
Erratic errorShort = E.Error("message");

// Both approaches provide the same functionality:
// Errable.Error(), Errable.Code(), Errable.Wrap() - Main API matching package name
// E.Error(), E.Code(), E.Wrap() - Shorter alias for convenience
```

## Benefits

1. **Type Clarity**: `Erratic` (unpredictable errors) vs `Errable<T>` (results) are clearly distinguished
2. **Intuitive API**: `Errable.Error()` matches the package name for discoverability
3. **Variable Naming**: Variables can have meaningful names that match their types
4. **Flexible Usage**: Both `Errable.Error()` (main API) and `E.Error()` (short alias) available
5. **Backward Compatibility**: Existing code continues to work
6. **Result Pattern**: Clear separation between errors and success values