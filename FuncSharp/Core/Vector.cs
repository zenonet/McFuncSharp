namespace FuncSharp.Core;

public class Vector
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public Vector(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vector(double x, double y, double z)
    {
        X = (float)x;
        Y = (float)y;
        Z = (float)z;
    }
    public override string ToString()
    {
        return $"{X} {Y} {Z}";
    }
    
    public static Vector operator +(Vector a, Vector b)
    {
        return new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }
    
    public static Vector operator -(Vector a, Vector b)
    {
        return new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }
    
    public static Vector operator *(Vector a, float b)
    {
        return new(a.X * b, a.Y * b, a.Z * b);
    }
    
    public static Vector operator /(Vector a, float b)
    {
        return new(a.X / b, a.Y / b, a.Z / b);
    }
}