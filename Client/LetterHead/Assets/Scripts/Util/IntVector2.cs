using UnityEngine;

public struct IntVector2
{
    public bool Equals(IntVector2 other)
    {
        return x == other.x && y == other.y;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (x*397) ^ y;
        }
    }

    public int x, y;
    public static IntVector2 zero = new IntVector2(0, 0);

    public IntVector2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public IntVector2(Vector2 vector2)
    {
        this.x = (int)vector2.x;
        this.y = (int)vector2.y;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is IntVector2))
            return false;

        IntVector2 mys = (IntVector2)obj;

        return mys.x == x && mys.y == y;
    }

    public static bool operator ==(IntVector2 vec1, IntVector2 vec2)
    {
        return vec1.x == vec2.x && vec1.y == vec2.y;
    }

    public static bool operator !=(IntVector2 vec1, IntVector2 vec2)
    {
        return !(vec1 == vec2);
    }

    public static IntVector2 operator +(IntVector2 a, IntVector2 b)
    {
        return new IntVector2(a.x + b.x, a.y + b.y);
    }

    public static IntVector2 operator +(IntVector2 a, Vector2 b)
    {
        return new IntVector2(a.x + (int)b.x, a.y + (int)b.y);
    }

    public static IntVector2 operator -(IntVector2 a, IntVector2 b)
    {
        return new IntVector2(a.x - b.x, a.y - b.y);
    }

    public static IntVector2 operator -(IntVector2 a, Vector2 b)
    {
        return new IntVector2(a.x - (int)b.x, a.y - (int)b.y);
    }

    public static IntVector2 operator *(IntVector2 a, int b)
    {
        return new IntVector2(a.x * b, a.y * b);
    }
}