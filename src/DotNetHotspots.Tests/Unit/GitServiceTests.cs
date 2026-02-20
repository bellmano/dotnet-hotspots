using System.Collections.Generic;
using System.Linq;
using DotNetHotspots.Models;
using DotNetHotspots.Services;
using Xunit;

namespace DotNetHotspots.Tests.Unit;

public class GitServiceTests
{
    // -------------------------------------------------------------------------
    // IsCodeFile — include
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("src/Services/UserService.cs")]
    [InlineData("Program.cs")]
    [InlineData("src/app.js")]
    [InlineData("Dockerfile")]
    public void IsCodeFile_Returns_True_ForCodeFiles(string path)
    {
        Assert.True(GitService.IsCodeFile(path));
    }

    // -------------------------------------------------------------------------
    // IsCodeFile — exclude
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("bin/Debug/MyApp.dll")]
    [InlineData("obj/Debug/net8.0/MyApp.dll")]
    [InlineData("node_modules/lodash/index.js")]
    [InlineData(".vs/config/applicationhost.config")]
    public void IsCodeFile_Returns_False_ForExcludedDirectories(string path)
    {
        Assert.False(GitService.IsCodeFile(path));
    }

    [Theory]
    [InlineData("README.md")]
    [InlineData("src/NOTES.txt")]
    [InlineData("logs/output.log")]
    public void IsCodeFile_Returns_False_ForExcludedExtensions(string path)
    {
        Assert.False(GitService.IsCodeFile(path));
    }

    [Theory]
    [InlineData(".gitignore")]
    [InlineData("LICENSE")]
    [InlineData("Makefile")]
    public void IsCodeFile_Returns_False_ForExcludedFileNames(string path)
    {
        Assert.False(GitService.IsCodeFile(path));
    }

    [Fact]
    public void IsCodeFile_HandlesWindowsBackslashPaths_Include()
    {
        Assert.True(GitService.IsCodeFile("src\\Services\\UserService.cs"));
    }

    [Fact]
    public void IsCodeFile_HandlesWindowsBackslashPaths_Exclude()
    {
        Assert.False(GitService.IsCodeFile("bin\\Debug\\MyApp.dll"));
    }

    // -------------------------------------------------------------------------
    // FilterCodeFiles
    // -------------------------------------------------------------------------

    [Fact]
    public void FilterCodeFiles_RemovesNonCodeFiles()
    {
        var input = new List<FileChangeStat>
        {
            new() { FilePath = "src/Services/UserService.cs", ChangeCount = 50 },
            new() { FilePath = "README.md", ChangeCount = 10 },
            new() { FilePath = "bin/Debug/App.dll", ChangeCount = 5 },
            new() { FilePath = ".gitignore", ChangeCount = 3 },
            new() { FilePath = "src/Models/Order.cs", ChangeCount = 20 },
        };

        var result = GitService.FilterCodeFiles(input);

        Assert.Equal(2, result.Count);
        Assert.All(result, f => Assert.EndsWith(".cs", f.FilePath));
    }

    [Fact]
    public void FilterCodeFiles_PreservesOrder()
    {
        var input = new List<FileChangeStat>
        {
            new() { FilePath = "src/Services/UserService.cs", ChangeCount = 100 },
            new() { FilePath = "src/Models/Order.cs", ChangeCount = 50 },
            new() { FilePath = "src/Controllers/AuthController.cs", ChangeCount = 25 },
        };

        var result = GitService.FilterCodeFiles(input);

        Assert.Equal(100, result[0].ChangeCount);
        Assert.Equal(50, result[1].ChangeCount);
        Assert.Equal(25, result[2].ChangeCount);
    }

    [Fact]
    public void FilterCodeFiles_EmptyInput_ReturnsEmpty()
    {
        Assert.Empty(GitService.FilterCodeFiles([]));
    }

    // -------------------------------------------------------------------------
    // ParseGitLogOutput
    // -------------------------------------------------------------------------

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

    [Fact]
    public void ParseGitLogOutput_EmptyInput_ReturnsEmpty()
    {
        Assert.Empty(GitService.ParseGitLogOutput(string.Empty));
    }

    [Fact]
    public void ParseGitLogOutput_BlankLinesAreIgnored()
    {
        Assert.Empty(GitService.ParseGitLogOutput("\n\n\n"));
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
    public void ParseGitLogOutput_TrimsWhitespace()
    {
        var result = GitService.ParseGitLogOutput(
            "  src/Services/UserService.cs  \n  src/Services/UserService.cs  \n"
        );

        Assert.Single(result);
        Assert.Equal("src/Services/UserService.cs", result[0].FilePath);
        Assert.Equal(2, result[0].ChangeCount);
    }
}
