using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript.Types;

public class FuncString : FuncScriptValue
{
    public string Value { get; }
    
    public FuncString(string value)
    {
        Value = value;
    }
    
    public static bool TryParse(TokenList list, out FuncString result)
    {
        if (!list.StartsWith(TokenType.String))
        {
            result = null;
            return false;
        }

        result = new (list.Pop().RawContent.Trim('"'));
        
        return true;
    }

    public override string Generate()
    {
        return "\"" + Value + "\"";
    }
}