# Errable 라이브러리 구현 계획서

## 1. 개요

### 1.1 프로젝트 목적
Go의 oops 패키지의 Fluent API와 C#의 Result 패턴을 결합한 에러 핸들링 라이브러리 개발. Go의 단순한 error 인터페이스 철학을 유지하면서 C#의 풍부한 생태계와 자연스럽게 통합.

### 1.2 핵심 설계 원칙
- **최소주의 인터페이스**: Error 인터페이스는 `string Error()` 메서드만 제공
- **Zero Friction**: Type assertion 없이 대부분의 시나리오 자동 처리
- **프레임워크 자동 통합**: 로깅, 직렬화, 포맷팅 등 자동 지원
- **불변성(Immutability)**: Thread-safe by design
- **확장성**: 사용자 정의 에러 타입 지원

## 2. 아키텍처

### 2.1 전체 구조도

```
┌─────────────────────────────────────────────────────────────┐
│                      Application Layer                       │
│                   (사용자 비즈니스 로직)                      │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│                    Public API Layer                         │
├──────────────────────────────────────────────────────────────┤
│   ┌─────────────┐  ┌──────────────┐  ┌─────────────┐      │
│   │   Errable   │  │   Result<T>   │  │    Error    │      │
│   │  (Factory)  │  │   (Pattern)   │  │ (Interface) │      │
│   └──────┬──────┘  └───────┬───────┘  └──────┬──────┘      │
└──────────┼─────────────────┼──────────────────┼─────────────┘
           │                 │                  │
┌──────────▼─────────────────▼──────────────────▼─────────────┐
│                    Integration Layer                        │
├──────────────────────────────────────────────────────────────┤
│  ┌──────────────────────────────────────────────────────┐  │
│  │         Framework Auto-Integration                    │  │
│  ├────────────────────────────────────────────────────────┤  │
│  │ • IFormattable    • ISerializable                    │  │
│  │ • JsonConverter   • ILogEventEnricher                │  │
│  │ • IProblemDetails • IEnumerable<KeyValuePair>        │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
           │
┌──────────▼───────────────────────────────────────────────────┐
│                     Core Implementation                      │
├──────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌──────────────┐  ┌─────────────────┐   │
│  │  ErrorImpl  │  │ ErrableBuilder │  │ StackCapture    │   │
│  │  (Errable)  │  │  (Builder)   │  │   (Auto)        │   │
│  └─────────────┘  └──────────────┘  └─────────────────┘   │
└──────────────────────────────────────────────────────────────┘
```

### 2.2 컴포넌트 관계도

```
      Error (Minimal Interface)
              ▲
              │ implements
    ┌─────────┼─────────────────┐
    │         │                 │
Errable   CustomError    DomainError
(Impl)    (사용자정의)    (사용자정의)

* Errable (Impl)은 ErrorCoder, ErrorCauser 등 기능 인터페이스도 구현

Errable (Static Factory)
    │
    ├─► ErrableBuilder ──► Errable (Impl) ──► Error
    │      (Builder)                         (Interface)
    │
    └─► Direct Methods ──► Error
         (Errorf, Wrap, Except)
```

## 3. 인터페이스 및 타입 정의

### 3.1 핵심 인터페이스

#### 3.1.1 최소 에러 인터페이스 (Error)
라이브러리의 가장 기본적인 계약으로, `Error()` 메서드만 제공합니다.
```csharp
public interface Error
{
    string Error();
}
```

#### 3.1.2 기능별 인터페이스 (Component Interfaces)
`Errable` 구현체가 제공하는 부가 기능에 접근하기 위한 독립적인 인터페이스들입니다.
```csharp
// Code 속성을 제공
public interface ErrorCoder
{
    Code Code { get; }
}

// Cause 속성을 제공
public interface ErrorCauser
{
    Error? Cause { get; }
}

// Context 속성을 제공
public interface ErrorContextProvider
{
    IReadOnlyDictionary<string, object> Context { get; }
}
```

### 3.2 핵심 타입

#### 3.2.1 Code (값 타입)
`string` 또는 `int` 형식의 에러 코드를 담는 값 타입(struct)입니다.
```csharp
public readonly struct Code
{
    private readonly object value;
    public Code(string code) { value = code; }
    public Code(int code) { value = code; }
    public static implicit operator Code(string code) => new Code(code);
    public static implicit operator Code(int code) => new Code(code);
    public override string ToString() => value.ToString();
}
```

#### 3.2.2 Errable<T> (반환 타입)
성공 값(`T`) 또는 실패(`Error`) 상태를 갖는 제네릭 구조체입니다.
```csharp
public readonly struct Errable<T>
{
    private readonly T _value;
    private readonly Error _error;

    public bool IsSuccess { get; }
    public bool IsError => !IsSuccess;

    public T Value => IsSuccess ? _value : throw new InvalidOperationException("Cannot access value of an error state.");
    public Error Error => IsError ? _error : throw new InvalidOperationException("Cannot access error of a success state.");

    // 내부 생성자 및 암시적 형 변환은 생략
    // public static implicit operator Errable<T>(T value) => ...;
    // public static implicit operator Errable<T>(Error error) => ...;
}
```

