namespace FuncSharp.Commands;

public class Say : CommandBase
{
    public Say(string text)
    {
        Text = text;
    }

    public string Text { get; set; }

    protected override string GenerateInternal()
    {
        return $"say {Text}";
    }
}