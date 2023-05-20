using FuncScript.Internal;
using FuncSharp;
using FuncSharp.Core;

namespace FuncScript;

public static class RaycastingResources
{
    public const string RaycastAccuracy = "0.1";

    public const bool VisualizeRaycasts = false;

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
    }

    public static void AddRaycastHitFunctions()
    {
        if (!Transpiler.AdditionalEntrypoints.Contains(RaycastHitBlock))
            Transpiler.Generator.AddEntrypoint(RaycastHitBlock);
    }

    #region Recursive Raycasting Functions

/*
    public static string RecursiveRaycastAll =>
        $"execute as @e[type=marker, tag=funcscript_ray] at @s run tp @s ^ ^ ^{RaycastAccuracy}\n" +
        $"execute if entity @e[type=marker, tag=funcscript_ray, distance=..0.1] run function {Transpiler.Config.DataPackNameSpace}:raycast_hit_all\n" +
        $"execute if block @e[type=marker, tag=funcscript_ray, distance=..0.1] run function {Transpiler.Config.DataPackNameSpace}:raycast_hit_all\n";

    
    public static string RecursiveRaycastEntity =>
        $"execute as @e[type=marker, tag=funcscript_ray] at @s run tp @s ^ ^ ^{RaycastAccuracy}\n" +
        $"execute if entity @e[type=marker, tag=funcscript_ray, distance=..0.1] run function {Transpiler.Config.DataPackNameSpace}:raycast_hit_entity\n";
    */
    public static FunctionEntrypoint RecursiveRaycastBlock => new ("recursive_raycast_block", (
        // Move the ray forward
        $"execute as @e[type=marker, tag=funcscript_ray] at @s run tp @s ^ ^ ^{RaycastAccuracy}\n" +
        // If the raycast hit a block, run the raycast hit block function
        $"execute at @e[type=marker, tag=funcscript_ray] unless block ~ ~ ~ #minecraft:funcscript_raycast_ignore " +
        $"run function {Transpiler.Config.DataPackNameSpace}:raycast_hit_block\n" + 
        // Potentially summon an entity to visualize the ray
        (VisualizeRaycasts ? "execute at @e[tag=funcscript_ray] run particle minecraft:enchanted_hit ~0.5 ~ ~0.5 0 0 0 0 10\n" : "") +
        // Recusively call the function again
        $"execute at @e[type=marker, tag=funcscript_ray] if block ~ ~ ~ #minecraft:funcscript_raycast_ignore " + 
        $"run function {Transpiler.Config.DataPackNameSpace}:recursive_raycast_block\n"
    ).CreateCommandArray());

    #endregion

    #region Callback Functions

    public static FunctionEntrypoint RaycastHitBlock => new ("raycast_hit_block", (
        // Save the hit position
        $"data modify storage {MemoryManagement.MemoryTag} variables.raycast_hit.hit_position set from entity @e[type=marker, tag=funcscript_ray, limit=1] Pos\n" +
        // Kill the ray
        "kill @e[type=marker, tag=funcscript_ray]\n"
    ).CreateCommandArray());

    #endregion
}