using LevelGen.Playground;

namespace LevelGen.Tests;

public sealed class BlocksPathResolverTests
{
    [Fact]
    public void ResolveBlocksPath_ThrowsUnauthorizedAccessException_WhenPathTraversesOutsideAllowedDirectories()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var allowedSubDir = Path.Combine(tempDir, "allowed");
        Directory.CreateDirectory(allowedSubDir);

        try
        {
            var secretFile = Path.Combine(tempDir, "secret.txt");
            File.WriteAllText(secretFile, "secret");

            var relativePathToSecret = Path.Combine(allowedSubDir, "..", "secret.txt");

            var ex = Assert.Throws<UnauthorizedAccessException>(() =>
                BlocksPathResolver.ResolveBlocksPath(relativePathToSecret, [allowedSubDir]));

            Assert.Contains("Access to the path", ex.Message);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void ResolveBlocksPath_ThrowsUnauthorizedAccessException_WhenAbsolutePathIsOutsideAllowedDirectories()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var allowedSubDir = Path.Combine(tempDir, "allowed");
        var outsideSubDir = Path.Combine(tempDir, "outside");
        Directory.CreateDirectory(allowedSubDir);
        Directory.CreateDirectory(outsideSubDir);

        try
        {
            var outsideFile = Path.Combine(outsideSubDir, "blocks.txt");
            File.WriteAllText(outsideFile, "> Room\n.#");

            var ex = Assert.Throws<UnauthorizedAccessException>(() =>
                BlocksPathResolver.ResolveBlocksPath(outsideFile, [allowedSubDir]));

            Assert.Contains("Access to the path", ex.Message);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void ResolveBlocksPath_ReturnsFullPath_WhenPathIsWithinAllowedDirectoryAndFileExists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var blocksFile = Path.Combine(tempDir, "blocks.txt");
            File.WriteAllText(blocksFile, "> Room\n.#");

            var resolved = BlocksPathResolver.ResolveBlocksPath(blocksFile, [tempDir]);

            Assert.Equal(Path.GetFullPath(blocksFile), resolved);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void ResolveBlocksPath_ThrowsFileNotFoundException_WhenFileDoesNotExistWithinAllowedDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var missingFile = Path.Combine(tempDir, "nonexistent.txt");

            Assert.Throws<FileNotFoundException>(() =>
                BlocksPathResolver.ResolveBlocksPath(missingFile, [tempDir]));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void IsSubPathOf_ReturnsFalse_WhenPathPrefixMatchesDirectoryNameWithoutSeparator()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), "allowed");
        var siblingDir = Path.Combine(Path.GetTempPath(), "allowed-sibling", "file.txt");

        Assert.False(BlocksPathResolver.IsSubPathOf(baseDir, siblingDir));
    }
}
