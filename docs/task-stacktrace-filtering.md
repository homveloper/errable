# Task: StackTrace Filtering Implementation

## Overview
Improve `Erratic` class to filter out test framework and system noise from stack traces, showing only relevant user code.

## Tasks

### Task 1: FilteredStackTrace 클래스 생성
**파일**: `/src/Errable/FilteredStackTrace.cs` (새 파일 생성)
**작업 내용**:
- `StackFrame[]` 배열을 관리하는 새로운 클래스 생성
- 생성자에서 자동 필터링 로직 적용
- ToString() 메서드로 포맷팅된 스택 트레이스 문자열 반환
- Serializable 특성 추가 (Erratic이 Serializable이므로)

### Task 2: 필터링 로직 구현
**파일**: `/src/Errable/FilteredStackTrace.cs`
**작업 내용**:
- `FilterFrames()` 메서드 구현
- 제외할 네임스페이스 배열 정의:
  - `Xunit.`
  - `System.Runtime.`
  - `System.Threading.`
  - `Microsoft.TestPlatform.`
  - `Microsoft.VisualStudio.`
  - `System.Reflection.`
- 사용자 코드만 남기는 필터링 로직

### Task 3: Erratic 클래스 필드 타입 변경
**파일**: `/src/Errable/Erratic.cs`
**작업 내용**:
- 21번 줄: `private readonly StackTrace? _stackTrace;` → `private readonly FilteredStackTrace? _stackTrace;`
- using 구문에 `System.Diagnostics` 유지 (StackFrame을 위해)

### Task 4: Erratic 생성자 수정
**파일**: `/src/Errable/Erratic.cs`
**작업 내용**:
- 42번 줄: 생성자 매개변수 `StackTrace? stackTrace = null` → `FilteredStackTrace? stackTrace = null`
- 58번 줄: `_stackTrace = stackTrace ?? new StackTrace(skipFrames: 2, fNeedFileInfo: true);` → `_stackTrace = stackTrace ?? new FilteredStackTrace(skipFrames: 2);`

### Task 5: Erratic 역직렬화 생성자 수정
**파일**: `/src/Errable/Erratic.cs`
**작업 내용**:
- 81번 줄: `_stackTrace = (StackTrace?)info.GetValue(nameof(_stackTrace), typeof(StackTrace));` → `_stackTrace = (FilteredStackTrace?)info.GetValue(nameof(_stackTrace), typeof(FilteredStackTrace));`

### Task 6: StackTrace 속성 타입 변경
**파일**: `/src/Errable/Erratic.cs`
**작업 내용**:
- 135번 줄: `public StackTrace? StackTrace => _stackTrace;` → `public FilteredStackTrace? StackTrace => _stackTrace;`

### Task 7: GetObjectData 메서드 수정
**파일**: `/src/Errable/Erratic.cs`
**작업 내용**:
- 232번 줄: 직렬화 시 FilteredStackTrace 타입으로 저장되도록 수정

### Task 8: ErrableFactory 파일 수정
**파일**: `/src/Errable/ErrableFactory.cs`
**작업 내용**:
- 23번 줄: `stackTrace: new StackTrace(skipFrames: 1, fNeedFileInfo: true)` → `stackTrace: new FilteredStackTrace(skipFrames: 1)`
- 59번 줄, 92번 줄, 104번 줄, 125번 줄, 137번 줄, 151번 줄, 168번 줄: 동일하게 수정

### Task 9: ErrableBuilder 파일 수정
**파일**: `/src/Errable/ErrableBuilder.cs`
**작업 내용**:
- 357번 줄: `stackTrace: new StackTrace(skipFrames: 1, fNeedFileInfo: true)` → `stackTrace: new FilteredStackTrace(skipFrames: 1)`

### Task 10: ErrableBuilderT 파일 수정
**파일**: `/src/Errable/ErrableBuilderT.cs`
**작업 내용**:
- 319번 줄: `stackTrace: new StackTrace(skipFrames: 1, fNeedFileInfo: true)` → `stackTrace: new FilteredStackTrace(skipFrames: 1)`

### Task 11: 테스트 업데이트
**파일**: `/tests/Errable.Tests/ErrableTests.cs`
**작업 내용**:
- `TestErrorLogging_WithStackTrace` 테스트 메서드에서 필터링 결과 확인
- 테스트 프레임워크 관련 스택이 제거되었는지 검증

### Task 12: 포맷팅 테스트 업데이트
**파일**: `/tests/Errable.Tests/ErrableFormattingTests.cs`
**작업 내용**:
- 56번 줄: `Errable_ToString_DebugFormat_ShouldIncludeStackTrace` 테스트에서 FilteredStackTrace 확인
- 필터링된 스택 트레이스가 포함되는지 검증

## 우선순위
1. **Task 1-2**: FilteredStackTrace 클래스 생성 및 필터링 로직 (핵심)
2. **Task 3-7**: Erratic 클래스 수정 (타입 변경)
3. **Task 8-10**: Factory 및 Builder 클래스 수정
4. **Task 11-12**: 테스트 검증

## 검증 방법
- 기존 테스트가 모두 통과하는지 확인
- 새로운 스택 트레이스에서 xUnit 관련 프레임이 제거되었는지 확인
- 사용자 코드와 Errable 라이브러리 코드만 남았는지 확인

## Expected Result
```
현재 출력:
Stack: at Errable.Tests.ErrableTests.TestErrorLogging_WithStackTrace() in /Users/.../ErrableTests.cs:line 208
   at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean isConstructor)
   at Xunit.Sdk.TestInvoker`1.CallTestMethod(Object testClassInstance)
   ...

개선 후 출력:
Stack: at Errable.Tests.ErrableTests.TestErrorLogging_WithStackTrace() in /Users/.../ErrableTests.cs:line 208
   at Errable.ErrableFactory.Error(String message) in /Users/.../ErrableFactory.cs:line 23
```