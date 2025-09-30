# Errable 개선 작업 태스크 리스트

## 📋 우선순위 태스크

### ✅ 1. 성능 최적화

- [ ] **Task #1.1**: HashCode 생성 최적화

**파일**: `src/Errable/ErrableT.cs:165-171`

**현재 문제**:
```csharp
public override int GetHashCode()
{
    if (_isSuccess)
        return HashCode.Combine(_isSuccess, _value);

    return HashCode.Combine(_isSuccess, _error?.Error()); // 문자열 생성 비용
}
```

**개선안**:
```csharp
public override int GetHashCode()
{
    if (_isSuccess)
        return HashCode.Combine(_isSuccess, _value);

    return HashCode.Combine(_isSuccess, _error?.GetHashCode()); // 객체 참조 해시코드 사용
}
```

**예상 효과**: 불필요한 문자열 생성 제거로 메모리 할당 감소

---

### ✅ 2. 에러 체이닝 일관성 개선

- [ ] **Task #2.1**: Code.Empty 정적 필드 추가
- [ ] **Task #2.2**: Code 타입 사용을 Code.Empty로 일관성 있게 변경

**파일**: `src/Errable/Code.cs`, `src/Errable/ErrableFactory.cs`

**Step 1 (Task #2.1)**: `Code` 구조체에 정적 필드 추가
```csharp
// src/Errable/Code.cs
public readonly struct Code : IEquatable<Code>
{
    /// <summary>
    /// Represents an empty error code.
    /// </summary>
    public static readonly Code Empty = new Code("");

    // ... 기존 코드
}
```

**Step 2 (Task #2.2)**: 모든 빈 코드 생성을 `Code.Empty`로 변경

**현재 문제 (ErrableFactory.cs:99)**:
```csharp
return new Erratic(
    code: 0,  // ⚠️ int 0 사용 - 의도가 불명확
    message: "",
    cause: result.Error,
    stackTrace: new FilteredStackTrace(skipFrames: 1)
);
```

**개선안**:
```csharp
return new Erratic(
    code: Code.Empty,  // ✅ 명시적으로 빈 Code 사용
    message: "",
    cause: result.Error,
    stackTrace: new FilteredStackTrace(skipFrames: 1)
);
```

**추가 변경이 필요한 위치들**:
- `ErrableFactory.cs:23-26` (Error 메서드)
- `ErrableFactory.cs:58` (Wrap 메서드)
- `ErrableFactory.cs:93` (Wrap<T> 메서드)
- `ErrableFactory.cs:123` (Wrapf<T> 메서드)
- `ErrableFactory.cs:131` (Wrapf 메서드)
- `ErrableFactory.cs:147` (Except 메서드)
- `ErrableFactory.cs:164` (Exceptf 메서드)
- `ErrableFactory.cs:191, 201, 212, 222, 232, 242, 252, 262, 272, 282, 292, 302, 312, 322` (빌더 팩토리 메서드들)

**일관성 원칙**:
- ❌ `new Code("")` 사용 금지
- ❌ `code: 0` 사용 금지
- ❌ `code: ""` 사용 금지
- ✅ `Code.Empty` 사용 권장

**예상 효과**: 코드 의도 명확화, 일관성 향상, 가독성 개선

---

### ✅ 3. API 일관성 및 발견성 개선

- [ ] **Task #3.1**: 명시적 문자열/int 오버로드 추가
- [ ] **Task #3.2**: Equals 메서드 성능 개선

**파일**: `src/Errable/ErrableFactory.cs:178`, `src/Errable/ErrableT.cs:142-152`

**현재 상태**:
```csharp
public static ErrableBuilder Code(Code code)
{
    return new ErrableBuilder(code);
}
```

**개선안 (Task #3.1)**: 명시적 문자열 오버로드 추가
```csharp
/// <summary>
/// Starts a fluent builder chain with the specified error code string.
/// </summary>
/// <param name="code">The error code string</param>
/// <returns>A new ErrableBuilder instance for method chaining</returns>
public static ErrableBuilder Code(string code)
{
    return new ErrableBuilder(new Code(code));
}

/// <summary>
/// Starts a fluent builder chain with the specified error code integer.
/// </summary>
/// <param name="code">The error code integer</param>
/// <returns>A new ErrableBuilder instance for method chaining</returns>
public static ErrableBuilder Code(int code)
{
    return new ErrableBuilder(new Code(code));
}
```

**현재 문제 (Task #3.2)**:
```csharp
public bool Equals(Errable<T> other)
{
    if (_isSuccess != other._isSuccess)
        return false;

    if (_isSuccess)
        return EqualityComparer<T>.Default.Equals(_value, other._value);

    return ReferenceEquals(_error, other._error) ||
           (_error != null && other._error != null && _error.Error() == other._error.Error());
}
```

**개선안 (Task #3.2)**: 에러 비교 시에도 참조 비교 우선 (성능 향상)
```csharp
public bool Equals(Errable<T> other)
{
    if (_isSuccess != other._isSuccess)
        return false;

    if (_isSuccess)
        return EqualityComparer<T>.Default.Equals(_value, other._value);

    // 동일 참조인 경우 빠른 반환
    if (ReferenceEquals(_error, other._error))
        return true;

    // null 체크를 명시적으로
    if (_error == null || other._error == null)
        return false;

    // 마지막으로 문자열 비교 (비용이 높음)
    return _error.Error() == other._error.Error();
}
```

**예상 효과**: API 발견성 향상, IntelliSense 개선, 성능 최적화

---

## 📊 작업 우선순위

| 순위 | 태스크 번호 | 설명 | 난이도 | 예상 소요 시간 | 영향도 | 완료 |
|------|------------|------|--------|----------------|--------|------|
| 1 | Task #1.1 | HashCode 생성 최적화 | 쉬움 | 15분 | 중간 | ⬜ |
| 2 | Task #2.1 | Code 타입 사용 명확화 | 쉬움 | 10분 | 높음 | ⬜ |
| 3 | Task #2.2 | Code.Empty 정적 필드 추가 | 쉬움 | 10분 | 높음 | ⬜ |
| 4 | Task #3.1 | 명시적 오버로드 추가 | 쉬움 | 15분 | 높음 | ⬜ |
| 5 | Task #3.2 | Equals 메서드 성능 개선 | 쉬움 | 15분 | 중간 | ⬜ |

**총 예상 소요 시간**: ~1시간 5분

---

## 🧪 테스트 체크리스트

각 개선 작업 후 다음을 확인:

- [ ] 기존 128개 테스트 모두 통과
- [ ] 새로운 테스트 케이스 추가 (필요시)
- [ ] 성능 벤치마크 비교 (1.1 작업)
- [ ] API 호환성 확인 (breaking change 없음)
- [ ] XML 문서 주석 업데이트

---

## 📝 커밋 메시지 가이드

```
perf: Optimize HashCode generation to avoid string allocation

refactor: Use explicit Code.Empty for consistency in error chaining

feat: Add explicit string/int overloads for Code() method
```

---

## 🚀 배포 계획

- [ ] **Task #4.1**: 버전 범프 (`0.2.1` → `0.2.2`)
- [ ] **Task #4.2**: CHANGELOG.md 업데이트
- [ ] **Task #4.3**: NuGet 패키지 재배포

---

**작성일**: 2025-09-30
**작성자**: Claude Code Analysis