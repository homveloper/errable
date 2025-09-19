# Fluent API 개선: 제네릭 메서드 오버로딩 구현 계획서

## 작업 개요
ErrableBuilder에 제네릭 메서드 오버로딩을 추가하여 `IError`와 `Errable<T>` 반환을 동시에 지원하는 Fluent API 구현

## 목표
```csharp
// 현재 (명시적 래핑 필요)
return Errable<User>.Wrap(
    Errable.Code("ERROR").Errorf("User {0} not found", id)
);

// 목표 (직접 반환)
return Errable.Code("ERROR")
    .With("userId", id)
    .Errorf<User>("User {0} not found", id);  // 🚀 바로 Errable<User> 반환
```

## 작업 목록

### Phase 1: ErrableBuilder 제네릭 메서드 추가

```
[Task 1] Error<T> 제네릭 메서드 추가
- 파일: src/Errable/ErrableBuilder.cs
- 작업:
  - public Errable<T> Error<T>(string message) 메서드 추가
  - CreateError 로직 재사용하여 Errable<T>.Wrap() 호출
  - XML 문서화 주석 추가
- 의존성: 없음
- 예상 시간: 10분
```

```
[Task 2] Errorf<T> 제네릭 메서드 추가
- 파일: src/Errable/ErrableBuilder.cs
- 작업:
  - public Errable<T> Errorf<T>(string format, params object[] args) 메서드 추가
  - 기존 Errorf 로직 재사용
  - XML 문서화 주석 추가
- 의존성: Task 1
- 예상 시간: 10분
```

```
[Task 3] Wrap<T> 제네릭 메서드 추가
- 파일: src/Errable/ErrableBuilder.cs
- 작업:
  - public Errable<T> Wrap<T>(Exception ex, string? message = null) 메서드 추가
  - 기존 Wrap 로직 재사용
  - XML 문서화 주석 추가
- 의존성: Task 2
- 예상 시간: 10분
```

```
[Task 4] Wrapf<T> 제네릭 메서드 추가
- 파일: src/Errable/ErrableBuilder.cs
- 작업:
  - public Errable<T> Wrapf<T>(Exception ex, string format, params object[] args) 메서드 추가
  - 기존 Wrapf 로직 재사용
  - XML 문서화 주석 추가
- 의존성: Task 3
- 예상 시간: 10분
```

### Phase 2: 테스트 작성

```
[Task 5] 제네릭 메서드 기본 테스트 작성
- 파일: tests/Errable.Tests/ErrableBuilderGenericTests.cs (새 파일)
- 작업:
  - Error<T> 메서드 기본 동작 테스트
  - Errorf<T> 메서드 포맷팅 테스트
  - Wrap<T>, Wrapf<T> 예외 래핑 테스트
  - 반환 타입 검증 테스트
- 의존성: Task 1-4
- 예상 시간: 20분
```

```
[Task 6] 호환성 테스트 추가
- 파일: tests/Errable.Tests/ErrableBuilderCompatibilityTests.cs (새 파일)
- 작업:
  - 기존 IError 반환 메서드가 정상 작동하는지 확인
  - 메서드 오버로딩이 올바르게 해결되는지 테스트
  - 제네릭 타입 추론 테스트
- 의존성: Task 5
- 예상 시간: 15분
```

```
[Task 7] Fluent API 사용성 테스트
- 파일: tests/Errable.Tests/FluentApiUsageTests.cs (새 파일)
- 작업:
  - 실제 사용 시나리오 테스트 (함수에서 직접 반환)
  - 복잡한 빌더 체이닝 테스트
  - 타입 안정성 검증
- 의존성: Task 6
- 예상 시간: 20분
```

### Phase 3: 기존 테스트 업데이트

```
[Task 8] UsageExampleTests 업데이트
- 파일: tests/Errable.Tests/UsageExampleTests.cs
- 작업:
  - 새로운 제네릭 메서드를 사용하는 예시 추가
  - 기존 예시와 신규 예시 비교 섹션 추가
  - 성능 및 가독성 개선 사례 추가
- 의존성: Task 7
- 예상 시간: 15분
```

### Phase 4: 문서화

