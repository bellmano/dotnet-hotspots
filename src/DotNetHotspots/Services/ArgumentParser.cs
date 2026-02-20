using DotNetHotspots.Models;

namespace DotNetHotspots.Services;

public static class ArgumentParser
{
    public static Options ParseArguments(string[] args)
    {
        var options = new Options();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            var argLower = arg.ToLower();

            if (argLower is "-h" or "--help")
            {
                options.ShowHelp = true;
            }
            else if (argLower is "-v" or "--version")
            {
                options.ShowVersion = true;
            }
            else if (argLower == "--all")
            {
                options.ShowAll = true;
            }
            else if (
                arg.StartsWith("--")
                && int.TryParse(arg[2..], out int shortCount)
                && shortCount > 0
            )
            {
                // Shorthand: --50 sets count to 50
                options.Count = shortCount;
            }
        }

        return options;
    }
}
