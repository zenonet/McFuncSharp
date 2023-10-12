using SlowLang.Engine.Values;

namespace FuncScript.Types;

public class VariableNameProvider : FuncScriptValue
{
    public VariableNameProvider(string variableName)
    {
        VariableName = variableName;
    }

    public string VariableName { get; set; }

    public override string ToString()
    {
        return VariableName;
    }
}