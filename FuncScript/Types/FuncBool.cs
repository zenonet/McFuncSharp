using SlowLang.Engine.Tokens;

namespace FuncScript.Types;

public class FuncBool : FuncScriptValue
{
    public string Value { get; }

    public FuncBool(string value)
    {
        Value = value;
    }

    public static string GetKeyword() => "bool";

    public static bool TryParse(ref TokenList list, out FuncBool result)
    {
        if (list.Peek().Type != TokenType.Bool)
        {
            result = null;
            return false;
        }

        result = new(list.Pop().RawContent == "true" ? "1b" : "0b");

        return true;
    }

    public override string Generate()
    {
        return Value;
    }
}