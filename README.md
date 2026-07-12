# LevelGen

`LevelGen` is a .NET 10 library for assembling 2D tile-based dungeon layouts from reusable prefab blocks.

It includes:

- **`src\LevelGen`**: core generator library
- **`src\LevelGen.Playground`**: console playground for rerolling maps
- **`tests\LevelGen.Tests`**: xUnit tests for parser and generator behavior

## Requirements

- .NET SDK 10.0+

## Build

```powershell
dotnet build LevelGen.slnx
```

## Run the playground

From the repository root:

```powershell
dotnet run --project src\LevelGen.Playground -- --once
```

The playground will auto-discover `blocks.txt` (included at `src\LevelGen.Playground\blocks.txt`), generate a level, print the ASCII map, and exit.

Useful options:

- `--blocks <path>`: use a specific blocks file
- `--seed <int>`: deterministic generation
- `--max-prefabs <int>`: target number of room placements
- `--max-corridor-length <int>`: generated corridor max length
- `--no-loops`: disallow extra loop connections
- `--no-corridors`: disable generated corridors
- `--no-mirror`: disable mirrored prefab variants
- `--once`: generate once and exit

## Use as a library

```csharp
using LevelGen;
using LevelGen.Blocks;

var text = File.ReadAllText(@"src\LevelGen.Playground\blocks.txt");
var prefabSet = BlocksPrefabParser.Parse(text);

var result = LevelGenerator.Generate(
    prefabSet,
    new GenerationOptions
    {
        Seed = 12345,
        MaxPrefabCount = 6,
        AllowLoops = true,
        AllowGeneratedCorridors = true,
        AllowMirrorTransforms = true,
        MaxCorridorLength = 8
    });

LevelMap map = result.Map;
IReadOnlyList<PlacedPrefab> placements = result.Placements;
```

## `blocks.txt` format

Prefabs are defined in sections:

- Section header starts with `>` (for example `> Square`)
- Tile rows follow the header
- Blank line ends the prefab
- Lines starting with `/` are comments

Supported tokens:

- `#` wall
- `.` floor
- `*` connector
- `?` optional doodad marker (stored as doodad, tile is floor)
- `A`-`P` doodad marker (stored as doodad, tile is floor)
- space = empty tile

## Test

```powershell
dotnet test tests\LevelGen.Tests\LevelGen.Tests.csproj
```

Generate a Cobertura coverage report:

```powershell
dotnet test tests\LevelGen.Tests\LevelGen.Tests.csproj --collect:"XPlat Code Coverage" --settings coverage.runsettings --results-directory .\TestResults\Coverage
```
