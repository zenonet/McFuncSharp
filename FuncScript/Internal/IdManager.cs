namespace FuncScript.Internal;

public static class IdManager
{
    private static int dataId;
    public static string GetDataId()
    {
        return "func_data_id_" + (dataId++);
    }
    
    private static int entityId;
    public static string GetEntityId()
    {
        return "func_entity_id_" + (entityId++);
    }
    
    private static int functionId;
    public static string GetFunctionId()
    {
        return "func_function_id_" + (functionId++);
    }
}