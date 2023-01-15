using SlowLang.Engine.Values;

namespace FuncScript.Types;

public class Block : Value
{
    public Block(FuncSharp.Core.Block value)
    {
        Value = value;
    }

    public FuncSharp.Core.Block Value { get; }
}