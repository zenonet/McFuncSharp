using FuncSharp.Core;

namespace FuncSharp.Commands;

public class SetBlock : CommandBase
{
    public Vector Position;
    public Block Block;

    public SetBlock(Vector position, Block block)
    {
        Position = position;
        Block = block;
    }

    public override string Generate()
    {
        return $"setblock {Position} {Block}";
    }
}