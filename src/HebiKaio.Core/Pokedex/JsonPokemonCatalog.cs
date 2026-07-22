using System.Text.Json;
using System.Text.Json.Serialization;

namespace HebiKaio.Core.Pokedex;

public sealed class JsonPokemonCatalog : IPokemonCatalog
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IReadOnlyList<PokemonSpecies> _species;

    private JsonPokemonCatalog(IReadOnlyList<PokemonSpecies> species)
    {
        _species = species;
    }

    public static JsonPokemonCatalog Load(string dataDirectory)
    {
        if (string.IsNullOrWhiteSpace(dataDirectory))
            throw new ArgumentException("A data directory is required.", nameof(dataDirectory));

        var filterPath = Path.Combine(dataDirectory, "filter_data.json");
        var evolutionPath = Path.Combine(dataDirectory, "evolve.json");
        if (!File.Exists(filterPath) || !File.Exists(evolutionPath))
            throw new FileNotFoundException("The Pokédex data files could not be found.", dataDirectory);

        var filterData = JsonSerializer.Deserialize<Dictionary<string, FilterRecord>>(
            File.ReadAllText(filterPath), SerializerOptions) ?? [];
        var evolutionData = JsonSerializer.Deserialize<Dictionary<string, EvolutionRecord>>(
            File.ReadAllText(evolutionPath), SerializerOptions) ?? [];

        var species = filterData.Select(entry =>
        {
            evolutionData.TryGetValue(entry.Key, out var evolution);
            return new PokemonSpecies
            {
                Number = entry.Value.Index,
                Name = entry.Key,
                Types = entry.Value.Types ?? [],
                Region = GetRegion(entry.Value.Index),
                EvolutionStage = GetEvolutionStage(evolution),
                SpeciesRating = entry.Value.SpeciesRating,
                MinimumWildLevel = entry.Value.MinimumWildLevel,
                EvolvesInto = evolution?.Into ?? []
            };
        })
        .OrderBy(item => item.Number)
        .ThenBy(item => item.Name)
        .ToList();

        return new JsonPokemonCatalog(species);
    }

    public IReadOnlyList<PokemonSpecies> GetAll() => _species;

    public PokemonSpecies? FindByNumber(int number) => _species.FirstOrDefault(item => item.Number == number);

    private static EvolutionStage GetEvolutionStage(EvolutionRecord? evolution)
    {
        if (evolution is null || evolution.TotalStages <= 1)
            return EvolutionStage.Standalone;

        return evolution.CurrentStage switch
        {
            <= 1 => EvolutionStage.Basic,
            2 => EvolutionStage.StageOne,
            _ => EvolutionStage.StageTwo
        };
    }

    private static string GetRegion(int number) => number switch
    {
        <= 151 => "Kanto",
        <= 251 => "Johto",
        <= 386 => "Hoenn",
        <= 493 => "Sinnoh",
        <= 649 => "Unova",
        <= 721 => "Kalos",
        <= 809 => "Alola",
        _ => "Unknown"
    };

    private sealed class FilterRecord
    {
        [JsonPropertyName("index")]
        public int Index { get; init; }

        [JsonPropertyName("Type")]
        public List<string>? Types { get; init; }

        [JsonPropertyName("SR")]
        public double SpeciesRating { get; init; }

        [JsonPropertyName("MIN LVL FD")]
        public int MinimumWildLevel { get; init; }
    }

    private sealed class EvolutionRecord
    {
        [JsonPropertyName("current_stage")]
        public int CurrentStage { get; init; }

        [JsonPropertyName("total_stages")]
        public int TotalStages { get; init; }

        [JsonPropertyName("into")]
        public List<string>? Into { get; init; }
    }
}
