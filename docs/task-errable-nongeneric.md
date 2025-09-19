# Errable 비제네릭 타입 추가 작업 계획서

## 작업 개요
`ErrableImpl`을 `Errable`로 변경하여 비제네릭 순수 에러 타입으로 만들고, Static Factory를 `E`로 분리

## 작업 목록

### Phase 1: 준비 작업
```
[Task 1] 백업 브랜치 생성
- 위치: Git
- 작업: `git checkout -b refactor/errable-nongeneric`
- 의존성: 없음
- 병렬처리: 불가 (시작점)
```

### Phase 2: 핵심 클래스 이름 변경
```
[Task 2] ErrableImpl → Errable 이름 변경
- 파일: src/Errable/ErrableImpl.cs
- 작업:
  - 파일명을 ErrableImpl.cs → Errable.cs로 변경
  - 클래스명 변경: `public sealed class ErrableImpl` → `public sealed class Errable`
  - 생성자명 변경: `ErrableImpl(...)` → `Errable(...)`
- 의존성: Task 1
- 병렬처리: 불가
```

### Phase 3: 기존 Errable.cs 파일 분리
```
[Task 3] Errable<T> struct 분리
- 파일: src/Errable/Errable.cs → src/Errable/ErrableT.cs (새 파일)
- 작업:
  - Errable<T> struct 정의 부분만 ErrableT.cs로 이동
  - 파일 상단에 namespace와 using 문 추가
  - XML 주석 유지
- 의존성: Task 2
- 병렬처리: Task 4와 병렬 가능

[Task 4] Static Factory를 E 클래스로 분리
- 파일: src/Errable/Errable.cs → src/Errable/E.cs (새 파일)
- 작업:
  - `public static class Errable` → `public static class E`
  - 모든 static 메서드들 E.cs로 이동
  - Direct Error Creation 메서드들 유지
  - Builder Entry Points 메서드들 유지
- 의존성: Task 2
- 병렬처리: Task 3과 병렬 가능

[Task 5] 기존 Errable.cs 파일 삭제
- 파일: src/Errable/Errable.cs
- 작업: 파일 삭제 (내용이 모두 이동된 후)
- 의존성: Task 3, Task 4 완료 후
- 병렬처리: 불가
```

### Phase 4: Factory 및 Builder 업데이트
```
[Task 6] ErrableFactory 클래스 업데이트
- 파일: src/Errable/ErrableFactory.cs
- 작업:
  - 모든 `new ErrableImpl(...)` → `new Errable(...)`
  - 반환 타입은 Error 인터페이스 유지
  - import 문 확인
- 의존성: Task 2
- 병렬처리: Task 7, 8과 병렬 가능

[Task 7] ErrableBuilder 클래스 업데이트
- 파일: src/Errable/ErrableBuilder.cs
- 작업:
  - CreateError 메서드에서 `new ErrableImpl(...)` → `new Errable(...)`
  - 반환 타입은 Error 유지
- 의존성: Task 2
- 병렬처리: Task 6, 8과 병렬 가능

[Task 8] E 클래스에서 ErrableFactory 호출 확인
- 파일: src/Errable/E.cs
- 작업:
  - 모든 `ErrableFactory.XXX` 호출이 정상 작동하는지 확인
  - namespace와 using 문 확인
- 의존성: Task 4
- 병렬처리: Task 6, 7과 병렬 가능
```

### Phase 5: 테스트 코드 업데이트
```
[Task 9] ErrableFactoryTests 업데이트
- 파일: tests/Errable.Tests/ErrableFactoryTests.cs
- 작업:
  - `Errable.Error(...)` → `E.Error(...)`
  - `Errable.Code(...)` → `E.Code(...)`
  - `Errable.Wrap(...)` → `E.Wrap(...)`
  - 모든 Errable. 정적 호출을 E.로 변경
  - `as ErrableImpl` → `as Errable`
- 의존성: Task 4, 6
- 병렬처리: Task 10, 11, 12와 병렬 가능

[Task 10] ErrableBuilderTests 업데이트
- 파일: tests/Errable.Tests/ErrableBuilderTests.cs
- 작업:
  - `Errable.Code(...)` → `E.Code(...)`
  - 모든 Errable. 정적 호출을 E.로 변경
  - `as ErrableImpl` → `as Errable`
- 의존성: Task 4, 7
- 병렬처리: Task 9, 11, 12와 병렬 가능

[Task 11] UsageExampleTests 업데이트
- 파일: tests/Errable.Tests/UsageExampleTests.cs
- 작업:
  - `Errable.Error(...)` → `E.Error(...)`
  - `Errable.Code(...)` → `E.Code(...)`
  - 모든 Errable. 정적 호출을 E.로 변경
  - `as ErrableImpl` → `as Errable`
- 의존성: Task 4
- 병렬처리: Task 9, 10, 12와 병렬 가능

[Task 12] 나머지 테스트 파일 업데이트
- 파일: tests/Errable.Tests/*.cs (나머지 모든 테스트 파일)
- 작업:
  - ErrableImpl 참조를 Errable로 변경
  - Errable. 정적 호출을 E.로 변경
- 의존성: Task 4
- 병렬처리: Task 9, 10, 11과 병렬 가능
```

