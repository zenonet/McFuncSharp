using System.Reflection;
using Microsoft.Extensions.Logging;
using SlowLang.Engine;
using SlowLang.Engine.Values;

namespace FuncScript.Types;

public abstract class FuncScriptValue : Value
{
    public virtual string Generate()
    {
        return string.Empty;
    }

    public bool IsOfType(Type type)
    {
        if (this is VariableNameProvider v)
        {
            if (v.VariableName.Contains('.'))
            {
                Type t = GetTypeOfPropertyChain(v.VariableName);

                return type == t;
            }
            
            if (!Transpiler.MemoryTypes.ContainsKey(v.VariableName))
                Logger.LogError($"The type of the variable {v.VariableName} is not defined. {this}");
            return Transpiler.MemoryTypes[v.VariableName] == type;
        }

        return this.GetType() == type;
    }

    public bool IsOfType<T>() where T : FuncScriptValue
    {
        return IsOfType(typeof(T));
    }

    private static Type GetTypeOfPropertyChain(string v)
    {
        string[] parts = v.Split('.');

        Type currentType = Transpiler.MemoryTypes[parts[0]];
        for (int i = 1; i < parts.Length; i++)
        {
            PropertyInfo? propertyListProperty = currentType.GetProperties().FirstOrDefault(p => Attribute.IsDefined(p, typeof(FuncPropertyListAttribute)));

            
            
            if (propertyListProperty == null)
            {
                LoggingManager.LogError($"Class {currentType.Name} does not have a property called {parts[i]}");
                return null;
            }

            // Query the type of the property in the types property dictionary
            IDictionary<string, Type> propertyDictionary = propertyListProperty.GetValue(null) as IDictionary<string, Type>;
            
           
                // The type of the next property in the chain was successfully found
                currentType = propertyDictionary[parts[i]];
                continue;
            

            throw new($"The property list of {currentType} does not have the correct signature.");
        }

        return currentType;
    }

    public string GetFuncTypeName()
    {
        // TODO: Make use GetTypeOfPropertyChain()
        if (this is VariableNameProvider v)
        {
            if (!Transpiler.MemoryTypes.ContainsKey(v.VariableName))
                Logger.LogError($"The type of the variable {v.VariableName} is not defined. {this}");
            return Transpiler.MemoryTypes[v.VariableName].Name;
        }
        return this.GetType().Name;
    }
}