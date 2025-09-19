# Errable

## 개요 (Overview)

Errable은 .NET을 위한 강력하고 유연한 오류 처리 라이브러리입니다. 기존의 예외 처리 방식 대신, 함수형 프로그래밍에서 영감을 받은 `Result` 패턴을 사용하여 오류를 값으로 다룰 수 있게 해줍니다. 이를 통해 코드의 안정성과 가독성을 높이고, 오류 처리 로직을 명확하게 표현할 수 있습니다.

## 주요 특징 (Features)

*   **타입-안전 결과 패턴**: `Errable<T>` 타입을 통해 성공 값 또는 오류 객체를 명확하게 반환합니다.
*   **Fluent API**: `Errable.Code("...").With(...).Error(...)`와 같이 메서드 체이닝을 통해 풍부한 컨텍스트를 가진 오류 객체를 쉽게 생성할 수 있습니다.
*   **자세한 오류 컨텍스트**: 오류 코드, 태그, 사용자 정의 데이터, 원인(Cause) 등 상세한 정보를 오류 객체에 담을 수 있습니다.
*   **패턴 매칭**: `Match` 메서드를 사용하여 성공과 실패 케이스를 깔끔하게 처리할 수 있습니다.
*   **타입 추론 지원**: `Errable.For<T>()` 팩토리를 통해 제네릭 타입 추론을 완벽하게 지원하여, 더 깔끔하고 읽기 좋은 코드를 작성할 수 있습니다.
*   **다양한 포맷팅**: 생성된 오류 객체를 로그, 디버깅, 공개 메시지 등 다양한 목적에 맞게 포맷팅할 수 있습니다.

## 주요 API (Core APIs)

*   **`Errable<T>`**: 성공(`T`) 또는 실패(`IError`) 상태를 가지는 핵심 결과 타입입니다.
    *   `IsSuccess`: 성공 여부를 확인합니다.
    *   `IsError`: 실패 여부를 확인합니다.
    *   `Value`: 성공 시 결과 값에 접근합니다.
    *   `Error`: 실패 시 오류 객체에 접근합니다.
    *   `Match(onSuccess, onError)`: 결과 상태에 따라 적절한 함수를 실행합니다.

*   **`Errable` (Static Factory)**: 오류 객체 및 빌더를 생성하기 위한 정적 메서드를 제공합니다.
    *   `Errable.Error(string message)`: 간단한 오류 메시지를 가진 오류를 생성합니다.
    *   `Errable.Errorf(string format, ...)`: 서식이 지정된 메시지로 오류를 생성합니다.
    *   `Errable.Code(string code)`: 오류 코드를 지정하여 `ErrableBuilder`를 시작합니다.
    *   `Errable.For<T>()`: 타입 추론을 지원하는 제네릭 `ErrableBuilder<T>`를 시작합니다. (권장)
    *   `Errable.Wrap(Exception ex, ...)`: 예외를 `IError` 객체로 감쌉니다.

*   **`ErrableBuilder` / `ErrableBuilder<T>`**: 상세한 오류 객체를 생성하기 위한 Fluent API입니다.
    *   `.With(string key, object value)`: 컨텍스트 데이터를 추가합니다.
    *   `.Tags(params string[] tags)`: 검색 가능한 태그를 추가합니다.
    *   `.Public(string message)`: 사용자에게 보여줄 공개 메시지를 설정합니다.
    *   `.Error(string message)` / `.Errorf(...)`: 최종 오류 메시지를 설정하고 `IError` 또는 `Errable<T>`를 반환합니다.

## 사용 예시 (Usage Examples)

### 1. 기본적인 결과 패턴 사용

성공과 실패를 `Errable<T>`로 반환하는 함수를 작성하고, `Match`를 사용하여 결과를 처리합니다.

