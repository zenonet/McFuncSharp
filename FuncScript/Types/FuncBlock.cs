using FuncSharp.Core;
using SlowLang.Engine;
using SlowLang.Engine.Tokens;

namespace FuncScript.Types;

public class FuncBlock : ConstFuncScriptValue
{
    public Block Value { get; }
    
    public FuncBlock(Block value)
    {
        Value = value;
    }
    
    public static bool TryParse(ref TokenList list, out FuncBlock result)
    {
        if (list.Peek().Type != TokenType.Keyword || list.Peek().RawContent != "block")
        {
            result = null;
            return false;
        }

        list.Pop();

        if (!list.StartsWith(TokenType.Dot))
        {
            result = null;
            return false;
        }

        list.Pop();
        
        if (!Enum.TryParse(list.Peek().RawContent, out Block block))
        {
            LoggingManager.LogError("Unknown entity type: " + list.Peek().RawContent);
            result = null;
            return false;
        }

        list.Pop();
        result = new (block);

        return true;
    }

    public override string Generate()
    {
        return Value.ToString();
    }
}