using System.Runtime.CompilerServices;
using FuncSharp.Commands;
using FuncSharp.Core;
using FuncSharp.DataPackGen;

namespace FuncSharp;

public static class FunctionalCommandCreation
{
    private static List<CommandBase> OnLoadCommands = new();

    private static List<FunctionEntrypoint> Functions = new();

    public static event Action? OnTick;

    public static void Generate(string path, string name)
    {
        DataPackGenerator generator = new(path, name);

        // Evaluate logic in OnTick
        FunctionCommandList = new();
        OnTick?.Invoke();
        generator.AddEntrypoint(new TickEntrypoint("tick", FunctionCommandList.ToArray()));
        FunctionCommandList = null;
        

        generator.AddEntrypoint(new LoadEntrypoint("load", OnLoadCommands.ToArray()));

        foreach (FunctionEntrypoint entrypoint in Functions)
        {
            generator.AddEntrypoint(entrypoint);
        }
        
        generator.Generate();
    }

    private static List<CommandBase> CurrentCommandList => FunctionCommandList ?? OnLoadCommands;


    private static List<CommandBase>? FunctionCommandList;

    public static void AddFunction(string name, Action action)
    {
        FunctionCommandList = new();
        action();
        Functions.Add(new(name, FunctionCommandList.ToArray()));

        FunctionCommandList = null;
    }

    public static Say Say(string message)
    {
        Say cmd = new(message);
        CurrentCommandList.Add(cmd);
        return cmd;
    }

    public static SetBlock SetBlock(Vector position, Block block, SetBlockMode mode = SetBlockMode.Replace)
    {
        SetBlock cmd = new(position, block, mode);
        CurrentCommandList.Add(cmd);
        return cmd;
    }

    public static Kill Kill(string target)
    {
        Kill cmd = new(target);
        CurrentCommandList.Add(cmd);
        return cmd;
    }

    public static Fill Fill(Vector start, Vector end, Block block, FillMode mode = FillMode.Replace)
    {
        Fill cmd = new(start, end, block, mode);
        CurrentCommandList.Add(cmd);
        return cmd;
    }
    
    public static GameRule GameRule(string name, string value)
    {
        GameRule cmd = new(name, value);
        CurrentCommandList.Add(cmd);
        return cmd;
    }
}