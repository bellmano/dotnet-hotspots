using DotNetHotspots.Services;
using Xunit;

namespace DotNetHotspots.Tests.Unit;

public class ArgumentParserTests
{
    [Fact]
    public void NoArgs_Returns_DefaultOptions()
    {
        var options = ArgumentParser.ParseArguments([]);

        Assert.False(options.ShowHelp);
        Assert.False(options.ShowVersion);
        Assert.False(options.ShowAll);
        Assert.Equal(30, options.Count);
    }

    [Theory]
    [InlineData("-h")]
    [InlineData("--help")]
    [InlineData("--HELP")]
    public void HelpFlag_Sets_ShowHelp(string arg)
    {
        var options = ArgumentParser.ParseArguments([arg]);

        Assert.True(options.ShowHelp);
    }

    [Theory]
    [InlineData("-v")]
    [InlineData("--version")]
    [InlineData("--VERSION")]
    public void VersionFlag_Sets_ShowVersion(string arg)
    {
        var options = ArgumentParser.ParseArguments([arg]);

        Assert.True(options.ShowVersion);
    }

    [Theory]
    [InlineData("--all")]
    [InlineData("--ALL")]
    public void AllFlag_Sets_ShowAll(string arg)
    {
        var options = ArgumentParser.ParseArguments([arg]);

        Assert.True(options.ShowAll);
    }

    [Theory]
    [InlineData("--10", 10)]
    [InlineData("--50", 50)]
    public void ShorthandCount_Sets_Count(string arg, int expected)
    {
        var options = ArgumentParser.ParseArguments([arg]);

        Assert.Equal(expected, options.Count);
    }

    [Theory]
    [InlineData("--0")]
    [InlineData("--abc")]
    public void InvalidShorthandCount_Keeps_DefaultCount(string arg)
    {
        var options = ArgumentParser.ParseArguments([arg]);

        Assert.Equal(30, options.Count);
    }

    [Fact]
    public void MultipleFlags_AreAllParsed()
    {
        var options = ArgumentParser.ParseArguments(["--all", "--50"]);

        Assert.True(options.ShowAll);
        Assert.Equal(50, options.Count);
    }

    [Fact]
    public void UnknownArgs_AreIgnored()
    {
        var options = ArgumentParser.ParseArguments(["--unknown", "something"]);

        Assert.False(options.ShowHelp);
        Assert.False(options.ShowAll);
        Assert.Equal(30, options.Count);
    }
}
