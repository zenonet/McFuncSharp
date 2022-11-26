using FuncSharp.Commands;
using FuncSharp.Core;
using FuncSharp.DataPackGen;

namespace FuncSharp;

public static class FunctionalCommandCreation
{
    private static List<CommandBase> OnLoadCommands = new();

    public static void Generate(string path, string name)
    {
        DataPackGenerator generator = new(path, name);
        
        generator.AddEntrypoint(new LoadEntrypoint("load", OnLoadCommands.ToArray()));
        
        generator.Generate();
    }
    
    public static Say Say(string message)
    {
        Say cmd = new(message);
        OnLoadCommands.Add(cmd);
        return cmd;
    }

    public static SetBlock SetBlock(Vector position, Block block, SetBlockMode mode = SetBlockMode.Replace)
    {
        SetBlock cmd = new(position, block, mode);
        OnLoadCommands.Add(cmd);
        return cmd;
    }

    public static Kill Kill(string target)
    {
        Kill cmd = new(target);
        OnLoadCommands.Add(cmd);
        return cmd;
    }
}