```
[Task 9] API 문서 업데이트
- 파일: examples.md
- 작업:
  - 새로운 제네릭 메서드 사용법 추가
  - 기존 방식 vs 새로운 방식 비교
  - 권장 사용 패턴 가이드 작성
- 의존성: Task 8
- 예상 시간: 20분
```

```
[Task 10] 마이그레이션 가이드 작성
- 파일: docs/migration-to-generic-methods.md (새 파일)
- 작업:
  - 기존 코드에서 새 API로 마이그레이션하는 방법
  - 성능 및 가독성 개선 사례
  - 언제 어떤 방법을 사용할지 가이드
- 의존성: Task 9
- 예상 시간: 15분
```

### Phase 5: 검증 및 최적화

```
[Task 11] 전체 프로젝트 빌드 및 테스트
- 명령: dotnet build && dotnet test
- 작업:
  - 모든 기존 테스트가 통과하는지 확인
  - 새로운 테스트가 모두 통과하는지 확인
  - 컴파일 오류 및 경고 해결
- 의존성: Task 10
- 예상 시간: 10분
```

```
[Task 12] 성능 및 메모리 영향 검토
- 작업:
  - 제네릭 메서드 오버로딩의 성능 영향 분석
  - 메모리 사용량 변화 확인
  - 필요시 최적화 적용
- 의존성: Task 11
- 예상 시간: 15분
```

## 구현 세부사항

### 1. 메서드 시그니처
```csharp
public class ErrableBuilder
{
    // 기존 메서드 (하위 호환성 유지)
    public IError Error(string message) { ... }
    public IError Errorf(string format, params object[] args) { ... }
    public IError Wrap(Exception ex, string? message = null) { ... }
    public IError Wrapf(Exception ex, string format, params object[] args) { ... }

    // 새로운 제네릭 메서드
    public Errable<T> Error<T>(string message) { ... }
    public Errable<T> Errorf<T>(string format, params object[] args) { ... }
    public Errable<T> Wrap<T>(Exception ex, string? message = null) { ... }
    public Errable<T> Wrapf<T>(Exception ex, string format, params object[] args) { ... }
}
```

### 2. 구현 패턴
```csharp
public Errable<T> Error<T>(string message)
{
    return Errable<T>.Wrap(CreateError(message));
}

public Errable<T> Errorf<T>(string format, params object[] args)
{
    var message = string.Format(format, args);
    return Errable<T>.Wrap(CreateError(message));
}
```

### 3. 사용 예시
```csharp
// Before (명시적 래핑)
public Errable<User> GetUser(int id)
{
    return Errable<User>.Wrap(
        Errable.Code("USER_NOT_FOUND")
            .With("id", id)
            .Errorf("User {0} not found", id)
    );
}

// After (직접 반환)
public Errable<User> GetUser(int id)
{
    return Errable.Code("USER_NOT_FOUND")
        .With("id", id)
        .Errorf<User>("User {0} not found", id);
}
```

## 예상 결과

### 사용성 개선
```csharp
// 간단한 에러
return Errable.Error<User>("User not found");

// 복잡한 에러
return Errable.Code("VALIDATION_ERROR")
    .In("user-service")
    .Tags("validation", "input")
    .With("field", "email")
    .With("value", email)
    .Public("이메일 형식이 올바르지 않습니다")
    .Errorf<User>("Invalid email format: {0}", email);
```

### 타입 안정성
- 컴파일 타임에 반환 타입 체크
- 제네릭 타입 추론으로 타입 안정성 확보
- IDE에서 올바른 타입 힌트 제공

## 완료 기준
- [ ] 모든 기존 테스트 통과 (80/80)
- [ ] 새로운 테스트 모두 통과
- [ ] 기존 API 완전 호환성 유지
- [ ] 새로운 제네릭 메서드 정상 작동
- [ ] 문서화 완료
- [ ] 성능 영향 최소화

## 주의사항
- 기존 코드와의 완전한 하위 호환성 유지
- 메서드 오버로딩 해결이 명확해야 함
- 제네릭 타입 제약사항 고려
- XML 문서화 주석의 일관성 유지