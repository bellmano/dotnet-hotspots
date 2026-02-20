using System;
using System.Collections.Generic;
using System.IO;
using DotNetHotspots.Models;
using DotNetHotspots.Services;
using Xunit;

namespace DotNetHotspots.Tests.Unit;

public class OutputServiceTests
{
    private static string Capture(Action action)
    {
        var original = Console.Out;
        using var sw = new StringWriter();
        Console.SetOut(sw);
        try
        {
            action();
            return sw.ToString();
        }
        finally
        {
            Console.SetOut(original);
        }
    }

    // -------------------------------------------------------------------------
    // ShowHelp
    // -------------------------------------------------------------------------

    [Fact]
    public void ShowHelp_ContainsToolName()
    {
        var output = Capture(OutputService.ShowHelp);

        Assert.Contains("dotnet-hotspots", output);
    }

    [Fact]
    public void ShowHelp_ContainsAllFlags()
    {
        var output = Capture(OutputService.ShowHelp);

        Assert.Contains("--all", output);
        Assert.Contains("--version", output);
        Assert.Contains("--help", output);
    }

    // -------------------------------------------------------------------------
    // DisplayResults — title
    // -------------------------------------------------------------------------

    [Fact]
    public void DisplayResults_ShowAll_UseAllFilesTitle()
    {
        var stats = new List<FileChangeStat>
        {
            new() { FilePath = "src/Services/UserService.cs", ChangeCount = 10 },
        };

        var output = Capture(() => OutputService.DisplayResults(stats, 30, 100, showAll: true));

        Assert.Contains("All Files", output);
    }

    [Fact]
    public void DisplayResults_DefaultMode_UsesCodeFilesOnlyTitle()
    {
        var stats = new List<FileChangeStat>
        {
            new() { FilePath = "src/Services/UserService.cs", ChangeCount = 10 },
        };

        var output = Capture(() => OutputService.DisplayResults(stats, 30, 100, showAll: false));

        Assert.Contains("Code Files Only", output);
    }

    // -------------------------------------------------------------------------
    // DisplayResults — footer
    // -------------------------------------------------------------------------

    [Fact]
    public void DisplayResults_ShowAll_FooterShowsTotalFilesAnalyzed()
    {
        var stats = new List<FileChangeStat>
        {
            new() { FilePath = "src/Services/UserService.cs", ChangeCount = 10 },
        };

        var output = Capture(() => OutputService.DisplayResults(stats, 30, 247, showAll: true));

        Assert.Contains("Total files analyzed: 247", output);
    }

    [Fact]
    public void DisplayResults_DefaultMode_FooterShowsCodeFilesFound()
    {
        var stats = new List<FileChangeStat>
        {
            new() { FilePath = "src/Services/UserService.cs", ChangeCount = 10 },
        };

        var output = Capture(() => OutputService.DisplayResults(stats, 30, 247, showAll: false));

        Assert.Contains("Code files found:", output);
        Assert.Contains("--all", output);
    }

    // -------------------------------------------------------------------------
    // DisplayResults — count and truncation
    // -------------------------------------------------------------------------

    [Fact]
    public void DisplayResults_RespectsCountLimit()
    {
        var stats = new List<FileChangeStat>
        {
            new() { FilePath = "src/A.cs", ChangeCount = 30 },
            new() { FilePath = "src/B.cs", ChangeCount = 20 },
            new() { FilePath = "src/C.cs", ChangeCount = 10 },
        };

        var output = Capture(() => OutputService.DisplayResults(stats, 2, 3, showAll: false));

        Assert.Contains("src/A.cs", output);
        Assert.Contains("src/B.cs", output);
        Assert.DoesNotContain("src/C.cs", output);
    }

    [Fact]
    public void DisplayResults_TruncatesLongFilePaths()
    {
        var longPath = "src/" + new string('a', 60) + ".cs";
        var stats = new List<FileChangeStat>
        {
            new() { FilePath = longPath, ChangeCount = 5 },
        };

        var output = Capture(() => OutputService.DisplayResults(stats, 30, 1, showAll: false));

        Assert.Contains("...", output);
    }
}
