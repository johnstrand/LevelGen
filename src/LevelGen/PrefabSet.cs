namespace LevelGen;

public sealed class PrefabSet : IReadOnlyList<PrefabDefinition>
{
    private readonly IReadOnlyList<PrefabDefinition> _prefabs;

    public PrefabSet(IEnumerable<PrefabDefinition> prefabs)
    {
        ArgumentNullException.ThrowIfNull(prefabs);

        _prefabs = [.. prefabs];
        var duplicateName = _prefabs
            .GroupBy(prefab => prefab.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateName is not null)
        {
            throw new ArgumentException($"Prefab names must be unique. Duplicate: '{duplicateName.Key}'.", nameof(prefabs));
        }
    }

    public int Count => _prefabs.Count;

    public PrefabDefinition this[int index] => _prefabs[index];

    public IEnumerator<PrefabDefinition> GetEnumerator() => _prefabs.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
