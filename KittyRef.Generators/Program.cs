using System;
using System.Text;
using JetBrains.Annotations;
using KittyRef.Generators.Commands;
using McMaster.Extensions.CommandLineUtils;
using osu.Framework.Logging;

namespace KittyRef.Generators;

[Command(Name = "kitty-generators", Description = "Generate things")]
[Subcommand(typeof(ModsCommand))]
public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Logger.Enabled = false;

        CommandLineApplication.Execute<Program>(args);
    }

    [UsedImplicitly]
    public int OnExecute(CommandLineApplication app, IConsole console)
    {
        console.WriteLine("You must specify a subcommand.");
        app.ShowHelp();
        return 1;
    }
}
