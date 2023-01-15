using FuncScript.Internal;
using FuncScript.Types;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;

namespace FuncScript.Statements;

public class VariableSetter : Statement, IInitializable
{
    public string variableName;

    public static void Initialize()
    {
        StatementRegistration.Create<VariableSetter>(TokenType.Keyword, TokenType.Equals).Register();
    }

    protected override bool CutTokensManually() => true;

    protected override bool OnParse(ref TokenList list)
    {
        variableName = list.Pop().RawContent;
        list.Pop();
        Statement? value = Parse(ref list);

        if (value == null)
            return false;

        MemoryManagement.MoveVariable(((VariableNameProvider) value.Execute()).VariableName, variableName).Add();
        VariableCall.Variables.Add(variableName);
        return true;
    }
}