# CsTsTypeGen

[![NuGet](https://img.shields.io/nuget/v/CsTsTypeGen.svg)](https://www.nuget.org/packages/CsTsTypeGen/)

A streamlined tool to generate a typedefs.d.ts TypeScript type definitions file from all of your C# types in the entire codebase.

Works with vanilla no build js and the built in ts lsp in vs code intellisense.
With this tool you can have your types defined in your C# code, use DTO + json + http to send data directly to the frontend, annotate the types on the frontend with jsdoc, and get autocomplete for your js code.

## Features

- **Automatic Generation**: TypeScript definitions are generated automatically on project build
- **Seamless Integration**: Works with MSBuild and requires minimal configuration
- **Comprehensive Type Mapping**: Handles C# classes, interfaces, enums, and properties
- **Documentation Support**: Preserves C# XML documentation as JSDoc comments
- **Nullable Support**: Preserves nullability information from C#

## Installation

```bash
# Install as a .NET tool and package reference in one command
dotnet add package CsTsTypeGen
```

## Quick Start

After installing the package, TypeScript definitions will be generated automatically when you build your project. The default output file is `typedefs.d.ts` in your project directory.

To use the generated TypeScript definitions in your JavaScript/TypeScript files, add the reference at the top, with your correct path, and add a type annotation with your type:
```javascript
/// <reference path="../../typedefs.d.ts" />
// @ts-check

/** @type {Ui.Components.MetricsData} */
let chartData = null;
```

## Configuration

You can customize the behavior by adding the following properties to your `.csproj` file:

```xml
<PropertyGroup>
  <!-- Set to false to disable TypeScript generation -->
  <CsTsTypeGen_GenerateDefinitions>true</CsTsTypeGen_GenerateDefinitions>
  <!-- Output path for TypeScript definitions -->
  <CsTsTypeGen_DefinitionsPath>$(MSBuildProjectDirectory)/typedefs.d.ts</CsTsTypeGen_DefinitionsPath>
  <!-- Source directory for C# files (defaults to solution directory) -->
  <CsTsTypeGen_SourceDirectory>$(MSBuildProjectDirectory)/..</CsTsTypeGen_SourceDirectory>
</PropertyGroup>
```

These properties are automatically used by the MSBuild targets that are imported when you reference the package, and are passed to the tool as environment variables with the `CsTsTypeGen_` prefix.

## Manual Usage

You can also run the tool manually:

```bash
# Using command line arguments
dotnet tool run cstsgen -- <source-directory> <output-file>

# Or using environment variables
CsTsTypeGen_SourceDirectory=<source-directory> CsTsTypeGen_DefinitionsPath=<output-file> dotnet tool run cstsgen
```

## Type Mapping

CsTsTypeGen maps C# types to TypeScript types as follows:

| C# Type | TypeScript Type |
|---------|----------------|
| `string` | `string` |
| `int`, `long`, `decimal`, `float`, `double` | `number` |
| `bool` | `boolean` |
| `DateTime`, `DateTimeOffset` | `string` |
| `Guid` | `string` |
| `ICollection<T>`, `List<T>`, `IList<T>` | `T[]` |
| `DbSet<T>` | `DbSet<T>` |
| `enum` | Union type + enum |
| `class`, `interface` | `interface` |
| `Nullable<T>` or `T?` | Optional property |

## Develop
```sh
dotnet build -c Release
dotnet pack -c Release
```

## License
This project is dual-licensed:

- Community Edition: GNU AGPLv3 (see [LICENSE](./LICENSE))
- Commercial License: Available for proprietary SaaS and embedded use

If you would like to use this software in a proprietary, closed-source SaaS or on-prem product, contact kruserr for commercial licensing options.

By contributing, you agree to our [Contributor License Agreement](./CLA.md).
