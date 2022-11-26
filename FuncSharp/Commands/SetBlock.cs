using FuncSharp.Core;

namespace FuncSharp.Commands;

public class SetBlock : CommandBase
{
    public Vector Position;
    public Block Block;
    public SetBlockMode Mode;

    public SetBlock(Vector position, Block block, SetBlockMode mode = SetBlockMode.Replace)
    {
        Position = position;
        Block = block;
        Mode = mode;
    }

    protected override string GenerateInternal()
    {
        return $"setblock {Position} {Block} {Mode.ToString().ToLower()}";
    }
}

public enum SetBlockMode
{
    Destroy,
    Keep,
    Replace,
}