### 3.3 Framework 통합 인터페이스
`Errable` 구현체는 다음 인터페이스들을 구현합니다:
```
├─ Error, ErrorCoder, ErrorCauser, ErrorContextProvider
├─ IFormattable
├─ ISerializable
├─ IEnumerable<KeyValuePair>
├─ ILogEventEnricher (Serilog)
└─ IProblemDetailsProvider
```

## 4. API 명세

### 4.1 Errable Static Factory API

```
Errable (Static Class)
│
├─ 직접 생성 및 래핑(Wrapping)
│  ├─ Errorf(string format, params object[] args) → Error
│  ├─ Error(string message) → Error
│  ├─ Wrap(Exception ex, string? message) → Error
│  ├─ Wrapf(Exception ex, string format, params object[] args) → Error
│  ├─ Wrap<T>(Errable<T> result, string? message) → Error
│  ├─ Wrapf<T>(Errable<T> result, string format, params object[] args) → Error
│  ├─ Except(Exception ex) → Error
│  └─ Exceptf(Exception ex, string format, params object[] args) → Error
│
└─ 체이닝 빌더
   └─ Code(ErrorCode code) → ErrableBuilder
      │
      └─ ErrableBuilder (Fluent API)
         ├─ Code(ErrorCode code) → self
         ├─ With(string key, object value) → self
         ├─ With(params (string, object)[]) → self
         ├─ WithLazy(string key, Func<object>) → self
         ├─ Tags(params string[]) → self
         ├─ In(string domain) → self
         ├─ Public(string message) → self
         ├─ Hint(string hint) → self
         ├─ Owner(string owner) → self
         ├─ Trace(string? traceId) → self
         ├─ Span(string? spanId) → self
         ├─ Since(DateTime start) → self
         ├─ Duration(TimeSpan duration) → self
         ├─ User(string id, params (string, object)[]) → self
         ├─ Tenant(string id, params (string, object)[]) → self
         ├─ Cause(IError cause) → self
         ├─ Request(HttpContext, bool includeBody) → self
         ├─ Response(HttpContext, bool includeBody) → self
         │
         └─ 종료 메서드
            ├─ Errorf(string format, params object[] args) → Error
            ├─ Error(string message) → Error
            ├─ Wrap(Exception ex, string? message) → Error
            └─ Wrapf(Exception ex, string format, params object[] args) → Error
```

## 5. 자동 통합 메커니즘

### 5.1 로깅 프레임워크 자동 통합

```
로깅 호출: logger.Error(error, "message")
                    ↓
         Errable이 구현한 인터페이스 감지
                    ↓
    ┌───────────────┼───────────────┐
    ↓               ↓               ↓
Serilog         NLog         Extensions.Logging
    │               │               │
자동으로 Code,  자동으로      자동으로
Context, Tags   구조화 로깅    Scope 추가
등을 로깅
```

### 5.2 Serialization 자동 통합

```
JsonSerializer.Serialize(error)
            ↓
    JsonConverter 감지
            ↓
    Errable 내부 데이터
    자동 직렬화
            ↓
{
  "code": "...",
  "message": "...",
  "context": {...},
  "tags": [...],
  "stackTrace": "..."
}
```

### 5.3 Formatting 자동 통합

```
string.Format("{0:F}", error) 또는 $"{error:F}"
                    ↓
          IFormattable.ToString(format) 호출
                    ↓
Format에 따른 출력:
  - "F" → Full (모든 정보)
  - "C" → Code + Message
  - "D" → Debug (Stack 포함)
  - "J" → JSON 형식
  - "P" → Public message
  - "L" → Log 형식
```

## 6. 내부 데이터 구조

### 6.1 Errable 내부 상태

```
Errable (구현체)
├─ Core
│  ├─ Code: Code
│  ├─ Message: string  
│  ├─ Context: Dictionary<string, object>
│  └─ StackTrace: StackTrace (자동 캡처)
│
├─ Temporal
│  ├─ Time: DateTime (자동 설정)
│  └─ Duration: TimeSpan?
│
├─ Classification
│  ├─ Domain: string?
│  └─ Tags: List<string>
│
├─ Tracking
│  ├─ Trace: string? (correlation id)
│  └─ Span: string? (unit of work)
│
├─ Relations
│  ├─ User: (Id, Attributes)
│  ├─ Tenant: (Id, Attributes)
│  └─ Cause: Error?
│
└─ Additional
   ├─ Public: string?
   ├─ Hint: string?
   ├─ Owner: string?
   ├─ Request: object?
   └─ Response: object?
```

## 7. 사용 시나리오 흐름

### 7.1 기본 에러 생성 및 정보 접근

