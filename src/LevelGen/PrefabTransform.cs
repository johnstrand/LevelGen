namespace LevelGen;

public readonly record struct PrefabTransform(int QuarterTurnsClockwise, bool MirrorHorizontally)
{
    public int QuarterTurnsClockwise { get; } = NormalizeQuarterTurns(QuarterTurnsClockwise);

    public bool MirrorHorizontally { get; } = MirrorHorizontally;

    public static PrefabTransform Identity { get; } = new(0, false);

    private static int NormalizeQuarterTurns(int quarterTurnsClockwise)
    {
        var normalized = quarterTurnsClockwise % 4;
        return normalized < 0 ? normalized + 4 : normalized;
    }
}
