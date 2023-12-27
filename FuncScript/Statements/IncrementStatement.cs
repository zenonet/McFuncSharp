using FuncScript.Internal;
using FuncScript.Types;
using SlowLang.Engine;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript.Statements;

public class IncrementStatement : StatementExtension, IInitializable
{
    public static void Initialize()
    {
        StatementExtensionRegistration.CreateStatementExtensionRegistration<VariableCall, IncrementStatement>(
            TokenType.Plus, TokenType.Plus
        ).Register();
        StatementExtensionRegistration.CreateStatementExtensionRegistration<VariableCall, IncrementStatement>(
            TokenType.Minus, TokenType.Minus
        ).Register();
    }

    protected override bool CutTokensManually()
    {
        return true;
    }

    private VariableNameProvider variableNameProvider;

    private Operator? Operator;

    public override bool OnParse(ref TokenList list, Statement baseStatement)
    {
        VariableCall baseVariableCall = (VariableCall) baseStatement;

        TokenType operation = list.Pop().Type;
        list.Pop();

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
        variableNameProvider = new(baseVariableCall.VariableName);
        return true;
    }

    public override Value Execute()
    {
        return variableNameProvider;
    }
}