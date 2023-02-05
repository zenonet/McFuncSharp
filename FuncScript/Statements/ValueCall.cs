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
    private FuncScriptValue? value;
    
    public static void Initialize()
    {
        StatementRegistration.Create<ValueCall>(TokenType.String).Register();
        StatementRegistration.Create<ValueCall>(TokenType.Int).Register();
        StatementRegistration.Create<ValueCall>(TokenType.Minus, TokenType.Int).Register();
        StatementRegistration.Create<ValueCall>(TokenType.Float).Register();
        StatementRegistration.Create<ValueCall>(TokenType.Minus, TokenType.Float).Register();
        StatementRegistration.Create<ValueCall>(TokenType.Bool).Register();
        StatementRegistration.Create<ValueCall>(x => x.Peek().RawContent is "vector_c" or "entity" or "block",TokenType.Keyword).Register();
    }


    protected override bool CutTokensManually() => true;
    
    public string Id { get; private set; }
    
    public bool IsConstantValueCall { get; private set; }
    
    public Value ConstantValue { get; private set; }
    
    protected override bool OnParse(ref TokenList tokenList)
    {
        value = Value.Parse(ref tokenList) as FuncScriptValue;

        if (value == null)
        {
            Logger.LogError("Unable to parse {token}", tokenList.Peek());
            return false;
        }

        if (value is ConstFuncScriptValue)
        {
            IsConstantValueCall = true;
            ConstantValue = value;
            return true;
        }

        Id = IdManager.GetId();

        MemoryManagement.SetVariable(Id, ((FuncScriptValue)value).Generate()).Add();

        return true;
    }

    public override Value Execute()
    {
        if (IsConstantValueCall)
            return ConstantValue;
        
        // Return the name of the Variable the value is stored in
        return new VariableNameProvider(Id);
    }
}