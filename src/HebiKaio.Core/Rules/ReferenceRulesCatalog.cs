using System.Text.Json;

namespace HebiKaio.Core.Rules;

public sealed class ReferenceRulesCatalog
{
    private readonly IReadOnlyDictionary<string, PokemonRule> _pokemon;
    private readonly IReadOnlyDictionary<string, MoveRule> _moves;

    private ReferenceRulesCatalog(
        IReadOnlyDictionary<string, PokemonRule> pokemon,
        IReadOnlyDictionary<string, MoveRule> moves,
        IReadOnlyDictionary<string, string> abilities,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> natures,
        IReadOnlyDictionary<int, PokedexExtra> pokedexDetails)
    {
        _pokemon = pokemon;
        _moves = moves;
        Abilities = abilities;
        Natures = natures;
        PokedexDetails = pokedexDetails;
    }

    public IReadOnlyDictionary<string, string> Abilities { get; }
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> Natures { get; }
    public IReadOnlyDictionary<int, PokedexExtra> PokedexDetails { get; }
    public IReadOnlyCollection<PokemonRule> Pokemon => _pokemon.Values.ToList();
    public IReadOnlyCollection<MoveRule> Moves => _moves.Values.ToList();

    public PokemonRule? FindPokemon(string name) => _pokemon.GetValueOrDefault(name);
    public MoveRule? FindMove(string name) => _moves.GetValueOrDefault(name);

    public static ReferenceRulesCatalog Load(string dataDirectory)
    {
        var pokemon = Directory.GetFiles(Path.Combine(dataDirectory, "pokemon"), "*.json")
            .Select(LoadPokemon)
            .ToDictionary(rule => rule.Name, StringComparer.OrdinalIgnoreCase);
        var moves = Directory.GetFiles(Path.Combine(dataDirectory, "moves"), "*.json")
            .Select(LoadMove)
            .ToDictionary(rule => rule.Name, StringComparer.OrdinalIgnoreCase);
        var abilities = LoadDescriptions(Path.Combine(dataDirectory, "abilities.json"), "Description");
        var natures = LoadNatures(Path.Combine(dataDirectory, "natures.json"));
        var details = LoadPokedexDetails(Path.Combine(dataDirectory, "pokedex_extra.json"));
        return new ReferenceRulesCatalog(pokemon, moves, abilities, natures, details);
    }

    private static PokemonRule LoadPokemon(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;
        var moves = root.TryGetProperty("Moves", out var moveData) ? moveData : default;
        var levelMoves = new Dictionary<int, IReadOnlyList<string>>();
        if (moves.ValueKind == JsonValueKind.Object && moves.TryGetProperty("Level", out var levels))
            foreach (var level in levels.EnumerateObject())
                if (int.TryParse(level.Name, out var number)) levelMoves[number] = Strings(level.Value);
        return new PokemonRule
        {
            Name = Path.GetFileNameWithoutExtension(path),
            Number = Integer(root, "index"),
            ArmorClass = Integer(root, "AC"),
            BaseHp = Integer(root, "HP"),
            HitDie = Integer(root, "Hit Dice"),
            MinimumLevel = Integer(root, "MIN LVL FD"),
            SpeciesRating = Number(root, "SR"),
            Types = Strings(root, "Type"),
            Abilities = Strings(root, "Abilities"),
            HiddenAbility = Text(root, "Hidden Ability"),
            Skills = Strings(root, "Skill"),
            SavingThrows = Strings(root, "saving_throws"),
            Size = Text(root, "size") ?? string.Empty,
            WalkingSpeed = Integer(root, "WSp"),
            FlyingSpeed = Integer(root, "Fsp"),
            SwimmingSpeed = Integer(root, "Ssp"),
            BurrowingSpeed = Integer(root, "Bsp"),
            ClimbingSpeed = Integer(root, "Csp"),
            EvolvesInto = Text(root, "Evolve"),
            Attributes = root.TryGetProperty("attributes", out var attributes)
                ? attributes.EnumerateObject().ToDictionary(item => item.Name, item => item.Value.GetInt32(), StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
            StartingMoves = moves.ValueKind == JsonValueKind.Object && moves.TryGetProperty("Starting Moves", out var starting) ? Strings(starting) : [],
            LevelMoves = levelMoves,
            EggMoves = moves.ValueKind == JsonValueKind.Object && moves.TryGetProperty("egg", out var egg) ? Strings(egg) : [],
            TechnicalMachines = moves.ValueKind == JsonValueKind.Object && moves.TryGetProperty("TM", out var tm) ? tm.EnumerateArray().Select(value => value.GetInt32()).ToList() : []
        };
    }

    private static MoveRule LoadMove(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;
        return new MoveRule
        {
            Name = Path.GetFileNameWithoutExtension(path),
            Type = Text(root, "Type") ?? string.Empty,
            Description = Text(root, "Description") ?? string.Empty,
            Scaling = Text(root, "Scaling") ?? string.Empty,
            Duration = Text(root, "Duration") ?? string.Empty,
            Range = Text(root, "Range") ?? string.Empty,
            MoveTime = Text(root, "Move Time") ?? string.Empty,
            PowerAttributes = Strings(root, "Move Power"),
            PowerPoints = Integer(root, "PP"),
            IsAttack = root.TryGetProperty("atk", out var attack) && attack.ValueKind == JsonValueKind.True
        };
    }

    private static IReadOnlyDictionary<string, string> LoadDescriptions(string path, string propertyName)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.EnumerateObject().ToDictionary(
            item => item.Name,
            item => item.Value.TryGetProperty(propertyName, out var description) ? description.GetString() ?? string.Empty : string.Empty,
            StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> LoadNatures(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.EnumerateObject().ToDictionary(
            nature => nature.Name,
            nature => (IReadOnlyDictionary<string, int>)nature.Value.EnumerateObject().ToDictionary(value => value.Name, value => value.Value.GetInt32(), StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyDictionary<int, PokedexExtra> LoadPokedexDetails(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.EnumerateObject().ToDictionary(
            item => int.Parse(item.Name),
            item => new PokedexExtra(Text(item.Value, "genus") ?? string.Empty, Text(item.Value, "flavor") ?? string.Empty, Number(item.Value, "height"), Number(item.Value, "weight")));
    }

    private static int Integer(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var value)) return 0;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)) return number;
        return value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out number) ? number : 0;
    }

