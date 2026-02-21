using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetHotspots.Models;

namespace DotNetHotspots.Services;

public static class GitService
{
    public static Task<bool> IsGitRepositoryAsync() => IsGitRepositoryAsync(RunGitCommandAsync);

    internal static async Task<bool> IsGitRepositoryAsync(
        Func<string, Task<(int ExitCode, string Output, string Error)>> runGitCommand
    )
    {
        try
        {
            var result = await runGitCommand("rev-parse --git-dir");
            return result.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public static Task<List<FileChangeStat>> GetFileChangeStatsAsync() =>
        GetFileChangeStatsAsync(RunGitCommandAsync);

    internal static async Task<List<FileChangeStat>> GetFileChangeStatsAsync(
        Func<string, Task<(int ExitCode, string Output, string Error)>> runGitCommand
    )
    {
        var gitArgs = "log --name-only --pretty=format:";

        var result = await runGitCommand(gitArgs);

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException($"Failed to get git log: {result.Error}");
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
        // Docs / text
        ".md",
        ".txt",
        ".lock",
        ".sum",
        ".log",
        // Config / data
        ".json",
        ".xml",
        ".yaml",
        ".yml",
        ".toml",
        ".ini",
        ".cfg",
        ".config",
        // Images
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".bmp",
        ".svg",
        ".ico",
        ".webp",
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
                if (fileChanges.TryGetValue(trimmedLine, out var count))
                {
                    fileChanges[trimmedLine] = count + 1;
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

    private static string? _gitExecutablePath;

    [ExcludeFromCodeCoverage(
        Justification = "OS-level git PATH resolution with a static cache — cannot be unit-tested without removing git from PATH."
    )]
    private static string FindGitExecutable()
    {
        if (_gitExecutablePath is not null)
            return _gitExecutablePath;

        var gitName = OperatingSystem.IsWindows() ? "git.exe" : "git";
        var pathVar = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;

        foreach (
            var dir in pathVar.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
        )
        {
            try
            {
                var candidate = Path.GetFullPath(Path.Combine(dir, gitName));
                if (File.Exists(candidate))
                {
                    _gitExecutablePath = candidate;
                    return candidate;
                }
            }
            catch (Exception)
            {
                // Skip invalid PATH entries
            }
        }

        _gitExecutablePath = gitName; // Fallback — let the OS resolve it
        return _gitExecutablePath;
    }

    internal static async Task<(int ExitCode, string Output, string Error)> RunGitCommandAsync(
        string arguments
    )
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = FindGitExecutable(),
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
