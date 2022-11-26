using FuncSharp.Core;

namespace FuncSharp.Commands;

public class Fill : CommandBase
{
    public Vector Start;
    public Vector End;
    public Block Block;
    public FillMode Mode;

    public Fill(Vector start, Vector end, Block block, FillMode mode = FillMode.Replace)
    {
        Start = start;
        End = end;
        Block = block;
        Mode = mode;
    }

    protected override string GenerateInternal()
    {
        return $"fill {Start} {End} {Block} {Mode.ToString().ToLower()}";
    }
}

public enum FillMode
{
    Replace,
    Destroy,
    Hollow,
    Keep,
    Outline,
}