    private static double Number(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var value)) return 0;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number)) return number;
        return value.ValueKind == JsonValueKind.String && double.TryParse(value.GetString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out number) ? number : 0;
    }
    private static string? Text(JsonElement root, string name) => root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    private static IReadOnlyList<string> Strings(JsonElement root, string name) => root.TryGetProperty(name, out var value) ? Strings(value) : [];
    private static IReadOnlyList<string> Strings(JsonElement value) => value.ValueKind == JsonValueKind.Array ? value.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.String).Select(item => item.GetString()!).ToList() : [];
}

public sealed class PokemonRule
{
    public string Name { get; init; } = string.Empty;
    public int Number { get; init; }
    public int ArmorClass { get; init; }
    public int BaseHp { get; init; }
    public int HitDie { get; init; }
    public int MinimumLevel { get; init; }
    public double SpeciesRating { get; init; }
    public IReadOnlyList<string> Types { get; init; } = [];
    public IReadOnlyList<string> Abilities { get; init; } = [];
    public string? HiddenAbility { get; init; }
    public IReadOnlyList<string> Skills { get; init; } = [];
    public IReadOnlyList<string> SavingThrows { get; init; } = [];
    public string Size { get; init; } = string.Empty;
    public int WalkingSpeed { get; init; }
    public int FlyingSpeed { get; init; }
    public int SwimmingSpeed { get; init; }
    public int BurrowingSpeed { get; init; }
    public int ClimbingSpeed { get; init; }
    public string? EvolvesInto { get; init; }
    public IReadOnlyDictionary<string, int> Attributes { get; init; } = new Dictionary<string, int>();
    public IReadOnlyList<string> StartingMoves { get; init; } = [];
    public IReadOnlyDictionary<int, IReadOnlyList<string>> LevelMoves { get; init; } = new Dictionary<int, IReadOnlyList<string>>();
    public IReadOnlyList<string> EggMoves { get; init; } = [];
    public IReadOnlyList<int> TechnicalMachines { get; init; } = [];
}

public sealed class MoveRule
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Scaling { get; init; } = string.Empty;
    public string Duration { get; init; } = string.Empty;
    public string Range { get; init; } = string.Empty;
    public string MoveTime { get; init; } = string.Empty;
    public IReadOnlyList<string> PowerAttributes { get; init; } = [];
    public int PowerPoints { get; init; }
    public bool IsAttack { get; init; }
}

public sealed record PokedexExtra(string Genus, string FlavorText, double Height, double Weight);
