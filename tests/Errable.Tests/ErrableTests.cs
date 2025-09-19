using Xunit.Abstractions;

namespace Errable.Tests;

public class ErrableTests
{
    private readonly ITestOutputHelper _output;

    public ErrableTests(ITestOutputHelper output)
    {
        _output = output;
    }
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

    [Fact]
    public void TestErrorLogging_WithStackTrace()
    {
        // Arrange - Erratic 에러 객체 생성 (스택 트레이스 자동 캡처)
        var erraticError = ErrableFactory.Error("Test Erratic error with captured stack trace");
        var erraticResult = Errable<int>.Wrap(erraticError);

        _output.WriteLine($"=== ERRATIC ERROR WITH STACK TRACE ===");
        _output.WriteLine($"Error Message: {erraticResult.Error.Error()}");
        _output.WriteLine($"Error Type: {erraticResult.Error.GetType().Name}");

        // Erratic의 StackTrace 확인 및 출력
        if (erraticResult.Error is Erratic erratic)
        {
            _output.WriteLine($"Has StackTrace: {erratic.StackTrace != null}");

            // Debug 포맷으로 스택 트레이스 포함 전체 정보 출력
            _output.WriteLine($"\nDEBUG FORMAT (with StackTrace):");
            _output.WriteLine(erratic.ToString("D", null));

            // 직접 StackTrace 속성 접근
            _output.WriteLine($"\nDIRECT STACK TRACE ACCESS:");
            _output.WriteLine(erratic.StackTrace?.ToString());
        }

        // Assert
        Assert.True(erraticResult.IsError);
        Assert.IsType<Erratic>(erraticResult.Error);

        // Verify that the stack trace is filtered (should not contain xUnit frames)
        if (erraticResult.Error is Erratic erraticForVerification)
        {
            var stackTraceString = erraticForVerification.StackTrace?.ToString() ?? "";
            Assert.DoesNotContain("Xunit.", stackTraceString);
            Assert.DoesNotContain("System.Runtime.", stackTraceString);
            Assert.DoesNotContain("Microsoft.TestPlatform.", stackTraceString);
            _output.WriteLine($"\nFILTERED STACK TRACE VERIFICATION PASSED:");
            _output.WriteLine($"No test framework noise detected in stack trace");
        }
    }

    [Fact]
    public void TestNestedErrorLogging_WithStackTrace()
    {
        // Arrange & Act - 중첩된 함수 호출에서 에러 발생
        var nestedResult = CallD();

        _output.WriteLine($"=== NESTED ERROR WITH STACK TRACE ===");
        _output.WriteLine($"Error Message: {nestedResult.Error.Error()}");
        _output.WriteLine($"Error Type: {nestedResult.Error.GetType().Name}");

        // 중첩된 스택 트레이스 확인 및 출력
        if (nestedResult.Error is Erratic erratic)
        {
            _output.WriteLine($"Has StackTrace: {erratic.StackTrace != null}");

            // Debug 포맷으로 스택 트레이스 포함 전체 정보 출력
            _output.WriteLine($"\nDEBUG FORMAT (nested call stack):");
            _output.WriteLine(erratic.ToString("D", null));

            // 직접 StackTrace 속성 접근
            _output.WriteLine($"\nDIRECT NESTED STACK TRACE ACCESS:");
            _output.WriteLine(erratic.StackTrace?.ToString());
        }

        // Assert
        Assert.True(nestedResult.IsError);
        Assert.IsType<Erratic>(nestedResult.Error);

        // 중첩 호출 스택이 모두 포함되어 있는지 확인
        if (nestedResult.Error is Erratic erraticForVerification)
        {
            var stackTraceString = erraticForVerification.StackTrace?.ToString() ?? "";

            // 각 함수가 스택 트레이스에 포함되어 있는지 확인
            Assert.Contains("CallA", stackTraceString);
            Assert.Contains("CallB", stackTraceString);
            Assert.Contains("CallC", stackTraceString);
            Assert.Contains("CallD", stackTraceString);

            _output.WriteLine($"\nNESTED CALL VERIFICATION PASSED:");
            _output.WriteLine($"All nested function calls (D->C->B->A) detected in stack trace");
        }
    }

    // 중첩된 함수 호출 체인: D() -> C() -> B() -> A()
    private Errable<string> CallD()
    {
        _output.WriteLine("CallD() executed");
        return CallC();
    }

    private Errable<string> CallC()
    {
        _output.WriteLine("CallC() executed");
        return CallB();
    }

    private Errable<string> CallB()
    {
        _output.WriteLine("CallB() executed");
        return CallA();
    }

    private Errable<string> CallA()
    {
        _output.WriteLine("CallA() executed - creating error");
        // 여기서 실제 에러 발생
        var error = ErrableFactory.Error("Nested error from CallA in call chain D->C->B->A");
        return Errable<string>.Wrap(error);
    }
}