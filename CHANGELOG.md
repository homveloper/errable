# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.2.2] - 2025-09-30

### Performance
- Optimize `GetHashCode()` to avoid unnecessary string allocation by using `GetHashCode()` directly instead of `Error()` string generation
- Improve `Equals()` method with explicit fast-path reference checking before expensive string comparison

### Changed
- Add `Code.Empty` static field for consistent empty code representation
- Add explicit `Code(string)` and `Code(int)` overloads for better API discoverability
- Replace all `new Code("")` and `code: 0` usages with `Code.Empty` throughout the codebase

### Fixed
- Remove exception throwing from `Code` constructor to align with "no exceptions" philosophy
- `Code` constructor now uses defensive null coalescing (`?? string.Empty`) instead of throwing `ArgumentNullException`

### Documentation
- Enhance README with comprehensive "Error as Value" philosophy explanation
- Add before/after code examples comparing exception-based vs value-based error handling
- Highlight key benefits: explicit control flow, type safety, zero overhead

## [0.2.1] - 2025-09-29

### Added
- Unity compatibility through multi-targeting (.NET Standard 2.1 support)
- MIT License

### Changed
- Bump version to 0.2.0 for FilteredStackTrace feature

## [0.2.0] - 2025-09-28

### Added
- `FilteredStackTrace` for clean error debugging
- Remove framework noise from stack traces
- Configurable frame skipping for accurate error origins

### Fixed
- Remove duplicate project inspiration references in README

---

**Note**: This changelog was created retroactively. Future releases will maintain this format consistently.