using System;
using System.Collections.Generic;
using DotNetHotspots.Models;

namespace DotNetHotspots.Services;

public static class OutputService
{
    public static void ShowHelp()
    {
        Console.WriteLine("dotnet-hotspots  - Find the most frequently changed files in your repository");
        Console.WriteLine();
        Console.WriteLine("Usage: dotnet-hotspots [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --<number>              Show top files, e.g. --50 (default: 30)");
        Console.WriteLine("  --all                   Show all files including docs, configs and build artifacts");
        Console.WriteLine("  -v, --version           Show version information");
        Console.WriteLine("  -h, --help              Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet-hotspots                    # Show top 30 code files");
        Console.WriteLine("  dotnet-hotspots --50               # Show top 50 code files");
        Console.WriteLine("  dotnet-hotspots --all              # Show all files, no filtering");
    }

    public static void DisplayResults(List<FileChangeStat> fileStats, int count, int totalFilesInRepo, bool showAll)
    {
        var displayed = Math.Min(count, fileStats.Count);
        var title = showAll
            ? $"Top {displayed} Hot Files — All Files"
            : $"Top {displayed} Hot Files — Code Files Only";

        Console.WriteLine(title);
        Console.WriteLine("".PadRight(80, '='));
        Console.WriteLine($"{"Rank", -6} {"Changes", -8} {"File Path", -50}");
        Console.WriteLine("".PadRight(80, '-'));

        for (int i = 0; i < displayed; i++)
        {
            var stat = fileStats[i];
            var rank = (i + 1).ToString();
            var changes = stat.ChangeCount.ToString();
            var filePath =
                stat.FilePath.Length > 50
                    ? "..." + new string(stat.FilePath.AsSpan(stat.FilePath.Length - 47))
                    : stat.FilePath;

            Console.WriteLine($"{rank, -6} {changes, -8} {filePath}");
        }

        Console.WriteLine("".PadRight(80, '='));

        if (showAll)
        {
            Console.WriteLine($"Total files analyzed: {totalFilesInRepo}");
        }
        else
        {
            Console.WriteLine($"Code files found: {fileStats.Count}  |  Total files in repo history: {totalFilesInRepo}  |  Use --all to see everything");
        }
    }
}
