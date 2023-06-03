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

                if (!parameters[0].IsOfType<FuncEntityType>())
                    LoggingManager.LogError($"The summon function takes an entity as its first argument but received {parameters[0].GetType().Name}.");

                if (!parameters[1].IsOfType<FuncVector>())
                    LoggingManager.LogError($"The summon function takes a vector as its second argument but received {parameters[1].GetType().Name}.");

                string entityId = IdManager.GetEntityId();
                Transpiler.MemoryTypes[entityId] = typeof(FuncEntity);

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
            "setBlock", parameters =>
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


                string id = IdManager.GetDataId();

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

                string id = IdManager.GetDataId();
                Transpiler.MemoryTypes[id] = typeof(FuncBlock);
                ReturnValue = id;
                return $"data modify storage {MemoryManagement.MemoryTag} variables.{id}.type set value \"block\"\n" +
                       MemoryManagement.MoveVariable(parameters[0].AsVarnameProvider(), $"{id}.pos");
            }
        },
        {
            "addVector", parameters =>
            {
                if (parameters.Length != 2)
                    LoggingManager.LogError($"The addVector function takes 2 argument but received {parameters.Length} arguments.");

                if (!parameters[0].IsOfType<FuncVector>())
                    LoggingManager.LogError($"The getBlock function takes a vector as its first argument but received {parameters[0].GetType().Name}.");
                if (!parameters[1].IsOfType<FuncVector>())
                    LoggingManager.LogError($"The getBlock function takes a vector as its second argument but received {parameters[0].GetType().Name}.");

                string id = IdManager.GetDataId();
                Transpiler.MemoryTypes[id] = typeof(FuncVector);
                ReturnValue = id;

                return FuncVector.VectorAdd(parameters[0].AsVarnameProvider(), parameters[1].AsVarnameProvider(), id);
            }
        },
        {
            "raycastBlock", parameters =>
            {
                if (parameters.Length != 2)
                    LoggingManager.LogError($"The raycast function takes two arguments but received {parameters.Length} arguments.");

                if (!parameters[0].IsOfType<FuncVector>())
                    LoggingManager.LogError("The raycast function takes a vector (the starting position) as its first argument but received " + parameters[0].GetFuncTypeName());

                if (!parameters[0].IsOfType<FuncVector>())
                    LoggingManager.LogError("The raycast function takes a vector (the ray direction) as its second argument but received " + parameters[1].GetFuncTypeName());

                RaycastingResources.AddRaycastIgnoreBlockTag();

                RaycastingResources.AddRecursiveRaycastFunctions();

                string id = IdManager.GetDataId();

                string absoluteDirId = IdManager.GetDataId();

                ReturnValue = id;

                Transpiler.MemoryTypes[id] = typeof(FuncVector);

                return FuncVector.VectorAdd(parameters[0].AsVarnameProvider(), parameters[1].AsVarnameProvider(), absoluteDirId) + "\n" + // Adding the raycast direction to the rays origin to get the position the ray should look at"summon marker ~ ~ ~ {\"Tags\":[\"funcscript_controlled\", \"funcscript_raycastDirection\"]}\n" +
                       // Summon the ray:
                       "summon marker ~ ~ ~ {\"Tags\":[\"funcscript_controlled\", \"funcscript_ray\"]}\n" +
                       $"data modify entity @e[tag=funcscript_ray, limit=1] Pos set from storage {MemoryManagement.MemoryTag} variables.{parameters[0].AsVarnameProvider()}\n" + // Set the ray start position
                       // Summon the raycast direction marker:
                       "summon marker ~ ~ ~ {\"Tags\":[\"funcscript_controlled\", \"funcscript_raycastDirection\"]}\n" +
                       $"data modify entity @e[tag=funcscript_raycastDirection, limit=1] Pos set from storage {MemoryManagement.MemoryTag} variables.{absoluteDirId}\n" + // Set the raycast direction markers position
                       // Make the ray look at the raycastDirection marker:
                       $"execute as @e[tag=funcscript_ray] at @s run tp @s ~ ~ ~ facing entity @e[tag=funcscript_raycastDirection, limit=1] eyes\n" +
                       // Reset the iterations counter
                       "scoreboard players reset iterations funcscript_computation\n" +
                       // Invoke the recursive function (start the raycasting process)
                       $"function {Transpiler.Config.DataPackNameSpace}:{RaycastingResources.RecursiveRaycastBlock.Name}\n" +
                       MemoryManagement.MoveVariable("raycast_hit.hit_position", id) + "\n" +
                       "kill @e[tag=funcscript_raycastDirection, limit=1]\n" +
                       "kill @e[tag=funcscript_ray, limit=1]";
            }
        },
        {
            "raycastEntity", parameters =>
            {
                if (parameters.Length != 2)
                    LoggingManager.LogError($"The raycast function takes two arguments but received {parameters.Length} arguments.");

                if (!parameters[0].IsOfType<FuncVector>())
                    LoggingManager.LogError("The raycast function takes a vector (the starting position) as its first argument but received " + parameters[0].GetFuncTypeName());

                if (!parameters[0].IsOfType<FuncVector>())
                    LoggingManager.LogError("The raycast function takes a vector (the ray direction) as its second argument but received " + parameters[1].GetFuncTypeName());

                RaycastingResources.AddRaycastIgnoreBlockTag();

                RaycastingResources.AddRecursiveRaycastFunctions();

                string id = IdManager.GetEntityId();

                string absoluteDirId = IdManager.GetDataId();

                ReturnValue = id;

                Transpiler.MemoryTypes[id] = typeof(FuncEntity);

                return
                    // Prepare the iteration counter:
                    $"scoreboard players reset iterations {Computation.ComputationScoreboard}\n" +
                    $"scoreboard players set maxIterations {Computation.ComputationScoreboard} {RaycastingResources.IterationsToReachMaxRayLength}\n" +
                    // Dereference the last raycast hit:
                    $"tag @e[tag=funcscript_raycast_hit] remove funcscript_raycast_hit\n" +
                    // Adding the raycast direction to the rays origin to get the position the ray should look at
                    FuncVector.VectorAdd(parameters[0].AsVarnameProvider(), parameters[1].AsVarnameProvider(), absoluteDirId) + "\n" +
                    // Summon the ray:
                    "summon marker ~ ~ ~ {\"Tags\":[\"funcscript_controlled\", \"funcscript_ray\"]}\n" +
                    $"data modify entity @e[tag=funcscript_ray, limit=1] Pos set from storage {MemoryManagement.MemoryTag} variables.{parameters[0].AsVarnameProvider()}\n" + // Set the ray start position
                    // Summon the raycast direction marker:
                    "summon marker ~ ~ ~ {\"Tags\":[\"funcscript_controlled\", \"funcscript_raycastDirection\"]}\n" +
                    $"data modify entity @e[tag=funcscript_raycastDirection, limit=1] Pos set from storage {MemoryManagement.MemoryTag} variables.{absoluteDirId}\n" + // Set the raycast direction markers position
                    // Make the ray look at the raycastDirection marker:
                    $"execute as @e[tag=funcscript_ray] at @s run tp @s ~ ~ ~ facing entity @e[tag=funcscript_raycastDirection, limit=1] eyes\n" +
                    // Invoke the recursive function (start the raycasting process)
                    $"function {Transpiler.Config.DataPackNameSpace}:{RaycastingResources.RecursiveRaycastEntity.Name}\n" +
                    // When the raycast is done, re-tag the entity with a new id
                    "tag @e[tag=func_id_20] remove func_id_20\n" +
                    $"tag @e[tag=funcscript_raycast_hit] add {id}\n" +
                    $"tag @e[tag=funcscript_raycast_hit] remove funcscript_raycast_hit\n" +
                    "kill @e[tag=funcscript_ray, limit=1]\n" +
                    "kill @e[tag=funcscript_raycastDirection, limit=1]";
            }
        },
        {
            "glow", parameters =>
            {
                if (parameters.Length != 1)
                    LoggingManager.LogError($"The glow function takes 1 argument but received {parameters.Length} arguments.");

                if (!parameters[0].IsOfType<FuncEntity>())
                    LoggingManager.LogError("The glow function takes an entity as its argument but received " + parameters[1].GetFuncTypeName());

                return $"effect give @e[tag={parameters[0].AsVarnameProvider()}] minecraft:glowing 3 255 true";
            }
        }
    };
}