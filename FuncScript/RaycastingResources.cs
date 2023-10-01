using System.Globalization;
using System.Text;
using FuncScript.Internal;
using FuncSharp;
using FuncSharp.Core;

namespace FuncScript;

public static class RaycastingResources
{
    public const float RaycastAccuracy = 0.1f;

    public static string StringRaycastAccuracy => RaycastAccuracy.ToString(CultureInfo.InvariantCulture);

    public const float MaxRayLength = 100;

    public const int IterationsToReachMaxRayLength = (int) (MaxRayLength / RaycastAccuracy);

    public const bool VisualizeRaycasts = true;

    public static readonly BlockTag RaycastIgnoreBlockTag = new("funcscript_raycast_ignore", Block.water, Block.air, Block.cave_air, Block.void_air, Block.light, Block.structure_void, Block.tripwire, Block.tripwire_hook /*, Block.acacia_sign, Block.acacia_wall_sign, Block.birch_sign, Block.birch_wall_sign, Block.crimson_sign, Block.crimson_wall_sign*/);

    public static void AddRaycastIgnoreBlockTag()
    {
        if (!Transpiler.Generator.BlockTags.Contains(RaycastIgnoreBlockTag))
            Transpiler.Generator.AddBlockTag(RaycastIgnoreBlockTag);
    }

    public static void AddRecursiveRaycastFunctions()
    {
        if (!Transpiler.AdditionalEntrypoints.Contains(RecursiveRaycastBlock))
            Transpiler.Generator.AddEntrypoint(RecursiveRaycastBlock);
        if (!Transpiler.AdditionalEntrypoints.Contains(RecursiveRaycastEntity))
            Transpiler.Generator.AddEntrypoint(RecursiveRaycastEntity);
    }

    #region Recursive Raycasting Functions

    public static string RecursiveRaycastAny =>
        // Move the ray forward
        $"execute as @e[type=marker, tag=funcscript_ray] at @s run tp @s ^ ^ ^{StringRaycastAccuracy}\n" +
        // If the raycast hit an entity, run the raycast hit function
        $"execute if entity @e[type=marker, tag=funcscript_ray, distance=..0.1] run function {Transpiler.Config.DataPackNameSpace}:raycast_hit_any\n" +
        // If the raycast hit a block, run the raycast hit function
        $"execute if block @e[type=marker, tag=funcscript_ray, distance=..0.1] run function {Transpiler.Config.DataPackNameSpace}:raycast_hit_any\n" +
        // Potentially create particles to visualize the ray
        (VisualizeRaycasts ? "execute at @e[tag=funcscript_ray] run particle minecraft:enchanted_hit ~0.5 ~ ~0.5 0 0 0 0 10\n" : "") +
        // Recursively call the function again
        $"execute at @e[type=marker, tag=funcscript_ray] unless score iterations funcscript_computation > maxIterations funcscript_computation " +
        $"if block ~ ~ ~ #minecraft:funcscript_raycast_ignore " +
        $"run function {Transpiler.Config.DataPackNameSpace}:recursive_raycast_block\n";


    public static FunctionEntrypoint RecursiveRaycastEntity
    {
        get
        {
            StringBuilder func = new();

            // Increase the ray iteration couter
            func.AppendLine("scoreboard players add iterations funcscript_computation 1");

            // Check if the iteration counter is greater than the max ray iterations
            func.AppendLine("");

            // Move the ray forward
            func.AppendLine($"execute as @e[type=marker, tag=funcscript_ray] at @s run tp @s ^ ^ ^{StringRaycastAccuracy}");

            // If the raycast hit an entity, run save the position of the ray to variables.raycast_hit.hit_position
            func.AppendLine(
                $"execute at @e[tag=funcscript_ray] as @e[type=!marker, distance=..0.8] run " +
                $"data modify storage {MemoryManagement.MemoryTag} variables.raycast_hit.hit_position set from entity @e[type=marker, tag=funcscript_ray, limit=1] Pos");

            // If the raycast hit an entity, give that entity a tag
            func.AppendLine(
                $"execute at @e[tag=funcscript_ray] as @e[type=!marker, distance=..0.8] run " +
                $"tag @s add funcscript_raycast_hit");

            // Potentially create particles to visualize the ray
            func.AppendLine(VisualizeRaycasts ? "execute at @e[tag=funcscript_ray] run particle minecraft:enchanted_hit ~0.5 ~ ~0.5 0 0 0 0 10\n" : ""); // Potentially create particles to visualize the ray

            // Recursively call the function again
            func.AppendLine(
                $"execute at @e[tag=funcscript_ray] unless entity @e[type=!marker, distance=..0.8] " +
                $"unless score iterations funcscript_computation > maxIterations funcscript_computation run " +
                $"function {Transpiler.Config.DataPackNameSpace}:recursive_raycast_entity\n");

            return new("recursive_raycast_entity", func.ToString().CreateCommandArray());
        }
    }


    public static FunctionEntrypoint RecursiveRaycastBlock => new("recursive_raycast_block", (
        // Move the ray forward
        $"execute as @e[type=marker, tag=funcscript_ray] at @s run tp @s ^ ^ ^{StringRaycastAccuracy}\n" +
        // If the raycast hit a block, run the raycast hit block function
        $"execute as @e[type=marker, tag=funcscript_ray] at @s unless block ~ ~ ~ #minecraft:funcscript_raycast_ignore " +
        $"run data modify storage {MemoryManagement.MemoryTag} variables.raycast_hit.hit_position set from entity @s Pos\n" +
        // Potentially create particles to visualize the ray
        (VisualizeRaycasts ? "execute at @e[tag=funcscript_ray] run particle minecraft:enchanted_hit ~0.5 ~ ~0.5 0 0 0 0 10\n" : "") +
        // Recursively call the function again
        $"execute at @e[type=marker, tag=funcscript_ray] unless score iterations funcscript_computation > maxIterations funcscript_computation " +
        $"if block ~ ~ ~ #minecraft:funcscript_raycast_ignore " +
        $"run function {Transpiler.Config.DataPackNameSpace}:recursive_raycast_block\n"
    ).CreateCommandArray());

    #endregion
}