# Fluent API 완전 개선: 제네릭 메서드 + 타입 추론 구현 계획서

## 작업 개요
ErrableBuilder에 제네릭 메서드 오버로딩과 타입 추론을 지원하는 혁신적인 Fluent API 구현

## 목표 비교

### 현재
```csharp
return Errable<User>.Wrap(
    Errable.Code("USER_NOT_FOUND")
        .With("userId", id)
        .Errorf("User {0} not found", id)
);
```

### 목표 1: 제네릭 메서드
```csharp
return Errable.Code("USER_NOT_FOUND")
    .With("userId", id)
    .Errorf<User>("User {0} not found", id);
```

### 목표 2: 타입 추론 빌더 (혁신!)
```csharp
return Errable.For<User>()
    .Code("USER_NOT_FOUND")
    .With("userId", id)
    .Error("User {0} not found", id); // 🚀 완전 자동 타입 추론!
```

## 작업 목록

### Phase 1: 기본 제네릭 메서드 추가

```
[Task 1] ErrableBuilder 제네릭 메서드 추가
- 파일: src/Errable/ErrableBuilder.cs
- 작업:
  - public Errable<T> Error<T>(string message) 메서드 추가
  - public Errable<T> Errorf<T>(string format, params object[] args) 메서드 추가
  - public Errable<T> Wrap<T>(Exception ex, string? message = null) 메서드 추가
  - public Errable<T> Wrapf<T>(Exception ex, string format, params object[] args) 메서드 추가
  - XML 문서화 주석 추가
- 의존성: 없음
- 예상 시간: 20분
```

### Phase 2: 타입 추론 빌더 시스템 구현

```
[Task 2] ErrableBuilder<T> 제네릭 빌더 클래스 생성
- 파일: src/Errable/ErrableBuilderT.cs (새 파일)
- 작업:
  - public sealed class ErrableBuilder<T> 생성
  - 모든 체이닝 메서드를 ErrableBuilder<T> 반환으로 구현
  - Code, With, WithLazy, Tags, In, Public, Hint, Owner, Trace, Span, Since, Duration, User, Tenant, Cause 메서드
  - Error, Errorf, Wrap, Wrapf 메서드는 Errable<T> 반환 (타입 추론됨)
- 의존성: Task 1
- 예상 시간: 40분
```

```
[Task 3] Errable.For<T>() 타입 지정 진입점 추가
- 파일: src/Errable/ErrableStatic.cs
- 작업:
  - public static ErrableBuilder<T> For<T>() 메서드 추가
  - XML 문서화 및 사용 예시 추가
- 의존성: Task 2
- 예상 시간: 10분
```

### Phase 3: 간편 팩토리 메서드 추가

```
[Task 4] 정적 팩토리 메서드 추가
- 파일: src/Errable/ErrableStatic.cs
- 작업:
  - ErrorFor<T>(string code, string message) 메서드 추가
  - ErrorfFor<T>(string code, string format, params object[] args) 메서드 추가
  - WrapFor<T>(Exception ex, string? message = null) 메서드 추가
  - WrapfFor<T>(Exception ex, string format, params object[] args) 메서드 추가
- 의존성: Task 3
- 예상 시간: 15분
```

### Phase 4: 기본 테스트 작성

```
[Task 5] 제네릭 메서드 기본 테스트
- 파일: tests/Errable.Tests/ErrableBuilderGenericTests.cs (새 파일)
- 작업:
  - 기존 ErrableBuilder의 제네릭 메서드 테스트
  - Error<T>, Errorf<T>, Wrap<T>, Wrapf<T> 검증
  - 반환 타입 및 기능 테스트
- 의존성: Task 4
- 예상 시간: 20분
```

```
[Task 6] 타입 추론 빌더 테스트
- 파일: tests/Errable.Tests/ErrableBuilderTTests.cs (새 파일)
- 작업:
  - ErrableBuilder<T> 클래스 전체 기능 테스트
  - Errable.For<T>() 진입점 테스트
  - 복잡한 체이닝 시나리오 테스트
  - 타입 안정성 검증
- 의존성: Task 5
- 예상 시간: 30분
```

```
[Task 7] 팩토리 메서드 테스트
- 파일: tests/Errable.Tests/FluentApiFactoryTests.cs (새 파일)
- 작업:
  - ErrorFor<T>, ErrorfFor<T> 등 팩토리 메서드 테스트
  - 간편 생성 패턴 테스트
  - 기존 API와 성능 비교
- 의존성: Task 6
- 예상 시간: 20분
```

### Phase 5: 고급 사용성 테스트

```
[Task 8] 실제 사용 시나리오 테스트
- 파일: tests/Errable.Tests/FluentApiUsageScenarios.cs (새 파일)
- 작업:
  - 다양한 함수 반환 패턴 테스트
  - 복잡한 도메인 로직 시뮬레이션
  - 성능 비교 테스트 (기존 vs 새 API)
  - IDE IntelliSense 지원 검증
- 의존성: Task 7
- 예상 시간: 30분
```

```
[Task 9] 호환성 및 마이그레이션 테스트
- 파일: tests/Errable.Tests/BackwardCompatibilityTests.cs (새 파일)
- 작업:
  - 기존 API 완전 호환성 확인
  - 메서드 오버로딩 해결 테스트
  - 점진적 마이그레이션 시나리오 테스트
- 의존성: Task 8
- 예상 시간: 20분
```

### Phase 6: 문서화 및 가이드

