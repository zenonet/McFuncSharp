using FuncSharp.Commands;

namespace FuncSharp;

public class TickEntrypoint : Entrypoint
{
    public TickEntrypoint(string name, CommandBase[] commands) : base(name, commands)
    {
    }
}