### Phase 6: 빌드 및 테스트
```
[Task 13] 전체 프로젝트 빌드
- 명령: `dotnet build`
- 작업: 컴파일 에러 확인 및 수정
- 의존성: Task 9, 10, 11, 12 완료 후
- 병렬처리: 불가

[Task 14] 단위 테스트 실행
- 명령: `dotnet test`
- 작업: 모든 테스트가 통과하는지 확인
- 의존성: Task 13
- 병렬처리: 불가

[Task 15] 사용 예제 작성 및 테스트
- 파일: 새로운 예제 파일 생성 (옵션)
- 작업:
  - Errable err = E.Error("test") 형태 테스트
  - Errable<int> result 형태 테스트
  - 타입 구분이 명확한지 확인
- 의존성: Task 14
- 병렬처리: 불가
```

## 작업 흐름도
```
Task 1
  ↓
Task 2
  ↓
  ├─→ Task 3 ─┐
  ├─→ Task 4 ─┼─→ Task 5
  ├─→ Task 6 ─┤
  ├─→ Task 7 ─┤
  └─→ Task 8 ─┘
       ↓
  ├─→ Task 9  ─┐
  ├─→ Task 10 ─┤
  ├─→ Task 11 ─┼─→ Task 13 → Task 14 → Task 15
  └─→ Task 12 ─┘
```

## 병렬 처리 그룹

### Group A (Phase 4 - Factory/Builder 업데이트)
- Task 6 (ErrableFactory 업데이트)
- Task 7 (ErrableBuilder 업데이트)
- Task 8 (E 클래스 검증)

**병렬 가능**: 모두 Task 2 완료 후 동시 진행 가능

### Group B (Phase 5 - 테스트 업데이트)
- Task 9 (ErrableFactoryTests)
- Task 10 (ErrableBuilderTests)
- Task 11 (UsageExampleTests)
- Task 12 (나머지 테스트들)

**병렬 가능**: 모두 Task 4 완료 후 동시 진행 가능

## 예상 결과
```csharp
// 변경 전
ErrableImpl err = new ErrableImpl(...);
var error = Errable.Error("message");
Errable<int> result = GetValue();

// 변경 후
Errable err = new Errable(...);  // 순수 에러 타입
var error = E.Error("message");   // Static factory
Errable<int> result = GetValue(); // Result 패턴
```

## 변수명 명확성 개선
```csharp
// 이제 타입이 명확하게 구분됨
Errable pureError = E.Error("Database connection failed");
Errable<User> resultWithUser = userService.GetUser(123);
Errable<string> resultWithString = fileService.ReadFile(path);

// 변수명으로 용도가 명확해짐
if (resultWithUser.IsError)
{
    Errable userError = resultWithUser.Error;  // 순수 에러 타입
    logger.LogError(userError.Error());
}
```

## 주의사항
- Task 2는 전체 작업의 기반이므로 신중하게 진행
- Task 3, 4는 병렬 처리 가능하지만 Task 5 전에 완료 필요
- Phase 5의 테스트 업데이트는 병렬 처리하여 시간 단축 가능
- 각 Phase 완료 후 컴파일 가능한지 확인
- Static Factory 이름을 `E`로 선택한 이유: 짧고 타이핑하기 쉬움

## 체크리스트

### Phase 1
- [ ] Task 1: 백업 브랜치 생성

### Phase 2
- [ ] Task 2: ErrableImpl → Errable 이름 변경

### Phase 3
- [ ] Task 3: Errable<T> struct 분리
- [ ] Task 4: Static Factory를 E 클래스로 분리
- [ ] Task 5: 기존 Errable.cs 파일 삭제

### Phase 4
- [ ] Task 6: ErrableFactory 클래스 업데이트
- [ ] Task 7: ErrableBuilder 클래스 업데이트
- [ ] Task 8: E 클래스에서 ErrableFactory 호출 확인

### Phase 5
- [ ] Task 9: ErrableFactoryTests 업데이트
- [ ] Task 10: ErrableBuilderTests 업데이트
- [ ] Task 11: UsageExampleTests 업데이트
- [ ] Task 12: 나머지 테스트 파일 업데이트

### Phase 6
- [ ] Task 13: 전체 프로젝트 빌드
- [ ] Task 14: 단위 테스트 실행
- [ ] Task 15: 사용 예제 작성 및 테스트

## 완료 기준
- 모든 테스트가 통과함
- `Errable`과 `Errable<T>`가 명확히 구분됨
- Static Factory `E` 클래스가 정상 작동함
- 변수명과 타입이 일치하여 코드 가독성 향상됨