namespace FuncScript.Internal;

public class IdManager
{
    private static int _id;
    public static string GetId()
    {
        return "func_id_" + (_id++);
    }
}