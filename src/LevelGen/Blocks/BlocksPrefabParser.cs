using LevelGen.Internal;

namespace LevelGen.Blocks;

public static class BlocksPrefabParser
{
    public static PrefabSet Parse(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        return BlocksPrefabParserCore.Parse(text);
    }
}
