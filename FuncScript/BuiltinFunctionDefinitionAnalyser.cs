using System.Linq.Expressions;
using System.Reflection;
using FuncScript.Types;
using SlowLang.Engine;

namespace FuncScript;

public static class BuiltinFunctionDefinitionAnalyser
{
    public static void LoadBuiltinFunctionDefinitions()
    {
        Dictionary<string, Func<FuncScriptValue[], string>> functions = new();

        IEnumerable<Type> builtinClasses = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetCustomAttribute<BuiltinClassAttribute>() != null);

        foreach (Type builtinClass in builtinClasses)
        {
            MethodInfo[] builtinFunctions = builtinClass.GetMethods();
            foreach (MethodInfo function in builtinFunctions)
            {
                if (function.IsPrivate || !function.IsStatic)
                    continue;

                ParameterInfo[] parameters = function.GetParameters();

                string name = function.GetCustomAttribute<BuiltinFunctionAttribute>()?.Name ?? function.Name;
                name = builtinClass.Name + "." + name;

                BuiltinFunctionDefinition definition = new()
                {
                    Name = name,
                    GeneratorMethod = function,
                    ParameterTypes = parameters.Select(x => x.GetCustomAttribute<BuiltinFunctionParameterAttribute>()!.Type).ToArray(),
                };

                // Add the function to the default function list (remembering the definition using a closure)
                functions.Add(name, p => FunctionWrapper(definition, p));

                continue;

                string FunctionWrapper(BuiltinFunctionDefinition definition, FuncScriptValue[] p)
                {
                    if(definition.ParameterTypes.Length != p.Length)
                    {
                        LoggingManager.LogError($"Invalid parameter count for call to {definition.Name}(). Expected {definition.ParameterTypes.Length}, got {p.Length}");
                    }
                    
                    for (int i = 0; i < definition.ParameterTypes.Length; i++)
                    {
                        if(!p[i].IsOfType(definition.ParameterTypes[i]))
                        {
                            LoggingManager.LogError($"Invalid parameter type at parameter {i+1} of call to {definition.Name}(). Expected {definition.ParameterTypes[i].Name}, got {p[i].GetFuncTypeName()}");
                        }
                    }

                    return (string) definition.GeneratorMethod.Invoke(null, p);
                }
            }
        }

        foreach (var func in functions)
        {
            Resources.Functions.Add(func.Key, func.Value);
        }
    }
}

public class BuiltinFunctionAttribute : Attribute
{
    public string Name { get; }

    public BuiltinFunctionAttribute(string name)
    {
        Name = name;
    }
}

[AttributeUsage(AttributeTargets.Parameter)]
public class BuiltinFunctionParameterAttribute : Attribute
{
    public BuiltinFunctionParameterAttribute(Type type)
    {
        Type = type;
    }

    public Type Type { get; }
}

[AttributeUsage(AttributeTargets.Class)]
public class BuiltinClassAttribute : Attribute
{
}

public class BuiltinFunctionDefinition
{
    public string Name;
    public MethodInfo GeneratorMethod;
    public Type[] ParameterTypes;
}