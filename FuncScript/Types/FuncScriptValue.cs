using SlowLang.Engine;
using SlowLang.Engine.Values;

namespace FuncScript.Types;

public abstract class FuncScriptValue : Value
{
    public virtual string Generate()
    {
        return string.Empty;
    }

    public string AsVarnameProvider()
    {
        if(this is not VariableNameProvider)
            LoggingManager.LogError($"Cannot use {this.GetType().Name} as a variable name provider");
        return ((VariableNameProvider) this).VariableName;
    }
}