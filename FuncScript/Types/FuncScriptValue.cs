using SlowLang.Engine;
using SlowLang.Engine.Values;

namespace FuncScript.Types;

public abstract class FuncScriptValue : Value
{
    public virtual string Generate()
    {
        return string.Empty;
    }

    public bool IsOfType(Type value)
    {
        if (this is VariableNameProvider v)
        {
            if (!Transpiler.MemoryTypes.ContainsKey(v.VariableName))
                Logger.LogError($"The type of the variable {v.VariableName} is not defined. {this}");
            return Transpiler.MemoryTypes[v.VariableName] == value;
        }

        return this.GetType() == value;
    }

    public bool IsOfType<T>() where T : FuncScriptValue
    {
        if (this is VariableNameProvider v)
        {
            if (!Transpiler.MemoryTypes.ContainsKey(v.VariableName))
                Logger.LogError($"The type of the variable {v.VariableName} is not defined. {this}");
            return Transpiler.MemoryTypes[v.VariableName] == typeof(T);
        }

        return this is T;
    }

    public string GetFuncTypeName()
    {
        if (this is VariableNameProvider v)
        {
            if (!Transpiler.MemoryTypes.ContainsKey(v.VariableName))
                Logger.LogError($"The type of the variable {v.VariableName} is not defined. {this}");
            return Transpiler.MemoryTypes[v.VariableName].Name;
        }
        return this.GetType().Name;
    }
}