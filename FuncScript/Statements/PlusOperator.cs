using SlowLang.Engine;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;

namespace FuncScript.Statements;

public class PlusOperator : Operator, IInitializable
{
    public static void Initialize()
    {
        //StatementExtensionRegistration.CreateStatementExtensionRegistration<Statement, PlusOperator>(Tok);
    }
}