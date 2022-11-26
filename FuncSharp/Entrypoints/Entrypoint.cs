using FuncSharp.Commands;
using FuncSharp.DataPackGen;

namespace FuncSharp;

public abstract class Entrypoint
{
    public string Name { get; }
    public CommandBase[] Commands { get; set; }

    protected Entrypoint(string name, CommandBase[] commands)
    {
        Name = name;
        Commands = commands;
    }

    public virtual void GenerateFile(string dataPackPack, DataPackGenerator generator)
    {
        // Create the file
        StreamWriter writer = File.CreateText(Path.Combine(dataPackPack, "data", generator.Namespace, "functions", Name + ".mcfunction"));

        // Write the commands
        foreach (CommandBase command in Commands)
        {
            writer.WriteLine(command.Generate());
        }

        // Close the file
        writer.Close();
    }
}