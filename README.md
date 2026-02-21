# :fire: dotnet-hotspots

[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=bellmano_dotnet-hotspots&metric=coverage&token=af8ca8be11fe142ca6f11d3bc520c7578e4d66b7)](https://sonarcloud.io/summary/overall?id=bellmano_dotnet-hotspots)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=bellmano_dotnet-hotspots&metric=security_rating&token=af8ca8be11fe142ca6f11d3bc520c7578e4d66b7)](https://sonarcloud.io/summary/overall?id=bellmano_dotnet-hotspots)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=bellmano_dotnet-hotspots&metric=sqale_rating&token=af8ca8be11fe142ca6f11d3bc520c7578e4d66b7)](https://sonarcloud.io/summary/overall?id=bellmano_dotnet-hotspots)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=bellmano_dotnet-hotspots&metric=reliability_rating&token=af8ca8be11fe142ca6f11d3bc520c7578e4d66b7)](https://sonarcloud.io/summary/overall?id=bellmano_dotnet-hotspots)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=bellmano_dotnet-hotspots&metric=bugs&token=af8ca8be11fe142ca6f11d3bc520c7578e4d66b7)](https://sonarcloud.io/summary/overall?id=bellmano_dotnet-hotspots)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=bellmano_dotnet-hotspots&metric=vulnerabilities&token=af8ca8be11fe142ca6f11d3bc520c7578e4d66b7)](https://sonarcloud.io/summary/overall?id=bellmano_dotnet-hotspots)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=bellmano_dotnet-hotspots&metric=code_smells&token=af8ca8be11fe142ca6f11d3bc520c7578e4d66b7)](https://sonarcloud.io/summary/overall?id=bellmano_dotnet-hotspots)

dotnet-hotspots is a .NET global tool that analyzes your Git repository's commit history to identify the most frequently changed files — often a sign of bug-prone areas, architectural bottlenecks, or critical business logic that needs extra attention.

## :camera: Example

```
Top 10 Hot Files — Code Files Only
================================================================================
Rank   Changes  File Path
--------------------------------------------------------------------------------
1      312      src/Services/UserService.cs
2      287      src/Data/ApplicationDbContext.cs
3      201      src/Controllers/AuthController.cs
4      198      src/Services/OrderService.cs
5      176      src/Models/User.cs
6      154      src/Services/PaymentService.cs
7      143      src/Controllers/OrderController.cs
8      121      src/Repositories/UserRepository.cs
9      115      src/Services/EmailService.cs
10     98       src/Models/Order.cs
================================================================================
Code files found: 143  |  Total files in repo history: 381  |  Use --all to see everything
```

## :rocket: Quick Start

### Installation

Install as a .NET global tool:

```bash
dotnet tool install --global dotnet-hotspots-tool
```

### Basic Usage

Navigate to any Git repository and run:

```bash
dotnet-hotspots
```

That's it! You'll see the top 30 most frequently changed files.

## :books: Usage Examples

### Show top 10 hot files
```bash
dotnet-hotspots --10
```

### Show all files (including docs, configs, build artifacts, etc.)
```bash
dotnet-hotspots --all
```

### Get help
```bash
dotnet-hotspots --help
```

### Show version
```bash
dotnet-hotspots --version
```

## :hammer_and_wrench: Requirements

- **.NET 8.0+** (for installation)
- **Git** repository (the tool analyzes Git history)
- Works on **Windows**, **macOS**, and **Linux**

## :package: Installation Options

### Global Tool (Recommended)
```bash
dotnet tool install --global dotnet-hotspots-tool
```

### Update to Latest Version
```bash
dotnet tool update --global dotnet-hotspots-tool
```

### Uninstall
```bash
dotnet tool uninstall --global dotnet-hotspots-tool
```

### Local Installation (Per Project)
```bash
dotnet tool install dotnet-hotspots-tool
```

## :brain: How It Works

dotnet-hotspots analyzes your repository's commit history using `git log` to:

1. **Extract file paths** from all commits
2. **Count occurrences** of each file across commits
3. **Rank files** by frequency of changes
4. **Display results** in a clean, readable format

The tool focuses on **commit frequency** rather than lines changed, giving you insight into which files require the most attention from developers.

## :mag: Smart Filtering

By default, dotnet-hotspots filters out non-code files so the results focus on what matters. Use `--all` to disable filtering and see everything.

**Excluded folders:**
`bin`, `obj`, `dist`, `build`, `publish`, `packages`, `.git`, `.vs`, `.idea`, `.nuget`, `.vscode`, `node_modules`

**Excluded extensions:**
`.md`, `.txt`, `.log`, `.lock`, `.sum`

**Excluded files:**
`.gitignore`, `.gitattributes`, `.editorconfig`, `.csharpierignore`, `.dockerignore`, `.env`, `Makefile`, `LICENSE`

## :handshake: Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## :mega: Issues or Suggestions
Any issues or suggestions, please [create an issue on Github](https://github.com/bellmano/dotnet-hotspots/issues).

## :coffee: Buy me a coffee
Donations are welcome to appreciate my work and to keep this project alive, but isn't required at all.

<a href="https://ko-fi.com/bellmano"><img src="img/bellmano-kofi.jpg" width="50%"></a>