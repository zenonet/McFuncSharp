namespace FuncSharp.Commands;

public class Kill : CommandBase
{
    public Kill(string target)
    {
        Target = target;
    }

    public string Target { get; set; }


    protected override string GenerateInternal()
    {
        return $"kill {Target}";
    }
}