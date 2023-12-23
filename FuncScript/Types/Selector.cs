using SlowLang.Engine.Values;

namespace FuncScript.Types;

public class Selector : Value
{
    public string String { get; set; }
    public static string GetKeyword() => "Selector";
}