```csharp
// 1. 에러 생성: Errable 팩토리는 'Error' 인터페이스를 반환
Error err = Errable.Code(404)
    .With("id", 123)
    .Errorf("Item {0} not found", 123);

// 2. 로깅: 로깅 프레임워크는 IFormattable 등을 통해 자동으로 상세 정보 로깅
logger.LogError(err, "Data processing failed");

// 3. 정보 접근: 패턴 매칭으로 필요한 정보에 타입 안전하게 접근
if (err is ErrorCoder coder)
{
    Console.WriteLine($"Error Code: {coder.Code}"); // "404"
}

if (err is ErrorContextProvider contextual)
{
    Console.WriteLine($"Context ID: {contextual.Context["id"]}"); // 123
}
```

### 7.2 Errable<T>를 이용한 에러 처리 및 래핑

```csharp
// 1. GetScore는 성공 시 int, 실패 시 Error를 Errable<T>로 감싸 반환
public Errable<int> GetScore(string type)
{
    if (type == "none")
    {
        // Error 인터페이스가 Errable<int>로 암시적 변환
        return Errable.Code("SCORE_UNAVAILABLE").Errorf("Not supported type: {0}", type);
    }
    // int 값이 Errable<int>로 암시적 변환
    return 100;
}

// 2. GetProfile에서 GetScore의 결과를 처리
public Errable<UserProfile> GetProfile(int userId)
{
    Errable<int> scoreResult = GetScore("none");

    // 3. 에러 처리 및 래핑
    if (scoreResult.IsError)
    {
        // Wrapf는 Errable<T>의 내부 에러를 자동으로 Cause로 설정하여 래핑
        return Errable.Wrapf(scoreResult, "Failed to get profile for user {0}", userId);
    }

    var profile = new UserProfile { Id = userId, Score = scoreResult.Value };
    return profile;
}
```

### 7.3 Exception 체인 처리

```
1. Database Exception 발생
          ↓
2. Wrap with context
   Errable.Code("DB_ERROR")
     .Wrap(dbEx, "Database connection failed")
          ↓
3. Service layer wrap
   Errable.Code("SERVICE_ERROR")  
     .Cause(dbError)
     .Errorf("Failed to fetch data")
          ↓
4. Error chain 자동 직렬화
   {
     "code": "SERVICE_ERROR",
     "message": "Failed to fetch data",
     "cause": {
       "code": "DB_ERROR",
       "message": "Database connection failed",
       "innerException": {...}
     }
   }
```

## 8. 확장 포인트

### 8.1 사용자 정의 Error 타입

```
CustomError : Error
     ↓
Error() 메서드만 구현
     ↓
Errable과 함께 사용 가능
     ↓
프레임워크 통합 자동 지원
```

### 8.2 사용자 정의 통합

```
사용자 정의 Formatter
     ↓
IErrorFormatter 구현
     ↓
Errable에 등록
     ↓
새로운 format 지원
```

## 9. 패키지 구조

```
Errable/
├─ Core/
│  ├─ Error.cs                 # Error 인터페이스
│  ├─ Errable.cs              # Static factory & 구현체
│  └─ ErrableBuilder.cs       # Fluent API 빌더
│
├─ Result/
│  ├─ Result.cs               # Result<T> 구조체
│  └─ ResultExtensions.cs     # LINQ 확장
│
├─ Integration/
│  ├─ Formatting/
│  │  └─ ErrableFormatter.cs  # IFormattable 구현
│  ├─ Serialization/
│  │  └─ ErrableJsonConverter.cs
│  └─ Logging/
│     ├─ SerilogEnricher.cs
│     └─ LoggingExtensions.cs
│
└─ Extensions/
   ├─ ErrorExtensions.cs      # Error 확장 메서드
   └─ LinqExtensions.cs       # LINQ 통합
```

## 10. 설계 목표 달성

| 목표 | 달성 방법 |
|------|-----------|
| Go 스타일 단순함 | Error 인터페이스는 Error() 메서드만 제공 |
| Type Assertion 최소화 | 프레임워크 자동 통합으로 대부분 시나리오 커버 |
| Fluent API | 메서드 체이닝으로 직관적 사용 |
| LINQ 호환 | Result<T>와 확장 메서드 제공 |
| Zero Friction | 표준 인터페이스 구현으로 자동 동작 |
| 확장성 | 사용자 정의 Error 타입 지원 |서드만 제공 |
| Type Assertion 최소화 | 프레임워크 자동 통합으로 대부분 시나리오 커버 |
| Fluent API | 메서드 체이닝으로 직관적 사용 |
| LINQ 호환 | Result<T>와 확장 메서드 제공 |
| Zero Friction | 표준 인터페이스 구현으로 자동 동작 |
| 확장성 | 사용자 정의 Error 타입 지원 |�� |ction | 표준 인터페이스 구현으로 자동 동작 |
| 확장성 | 사용자 정의 Error 타입 지원 |�� |