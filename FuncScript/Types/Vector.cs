using SlowLang.Engine.Statements;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript.Types;

public class Vector : FuncScriptValue
{
    public FuncSharp.Core.Vector Value { get; set; }
    
    public Vector(FuncSharp.Core.Vector value)
    {
        Value = value;
    }

    public override string Generate()
    {
        return $"[{Value.X}d,{Value.Y}d,{Value.Z}d]";
    }
    
    public static bool TryParse(TokenList list, out Vector result)
    {
        if (!list.StartsWith(TokenType.Keyword) || list.Peek().RawContent != "vector")
        {
            goto error;
        }
        // Remove the vector keyword
        list.Pop();
        
        if(list.Peek().RawContent != "[")
        {
            goto error;
        }
        // Remove the opening bracket
        list.Pop();

        Statement xStatement = Statement.Parse(ref list)!;
        Statement yStatement = Statement.Parse(ref list)!;
        Statement zStatement = Statement.Parse(ref list)!;

        xStatement.Execute();
        
        
        return true;
        
        error:
        result = null;
        return false;
    }
}