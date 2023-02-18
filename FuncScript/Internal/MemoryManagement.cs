﻿namespace FuncScript.Internal;

public static class MemoryManagement
{
    public const string MemoryTag = "funcscript_memory";

    public static string GetVariable(string variableName)
    {
        return "data get storage " + MemoryTag + " variables." + variableName;
    }

    public static string SetVariable(string variableName, string value)
    {
        return "data modify storage " + MemoryTag + " variables." + variableName + " set value " + value;
    }
    
    public static string MoveVariable(string variableName, string target)
    {
        return "data modify storage " + MemoryTag + " variables." + target + " set from storage " + MemoryTag + " variables." + variableName;
    }

    public static string FreeMemory(string variableName)
    {
        return "data remove storage " + MemoryTag + " variables." + variableName;
    }

    public static string MoveToComputationScoreboard(string variableName, string nameOnScoreboard)
    {
        return "execute store result score " + nameOnScoreboard + " " + Computation.ComputationScoreboard + " run data get storage " + MemoryTag + " variables." + variableName;
    }

    public static string MoveToStorage(string variableName, string nameOnScoreboard)
    {
        return "execute store result storage " + MemoryTag + " variables." + variableName + " double 1 run scoreboard players get " + nameOnScoreboard + " " + Computation.ComputationScoreboard;
    }



    public static class CallStack
    {
        public static string Push(string variableName)
        {
            return "data modify storage " + MemoryTag + " callStack append from storage " + MemoryTag + " variables." + variableName;
        }

        public static string Pop()
        {
            return "data remove storage " + MemoryTag + " callStack[-1]";
        }

        public static string Peek(string variableName)
        {
            return "data modify storage " + MemoryTag + " variables." + variableName + " set from storage " + MemoryTag + " callStack[-1]";
        }
    }
}