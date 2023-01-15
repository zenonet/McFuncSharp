using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript.Types;

public class FuncNumber : FuncScriptValue
{
    public string Value { get; }
    
    public FuncNumber(string value)
    {
        Value = value;
    }
    
    public static bool TryParse(TokenList list, out FuncNumber result)
    {
        if (!list.StartsWith(TokenType.Float) && !list.StartsWith(TokenType.Int))
        {
            result = null;
            return false;
        }

        result = new (list[0].RawContent);

        // SlowLang doesn't support d's after numbers to make them doubles, so we do that manually here
        if (list.StartsWith(TokenType.Keyword) && list.Peek().RawContent.ToLower() == "d")
            list.Pop();
        
        return true;
    }

    public override string Generate()
    {
        return Value;
    }
}