using FuncSharp.Core;
using SlowLang.Engine;
using SlowLang.Engine.Tokens;

namespace FuncScript.Types;

public class FuncEntity : ConstFuncScriptValue
{
    public Entity Value { get; }
    
    public FuncEntity(Entity value)
    {
        Value = value;
    }
    
    public static bool TryParse(TokenList list, out FuncEntity result)
    {
        if (list.Peek().Type != TokenType.Keyword || list.Peek().RawContent != "entity")
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
        
        if (!Enum.TryParse(list.Peek().RawContent, out Entity entity))
        {
            LoggingManager.LogError("Unknown entity type: " + list.Peek().RawContent);
            result = null;
            return false;
        }

        list.Pop();
        result = new (entity);

        return true;
    }

    public override string Generate()
    {
        return $"{Value}";
    }
}