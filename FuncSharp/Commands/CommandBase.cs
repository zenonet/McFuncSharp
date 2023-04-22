using System.Reflection.Metadata;
using FuncSharp.Core;

namespace FuncSharp.Commands;

public abstract class CommandBase
{
    public string? RunAs { get; set; }
    public Vector? RunAt { get; set; }

    protected abstract string GenerateInternal();

    public string Generate()
    {
        if (RunAs == null && RunAt == null)
            return GenerateInternal();

        string execute = "execute ";

        if (RunAt != null)
            execute += $"at {RunAt} ";

        // Completely over engineered shit to allow things like "Say("Amogus").As("<Any string>") to work"
        if (RunAs != null)
        {
            if (!RunAs.Contains(' '))
            {
                execute += $"run execute if entity {RunAs} as {RunAs} run {GenerateInternal()}\n" +
                           $"execute unless entity {RunAs} run summon marker ~ ~ ~ {{Tags:[\"funcsharp_run_at_helper\"], CustomName:'{{\"text\":\"{RunAs}\"}}'}}\n" +
                           $"execute unless entity {RunAs} run execute as @e[type=marker,tag=funcsharp_run_at_helper, limit=1] {(RunAt == null ? string.Empty : $"at {RunAt} ")}run {GenerateInternal()}\n" +
                           $"execute unless entity RunAs run kill @e[type=marker,tag=funcsharp_run_at_helper]";
            }
            else
            {
                // If the string contains spaces, it's a random string and we need to create an entity with that name
                execute += $"run summon marker ~ ~ ~ {{Tags:[\"funcsharp_run_at_helper\"], CustomName:'{{\"text\":\"{RunAs}\"}}'}}\n" +
                           $"execute as @e[type=marker,tag=funcsharp_run_at_helper, limit=1] {(RunAt == null ? string.Empty : $"at {RunAt} ")}run {GenerateInternal()}\n" +
                           $"kill @e[type=marker,tag=funcsharp_run_at_helper, limit=1]";
            }
        }


        return execute;
    }


    public CommandBase As(string entity)
    {
        RunAs = entity;
        return this;
    }

    public CommandBase At(Vector position)
    {
        RunAt = position;
        return this;
    }
}