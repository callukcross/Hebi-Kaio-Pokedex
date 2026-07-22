using System.Text.Json;
using System.Text.Json.Serialization;
using HebiKaio.Core.Content;

namespace HebiKaio.Core.Trainer;

public sealed class TrainerRulesCatalog
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly IReadOnlyDictionary<int, LevelRule> _levels;

    private TrainerRulesCatalog(IReadOnlyDictionary<string, string> feats, IReadOnlyDictionary<string, string> items, IReadOnlyDictionary<int, LevelRule> levels)
    {
        Feats = feats;
        Items = items;
        _levels = levels;
    }

    public IReadOnlyDictionary<string, string> Feats { get; }
    public IReadOnlyDictionary<string, string> Items { get; }
    public IReadOnlyList<string> Classes { get; } = ["Ace Trainer", "Capture Specialist", "Type Specialist", "Researcher", "Breeder", "Ranger"];

    public static TrainerRulesCatalog Load(string dataDirectory, string? customModuleDirectory = null)
    {
        var feats = new Dictionary<string, string>(LoadDescriptions(Path.Combine(dataDirectory, "feats.json"), "Description"), StringComparer.OrdinalIgnoreCase);
        var items = new Dictionary<string, string>(LoadDescriptions(Path.Combine(dataDirectory, "items.json"), "Effect"), StringComparer.OrdinalIgnoreCase);
        foreach (var (module, _) in CustomContentService.LoadModules(customModuleDirectory))
        {
            Merge(module.Feats!, feats, module.Name, "feat");
            Merge(module.Items!, items, module.Name, "item");
        }
        var levels = JsonSerializer.Deserialize<Dictionary<int, LevelRule>>(File.ReadAllText(Path.Combine(dataDirectory, "leveling.json")), SerializerOptions) ?? [];
        return new TrainerRulesCatalog(feats, items, levels);
    }

    public LevelRule GetLevel(int level) => _levels.TryGetValue(Math.Clamp(level, 1, 20), out var rule) ? rule : new LevelRule();

    private static IReadOnlyDictionary<string, string> LoadDescriptions(string path, string fieldName)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.EnumerateObject().ToDictionary(
            property => property.Name,
            property => property.Value.TryGetProperty(fieldName, out var value) ? value.GetString() ?? string.Empty : string.Empty,
            StringComparer.OrdinalIgnoreCase);
    }

    private static void Merge(IReadOnlyDictionary<string, string> additions, IDictionary<string, string> destination, string moduleName, string kind)
    {
        foreach (var entry in additions)
        {
            if (destination.ContainsKey(entry.Key))
                throw new InvalidDataException($"Module '{moduleName}' conflicts with the existing {kind} '{entry.Key}'.");
            destination.Add(entry.Key, entry.Value);
        }
    }
}

public sealed class LevelRule
{
    [JsonPropertyName("prof")]
    public int ProficiencyBonus { get; init; }

    [JsonPropertyName("STAB")]
    public int StabBonus { get; init; }

    [JsonPropertyName("ASI")]
    public int AbilityScoreIncreases { get; init; }

    [JsonPropertyName("exp")]
    public int ExperienceThreshold { get; init; }
}
