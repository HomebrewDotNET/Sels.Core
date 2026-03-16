---
description: This file describes the C# unit testing conventions and best practices for the project.
applyTo: **/tests/Integration/*.Tests*/*.cs
---

# C# Integration Testing Instructions

## Overview

This instructions file defines the standard practices and conventions for writing integration tests in C# projects. All tests must follow xUnit framework with Moq for dependency mocking only when needed.

## Project Structure

### Test Project Organization

Source code structure:
```
src/
├── Services/
│   └── Services.csproj
├── Data/
│   └── Data.csproj
└── Models/
    └── Models.csproj
```

Test structure:
```
tests/Integration/
├── Services/
│   └── Services.IntegrationTests.csproj
├── Data/
│   └── Data.IntegrationTests.csproj
└── Models/
    └── Models.IntegrationTests.csproj
```

**Rules:**
- Test projects are created in the `tests/Integration/` folder at the root level
- Test project names follow the pattern: `<original-module-name>.IntegrationTests`
- Example: If source project is `src/Services/Services.csproj`, test project is `tests/Integration/Services/Services.IntegrationTests.csproj`

### Test File Structure

**Folder Structure:**
- Test folders within the test project must **mirror the relative path** of source code files in the source project
- Example: If source file is `Services/UserService.cs`, test file is `Services/UserServiceIntegrationTests.cs` within the csproject files

**File Naming:**
- Test files must be named `<original-file-name>IntegrationTests.cs`
- Example: `UserService.cs` → `UserServiceIntegrationTests.cs`

## Test Method Naming Convention

Test method names must follow this pattern:

```
<OriginalMethodName>_<WhenCondition>_<ExpectedResult>
```

**Examples:**
- `GetUser_WithValidId_ReturnsUser`
- `GetUser_WithInvalidId_ThrowsArgumentException`
- `CalculateTotal_WithEmptyList_ReturnsZero`
- `ValidateEmail_WithNullInput_ThrowsArgumentNullException`

## Testing Framework & Dependencies

### Framework
- **Test Framework:** xUnit
- **Mocking Framework:** Moq
- **NuGet Packages Required:**
  - `xunit` (latest stable)
  - `xunit.runner.visualstudio` (latest stable)
  - `Moq` (latest stable)
  - `Microsoft.NET.Test.Sdk` (latest stable)
- **Test SDK Version:** Match the .NET version of the source project
- **Test Runner:** Use `dotnet test` CLI command on the sln file to run all tests
- **AAA Pattern:** Follow Arrange-Act-Assert pattern for test method structure
- **Mark test category** Add `[Trait("Category", "Integration")]` attribute to class to allow filtering by test category
- **Mark external tests with external dependencies** Add `[Trait("Dependency", "<DependencyNameHere>")]` if the test relies on an externally mocked service
- **Integration Testing** No dependency mocking by default. Use real dependencies and external resources where possible to ensure true integration testing. Mocking is only allowed for non-critical dependencies that are difficult to set up in test environments (e.g., third-party APIs).
- **Scope** Tests always target a class with all it's real dependencies.
- **Multiple implementations** When multiple implementations exist for a dependency shared tests should be created in a base class and then inherited by separate test classes for each implementation. End result should be permutation of tests across all implementations without duplication of test code.
- **Use same layer for data** When testing a service that depends on a data layer, use the actual data layer implementation instead of mocking it. This ensures that the integration test covers the interaction between the service and data layer as it would occur in production.
- **Each test that tests state should create it** Each test that relies on specific data or state should create that state within the test method itself. Avoid relying on shared state or pre-existing data to ensure tests are independent and repeatable.
- **Use a variety of test data in integration tests** Use a mix of valid, invalid, edge case, and typical data in integration tests to ensure comprehensive coverage of real-world scenarios. This helps identify issues that may arise from different types of input and interactions between components.