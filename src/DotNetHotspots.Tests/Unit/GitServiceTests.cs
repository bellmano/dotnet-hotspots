using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetHotspots.Models;
using DotNetHotspots.Services;
using Xunit;

namespace DotNetHotspots.Tests.Unit;

public class GitServiceTests
{
    [Theory]
    [InlineData("src/Services/UserService.cs")]
    [InlineData("Program.cs")]
    [InlineData("src/app.js")]
    [InlineData("src\\Services\\UserService.cs")]
    public void IsCodeFile_Returns_True_ForCodeFiles(string path)
    {
        Assert.True(GitService.IsCodeFile(path));
    }

    [Theory]
    // Excluded directories (all variants)
    [InlineData("bin/Debug/MyApp.dll")]
    [InlineData("obj/Debug/net8.0/MyApp.dll")]
    [InlineData("node_modules/lodash/index.js")]
    [InlineData(".vs/config/applicationhost.config")]
    [InlineData(".idea/project.xml")]
    [InlineData("dist/bundle.js")]
    [InlineData("build/output.o")]
    [InlineData("publish/app.dll")]
    [InlineData("packages/library.nupkg")]
    [InlineData(".nuget/nuget.config")]
    [InlineData(".vscode/settings.json")]
    [InlineData(".git/config")]
    [InlineData("bin\\Debug\\MyApp.dll")]
    [InlineData("src/bin/nested/file.cs")]
    [InlineData("src/obj/nested/file.cs")]
    [InlineData("README.md")]
    [InlineData("src/NOTES.txt")]
    [InlineData("logs/output.log")]
    [InlineData("Gemfile.lock")]
    [InlineData("go.sum")]
    [InlineData("appsettings.json")]
    [InlineData("src/config/settings.xml")]
    [InlineData(".github/workflows/ci.yml")]
    [InlineData("docker-compose.yaml")]
    [InlineData("pyproject.toml")]
    [InlineData("setup.ini")]
    [InlineData("app.cfg")]
    [InlineData("web.config")]
    [InlineData("img/logo.jpg")]
    [InlineData("assets/banner.jpeg")]
    [InlineData("docs/screenshot.png")]
    [InlineData("src/icon.ico")]
    [InlineData("logo.svg")]
    [InlineData("img/photo.gif")]
    [InlineData("img/photo.bmp")]
    [InlineData("img/photo.webp")]
    [InlineData(".gitignore")]
    [InlineData(".gitattributes")]
    [InlineData(".editorconfig")]
    [InlineData(".csharpierignore")]
    [InlineData(".dockerignore")]
    [InlineData(".env")]
    [InlineData("LICENSE")]
    [InlineData("Makefile")]
    [InlineData("Dockerfile")]
    [InlineData("Dockerfile.dev")]
    [InlineData("Dockerfile.prod")]
    [InlineData("src/Dockerfile")]
    [InlineData("docker/Dockerfile.ci")]
    public void IsCodeFile_Returns_False_ForExcludedPaths(string path)
    {
        Assert.False(GitService.IsCodeFile(path));
    }

    [Fact]
    public void FilterCodeFiles_RemovesNonCodeFiles_AndPreservesOrder()
    {
        var input = new List<FileChangeStat>
        {
            new() { FilePath = "src/Services/UserService.cs", ChangeCount = 100 },
            new() { FilePath = "README.md", ChangeCount = 10 },
            new() { FilePath = "bin/Debug/App.dll", ChangeCount = 5 },
            new() { FilePath = ".gitignore", ChangeCount = 3 },
            new() { FilePath = "src/Models/Order.cs", ChangeCount = 20 },
        };

        var result = GitService.FilterCodeFiles(input);

        Assert.Equal(2, result.Count);
        Assert.Equal("src/Services/UserService.cs", result[0].FilePath);
        Assert.Equal("src/Models/Order.cs", result[1].FilePath);
    }

    [Fact]
    public void FilterCodeFiles_EmptyInput_ReturnsEmpty()
    {
        Assert.Empty(GitService.FilterCodeFiles([]));
    }

