# Tasks: Errable Error Handling Library

**Input**: Design documents from `spec.md` and `IMPLEMENTATION_TASKS.md`
**Prerequisites**: Specification document defining Go-like error handling for C#
**Tech Stack**: C# (.NET 6+), System.Text.Json, Serilog integration

## Execution Flow (main)
```
1. Load spec.md from project root
   → Extract: Core interfaces, Result pattern, Builder API
2. Analyze implementation requirements:
   → Core: Error interface, Errable<T>, ErrableImpl
   → Builder: Fluent API, Static factory methods
   → Integration: Serialization, logging, formatting
3. Generate tasks by category:
   → Setup: Project structure, dependencies, configuration
   → Core: Minimal interfaces and core types
   → Builder: Fluent API and factory methods
   → Integration: Framework auto-integration
   → Testing: Unit tests, integration tests, benchmarks
4. Apply task rules:
   → Different files = mark [P] for parallel
   → Same file = sequential (no [P])
   → Tests before implementation (TDD approach)
5. Number tasks sequentially (T001, T002...)
6. Generate dependency graph
7. Create parallel execution examples
```

## Format: `[ID] [P?] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

## Path Conventions
- **Library project**: `src/Errable/`, `tests/Errable.Tests/` at repository root
- **Benchmarks**: `benchmarks/Errable.Benchmarks/`
- **Samples**: `samples/Errable.Samples/`

## Phase 1: Project Setup

- [ ] T001 Create solution and project structure with .NET 6+ targeting
- [ ] T002 [P] Configure NuGet package metadata and dependencies
- [ ] T003 [P] Setup EditorConfig and code style rules
- [ ] T004 [P] Configure CI/CD pipeline with GitHub Actions

## Phase 2: Core Infrastructure (MUST COMPLETE BEFORE 2.3)

### 2.1 Minimal Interfaces
- [ ] T005 [P] Error interface in `src/Errable/Error.cs`
- [ ] T006 [P] ErrorCoder interface in `src/Errable/ErrorCoder.cs`
- [ ] T007 [P] ErrorCauser interface in `src/Errable/ErrorCauser.cs`
- [ ] T008 [P] ErrorContextProvider interface in `src/Errable/ErrorContextProvider.cs`

### 2.2 Core Types
- [ ] T009 [P] Code value type in `src/Errable/Code.cs`
- [ ] T010 [P] Errable<T> struct in `src/Errable/Errable.cs`

### 2.3 Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 2.4
**CRITICAL: These tests MUST be written and MUST FAIL before ANY implementation**
- [ ] T011 [P] Error interface tests in `tests/Errable.Tests/ErrorTests.cs`
- [ ] T012 [P] Code type tests in `tests/Errable.Tests/CodeTests.cs`
- [ ] T013 [P] Errable<T> tests in `tests/Errable.Tests/ErrableTests.cs`
- [ ] T014 [P] Builder API tests in `tests/Errable.Tests/ErrableBuilderTests.cs`
- [ ] T015 [P] Static factory tests in `tests/Errable.Tests/ErrableFactoryTests.cs`

### 2.4 Core Implementation (ONLY after tests are failing)
- [ ] T016 ErrableImpl class core functionality in `src/Errable/ErrableImpl.cs`
- [ ] T017 Internal data structures and immutability in `src/Errable/ErrableImpl.cs`
- [ ] T018 Automatic stack trace capture in `src/Errable/ErrableImpl.cs`
- [ ] T019 Error() method implementation in `src/Errable/ErrableImpl.cs`

## Phase 3: Builder Pattern and Factory

### 3.1 Builder Implementation
- [ ] T020 [P] ErrableBuilder core class in `src/Errable/ErrableBuilder.cs`
- [ ] T021 Context builder methods (With, WithLazy) in `src/Errable/ErrableBuilder.cs`
- [ ] T022 Classification methods (Tags, In, Domain) in `src/Errable/ErrableBuilder.cs`
- [ ] T023 Metadata methods (Public, Hint, Owner) in `src/Errable/ErrableBuilder.cs`
- [ ] T024 Tracking methods (Trace, Span, Since, Duration) in `src/Errable/ErrableBuilder.cs`
- [ ] T025 User/Tenant methods in `src/Errable/ErrableBuilder.cs`
- [ ] T026 [P] HTTP integration methods in `src/Errable/HttpExtensions.cs`
- [ ] T027 Builder termination methods (Error, Errorf, Wrap, Wrapf) in `src/Errable/ErrableBuilder.cs`

