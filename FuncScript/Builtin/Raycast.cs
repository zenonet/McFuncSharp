// ReSharper disable InconsistentNaming

using FuncScript.Internal;
using FuncScript.Types;

namespace FuncScript.Builtin;

[BuiltinClass]
public class Raycast
{
    public static string block(
        [BuiltinFunctionParameter(typeof(FuncVector))]
        VariableNameProvider position,
        [BuiltinFunctionParameter(typeof(FuncVector))]
        VariableNameProvider direction)
    {
        RaycastingResources.AddRaycastIgnoreBlockTag();

        RaycastingResources.AddRecursiveRaycastFunctions();

        string id = IdManager.GetDataId();

        string absoluteDirId = IdManager.GetDataId();

        Resources.ReturnValue = id;

        Transpiler.MemoryTypes[id] = typeof(FuncVector);

        return FuncVector.VectorAdd(position.VariableName, direction.VariableName, absoluteDirId, 64) + "\n" + // Adding the raycast direction to the rays origin to get the position the ray should look at"summon marker ~ ~ ~ {\"Tags\":[\"funcscript_controlled\", \"funcscript_raycastDirection\"]}\n" +
               // Summon the rayd shadow legends:
               "summon marker ~ ~ ~ {\"Tags\":[\"funcscript_controlled\", \"funcscript_ray\"]}\n" +
               $"data modify entity @e[tag=funcscript_ray, limit=1] Pos set from storage {MemoryManagement.MemoryTag} variables.{position.VariableName}\n" + // Set the ray start position
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
    
    public static string entity(
        [BuiltinFunctionParameter(typeof(FuncVector))]
        VariableNameProvider position,
        [BuiltinFunctionParameter(typeof(FuncVector))]
        VariableNameProvider direction)
    {
        RaycastingResources.AddRaycastIgnoreBlockTag();

                RaycastingResources.AddRecursiveRaycastFunctions();

                string id = IdManager.GetEntityId();

                string absoluteDirId = IdManager.GetDataId();

                Resources.ReturnValue = id;

                Transpiler.MemoryTypes[id] = typeof(FuncEntity);

                return
                    // Prepare the iteration counter:
                    $"scoreboard players reset iterations {Computation.ComputationScoreboard}\n" +
                    $"scoreboard players set maxIterations {Computation.ComputationScoreboard} {RaycastingResources.IterationsToReachMaxRayLength}\n" +
                    // Dereference the last raycast hit:
                    $"tag @e[tag=funcscript_raycast_hit] remove funcscript_raycast_hit\n" +
                    // Adding the raycast direction to the rays origin to get the position the ray should look at
                    FuncVector.VectorAdd(position.VariableName, direction.VariableName, absoluteDirId) + "\n" +
                    // Summon the ray:
                    "summon marker ~ ~ ~ {\"Tags\":[\"funcscript_controlled\", \"funcscript_ray\"]}\n" +
                    $"data modify entity @e[tag=funcscript_ray, limit=1] Pos set from storage {MemoryManagement.MemoryTag} variables.{position.VariableName}\n" + // Set the ray start position
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
}