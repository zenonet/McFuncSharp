using FuncSharp.Commands;

namespace FuncSharp;

public class LoadEntrypoint : Entrypoint
{
    public LoadEntrypoint(string name, CommandBase[] commands) : base(name, commands)
    {
    }
}