# Errable → Erratic 리네이밍 작업 계획서

## 작업 개요
`Errable` 타입을 `Erratic`으로 변경하고, 패키지 레벨 API를 `Errable` static 클래스로 통합하여 직관적인 API 제공

## 목표 구조
```csharp
namespace Errable  // 패키지명 유지
{
    // IError 구현체 (기존 non-generic Errable → Erratic)
    public sealed class Erratic : IError, IErrorCoder, IErrorCauser, IErrorContextProvider

    // Result 패턴 (기존 generic Errable<T> 이름 유지)
    public readonly struct Errable<T>

    // API 진입점 (패키지명과 동일, 기존 E 클래스 대체)
    public static class Errable
    {
        public static IError Error(string message);
        public static ErrableBuilder Code(string code);
        // 모든 E 클래스 메서드들
    }
}
```

## 작업 목록

### Phase 1: 핵심 타입 리네이밍
```
[Task 1] 클래스명 변경: Errable → Erratic
- 파일: src/Errable/Errable.cs → src/Errable/Erratic.cs
- 작업:
  - 파일명 변경 Errable.cs → Erratic.cs
  - 클래스명 변경: public sealed class Errable → public sealed class Erratic
  - 생성자명 변경: Errable(...) → Erratic(...)
  - XML 주석 업데이트
- 의존성: 없음 (시작점)
- 병렬처리: 불가
```

### Phase 2: Factory 및 Builder 업데이트
```
[Task 2] ErrableFactory 클래스 업데이트
- 파일: src/Errable/ErrableFactory.cs
- 작업:
  - 모든 new Errable(...) → new Erratic(...)
  - 반환 타입은 IError 유지
  - import 문 확인
- 의존성: Task 1
- 병렬처리: Task 3과 병렬 가능

[Task 3] ErrableBuilder 클래스 업데이트
- 파일: src/Errable/ErrableBuilder.cs
- 작업:
  - CreateError 메서드에서 new Errable(...) → new Erratic(...)
  - 반환 타입은 IError 유지
- 의존성: Task 1
- 병렬처리: Task 2와 병렬 가능
```

### Phase 3: Static API 통합
```
[Task 4] 새로운 Errable static 클래스 생성
- 파일: src/Errable/ErrableStatic.cs (새 파일)
- 작업:
  - Index.cs의 모든 내용을 복사
  - public static class E → public static class Errable
  - 모든 static 메서드들 이동
  - XML 주석 업데이트 (E → Errable)
- 의존성: Task 2, 3 완료 후
- 병렬처리: 불가

[Task 5] Index.cs 파일 삭제
- 파일: src/Errable/Index.cs
- 작업: 파일 삭제 (내용이 Task 4로 이동됨)
- 의존성: Task 4 완료 후
- 병렬처리: 불가

[Task 6] 하위 호환성을 위한 E 별칭 추가 (선택사항)
- 파일: src/Errable/ErrableStatic.cs
- 작업:
  - E 클래스를 Errable의 별칭으로 추가
  - public static class E => Errable 또는 using 별칭 활용
- 의존성: Task 4
- 병렬처리: Task 5와 병렬 가능
```

### Phase 4: 테스트 파일 업데이트
```
[Task 7] ErrableFactoryTests 업데이트
- 파일: tests/Errable.Tests/ErrableFactoryTests.cs
- 작업:
  - global::Errable.E.Error(...) → Errable.Error(...)
  - global::Errable.E.Code(...) → Errable.Code(...)
  - as Errable → as Erratic (4개 occurrences)
- 의존성: Task 4
- 병렬처리: Task 8, 9와 병렬 가능

[Task 8] ErrableBuilderTests 업데이트
- 파일: tests/Errable.Tests/ErrableBuilderTests.cs
- 작업:
  - global::Errable.E.Code(...) → Errable.Code(...)
  - as Errable → as Erratic (15개 occurrences)
- 의존성: Task 4
- 병렬처리: Task 7, 9와 병렬 가능

[Task 9] UsageExampleTests 업데이트
- 파일: tests/Errable.Tests/UsageExampleTests.cs
- 작업:
  - global::Errable.E.Error(...) → Errable.Error(...)
  - global::Errable.E.Code(...) → Errable.Code(...)
  - as Errable → as Erratic (3개 occurrences)
- 의존성: Task 4
- 병렬처리: Task 7, 8과 병렬 가능

[Task 10] ErrableFormattingTests 업데이트
- 파일: tests/Errable.Tests/ErrableFormattingTests.cs
- 작업:
  - global::Errable.E.Error(...) → Errable.Error(...)
  - global::Errable.E.Code(...) → Errable.Code(...)
- 의존성: Task 4
- 병렬처리: Task 7, 8, 9와 병렬 가능
```

