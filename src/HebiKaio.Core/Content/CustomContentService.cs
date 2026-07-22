using System.Text.Json;
using System.Text.RegularExpressions;

namespace HebiKaio.Core.Content;

public sealed class CustomContentService
{
    private static readonly Regex ValidId = new("^[a-z0-9][a-z0-9.-]{1,49}$", RegexOptions.Compiled);
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };
    private readonly string _moduleDirectory;

    public CustomContentService(string moduleDirectory) => _moduleDirectory = Path.GetFullPath(moduleDirectory);

    public IReadOnlyList<ContentModuleSummary> GetInstalledModules() =>
        LoadModules(_moduleDirectory).Select(ToSummary).OrderBy(module => module.Name).ToList();

    public ContentModuleSummary Install(string sourcePath)
    {
        var module = ReadAndValidate(sourcePath);
        Directory.CreateDirectory(_moduleDirectory);
        var destination = Path.Combine(_moduleDirectory, module.Id + ".json");
        var temporary = destination + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(module, Options));
        File.Move(temporary, destination, overwrite: true);
        return ToSummary(module, destination);
    }

    public void Remove(string moduleId)
    {
        if (!ValidId.IsMatch(moduleId ?? string.Empty))
            throw new ArgumentException("A valid module id is required.", nameof(moduleId));
        var path = Path.Combine(_moduleDirectory, moduleId + ".json");
        if (File.Exists(path))
            File.Delete(path);
    }

    public static IReadOnlyList<(CustomContentModule Module, string Path)> LoadModules(string? directory)
    {
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            return [];
        var modules = new List<(CustomContentModule, string)>();
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in Directory.GetFiles(directory, "*.json").OrderBy(path => path))
        {
            var module = ReadAndValidate(path);
            if (!ids.Add(module.Id))
                throw new InvalidDataException($"Duplicate custom module id '{module.Id}'.");
            modules.Add((module, path));
        }
        return modules;
    }

    private static CustomContentModule ReadAndValidate(string path)
    {
        try
        {
            var module = JsonSerializer.Deserialize<CustomContentModule>(File.ReadAllText(path), Options)
                ?? throw new InvalidDataException("The custom module is empty.");
            if (module.FormatVersion != 1)
                throw new InvalidDataException("Only custom module format version 1 is supported.");
            if (!ValidId.IsMatch(module.Id ?? string.Empty))
                throw new InvalidDataException("Module ids must be 2-50 lowercase letters, numbers, dots, or hyphens.");
            if (string.IsNullOrWhiteSpace(module.Name) || module.Name.Length > 80)
                throw new InvalidDataException("Module names must contain between 1 and 80 characters.");
            if (module.Feats is null || module.Items is null)
                throw new InvalidDataException("Module feat and item collections cannot be null.");
            if (module.Feats.Count + module.Items.Count == 0 || module.Feats.Count + module.Items.Count > 500)
                throw new InvalidDataException("Modules must contain between 1 and 500 feats or items.");
            ValidateEntries(module.Feats, "feat");
            ValidateEntries(module.Items, "item");
            return module;
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("The custom module is not valid JSON.", exception);
        }
    }

    private static void ValidateEntries(Dictionary<string, string> entries, string kind)
    {
        if (entries.Any(entry => string.IsNullOrWhiteSpace(entry.Key) || entry.Key.Length > 80 || string.IsNullOrWhiteSpace(entry.Value) || entry.Value.Length > 2000))
            throw new InvalidDataException($"Every custom {kind} needs a name (up to 80 characters) and description (up to 2000 characters).");
    }

    private static ContentModuleSummary ToSummary((CustomContentModule Module, string Path) value) => ToSummary(value.Module, value.Path);
    private static ContentModuleSummary ToSummary(CustomContentModule module, string path) =>
        new(module.Id, module.Name, module.Author, module.Feats!.Count, module.Items!.Count, path);
}
