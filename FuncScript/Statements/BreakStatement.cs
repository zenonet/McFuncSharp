using FuncScript.Internal;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;

namespace FuncScript.Statements;

public class BreakStatement : Statement, IInitializable
{
    public static void Initialize()
    {
        StatementRegistration.Create<BreakStatement>(list => list.Peek().RawContent == "break", TokenType.Keyword).Register();
    }

    public override bool OnParse(ref TokenList list)
    {
        Loop loop = (Loop) Transpiler.StackTrace.Last(x => x is Loop);

        MemoryManagement.SetVariable("break_from_" + loop.LoopFunctionName, "1b").Add();
        loop.ContainsBreakStatement = true;
        return true;
    }
}