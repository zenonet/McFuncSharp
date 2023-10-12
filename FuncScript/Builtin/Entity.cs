using FuncScript.Internal;
using FuncScript.Types;
// ReSharper disable InconsistentNaming

namespace FuncScript.Builtin;

[BuiltinClass]
public class Entity
{
    public static string getPos(
        [BuiltinFunctionParameter(typeof(FuncEntity))]
        VariableNameProvider entity)
    {
        string id = IdManager.GetDataId();
        Transpiler.MemoryTypes[id] = typeof(FuncVector);
        Resources.ReturnValue = id;

        return $"data modify storage {MemoryManagement.MemoryTag} variables.{id} set from entity @e[tag={entity.VariableName}, limit=1] Pos";
    }
    
    public static string getRotation(
        [BuiltinFunctionParameter(typeof(FuncEntity))]
        VariableNameProvider entity)
    {
        string id = IdManager.GetDataId();
        Transpiler.MemoryTypes[id] = typeof(FuncVector);
        Resources.ReturnValue = id;

        return $"data modify storage {MemoryManagement.MemoryTag} variables.{id} set from entity @e[tag={entity.VariableName}, limit=1] Rotation";
    }
    
    public static string getMotion(
        [BuiltinFunctionParameter(typeof(FuncEntity))]
        VariableNameProvider entity)
    {
        string id = IdManager.GetDataId();
        Transpiler.MemoryTypes[id] = typeof(FuncVector);
        Resources.ReturnValue = id;

        return $"data modify storage {MemoryManagement.MemoryTag} variables.{id} set from entity @e[tag={entity.VariableName}, limit=1] Motion";
    }

    public static string getViewDirection([BuiltinFunctionParameter(typeof(FuncEntity))] VariableNameProvider entity)
    {
        string id = IdManager.GetDataId();
        Transpiler.MemoryTypes[id] = typeof(FuncVector);
        Resources.ReturnValue = id;

        return $"summon marker .0 .0 .0 {{Tags:[\"funcscript_controlled\", \"funcscript_view_direction_marker\"]}}\n" +
                $"data modify entity @e[tag=funcscript_view_direction_marker, limit=1] Rotation set from entity @e[tag={entity.VariableName}, limit=1] Rotation\n" +
                "execute as @e[tag=funcscript_view_direction_marker] at @s run tp @s ^ ^ ^1\n"+
                $"data modify storage funcscript_memory variables.{id} set from entity @e[tag=funcscript_view_direction_marker, limit=1] Pos\n" +
                "kill @e[tag=funcscript_view_direction_marker]\n";
    }
    
    
    public static string getName([BuiltinFunctionParameter(typeof(FuncEntity))] VariableNameProvider entity)
    {
        string id = IdManager.GetDataId();
        Transpiler.MemoryTypes[id] = typeof(FuncString);
        Resources.ReturnValue = id;

        return 
            $"execute if entity @e[tag={entity.VariableName}, type=player] as @e[tag={entity.VariableName}] run data modify storage {MemoryManagement.MemoryTag} variables.{id} set value \"Player name can't be saved\" \n" +
            $"execute unless entity @e[tag={entity.VariableName}, type=player] run data modify storage {MemoryManagement.MemoryTag} variables.{id} set from entity @e[tag={entity.VariableName}, limit=1] CustomName\n";

    }
    
    public static string teleport([BuiltinFunctionParameter(typeof(FuncEntity))] VariableNameProvider entity, [BuiltinFunctionParameter(typeof(FuncVector))]VariableNameProvider position)
    {
        return 
            $"execute if entity @e[tag={entity}] run summon marker 0 0 0 {{Tags:[\"funcscript_controlled\", \"funscript_teleportation_marker\"]}}\n" +
            $"execute if entity @e[tag={entity}] run data modify entity @e[tag=funscript_teleportation_marker, limit=1] Pos set from storage {MemoryManagement.MemoryTag} variables.{position}\n" +
            $"execute if entity @e[tag={entity}] run execute at @e[tag=funscript_teleportation_marker] run tp @e[tag={entity}] ~ ~ ~\n";

    }
    
}