### 3.2 Static Factory
- [ ] T028 [P] Direct creation methods in `src/Errable/Errable.Factory.cs`
- [ ] T029 [P] Exception wrapping methods in `src/Errable/Errable.Factory.cs`
- [ ] T030 [P] Errable<T> wrapping methods in `src/Errable/Errable.Factory.cs`
- [ ] T031 Builder entry point (Code method) in `src/Errable/Errable.Factory.cs`

## Phase 4: Framework Integration

### 4.1 Formatting and Serialization
- [ ] T032 [P] IFormattable implementation in `src/Errable/Integration/ErrableFormatter.cs`
- [ ] T033 [P] Format string support (F, C, D, J, P, L) in `src/Errable/Integration/ErrableFormatter.cs`
- [ ] T034 [P] JsonConverter implementation in `src/Errable/Integration/ErrableJsonConverter.cs`
- [ ] T035 [P] ISerializable implementation in `src/Errable/Integration/ErrableSerializer.cs`

### 4.2 Logging Integration
- [ ] T036 [P] Serilog ILogEventEnricher in `src/Errable/Integration/SerilogEnricher.cs`
- [ ] T037 [P] Microsoft.Extensions.Logging integration in `src/Errable/Integration/LoggingExtensions.cs`
- [ ] T038 [P] NLog integration in `src/Errable/Integration/NLogIntegration.cs`
- [ ] T039 Auto-detection mechanism in `src/Errable/Integration/LoggingDetector.cs`

### 4.3 Framework Integration Tests
- [ ] T040 [P] Formatting integration tests in `tests/Errable.Tests/Integration/FormattingTests.cs`
- [ ] T041 [P] JSON serialization tests in `tests/Errable.Tests/Integration/JsonTests.cs`
- [ ] T042 [P] Serilog integration tests in `tests/Errable.Tests/Integration/SerilogTests.cs`
- [ ] T043 [P] Extensions.Logging tests in `tests/Errable.Tests/Integration/LoggingTests.cs`

### 4.4 ASP.NET Core Integration
- [ ] T044 [P] IProblemDetailsProvider in `src/Errable/Integration/ProblemDetailsProvider.cs`
- [ ] T045 [P] IEnumerable<KeyValuePair> in `src/Errable/Integration/ErrableEnumerable.cs`
- [ ] T046 [P] ASP.NET Core integration tests in `tests/Errable.Tests/Integration/AspNetCoreTests.cs`

## Phase 5: Extensions and LINQ

### 5.1 LINQ Extensions
- [ ] T047 [P] Errable<T> Map and FlatMap in `src/Errable/Extensions/ErrableExtensions.cs`
- [ ] T048 [P] Filter and OrElse methods in `src/Errable/Extensions/ErrableExtensions.cs`
- [ ] T049 [P] Collection extensions (Sequence, Traverse) in `src/Errable/Extensions/CollectionExtensions.cs`
- [ ] T050 [P] Error type-safe extensions in `src/Errable/Extensions/ErrorExtensions.cs`

### 5.2 Extension Tests
- [ ] T051 [P] LINQ extension tests in `tests/Errable.Tests/Extensions/LinqTests.cs`
- [ ] T052 [P] Error extension tests in `tests/Errable.Tests/Extensions/ErrorExtensionTests.cs`

## Phase 6: Performance and Polish

### 6.1 Performance Optimization
- [ ] T053 [P] Memory allocation optimization in `src/Errable/ErrableImpl.cs`
- [ ] T054 [P] String formatting optimization in `src/Errable/Integration/ErrableFormatter.cs`
- [ ] T055 [P] Builder performance optimization in `src/Errable/ErrableBuilder.cs`

