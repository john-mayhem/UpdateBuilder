using Microsoft.Extensions.FileSystemGlobbing;

namespace UpdateBuilder.Utilities
{
    public class ExclusionMatcher(List<string> excludedExtensions, List<string> excludedFolders)
    {
        private readonly List<string> _excludedExtensions = excludedExtensions ?? [];
        private readonly List<string> _excludedFolders = excludedFolders ?? [];

        public bool ShouldExclude(string relativePath)
        {
            // Check extension
            string extension = Path.GetExtension(relativePath);
            if (_excludedExtensions.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            // Check folders
            string[] pathParts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            foreach (string folder in _excludedFolders)
            {
                if (pathParts.Any(part => part.Equals(folder, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
