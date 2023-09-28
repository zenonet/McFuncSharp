using FuncScript.Internal;
using FuncScript.Types;
using SlowLang.Engine;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript.Statements;

public class TernaryConditionalOperator : StatementExtension, IInitializable
{
    public static void Initialize()
    {
        StatementExtensionRegistration.CreateStatementExtensionRegistration<Statement, TernaryConditionalOperator>(TokenType.QuestionMark).Register();
    }


    protected override bool CutTokensManually()
    {
        return true;
    }

    VariableNameProvider variableNameProvider = null!;
    public override bool OnParse(ref TokenList list, Statement baseStatement)
    {
        list.Pop(); // Remove the question mark

        FuncScriptValue condition = (FuncScriptValue) baseStatement.Execute();

        if (!condition.IsOfType<FuncBool>())
        {
            LoggingManager.LogError("Expected condition of ternary conditional operator to be a boolean value");
            return false;
        }
        
        TokenList copy = list;
        FuncScriptValue positiveOutputVarname = null!;
        string positiveOutputCode = Transpiler.YoinkGeneratedCode(() => positiveOutputVarname = (FuncScriptValue) Statement.Parse(ref copy)!.Execute());
        
        if(!copy.StartsWith(TokenType.Colon))
        {
            LoggingManager.LogError("Expected colon after true statement in ternary conditional operator");
            return false;
        }
        copy.Pop(); // Remove the colon
        
        
        FuncScriptValue negativeOutputVarname = null!;
        string negativeOutputCode = Transpiler.YoinkGeneratedCode(() => negativeOutputVarname = (FuncScriptValue) Statement.Parse(ref copy)!.Execute());
        list = copy;
        
        if(negativeOutputVarname.GetFuncType() != positiveOutputVarname.GetFuncType())
        {
            LoggingManager.LogError("Expected both outputs of ternary conditional operator to be of the same type");
            return false;
        }
        
        string id = IdManager.GetDataId();
        variableNameProvider = new(id);
        Transpiler.MemoryTypes[id] = positiveOutputVarname.GetFuncType();
        
        $"execute if data storage {MemoryManagement.MemoryTag} {Utils.PathToIfData($"variables.{condition.AsVarnameProvider()}", "1b")} run {positiveOutputCode}".Add();
        $"execute unless data storage {MemoryManagement.MemoryTag} {Utils.PathToIfData($"variables.{condition.AsVarnameProvider()}", "1b")} run {negativeOutputCode}".Add();
        
        $"execute if data storage {MemoryManagement.MemoryTag} {Utils.PathToIfData($"variables.{condition.AsVarnameProvider()}", "1b")} run {MemoryManagement.MoveVariable(positiveOutputVarname.AsVarnameProvider(), id)}".Add();
        $"execute unless data storage {MemoryManagement.MemoryTag} {Utils.PathToIfData($"variables.{condition.AsVarnameProvider()}", "1b")} run {MemoryManagement.MoveVariable(negativeOutputVarname.AsVarnameProvider(), id)}".Add();
        
        return true;
    }

    public override Value Execute()
    {
        return variableNameProvider;
    }
}