```csharp
// 사용자 나이를 반환하는 함수 (성공 또는 실패)
Errable<int> GetUserAge(string username)
{
    if (username == "john")
        return 25; // 성공 시 값을 직접 반환 (암시적 변환)

    // 실패 시 오류 빌더를 사용하여 Errable<T> 반환
    return Errable.For<int>()
        .Code("USER_NOT_FOUND")
        .With("username", username)
        .Errorf("User '{0}' not found", username);
}

// 함수 사용
var ageResult = GetUserAge("unknown");

var message = ageResult.Match(
    onSuccess: age => $"User's age is {age}.",
    onError: error => $"Operation failed: {error.Error()}"
);

// message: "Operation failed: User 'unknown' not found"
```

### 2. Fluent API를 이용한 상세한 오류 생성

다양한 컨텍스트 정보를 포함하는 풍부한 오류 객체를 생성합니다.

```csharp
IError richError = Errable.Code("AUTH_FAILED")
    .In("auth-service")
    .Tags("security", "authentication")
    .With("userId", 123)
    .With("attemptCount", 3)
    .Public("Login failed. Please check your credentials.")
    .Errorf("Authentication failed for user {0}", "john");

// 생성된 오류는 다양한 인터페이스로 캐스팅하여 상세 정보에 접근할 수 있습니다.
var coder = (IErrorCoder)richError;
// coder.Code -> "AUTH_FAILED"

var contextProvider = (IErrorContextProvider)richError;
// contextProvider.Context["userId"] -> 123
```

### 3. 오류 체이닝 (Wrapping)

한 계층에서 발생한 오류를 상위 계층에서 감싸 더 많은 컨텍스트를 추가합니다.

```csharp
// 데이터베이스 계층
Errable<string> DatabaseQuery(string sql)
{
    var dbException = new InvalidOperationException("Connection timeout");
    return Errable.For<string>()
        .Code("DB_TIMEOUT")
        .Wrap(dbException, "Database query failed");
}

// 서비스 계층
Errable<string> ServiceCall(int userId)
{
    var dbResult = DatabaseQuery($"SELECT * FROM Users WHERE Id = {userId}");
    if (dbResult.IsError)
    {
        // 데이터베이스 오류를 감싸서 서비스 컨텍스트 추가
        return Errable.For<string>()
            .Code("SERVICE_ERROR")
            .With("userId", userId)
            .Cause(dbResult.Error) // 하위 오류를 원인으로 지정
            .Errorf("Failed to retrieve user {0}", userId);
    }
    return dbResult.Value;
}

var result = ServiceCall(123);

// result.Error.Cause를 통해 내부 오류에 접근 가능
```

### 4. 타입 추론을 활용한 권장 패턴

`Errable.For<T>()`를 사용하면 제네릭 타입을 명시하지 않아도 되어 코드가 훨씬 간결해집니다.

```csharp
// 복잡한 결제 시나리오 시뮬레이션
private Errable<PaymentResult> SimulatePaymentProcessing(string orderId, decimal amount)
{
    try
    {
        // 외부 게이트웨이 호출 실패 시뮬레이션
        throw new HttpRequestException("Gateway timeout");
    }
    catch (Exception ex)
    {
        // Errable.For<T>() 덕분에 반환 타입을 명시할 필요가 없습니다.
        return Errable.For<PaymentResult>()
            .Code("PAYMENT_GATEWAY_ERROR")
            .In("payment-processing")
            .Tags("payment", "gateway", "critical")
            .With("orderId", orderId)
            .With("amount", amount)
            .Public("Payment processing is temporarily unavailable")
            .Wrap(ex, $"Failed to process payment for order {orderId}");
    }
}
```

---

## 참고 (References)

이 패키지는 다음 프로젝트들로부터 영감을 얻어 개발되었습니다.

*   [amantinband/error-or](https://github.com/amantinband/error-or)
*   [samber/oops](https://github.com/samber/oops)�� 패키지는 다음 프로젝트들로부터 영감을 얻어 개발되었습니다.

*   [amantinband/error-or](https://github.com/amantinband/error-or)
*   [samber/oops](https://github.com/samber/oops)