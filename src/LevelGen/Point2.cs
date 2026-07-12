namespace LevelGen;

public readonly record struct Point2(int X, int Y)
{
    public static Point2 operator +(Point2 left, Point2 right) => new(left.X + right.X, left.Y + right.Y);

    public static Point2 operator -(Point2 left, Point2 right) => new(left.X - right.X, left.Y - right.Y);
}
