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
                    LoggingManager.LogError($"The say function takes one argument but received {parameters.Length} arguments.");
                }
                
                if(parameters[0] is ConstFuncScriptValue)
                    return $"tellraw @a {{\"text\":\"{parameters[0].Generate()}\"}}";
                else
                    return $"tellraw @a {{\"storage\":\"funcscript_memory\",\"nbt\":\"variables.{parameters[0].AsVarnameProvider()}\"}}";
            }
        },
        {
            "summon", parameters =>
            {
                if(parameters.Length != 2)
                {
                    LoggingManager.LogError($"The summon function takes two arguments but received {parameters.Length} arguments.");
                }
                
                if(parameters[0] is not FuncEntity)
                    LoggingManager.LogError($"The summon function takes an entity as its first argument but received {parameters[0].GetType().Name}.");
                
                if(parameters[1] is not VariableNameProvider)
                    LoggingManager.LogError($"The summon function takes a vector as its second argument but received {parameters[1].GetType().Name}.");

                string entityId = IdManager.GetId();

                ReturnValue = entityId;
                
                return $"summon {parameters[0].Generate()} ~ ~ ~" + " {\"Tags\":[\"" + entityId + "\", \"funcscript_controlled\"]}\n" + // Summon the entity
                       $"data modify entity @e[tag=funcscript_controlled, tag={entityId}, limit=1] Pos set from storage funcscript_memory variables.{parameters[1].AsVarnameProvider()}"; // Set the position of the entity
            }
        },
    };
}