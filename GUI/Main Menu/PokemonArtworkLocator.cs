using System;
using System.IO;
using System.Text;

namespace GUI
{
    internal static class PokemonArtworkLocator
    {
        public static string Find(string speciesName, string customImagePath = null)
        {
            if (!string.IsNullOrWhiteSpace(customImagePath) && File.Exists(customImagePath))
                return customImagePath;

            var slug = CreateSlug(speciesName);
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, "models", "Pokemon Official Art", "PokemonMainArt", slug + ".png");
                if (File.Exists(candidate))
                    return candidate;
                directory = directory.Parent;
            }

            return null;
        }

        private static string CreateSlug(string value)
        {
            var builder = new StringBuilder();
            foreach (var character in value.ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(character))
                    builder.Append(character);
                else if ((character == ' ' || character == '.' || character == ':' || character == '’' || character == '\'') && builder.Length > 0 && builder[^1] != '-')
                    builder.Append('-');
            }
            return builder.ToString().Trim('-');
        }
    }
}
