using System.Diagnostics;
using System.Runtime.CompilerServices;
using FuncScript.Internal;
using FuncScript.Types;
using SlowLang.Engine;

namespace FuncScript;

public static class Resources
{
    public static string ReturnValue = string.Empty;

    public static readonly Dictionary<string, Func<FuncScriptValue[], string>> Functions = new()
    {
        /*
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

                if (parameters[0] is ConstFuncScriptValue)
                    return $"tellraw @a {{\"text\":\"{parameters[0].Generate()}\"}}";
                else
                    return $"tellraw @a {{\"storage\":\"{MemoryManagement.MemoryTag}\",\"nbt\":\"variables.{parameters[0].AsVarnameProvider()}\"}}";
            }
        },
        {
            "summon", parameters =>
            {
                if (parameters.Length != 2)
                {
                    LoggingManager.LogError($"The summon function takes two arguments but received {parameters.Length} arguments.");
                }

                if (parameters[0] is not FuncEntity)
                    LoggingManager.LogError($"The summon function takes an entity as its first argument but received {parameters[0].GetType().Name}.");

                if (parameters[1] is not VariableNameProvider)
                    LoggingManager.LogError($"The summon function takes a vector as its second argument but received {parameters[1].GetType().Name}.");

                string entityId = IdManager.GetId();

                ReturnValue = entityId;

                // First, we summon a helper marker
                // Then we can set its position from the variable.
                // Then we summon the actual entity at the marker's position. This is because some entities can't be moved after they're summoned.
                // Lastly, we remove the marker.

                return "summon marker ~ ~ ~" + " {\"Tags\":[\"funcscript_helper\", \"funcscript_controlled\"]}\n" + // Summon a helper entity
                       $"data modify entity @e[tag=funcscript_controlled, tag=funcscript_helper, limit=1] Pos set from storage {MemoryManagement.MemoryTag} variables.{parameters[1].AsVarnameProvider()}\n" + // Set the position of the helper entity
                       $"execute at @e[tag=funcscript_controlled, tag=funcscript_helper, limit=1] run summon {parameters[0].Generate()} ~ ~ ~" + " {\"Tags\":[\"" + entityId + "\", \"funcscript_controlled\"]}\n" + // Summon the entity at the position of the helper
                       "kill @e[tag=funcscript_controlled, tag=funcscript_helper, limit=1]\n"; // Remove the helper entity
            }
        },
        {
            "setblock", parameters =>
            {
                if (parameters.Length != 2)
                {
                    LoggingManager.LogError($"The setblock function takes two arguments but received {parameters.Length} arguments.");
                }

                if (!parameters[0].IsOfType<FuncBlock>())
                    LoggingManager.LogError($"The setblock function takes a block as its first argument but received {parameters[0].GetFuncTypeName()}.");

                if (!parameters[1].IsOfType<FuncVector>())
                    LoggingManager.LogError($"The setblock function takes a vector as its second argument but received {parameters[1].GetFuncTypeName()}.");

                // We need to create a temporary entity to set the block at the position of the entity because otherwise it's not possible to set a block at a dynamic position
                return "summon minecraft:marker ~ ~ ~ {\"Tags\":[\"funcscript_controlled\", \"blockPositioningMarker\"]}\n" + // Summon a temporary marker
                       $"data modify entity @e[tag=blockPositioningMarker, limit=1] Pos set from storage {MemoryManagement.MemoryTag} variables.{parameters[1].AsVarnameProvider()}\n" + // Set the position of the marker
                       $"execute at @e[tag=blockPositioningMarker, limit=1] run setblock ~ ~ ~ {parameters[0].Generate()}\n" + // Set the block at the markers position
                       "kill @e[tag=blockPositioningMarker, limit=1]"; // Kill the marker
            }
        },
        {
            "vector", parameters =>
            {
                if (parameters.Length != 3)
                {
                    LoggingManager.LogError($"The vector creation function (No, it's not a constructor) takes three arguments but received {parameters.Length} arguments.");
                }

                for (int i = 0; i < 3; i++)
                {
                    if (!parameters[i].IsOfType<FuncNumber>())
                        LoggingManager.LogError($"The vector creation function (No, it's not a constructor) takes three numbers as arguments but received {parameters[i].GetFuncTypeName()}.");
                }


                string id = IdManager.GetId();

                Transpiler.MemoryTypes[id] = typeof(FuncVector);

                ReturnValue = id;

                return $"data remove storage {MemoryManagement.MemoryTag} variables.{id}\n" +
                       $"data modify storage {MemoryManagement.MemoryTag} variables.{id} insert 0 from storage {MemoryManagement.MemoryTag} variables.{parameters[0].AsVarnameProvider()}\n" +
                       $"data modify storage {MemoryManagement.MemoryTag} variables.{id} insert 1 from storage {MemoryManagement.MemoryTag} variables.{parameters[1].AsVarnameProvider()}\n" +
                       $"data modify storage {MemoryManagement.MemoryTag} variables.{id} insert 2 from storage {MemoryManagement.MemoryTag} variables.{parameters[2].AsVarnameProvider()}\n";
            }
        },
        {
            "getBlock", parameters =>
            {
                if (parameters.Length != 1)
                    LoggingManager.LogError($"The getBlock function takes one argument but received {parameters.Length} arguments.");

                if (!parameters[0].IsOfType<FuncVector>())
                    LoggingManager.LogError($"The getBlock function takes a vector as its argument but received {parameters[0].GetType().Name}.");

                string id = IdManager.GetId();
                Transpiler.MemoryTypes[id] = typeof(FuncBlock);
                ReturnValue = id;
                return $"data modify storage {MemoryManagement.MemoryTag} variables.{id}.type set value \"block\"\n" +
                       MemoryManagement.MoveVariable(parameters[0].AsVarnameProvider(), $"{id}.pos");

            }
        }
    };
}