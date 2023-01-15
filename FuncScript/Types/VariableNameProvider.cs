using SlowLang.Engine.Values;

namespace FuncScript.Types;

public class VariableNameProvider : Value
{
    public VariableNameProvider(string variableName)
    {
        VariableName = variableName;
    }

    public string VariableName { get; set; }
}