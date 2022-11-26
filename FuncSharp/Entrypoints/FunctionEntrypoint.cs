using FuncSharp.Commands;

namespace FuncSharp;

public class FunctionEntrypoint : Entrypoint
{
    public FunctionEntrypoint(string name, CommandBase[] commands) : base(name, commands)
    {
    }
}