---
description: This file describes the C# unit testing conventions and best practices for the project.
applyTo: **/tests/Unit/*.Tests*/*.cs
---

# C# Unit Testing Instructions

## Overview

This instructions file defines the standard practices and conventions for writing unit tests in C# projects. All tests must follow xUnit framework with Moq for dependency mocking.

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
tests/Unit/
├── Services/
│   └── Services.Tests.csproj
├── Data/
│   └── Data.Tests.csproj
└── Models/
    └── Models.Tests.csproj
```

**Rules:**
- Test projects are created in the `tests/Unit/` folder at the root level
- Test project names follow the pattern: `<original-module-name>.Tests`
- Example: If source project is `src/Services/Services.csproj`, test project is `tests/Unit/Services/Services.Tests.csproj`

### Test File Structure

**Folder Structure:**
- Test folders within the test project must **mirror the relative path** of source code files in the source project
- Example: If source file is `Services/UserService.cs`, test file is `Services/UserServiceTests.cs` within the csproject files

**File Naming:**
- Test files must be named `<original-file-name>Tests.cs`
- Example: `UserService.cs` → `UserServiceTests.cs`

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