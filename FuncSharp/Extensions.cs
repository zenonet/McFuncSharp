using FuncSharp.Commands;

namespace FuncSharp;

public static class Extensions
{
    public static CustomCommand CreateCommand(this string command)
    {
        return new (command);
    }
    public static CustomCommand[] CreateCommandArray(this string commands)
    {
        return commands.Split('\n').Select(x => x.CreateCommand()).ToArray();
    }
}