### 6.2 Benchmarks and Testing
- [ ] T056 [P] Performance benchmarks in `benchmarks/Errable.Benchmarks/ErrableBenchmarks.cs`
- [ ] T057 [P] Memory benchmarks in `benchmarks/Errable.Benchmarks/MemoryBenchmarks.cs`
- [ ] T058 [P] Thread safety tests in `tests/Errable.Tests/ConcurrencyTests.cs`
- [ ] T059 [P] Comprehensive integration tests in `tests/Errable.Tests/Integration/ComprehensiveTests.cs`

### 6.3 Documentation and Samples
- [ ] T060 [P] XML documentation completion across all public APIs
- [ ] T061 [P] ASP.NET Core sample in `samples/Errable.Samples.WebApi/`
- [ ] T062 [P] Console application sample in `samples/Errable.Samples.Console/`
- [ ] T063 [P] Custom error type example in `samples/Errable.Samples.CustomTypes/`

## Dependencies

**Critical Path:**
1. Setup (T001-T004) → Core interfaces (T005-T010) → Tests (T011-T015) → Implementation (T016-T019)
2. T019 → Builder (T020-T031) → Integration (T032-T046) → Extensions (T047-T052) → Polish (T053-T063)

**Parallel Groups:**
- Phase 1: T002, T003, T004 can run together
- Core interfaces: T005-T008 can run together
- Core types: T009, T010 can run together
- TDD tests: T011-T015 can run together
- Integration: T032-T035, T036-T039, T040-T043, T044-T046 groups can run in parallel
- Extensions: T047-T050, T051-T052 can run together
- Polish: T053-T055, T056-T059, T060-T063 groups can run in parallel

## Parallel Example
```bash
# Launch core interface tests together (T011-T015):
Task: "Error interface tests in tests/Errable.Tests/ErrorTests.cs"
Task: "Code type tests in tests/Errable.Tests/CodeTests.cs"
Task: "Errable<T> tests in tests/Errable.Tests/ErrableTests.cs"
Task: "Builder API tests in tests/Errable.Tests/ErrableBuilderTests.cs"
Task: "Static factory tests in tests/Errable.Tests/ErrableFactoryTests.cs"

# Launch integration components together (T032-T035):
Task: "IFormattable implementation in src/Errable/Integration/ErrableFormatter.cs"
Task: "Format string support in src/Errable/Integration/ErrableFormatter.cs"
Task: "JsonConverter implementation in src/Errable/Integration/ErrableJsonConverter.cs"
Task: "ISerializable implementation in src/Errable/Integration/ErrableSerializer.cs"
```

## Notes
- [P] tasks = different files, no dependencies
- Verify tests fail before implementing
- Follow TDD approach strictly
- Maintain immutability throughout
- Optimize for minimal allocations
- All framework integrations must be automatic
- Support custom Error types seamlessly

## Task Generation Rules
*Applied during task creation*

1. **From Specification**:
   - Each interface → separate file and test [P]
   - Each core type → separate implementation and test [P]
   - Each integration → separate module [P]

2. **From Architecture**:
   - Builder pattern → fluent methods in same file
   - Factory methods → static methods grouped by function
   - Framework integration → conditional and automatic

3. **TDD Ordering**:
   - Tests → Implementation → Integration → Polish
   - Core → Builder → Extensions → Optimization

4. **Performance Requirements**:
   - Memory optimization tasks for core components
   - Benchmark tasks for critical paths
   - Thread safety validation

## Validation Checklist
*GATE: Checked before implementation completion*

- [ ] All Error interface contracts have tests
- [ ] All Builder methods have tests
- [ ] All framework integrations work automatically
- [ ] Parallel tasks truly independent (different files)
- [ ] Each task specifies exact file path
- [ ] No task modifies same file as another [P] task
- [ ] TDD approach maintained throughout
- [ ] Performance requirements met
- [ ] Immutability guaranteed
- [ ] Zero-friction principle upheld