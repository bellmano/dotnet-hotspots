using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DotNetHotspots.Models;
using DotNetHotspots.Services;

namespace DotNetHotspots;

static class Program
{
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
                var version =
                    Assembly
                        .GetEntryAssembly()
                        ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                        ?.InformationalVersion ?? "1.0.0";
                Console.WriteLine($"dotnet-hotspots v{version}");
                return 0;
            }

            if (!await isGitRepo())
            {
                Console.WriteLine("Error: This directory is not a git repository.");
                Console.WriteLine("Please run this command from within a git repository.");
                return 1;
            }

            var allFileStats = await getStats();

            if (!allFileStats.Any())
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
}