```
[Task 10] 포괄적 사용 가이드 작성
- 파일: docs/fluent-api-complete-guide.md (새 파일)
- 작업:
  - 4가지 사용 패턴 비교 및 설명
  - 언제 어떤 방법을 사용할지 가이드
  - 성능 및 가독성 고려사항
  - 실제 코드 예시 및 Best Practices
- 의존성: Task 9
- 예상 시간: 30분
```

```
[Task 11] examples.md 대대적 업데이트
- 파일: examples.md
- 작업:
  - 새로운 4가지 API 패턴 예시 추가
  - 마이그레이션 가이드 섹션
  - 성능 비교 및 권장사항
  - 복잡한 실제 사용 사례
- 의존성: Task 10
- 예상 시간: 25분
```

### Phase 7: 최종 검증

```
[Task 12] 전체 통합 테스트
- 작업:
  - dotnet build: 모든 컴파일 오류 해결
  - dotnet test: 모든 테스트 통과 확인 (예상: 100+ 테스트)
  - 성능 벤치마크 실행
  - 메모리 사용량 프로파일링
- 의존성: Task 11
- 예상 시간: 20분
```

## 구현 세부사항

### 1. ErrableBuilder<T> 제네릭 빌더 설계

```csharp
public sealed class ErrableBuilder<T>
{
    private Code _code;
    private readonly Dictionary<string, object> _context = new();
    private IError? _cause;
    // ... 기타 필드들

    internal ErrableBuilder() { }
    internal ErrableBuilder(Code code) { _code = code; }

    // 체이닝 메서드들 - 모두 ErrableBuilder<T> 반환
    public ErrableBuilder<T> Code(Code code) { _code = code; return this; }
    public ErrableBuilder<T> Code(string code) => Code(new Code(code));
    public ErrableBuilder<T> With(string key, object value) { _context[key] = value; return this; }
    public ErrableBuilder<T> Tags(params string[] tags) { /* ... */ return this; }
    // ... 모든 기존 체이닝 메서드들

    // 종료 메서드들 - Errable<T> 반환 (타입 추론됨!)
    public Errable<T> Error(string message) => Errable<T>.Wrap(CreateError(message));
    public Errable<T> Errorf(string format, params object[] args) => Errable<T>.Wrap(CreateError(string.Format(format, args)));
    public Errable<T> Wrap(Exception ex, string? message = null) { /* ... */ }
    public Errable<T> Wrapf(Exception ex, string format, params object[] args) { /* ... */ }

    private Erratic CreateError(string message) { /* 기존 로직 재사용 */ }
}
```

### 2. 새로운 진입점들

```csharp
public static class Errable  // ErrableStatic.cs에 추가
{
    // 타입 추론 빌더 진입점
    public static ErrableBuilder<T> For<T>() => new();
    public static ErrableBuilder<T> For<T>(Code code) => new(code);
    public static ErrableBuilder<T> For<T>(string code) => new(new Code(code));

    // 간편 팩토리 메서드들
    public static Errable<T> ErrorFor<T>(string code, string message)
        => Code(code).Error<T>(message);
    public static Errable<T> ErrorfFor<T>(string code, string format, params object[] args)
        => Code(code).Errorf<T>(format, args);
    public static Errable<T> WrapFor<T>(Exception ex, string? message = null)
        => Wrap(ex, message).AsResult<T>();

    // 기존 메서드들은 그대로 유지...
}
```

## 4가지 사용 패턴 비교

### Pattern 1: 기존 방식 (호환성)
```csharp
return Errable<User>.Wrap(
    Errable.Code("ERROR").With("key", "value").Error("message")
);
```

### Pattern 2: 제네릭 메서드
```csharp
return Errable.Code("ERROR")
    .With("key", "value")
    .Error<User>("message");
```

### Pattern 3: 타입 추론 빌더 ⭐ **최고의 사용성**
```csharp
return Errable.For<User>()
    .Code("ERROR")
    .With("key", "value")
    .Error("message"); // 🚀 완전 자동!
```

### Pattern 4: 간편 팩토리 (단순한 경우)
```csharp
return Errable.ErrorFor<User>("ERROR", "message");
```

## 예상 결과

### 1. 완전한 타입 추론
```csharp
public Errable<User> GetUser(int id)
{
    if (id <= 0)
        return Errable.For<User>().Code("INVALID_ID").Error("Invalid ID");

    try
    {
        // 복잡한 로직...
        return user;
    }
    catch (Exception ex)
    {
        return Errable.For<User>()
            .Code("DATABASE_ERROR")
            .With("userId", id)
            .With("operation", "GetUser")
            .Tags("database", "error")
            .In("user-service")
            .Wrap(ex, "Failed to retrieve user");
    }
}
```

### 2. IDE 지원 완벽
- IntelliSense에서 정확한 타입 힌트
- 컴파일 타임 타입 체크
- 리팩토링 도구 완벽 지원

### 3. 성능 최적화
- 제네릭 특화로 박싱/언박싱 최소화
- 체이닝 중간 객체 생성 최적화

## 완료 기준
- [ ] 기존 모든 테스트 통과 (80/80)
- [ ] 새로운 테스트 모두 통과 (예상 30+ 추가)
- [ ] 4가지 사용 패턴 모두 정상 작동
- [ ] 완전한 하위 호환성 유지
- [ ] 성능 회귀 없음 (벤치마크 기준)
- [ ] 메모리 사용량 증가 최소화 (5% 이내)
- [ ] 포괄적 문서화 완료

## 예상 전체 소요시간
**총 4-5시간** (단계별 구현 시)

이 구현으로 Errable이 C# 생태계에서 **가장 사용하기 쉬운 에러 처리 라이브러리**가 될 것입니다! 🚀