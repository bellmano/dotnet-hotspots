using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotNetHotspots.Models;
using Xunit;

namespace DotNetHotspots.Tests.Unit;

public class ProgramTests
{
    private static Restore SuppressConsole()
    {
        var original = Console.Out;
        Console.SetOut(TextWriter.Null);
        return new Restore(() => Console.SetOut(original));
    }

    private sealed class Restore : IDisposable
    {
        private readonly Action _restore;

        public Restore(Action restore)
        {
            _restore = restore;
        }

        public void Dispose() => _restore();
    }

    private static Task<int> Run(
        string[] args,
        bool isGitRepo = true,
        List<FileChangeStat>? stats = null
    ) =>
        Program.RunAsync(
            args,
            () => Task.FromResult(isGitRepo),
            () => Task.FromResult(stats ?? [])
        );

    [Fact]
    public async Task RunAsync_HelpFlag_Returns0()
    {
        using var _ = SuppressConsole();
        Assert.Equal(0, await Run(["--help"]));
    }

    [Fact]
    public async Task RunAsync_VersionFlag_Returns0()
    {
        using var _ = SuppressConsole();
        Assert.Equal(0, await Run(["--version"]));
    }

    [Fact]
    public async Task RunAsync_NotAGitRepo_Returns1()
    {
        using var _ = SuppressConsole();
        Assert.Equal(1, await Run([], isGitRepo: false));
    }

    [Fact]
    public async Task RunAsync_NoFilesFound_Returns0()
    {
        using var _ = SuppressConsole();
        Assert.Equal(0, await Run([], stats: []));
    }

    [Fact]
    public async Task RunAsync_WithCodeFiles_Returns0()
    {
        using var _ = SuppressConsole();
        Assert.Equal(
            0,
            await Run(
                [],
                stats: [new() { FilePath = "src/Services/UserService.cs", ChangeCount = 10 }]
            )
        );
    }

    [Fact]
    public async Task RunAsync_AllFlag_SkipsFilter_Returns0()
    {
        using var _ = SuppressConsole();
        Assert.Equal(
            0,
            await Run(
                ["--all"],
                stats:
                [
                    new() { FilePath = "README.md", ChangeCount = 5 },
                    new() { FilePath = "src/Services/UserService.cs", ChangeCount = 10 },
                ]
            )
        );
    }

    [Fact]
    public async Task RunAsync_ExceptionFromGitService_Returns1()
    {
        using var _ = SuppressConsole();
        var result = await Program.RunAsync(
            [],
            () => Task.FromResult(true),
            () => throw new InvalidOperationException("git exploded")
        );

        Assert.Equal(1, result);
    }
}
