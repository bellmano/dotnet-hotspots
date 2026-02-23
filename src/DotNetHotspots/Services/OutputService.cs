using System;
using System.Collections.Generic;
using System.Linq;
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

        const int rankWidth = 6;
        const int changesWidth = 8;
        const int minPathWidth = 9; // "File Path".Length
        var pathColumnWidth = Math.Max(
            minPathWidth,
            fileStats.Count > 0 ? fileStats.Take(displayed).Max(f => f.FilePath.Length) : minPathWidth
        );
        var totalWidth = rankWidth + 1 + changesWidth + 1 + pathColumnWidth;

        Console.WriteLine(title);
        Console.WriteLine("".PadRight(totalWidth, '='));
        Console.WriteLine($"{"Rank", -6} {"Changes", -8} {"File Path"}");
        Console.WriteLine("".PadRight(totalWidth, '-'));

        for (int i = 0; i < displayed; i++)
        {
            var stat = fileStats[i];
            var rank = (i + 1).ToString();
            var changes = stat.ChangeCount.ToString();

            Console.WriteLine($"{rank, -6} {changes, -8} {stat.FilePath}");
        }

        Console.WriteLine("".PadRight(totalWidth, '='));

        if (showAll)
        {
            Console.WriteLine($"Total files analyzed: {totalFilesInRepo}");
        }
        else
        {
            Console.WriteLine($"Code files found: {fileStats.Count}  |  Total files in repo: {totalFilesInRepo}  |  Use --all to see everything");
        }
    }
}
