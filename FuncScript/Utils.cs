using System.Text.RegularExpressions;
using SlowLang.Engine.Tokens;

public static class Utils{
    
    private static string Escape(string input)
    {
        return new Regex("[\\[\\\\\\^\\$\\.\\|\\?\\*\\+\\(\\)\\{\\}\\-\\%]").Replace(input, "\\$&");
    }

    public static bool IsOperator(this TokenType t) => t.IsMathematicOperator() || t.IsEqualityOperator();
    public static bool IsMathematicOperator(this TokenType t) => t is TokenType.Plus or TokenType.Minus or TokenType.Multiply or TokenType.Divide;
    public static bool IsEqualityOperator(this TokenType t) => t is TokenType.DoubleEquals or TokenType.GreaterThan or TokenType.LessThan;
    
    public static string PathToIfData(string path, string expectedValue)
    {
        // example input: "variables.ccu.i"
        // example output: "{variables:{ccu:{i:1b}}}"
        string[] parts = path.Split('.');
        string output = "{";
        for (int i = 0; i < parts.Length - 1; i++)
        {
            output += parts[i] + ":{"; // variables:{
        }
        output += parts[^1] + ":" + expectedValue;
        for (int i = 0; i < parts.Length; i++)
        {
            output += "}";
        }

        return output;
        
    }

    public static string GetDotPath(this TokenList list)
    {
        string output = "";
        for (int i = 0; i < list.List.Count; i++)
        {
            if (list.Peek(i).Type == TokenType.Dot)
            {
                output += ".";
            }
            else
            {
                output += list.Peek(i).RawContent;
            }

            list.Pop();
        }

        return output;
    }
}