### Phase 5: 빌드 및 검증
```
[Task 11] 전체 프로젝트 빌드
- 명령: dotnet build
- 작업: 컴파일 에러 확인 및 수정
- 의존성: Task 7, 8, 9, 10 완료 후
- 병렬처리: 불가

[Task 12] 단위 테스트 실행
- 명령: dotnet test
- 작업: 모든 테스트가 통과하는지 확인 (80/80)
- 의존성: Task 11
- 병렬처리: 불가

[Task 13] API 사용 예제 업데이트
- 파일: examples.md, README.md 등
- 작업:
  - E.Error() → Errable.Error() 예제 변경
  - Errable → Erratic 타입 예제 변경
  - 새로운 API 구조 설명 추가
- 의존성: Task 12
- 병렬처리: 불가
```

## 작업 흐름도
```
Task 1 (Errable→Erratic)
  ↓
  ├─→ Task 2 (ErrableFactory) ─┐
  └─→ Task 3 (ErrableBuilder) ─┼─→ Task 4 (Errable static) → Task 5 (Index 삭제)
                                 │                              ↓
                                 │                          Task 6 (E 별칭)
                                 ↓                              ↓
  ├─→ Task 7 (FactoryTests) ─┐                              ↓
  ├─→ Task 8 (BuilderTests) ─┼─→ Task 11 (Build) → Task 12 (Test) → Task 13 (Examples)
  ├─→ Task 9 (UsageTests) ──┤
  └─→ Task 10 (FormatTests) ─┘
```

## 병렬 처리 그룹

### Group A (Phase 2 - Factory/Builder 업데이트)
- Task 2 (ErrableFactory 업데이트)
- Task 3 (ErrableBuilder 업데이트)

**병렬 가능**: 모두 Task 1 완료 후 동시 진행 가능

### Group B (Phase 4 - 테스트 업데이트)
- Task 7 (ErrableFactoryTests)
- Task 8 (ErrableBuilderTests)
- Task 9 (UsageExampleTests)
- Task 10 (ErrableFormattingTests)

**병렬 가능**: 모두 Task 4 완료 후 동시 진행 가능

## 예상 결과
```csharp
// 변경 전
var error = global::Errable.E.Error("message");
Errable err = error as Errable;
Errable<int> result = GetValue();

// 변경 후
var error = Errable.Error("message");        // 직관적인 API
Erratic err = error as Erratic;              // 명확한 타입명
Errable<int> result = GetValue();            // Result 패턴 유지
```

## 변수명 명확성 개선
```csharp
// 이제 타입이 더욱 명확하게 구분됨
Erratic pureError = Errable.Error("Database failed");      // 순수 에러
Errable<User> userResult = userService.GetUser(123);       // Result 패턴

// 변수명으로 용도가 명확해짐
if (userResult.IsError)
{
    Erratic userError = userResult.Error;    // Erratic = 예측불가한 에러
    logger.LogError(userError.Error());
}
```

## 주의사항
- Task 1은 전체 작업의 기반이므로 신중하게 진행
- Task 2, 3은 병렬 처리 가능하지만 Task 4 전에 완료 필요
- Phase 4의 테스트 업데이트는 병렬 처리하여 시간 단축 가능
- 각 Phase 완료 후 컴파일 가능한지 확인
- Erratic = "예측불가한, 불규칙한" 의미로 에러의 특성 표현

## 체크리스트

### Phase 1
- [ ] Task 1: Errable → Erratic 클래스명 변경

### Phase 2
- [ ] Task 2: ErrableFactory 클래스 업데이트
- [ ] Task 3: ErrableBuilder 클래스 업데이트

### Phase 3
- [ ] Task 4: 새로운 Errable static 클래스 생성
- [ ] Task 5: Index.cs 파일 삭제
- [ ] Task 6: E 별칭 추가 (선택사항)

### Phase 4
- [ ] Task 7: ErrableFactoryTests 업데이트
- [ ] Task 8: ErrableBuilderTests 업데이트
- [ ] Task 9: UsageExampleTests 업데이트
- [ ] Task 10: ErrableFormattingTests 업데이트

### Phase 5
- [ ] Task 11: 전체 프로젝트 빌드
- [ ] Task 12: 단위 테스트 실행
- [ ] Task 13: API 사용 예제 업데이트

## 완료 기준
- 모든 테스트가 통과함 (80/80)
- `Erratic`과 `Errable<T>`가 명확히 구분됨
- Static API `Errable.Error()`, `Errable.Code()` 정상 작동
- 패키지명과 주요 API 클래스명이 일치함 (`Errable`)
- 에러 타입명이 의미상 적절함 (`Erratic` = 예측불가한)