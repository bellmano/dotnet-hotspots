using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetHotspots.Models;

namespace DotNetHotspots.Services;

public static class GitService
{
    public static async Task<bool> IsGitRepositoryAsync()
    {
        try
        {
            var result = await RunGitCommandAsync("rev-parse --git-dir");
            return result.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<List<FileChangeStat>> GetFileChangeStatsAsync()
    {
        var gitArgs = "log --name-only --pretty=format:";

        var result = await RunGitCommandAsync(gitArgs);

        if (result.ExitCode != 0)
        {
            throw new Exception($"Failed to get git log: {result.Error}");
        }

        return ParseGitLogOutput(result.Output);
    }

    private static readonly HashSet<string> ExcludedDirectories = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "bin",
        "obj",
        ".vs",
        ".idea",
        "node_modules",
        ".git",
        "dist",
        "build",
        "publish",
        "packages",
        ".nuget",
        ".vscode",
    };

    private static readonly HashSet<string> ExcludedExtensions = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        ".md",
        ".txt",
        ".lock",
        ".sum",
        ".log",
    };

    private static readonly HashSet<string> ExcludedFileNames = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        ".gitignore",
        ".gitattributes",
        ".editorconfig",
        ".csharpierignore",
        ".dockerignore",
        ".env",
        "Makefile",
        "LICENSE",
    };

    public static List<FileChangeStat> FilterCodeFiles(List<FileChangeStat> files)
    {
        return files.Where(f => IsCodeFile(f.FilePath)).ToList();
    }

    internal static bool IsCodeFile(string filePath)
    {
        var normalized = filePath.Replace('\\', '/');
        var segments = normalized.Split('/');

        // Check if any parent directory segment is excluded
        for (int i = 0; i < segments.Length - 1; i++)
        {
            if (ExcludedDirectories.Contains(segments[i]))
                return false;
        }

        var fileName = segments[^1];

        // Check excluded file names (e.g. .gitignore, LICENSE)
        if (ExcludedFileNames.Contains(fileName))
            return false;

        // Check excluded extensions
        var ext = Path.GetExtension(fileName);
        if (!string.IsNullOrEmpty(ext) && ExcludedExtensions.Contains(ext))
            return false;

        return true;
    }

    internal static List<FileChangeStat> ParseGitLogOutput(string gitOutput)
    {
        var fileChanges = new Dictionary<string, int>();
        var lines = gitOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (!string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith("commit"))
            {
                if (fileChanges.ContainsKey(trimmedLine))
                {
                    fileChanges[trimmedLine]++;
                }
                else
                {
                    fileChanges[trimmedLine] = 1;
                }
            }
        }

        return fileChanges
            .Select(kvp => new FileChangeStat { FilePath = kvp.Key, ChangeCount = kvp.Value })
            .OrderByDescending(stat => stat.ChangeCount)
            .ToList();
    }

    private static async Task<(int ExitCode, string Output, string Error)> RunGitCommandAsync(
        string arguments
    )
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        return (process.ExitCode, output, error);
    }
}
