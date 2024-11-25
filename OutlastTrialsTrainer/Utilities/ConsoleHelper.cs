using System.Collections.Frozen;
using System.Reflection;
using ReadWriteMemory.Interfaces;
using Spectre.Console;

namespace OutlastTrialsTrainer.Utilities;

public static class ConsoleHelper
{
    private const string ImportantNote = "[darkorange3_1][b]Important note:[/][/] Be aware that using this mod " +
                                         "[red][b][u]can crash[/][/][/] your game and/or cause other weird behavior. " +
                                         "Every update for the game can make the trainer useless. So check for " +
                                         "updates on the github site. Perhaps I will add a auto updater in the " +
                                         "future, when I'm not to lazy to do that ;). Also, make sure you " +
                                         "[red][b][u]do not[/][/][/] close the mod when freecam is enabled and " +
                                         "the game is still running.";
    
    public static void InitConsole()
    {
        AnsiConsole.Write(new FigletText("Tot Freecam Mod")
            .Centered()
            .Color(Color.DarkGreen));

        var rule = new Rule();
        rule.RuleStyle(Style.Parse("green"));

        AnsiConsole.Write(rule);

        var version = Assembly.GetExecutingAssembly()
            .GetName().Version?
            .ToString();

        version = !string.IsNullOrEmpty(version) ? version.Remove(version.Length - 2) : "v1.0.0";

        var titleText = new Text($"v{version} | Made for steam version",
                new Style(Color.Yellow2, Color.Black, Decoration.Bold))
            .Centered();

        AnsiConsole.Write(titleText);

        var titleText2 = new Text("By Daiserdyne",
                new Style(Color.Blue, Color.Black, Decoration.Bold,
                    "https://github.com/Daiserdyne/OutlastTrialsTrainer")
            )
            .Centered();
        
        AnsiConsole.Write(titleText2);
        
        var titleText3 = new Text("YouTube",
                new Style(Color.Red3, Color.Black, Decoration.Bold,
                    "https://www.youtube.com/@Daiserdyne")
            )
            .Centered();

        AnsiConsole.Write(titleText3);

        AnsiConsole.Write("\n\n\n");

        AnsiConsole.MarkupLine(ImportantNote);

        Console.WriteLine();
    }

    public static Table GetPreconfiguredTable(Table table, FrozenDictionary<string, IMemoryTrainer> trainerList)
    {
        table.Title("\n");

        table.BorderColor(Color.LightCyan1);
        
        table.AddColumns("[blue]Trainer[/]", "[orange1]Hotkey[/]", "[darkorange3]Status[/]", "[grey]Description[/]")
            .Expand();
        
        foreach (var trainer in trainerList.Values)
        {
            if (trainer.DisableWhenDispose)
            {
                table.AddRow(trainer.TrainerName, $"F{trainer.Id}", "[red]off[/]", trainer.Description);
                continue;
            }

            table.AddRow(trainer.TrainerName, $"F{trainer.Id}", "-", trainer.Description);
        }
        
        return table;
    }
}