    [Fact]
    public void ParseGitLogOutput_CountsOccurrencesAndSortsDescending()
    {
        var gitOutput = """
            src/Services/UserService.cs
            src/Models/User.cs

            src/Services/UserService.cs
            src/Controllers/AuthController.cs

            src/Services/UserService.cs
            src/Models/User.cs
            """;

        var result = GitService.ParseGitLogOutput(gitOutput);

        Assert.Equal("src/Services/UserService.cs", result[0].FilePath);
        Assert.Equal(3, result[0].ChangeCount);
        Assert.Equal("src/Models/User.cs", result[1].FilePath);
        Assert.Equal(2, result[1].ChangeCount);
        Assert.Equal("src/Controllers/AuthController.cs", result[2].FilePath);
        Assert.Equal(1, result[2].ChangeCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("\n\n\n")]
    [InlineData("   \n   \n")] // lines that trim to empty string
    public void ParseGitLogOutput_EmptyOrBlankInput_ReturnsEmpty(string input)
    {
        Assert.Empty(GitService.ParseGitLogOutput(input));
    }

    [Fact]
    public void ParseGitLogOutput_SingleFile_ReturnsOneEntry()
    {
        var result = GitService.ParseGitLogOutput("src/Program.cs\n");

        Assert.Single(result);
        Assert.Equal("src/Program.cs", result[0].FilePath);
        Assert.Equal(1, result[0].ChangeCount);
    }

    [Fact]
    public void ParseGitLogOutput_IgnoresCommitLines()
    {
        var result = GitService.ParseGitLogOutput(
            "commit abc123def456\nsrc/Services/UserService.cs\n"
        );

        Assert.Single(result);
        Assert.Equal("src/Services/UserService.cs", result[0].FilePath);
    }

    [Fact]
    public void ParseGitLogOutput_TrimsWhitespaceAndDeduplicates()
    {
        var result = GitService.ParseGitLogOutput(
            "  src/Services/UserService.cs  \n  src/Services/UserService.cs  \n"
        );

        Assert.Single(result);
        Assert.Equal("src/Services/UserService.cs", result[0].FilePath);
        Assert.Equal(2, result[0].ChangeCount);
    }

    [Fact]
    public async Task GetCurrentFilePathsAsync_ReturnsCurrentFiles()
    {
        var fakeOutput = "src/Services/UserService.cs\nsrc/Program.cs\n";
        var result = await GitService.GetCurrentFilePathsAsync(_ =>
            Task.FromResult((0, fakeOutput, string.Empty))
        );

        Assert.Equal(2, result.Count);
        Assert.Contains("src/Services/UserService.cs", result);
        Assert.Contains("src/Program.cs", result);
    }

    [Fact]
    public async Task GetCurrentFilePathsAsync_GitFails_ReturnsEmpty()
    {
        var result = await GitService.GetCurrentFilePathsAsync(_ =>
            Task.FromResult((1, string.Empty, "fatal: not a git repository"))
        );

        Assert.Empty(result);
    }

    [Fact]
    public async Task IsGitRepositoryAsync_InsideGitRepo_ReturnsTrue()
    {
        var result = await GitService.IsGitRepositoryAsync();
        Assert.True(result);
    }

    [Fact]
    public async Task IsGitRepositoryAsync_NonZeroExitCode_ReturnsFalse()
    {
        var result = await GitService.IsGitRepositoryAsync(_ =>
            Task.FromResult((1, string.Empty, "fatal: not a git repository"))
        );
        Assert.False(result);
    }

    [Fact]
    public async Task IsGitRepositoryAsync_RunnerThrows_ReturnsFalse()
    {
        var result = await GitService.IsGitRepositoryAsync(_ =>
            throw new InvalidOperationException("git not found")
        );
        Assert.False(result);
    }

    [Fact]
    public async Task GetFileChangeStatsAsync_ReturnsListSortedDescending()
    {
        var result = await GitService.GetFileChangeStatsAsync();
        Assert.NotNull(result);
        for (int i = 0; i < result.Count - 1; i++)
            Assert.True(result[i].ChangeCount >= result[i + 1].ChangeCount);
    }

    [Fact]
    public async Task GetCurrentFilePathsAsync_InsideGitRepo_ReturnsHashSet()
    {
        var result = await GitService.GetCurrentFilePathsAsync();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetFileChangeStatsAsync_GitLogFails_ThrowsInvalidOperationException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            GitService.GetFileChangeStatsAsync(_ =>
                Task.FromResult((1, string.Empty, "fatal: not a git repository"))
            )
        );
    }

    [Fact]
    public async Task RunGitCommandAsync_ValidCommand_ReturnsZeroExitCode()
    {
        var result = await GitService.RunGitCommandAsync("--version");
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("git", result.Output, StringComparison.OrdinalIgnoreCase);
    }
}
