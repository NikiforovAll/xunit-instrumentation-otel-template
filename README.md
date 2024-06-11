# XUnit.Otel.Template

[![Build](https://github.com/NikiforovAll/xunit-instrumentation-otel-template/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/NikiforovAll/xunit-instrumentation-otel-template/actions/workflows/build.yml)
[![CodeQL](https://github.com/NikiforovAll/xunit-instrumentation-otel-template/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/NikiforovAll/xunit-instrumentation-otel-template/actions/workflows/codeql-analysis.yml)
[![NuGet](https://img.shields.io/nuget/dt/XUnit.Otel.Template.svg)](https://nuget.org/packages/XUnit.Otel.Template)
[![Conventional Commits](https://img.shields.io/badge/Conventional%20Commits-1.0.0-yellow.svg)](https://conventionalcommits.org)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/nikiforovall/xunit-instrumentation-otel-template/blob/main/LICENSE.md)

Template with OpenTelemetry test instrumentation for XUnit.

## Installation

```bash
dotnet new install XUnit.Otel.Template
```

## Usage

Find template:

```bash
❯ dotnet new list xunit-otel
# These templates matched your input: 'xunit-otel'

# Template Name  Short Name  Language  Tags
# -------------  ----------  --------  -------------------------
# XUnit Otel     xunit-otel  [C#]      XUnit/Tests/OpenTelemetry
```

Verify output:

```bash
❯ dotnet new xunit-otel -o $dev/XUnitOtelExample01 -n XUnitOtelExample --dry-run
# File actions would have been taken:
#   Create: $dev\XUnitOtelExample01\BaseFixture.cs
#   Create: $dev\XUnitOtelExample01\Directory.Packages.props
#   Create: $dev\XUnitOtelExample01\ExampleTests.cs
#   Create: $dev\XUnitOtelExample01\Extensions.cs
#   Create: $dev\XUnitOtelExample01\MonitoringFramework\BaseTraceTestAttribute.cs
#   Create: $dev\XUnitOtelExample01\MonitoringFramework\TestRunSpanProcessor.cs
#   Create: $dev\XUnitOtelExample01\MonitoringFramework\TracePerTestAttribute.cs
#   Create: $dev\XUnitOtelExample01\MonitoringFramework\TracePerTestRunAttribute.cs
#   Create: $dev\XUnitOtelExample01\README.md
#   Create: $dev\XUnitOtelExample01\XUnitOtelExample.csproj
#   Create: $dev\XUnitOtelExample01\xunit.runner.json
```

Install:

```bash
❯ dotnet new xunit-otel -o $dev/XUnitOtelExample01 -n XUnitOtelExample
```

Run tests:

```bash
❯ cd $dev/XUnitOtelExample01
❯ dotnet test
# Restore complete (0.4s)
# You are using a preview version of .NET. See: https://aka.ms/dotnet-support-policy
#   XUnitOtelExample succeeded (0.3s) → bin\Debug\net8.0\XUnitOtelExample.dll
#   XUnitOtelExample test failed with errors (3.3s)
#     C:\Program Files\dotnet\sdk\9.0.100-preview.2.24157.14\Microsoft.TestPlatform.targets(46,5): error : [xUnit.net 00:00:01.77]     XUnitOtelExample.ExampleTests.WaitRandomTime_Fail [FAIL] [$devXUnitOtelExample01\XUnitOtelExample.csproj]
#     $devXUnitOtelExample01\ExampleTests.cs(51): error VSTEST1: XUnitOtelExample.ExampleTests.<>c.<WaitRandomTime_Fail>b__3_0() Assert.True() Failure [$devXUnitOtelExample01\XUnitOtelExample.csproj]
# $devXUnitOtelExample01\ExampleTests.cs(51): error VSTEST1: Expected: True [$devXUnitOtelExample01\XUnitOtelExample.csproj]
# $devXUnitOtelExample01\ExampleTests.cs(51): error VSTEST1: Actual:   False [$devXUnitOtelExample01\XUnitOtelExample.csproj]

# Build failed with errors in 4.4s
# Test run failed. Total: 3 Failed: 1 Passed: 2 Skipped: 0, Duration: 3.3s
```

See traces:

Navigate to <http://localhost:18888/traces>.