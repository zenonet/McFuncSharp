using System.Text;
using System.Text.RegularExpressions;
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

    public static void Transpile(string funcScriptCode, Config config)
    {
        Config = config;
        
        DataPackGenerator generator = new DataPackGenerator(config.DataPackPath, config.DataPackNameSpace);
        
        McFunctionBuilder = new();

        TokenList tokens = Lexer.Lex(funcScriptCode);
        
        switch (config.ReloadBehavior)
        {
            case ReloadBehavior.KillOld:
                // Make sure the entities won't drop any items
                McFunctionBuilder.AppendLine("execute as @e[tag=funcscript_controlled] run data merge entity @s {DeathLootTable:\"minecraft:empty\"}");
                // Kill the entities
                McFunctionBuilder.AppendLine("kill @e[tag=funcscript_controlled]");
                break;
            case ReloadBehavior.DetachOld:
                // Detach the entities
                McFunctionBuilder.AppendLine("execute as @e[tag=funcscript_controlled] run tag @s remove funcscript_controlled");
                break;
            case ReloadBehavior.KeepOld:
                break;
        }
        
        Add("kill @e[tag=funcscript_controlled]");
        Add("scoreboard objectives add funcscript_computation dummy");
        Add("data remove storage funcscript_memory { }");
        
        
        Statement.ParseMultiple(ref tokens);

        // Add the load entrypoint
        generator.AddEntrypoint(new LoadEntrypoint("load", McFunctionBuilder.ToString().CreateCommandArray()));

        foreach (Entrypoint entrypoint in AdditionalEntrypoints)
        {
            generator.AddEntrypoint(entrypoint);
        }
        
        generator.Generate();
        
        Console.WriteLine("Done!");
    }

    /// <summary>
    /// Append a line to the function builder
    /// </summary>
    public static void Add(this string line)
    {
        McFunctionBuilder.Append(Regex.Replace(line + "\n", @"(.*\w.*)", $"{CombinedPrefix}$1", RegexOptions.Multiline));
    }
    
    public static string AsVarnameProvider(this Value val)
    {
        if(val is not VariableNameProvider)
            LoggingManager.LogError($"Cannot use {val.GetType().Name} as a variable name provider");
        return ((VariableNameProvider) val).VariableName;
    }
}