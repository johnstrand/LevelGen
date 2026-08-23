namespace LevelGen.Playground;

internal static class BlocksPathResolver
{
    public static string ResolveBlocksPath(string? explicitPath, string[]? allowedDirectories = null)
    {
        allowedDirectories ??= [Environment.CurrentDirectory, AppContext.BaseDirectory];

        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            var fullPath = Path.GetFullPath(explicitPath);

            bool isAllowed = false;
            foreach (var dir in allowedDirectories)
            {
                if (IsSubPathOf(dir, fullPath))
                {
                    isAllowed = true;
                    break;
                }
            }

            if (!isAllowed)
            {
                throw new UnauthorizedAccessException($"Access to the path '{explicitPath}' is denied.");
            }

            return File.Exists(fullPath) ? fullPath : throw new FileNotFoundException("Could not find the requested blocks file.", fullPath);
        }

        foreach (var startDirectory in allowedDirectories)
        {
            var directory = new DirectoryInfo(Path.GetFullPath(startDirectory));
            while (directory is not null)
            {
                var candidate = Path.Combine(directory.FullName, "blocks.txt");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }
        }

        throw new FileNotFoundException("Could not locate blocks.txt. Pass a path with --blocks <path>.");
    }

    public static bool IsSubPathOf(string basePath, string targetPath)
    {
        var fullBasePath = Path.GetFullPath(basePath);
        var fullTargetPath = Path.GetFullPath(targetPath);

        var relativePath = Path.GetRelativePath(fullBasePath, fullTargetPath);

        if (Path.IsPathRooted(relativePath))
        {
            return false;
        }

        if (relativePath == ".." ||
            relativePath.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
            relativePath.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal))
        {
            return false;
        }

        return true;
    }
}
