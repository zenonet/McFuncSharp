using SlowLang.Engine;
using SlowLang.Engine.Values;

namespace FuncScript.Types;

public abstract class FuncScriptValue : Value
{
    public virtual string Generate()
    {
        return string.Empty;
    }
}