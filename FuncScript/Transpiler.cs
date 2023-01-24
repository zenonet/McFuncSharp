using System.Text;
using SlowLang.Engine;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Tokens;

namespace FuncScript;

public static class Transpiler
{
    public static StringBuilder McFunctionBuilder { get; private set; } = new();
    

    public static void Transpile(string funcScriptCode, Config? config = null)
    {
        config ??= new();
        
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

        Console.WriteLine("Done!");
    }

    /// <summary>
    /// Append a line to the function builder
    /// </summary>
    public static void Add(this string line)
    {
        McFunctionBuilder.AppendLine(line);
    }
}