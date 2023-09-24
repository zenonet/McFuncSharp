using System.Text.RegularExpressions;
using SlowLang.Engine.Tokens;

public static class Utils{
    
    private static string Escape(string input)
    {
        return new Regex("[\\[\\\\\\^\\$\\.\\|\\?\\*\\+\\(\\)\\{\\}\\-\\%]").Replace(input, "\\$&");
    }

    public static bool IsMathematicOperator(this TokenType t) => t is TokenType.Plus or TokenType.Minus or TokenType.Multiply or TokenType.Divide or TokenType.DoubleEquals or TokenType.GreaterThan or TokenType.LessThan;
}