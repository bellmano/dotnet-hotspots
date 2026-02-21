using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using DotNetHotspots.Models;
using DotNetHotspots.Services;

namespace DotNetHotspots;

static class Program
{
    [ExcludeFromCodeCoverage]
    static Task<int> Main(string[] args) =>
        RunAsync(args, GitService.IsGitRepositoryAsync, GitService.GetFileChangeStatsAsync);

    internal static async Task<int> RunAsync(
        string[] args,
        Func<Task<bool>> isGitRepo,
        Func<Task<List<FileChangeStat>>> getStats
    )
    {
        try
        {
            var options = ArgumentParser.ParseArguments(args);

            if (options.ShowHelp)
            {
                OutputService.ShowHelp();
                return 0;
            }

            if (options.ShowVersion)
            {
                Console.WriteLine($"dotnet-hotspots v{GetVersion()}");
                return 0;
            }

            if (!await isGitRepo())
            {
                Console.WriteLine("Error: This directory is not a git repository.");
                Console.WriteLine("Please run this command from within a git repository.");
                return 1;
            }

            var allFileStats = await getStats();

            if (allFileStats.Count == 0)
            {
                Console.WriteLine("No files found with changes in this repository.");
                return 0;
            }

            var fileStats = options.ShowAll
                ? allFileStats
                : GitService.FilterCodeFiles(allFileStats);

            OutputService.DisplayResults(
                fileStats,
                options.Count,
                allFileStats.Count,
                options.ShowAll
            );

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    [ExcludeFromCodeCoverage(
        Justification = "Assembly version resolution depends on entry-assembly attributes — cannot be controlled from unit tests."
    )]
    private static string GetVersion()
    {
        var version =
            Assembly
                .GetEntryAssembly()
                ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion
            ?? "1.0.0";

        var plusIndex = version.IndexOf('+');
        return plusIndex >= 0 ? version[..plusIndex] : version;
    }
}
