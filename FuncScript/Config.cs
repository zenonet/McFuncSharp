namespace FuncScript;

public class Config
{
    public ReloadBehavior ReloadBehavior { get; set; } = ReloadBehavior.DetachOld;
}

/// <summary>
/// Specifies the behavior of the datapack when it is reloaded and there is stuff from the previous reload.
/// </summary>
public enum ReloadBehavior
{
    /// <summary>
    /// Kills all entities from the previous load.
    /// </summary>
    KillOld,
    
    /// <summary>
    /// Just keeps the old entities.
    /// Warning: this causes id conflicts and therefore unwanted behavior.
    /// </summary>
    KeepOld,
    
    /// <summary>
    /// Keeps the old entities but remove their ids
    /// </summary>
    DetachOld,
}