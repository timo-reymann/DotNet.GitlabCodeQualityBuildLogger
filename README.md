DotNet.GitlabCodeQualityBuildLogger
===
[![LICENSE](https://img.shields.io/github/license/timo-reymann/DotNet.GitlabCodeQualityBuildLogger)](https://github.com/timo-reymann/DotNet.GitlabCodeQualityBuildLogger/blob/main/LICENSE)
[![GitHub Actions](https://github.com/timo-reymann/DotNet.GitlabCodeQualityBuildLogger/actions/workflows/ci.yml/badge.svg)](https://github.com/timo-reymann/DotNet.GitlabCodeQualityBuildLogger/actions/workflows/ci.yml)
[![GitHub Release](https://img.shields.io/github/v/tag/timo-reymann/DotNet.GitlabCodeQualityBuildLogger?label=version)](https://github.com/timo-reymann/DotNet.GitlabCodeQualityBuildLogger/releases)

<p align="center">
    <img width="1024" src="./.github/images/banner.png">
    <br />
    An MSBuild logger that outputs build warnings and errors in <a href="https://docs.gitlab.com/ee/ci/testing/code_quality.html#implement-a-custom-tool">GitLab Code Quality report format</a>.
</p>

## Features

- Captures build errors and warnings during `dotnet build`
- Outputs JSON in GitLab Code Quality format
- Automatic severity mapping:
  - Build errors → `critical`
  - CS* (compiler warnings) → `major`
  - CA2*/CA5* (security rules) → `major`
  - CA* (other code analysis) → `minor`
  - IDE*/SA* (style suggestions) → `info`
  - Other warnings → `minor`
- Generates stable fingerprints for issue tracking

## Requirements

- .NET SDK 6.0 or later (for running the tool)
- MSBuild-based project

## Installation

### Project-level (recommended)

```sh
dotnet new tool-manifest # if you don't have a tool manifest yet
dotnet tool install GitlabCodeQualityBuildLogger.Tool
```

### Global

```sh
dotnet tool install -g GitlabCodeQualityBuildLogger.Tool
```

## Usage

### Global

```sh
dotnet build -logger:"$(dotnet tool run gitlab-code-quality-logger);gl-code-quality-report.json"
```

### Global

```sh
dotnet build -logger:"$(gitlab-code-quality-logger);gl-code-quality-report.json"
```

### GitLab CI Example

```yaml
build:
  stage: build
  script:
    - dotnet tool restore
    - dotnet build -logger:"$(gitlab-code-quality-logger);gl-code-quality-report.json"
  artifacts:
    reports:
      codequality: gl-code-quality-report.json
```

### Output Format

The logger produces JSON compatible with GitLab's Code Quality report format:

```json
[
  {
    "description": "CS0168: The variable 'x' is declared but never used",
    "check_name": "CS0168",
    "fingerprint": "A1B2C3D4E5F6...",
    "severity": "major",
    "location": {
      "path": "src/MyClass.cs",
      "lines": {
        "begin": 42
      }
    }
  }
]
```

## Motivation

GitLab's Code Quality feature provides a great way to track code issues directly in merge requests. However, there was no simple way to integrate .NET build warnings and errors into this workflow. This logger bridges that gap by converting MSBuild output into GitLab's expected format.

## Contributing

I love your input! I want to make contributing to this project as easy and transparent as possible, whether it's:

- Reporting a bug
- Discussing the current state of the configuration
- Submitting a fix
- Proposing new features
- Becoming a maintainer

To get started please read the [Contribution Guidelines](./CONTRIBUTING.md).

## Development

### Requirements

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)

### Build

```sh
dotnet build
```

### Test

```sh
dotnet test
```

### Project Structure

```
src/
├── DotNet.GitlabCodeQualityBuildLogger/       # Core logger library (netstandard2.1)
└── DotNet.GitlabCodeQualityBuildLogger.Tool/  # CLI tool wrapper (net9.0)
tests/
└── DotNet.GitlabCodeQualityBuildLogger.Tests/ # xUnit tests
```

---
