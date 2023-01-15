using System.Linq;
using FuncScript.Internal;
using FuncScript.Types;
using Microsoft.Extensions.Logging;
using SlowLang.Engine;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace FuncScript.Statements;

public class ValueCall : Statement, IInitializable
{
    private Value? value;
    
    public static void Initialize()
    {
        StatementRegistration.Create<ValueCall>(TokenType.String).Register();
        StatementRegistration.Create<ValueCall>(TokenType.Int).Register();
        StatementRegistration.Create<ValueCall>(TokenType.Float).Register();
        StatementRegistration.Create<ValueCall>(TokenType.Bool).Register();
    }


    protected override bool CutTokensManually() => true;
    
    public string Id { get; private set; }
    
    protected override bool OnParse(ref TokenList tokenList)
    {
        value = Value.Parse(tokenList.List.Take(..1).AsTokenList());

        if (value == null)
        {
            Logger.LogError("Unable to parse {token}", tokenList.Peek());
            return false;
        }

        Id = IdManager.GetId();

        MemoryManagement.SetVariable(Id, ((FuncScriptValue)value).Generate()).Add();

        tokenList.Pop();
        return true;
    }

    public override Value Execute()
    {
        // Return the name of the Variable the value is stored in
        return new VariableNameProvider(Id);
    }
}