using System.Diagnostics;
using FuncScript.Internal;
using FuncScript.Types;
using SlowLang.Engine;

namespace FuncScript;

public static class Resources
{
    public static string ReturnValue = string.Empty;
    
    public static readonly Dictionary<string, Func<FuncScriptValue[], string>> Functions = new()
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
            "say", parameters =>
            {
                if (parameters.Length != 1)
                {
                    LoggingManager.LogError($"The say function takes one argument but received {varname.Length} arguments.");
                }
                return $"tellraw @a {{\"storage\":\"funcscript_memory\",\"nbt\":\"variables.{varname[0]}\"}}";
            }
        },
    };
}