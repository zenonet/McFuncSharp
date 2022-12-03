namespace FuncSharp.Commands;

public class CustomCommand : CommandBase
{
    public string Command;

    public CustomCommand(string command)
    {
        Command = command;
    }

    protected override string GenerateInternal()
    {
        return Command.TrimStart('/');
    }
}