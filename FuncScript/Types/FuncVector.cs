using SlowLang.Engine;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript.Types;

public class FuncVector : FuncScriptValue
{
    public FuncSharp.Core.Vector Value { get; set; }
    
    public FuncVector(FuncSharp.Core.Vector value)
    {
        Value = value;
    }

    public override string Generate()
    {
        return $"[{Value.X}d,{Value.Y}d,{Value.Z}d]";
    }
    
    public static bool TryParse(ref TokenList list, out FuncVector result)
    {
        if (!list.StartsWith(TokenType.Keyword) || list.Peek().RawContent != "vector_c")
        {
            goto error;
        }
        // Remove the vector keyword
        list.Pop();
        
        if(list.Peek().Type != TokenType.OpeningSquareBrace)
        {
            goto error;
        }
        // Remove the opening bracket
        list.Pop();

        if (Parse(ref list) is not FuncNumber xValue)
        {
            LoggingManager.LogError("Invalid first parameter for vector_c. Expected constant number.");
            goto error;
        }
        list.TrimStart(TokenType.Comma);

        if (Parse(ref list) is not FuncNumber yValue)
        {
            LoggingManager.LogError("Invalid second parameter for vector_c. Expected constant number.");
            goto error;
        }
        list.TrimStart(TokenType.Comma);

        if (Parse(ref list) is not FuncNumber zValue)
        {
            LoggingManager.LogError("Invalid third parameter for vector_c. Expected constant number.");
            goto error;
        }

        result = new(new(float.Parse(xValue.Value), float.Parse(yValue.Value), float.Parse(zValue.Value)));

        if(!list.StartsWith(TokenType.ClosingSquareBrace))
        {
            LoggingManager.LogError("Invalid vector_c. Expected closing square brace.");
            goto error;
        }

        list.Pop();
        
        return true;
        
        error:
        
        result = null;
        return false;
    }
}