using FuncScript.Internal;
using FuncScript.Types;
using SlowLang.Engine;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript.Statements;

public class NotOperator : Statement, IInitializable
{
    public static void Initialize()
    {
        StatementRegistration.Create<NotOperator>(TokenType.ExclamationMark).Register();
    }

    private VariableNameProvider variableNameProvider = null!;
    public override bool OnParse(ref TokenList list)
    {
        list.Pop();
        Statement statement = Statement.Parse(ref list)!;
        
        FuncScriptValue value = (FuncScriptValue) statement.Execute();

        if (!value.IsOfType<FuncBool>())
        {
            LoggingManager.LogError("Expected boolean value for not operator");
            return false;
        }

        string id = IdManager.GetDataId();
        Transpiler.MemoryTypes[id] = typeof(FuncBool);
        variableNameProvider = new(id);
        
        // invert it
        $"execute if data storage {MemoryManagement.MemoryTag} {Utils.PathToIfData("variables." + value.AsVarnameProvider(), "0b")} run data modify storage {MemoryManagement.MemoryTag} variables.{id} set value 1b".Add();
        $"execute unless data storage {MemoryManagement.MemoryTag} {Utils.PathToIfData("variables." + value.AsVarnameProvider(), "0b")} run data modify storage {MemoryManagement.MemoryTag} variables.{id} set value 0b".Add();
        
        return true;
    }

    public override Value Execute() => variableNameProvider;
}