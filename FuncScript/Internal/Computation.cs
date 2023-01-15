namespace FuncScript.Internal;

public static class Computation
{
    public const string ComputationScoreboard = "funcscript_computation";

    public static string Initialize()
    {
        return "scoreboard objectives add " + ComputationScoreboard + " dummy";
    }

    public static string Copy(string source, string target)
    {
        return "scoreboard players operation " + target + " " + ComputationScoreboard + " = " + source + " " + ComputationScoreboard;
    }

    public static string Add(string addent1, string addent2, string output)
    {
        return Copy(addent1, output) +
               "\nscoreboard players operation " + output + " " + ComputationScoreboard + " += " + addent2 + " " + ComputationScoreboard;
    }
    public static string Subtract(string minuend, string subtractor, string output)
    {
        return Copy(minuend, output) +
               "\nscoreboard players operation " + output + " " + ComputationScoreboard + " -= " + subtractor + " " + ComputationScoreboard;
    }
    public static string Multiply(string factor1, string factor2, string output)
    {
        return Copy(factor1, output) +
               "\nscoreboard players operation " + output + " " + ComputationScoreboard + " *= " + factor2 + " " + ComputationScoreboard;
    }
    public static string Divide(string divident, string divisor, string output)
    {
        return Copy(divident, output) +
               "\nscoreboard players operation " + output + " " + ComputationScoreboard + " /= " + divisor + " " + ComputationScoreboard;
    }
}