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

    protected override bool CutTokensManually()
    {
        return true;
    }

    public override bool OnParse(ref TokenList list)
    {
        list.Pop();
        Statement statement = Statement.Parse(ref list)!;
        
        FuncScriptValue value = (FuncScriptValue) statement.Execute();

        if (!value.IsOfType<FuncBool>() && !value.IsOfType<FuncNumber>())
        {
            LoggingManager.LogError("Expected boolean or number value for not operator");
            return false;
        }
        
        string typeSuffix = value.IsOfType<FuncBool>() ? "b" : "d";

        string id = IdManager.GetDataId();
        Transpiler.MemoryTypes[id] = typeof(FuncBool);
        variableNameProvider = new(id);
        
        // invert it
        $"execute if data storage {MemoryManagement.MemoryTag} {Utils.PathToIfData("variables." + value.AsVarnameProvider(), $"0{typeSuffix}")} run data modify storage {MemoryManagement.MemoryTag} variables.{id} set value 1{typeSuffix}".Add();
        $"execute unless data storage {MemoryManagement.MemoryTag} {Utils.PathToIfData("variables." + value.AsVarnameProvider(), $"0{typeSuffix}")} run data modify storage {MemoryManagement.MemoryTag} variables.{id} set value 0{typeSuffix}".Add();
        
        return true;
    }

    public override Value Execute() => variableNameProvider;
}