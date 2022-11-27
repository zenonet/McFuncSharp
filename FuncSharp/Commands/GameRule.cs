namespace FuncSharp.Commands;

public class GameRule : CommandBase
{
    public GameRule(string rule, string value)
    {
        Rule = rule;
        Value = value;
    }

    protected override string GenerateInternal()
    {
        return "gamerule " + Rule + " " + Value;
    }

    public string Rule { get; set; }
    public string Value { get; set; }
}