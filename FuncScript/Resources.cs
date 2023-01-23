using FuncScript.Internal;
using SlowLang.Engine;

namespace FuncScript;

public static class Resources
{
    public static string ReturnValue = string.Empty;
    
    public static readonly Dictionary<string, Func<string[], string>> Functions = new()
    {/*
        {
            "kill", () =>
            {
                return "kill @e[tag=]";
            }
        },*/
        /*{
            "fill", varname =>
            {
                return $"fill {varname} ~ ~ ~ air";
            }
        },*/
        {
            "say", varname =>
            {
                return $"tellraw @a {{\"storage\":\"funcscript_memory\",\"nbt\":\"variables.{varname[0]}\"}}";
            }
        },
    };
}