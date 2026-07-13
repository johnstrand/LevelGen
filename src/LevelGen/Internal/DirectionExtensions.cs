namespace LevelGen.Internal;

internal static class DirectionExtensions
{
    public static readonly Direction[] AllDirections = Enum.GetValues<Direction>();

    public static Point2 Offset(this Direction direction) =>
        direction switch
        {
            Direction.North => new Point2(0, -1),
            Direction.East => new Point2(1, 0),
            Direction.South => new Point2(0, 1),
            Direction.West => new Point2(-1, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };

    public static Direction Opposite(this Direction direction) =>
        direction switch
        {
            Direction.North => Direction.South,
            Direction.East => Direction.West,
            Direction.South => Direction.North,
            Direction.West => Direction.East,
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };

    public static Direction RotateClockwise(this Direction direction, int quarterTurnsClockwise)
    {
        var normalized = ((int)direction + quarterTurnsClockwise) % 4;
        return (Direction)(normalized < 0 ? normalized + 4 : normalized);
    }

    public static Direction MirrorHorizontally(this Direction direction) =>
        direction switch
        {
            Direction.East => Direction.West,
            Direction.West => Direction.East,
            _ => direction,
        };
}
