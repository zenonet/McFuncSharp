using FuncScript.Internal;
using FuncScript.Types;
using SlowLang.Engine;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript.Statements;

public class PostIncrementStatement : StatementExtension, IInitializable
{
    public static void Initialize()
    {
        StatementExtensionRegistration.CreateStatementExtensionRegistration<VariableCall, PostIncrementStatement>(
            TokenType.Plus, TokenType.Plus
        ).Register();
        StatementExtensionRegistration.CreateStatementExtensionRegistration<VariableCall, PostIncrementStatement>(
            TokenType.Minus, TokenType.Minus
        ).Register();
    }

    protected override bool CutTokensManually()
    {
        return true;
    }

    private VariableNameProvider variableNameProvider;
    
    public override bool OnParse(ref TokenList list, Statement baseStatement)
    {
        VariableCall baseVariableCall = (VariableCall) baseStatement;

        TokenType operation = list.Pop().Type;
        list.Pop();

        // Copy the value to the output variable before incrementing
        string id = IdManager.GetDataId();
        Transpiler.MemoryTypes[id] = Transpiler.MemoryTypes[baseVariableCall.VariableName];
        MemoryManagement.MoveVariable(baseVariableCall.VariableName, id).Add();
        variableNameProvider = new(id);

        // TODO: Fix
        MemoryManagement.MoveToComputationScoreboard(baseVariableCall.VariableName, "a").Add();

        if (operation == TokenType.Plus)
        {
            Computation.Add("a", "one", "a").Add();
        }
        else if (operation == TokenType.Minus)
        {
            Computation.Subtract("a", "one", "a").Add();
        }

        MemoryManagement.MoveToStorage(baseVariableCall.VariableName, "a").Add();
        return true;
    }

    public override Value Execute()
    {
        return variableNameProvider;
    }
}