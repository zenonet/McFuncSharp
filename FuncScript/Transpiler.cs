using System.Text;
using System.Text.RegularExpressions;
using FuncScript.Internal;
using FuncScript.Types;
using FuncSharp;
using FuncSharp.Commands;
using FuncSharp.DataPackGen;
using SlowLang.Engine;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript;

public static class Transpiler
{
    public static StringBuilder McFunctionBuilder { get; set; } = new();


    public static List<string> prefixes = new();

    public static string CombinedPrefix => string.Join(string.Empty, prefixes);

    public static List<Entrypoint> AdditionalEntrypoints { get; set; } = new();

    public static Config Config { get; private set; }

    public static Dictionary<string, Type> MemoryTypes = new();

    public static DataPackGenerator Generator;

    public static void Transpile(string funcScriptCode, Config config)
    {
        Config = config;

        Generator = new(config.DataPackPath, config.DataPackNameSpace);

        McFunctionBuilder = new();

        TokenList tokens = Lexer.Lex(funcScriptCode);

        switch (config.ReloadBehavior)
        {
            case ReloadBehavior.KillOld:
                // Make sure the entities won't drop any items
                McFunctionBuilder.AppendLine("execute as @e[tag=funcscript_controlled] run data merge entity @s {DeathLootTable:\"minecraft:empty\"}");
                // Kill the entities
                McFunctionBuilder.AppendLine("kill @e[tag=funcscript_controlled]");

                // Clear the memory
                McFunctionBuilder.AppendLine("scoreboard players reset @a funcscript_memory");
                McFunctionBuilder.AppendLine($"data remove storage {MemoryManagement.MemoryTag} variables");

                // Clear the entity references created using tags
                McFunctionBuilder.AppendLine("<removeEntityTagsHere>");
                break;
            case ReloadBehavior.DetachOld:
                // Detach the entities
                McFunctionBuilder.AppendLine("tag @e[tag=funcscript_controlled] remove funcscript_controlled");
                break;
            case ReloadBehavior.KeepOld:
                break;
        }

        //Add("kill @e[tag=funcscript_controlled]");
        Add($"scoreboard objectives add {Computation.ComputationScoreboard} dummy");
        Add("data remove storage funcscript_memory { }");


        Statement.ParseMultiple(ref tokens);

        IEnumerable<string> tagRemovalCommands =
            from variable in MemoryTypes
            where variable.Value == typeof(FuncEntity)
            select $"tag @e[tag={variable.Key}] remove " + variable.Key;

        McFunctionBuilder.Replace("<removeEntityTagsHere>", string.Join("\n", tagRemovalCommands));


        // Add the load entrypoint
        Generator.AddEntrypoint(new LoadEntrypoint("load", McFunctionBuilder.ToString().CreateCommandArray()));

        foreach (Entrypoint entrypoint in AdditionalEntrypoints)
        {
            Generator.AddEntrypoint(entrypoint);
        }

        Generator.Generate();

        Console.WriteLine("Done!");
    }

    /// <summary>
    /// Append a line to the function builder
    /// </summary>
    public static void Add(this string line)
    {
        McFunctionBuilder.Append(Regex.Replace(line + "\n", @"(.*\w.*)", $"{CombinedPrefix}$1", RegexOptions.Multiline));
    }

    /// <summary>
    /// Extracts the code generated while executing the given action
    /// </summary>
    /// <param name="codeGeneration">Any code generated in here is captured</param>
    /// <returns>A string of the captured code</returns>
    public static string YoinkGeneratedCode(Action codeGeneration)
    {
        // Buffer the main string builder
        StringBuilder bufferedStringBuilder = Transpiler.McFunctionBuilder;
        // Create a new string builder for the function
        Transpiler.McFunctionBuilder = new();
        // Parse stuff
        codeGeneration();
        // Get the function string builder
        string yoinkedCode = Transpiler.McFunctionBuilder.ToString();
        // Restore the main string builder
        Transpiler.McFunctionBuilder = bufferedStringBuilder;

        return yoinkedCode;
    }

    public static string AsVarnameProvider(this Value val)
    {
        if (val is not VariableNameProvider)
            LoggingManager.LogError($"Cannot use {val.GetType().Name} as a variable name provider");
        return ((VariableNameProvider) val).VariableName;
    }
}