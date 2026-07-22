namespace HebiKaio.Core.Pokedex;

public sealed class PokedexFilter
{
    public string? SearchText { get; init; }

    public int? Number { get; init; }

    public string? Type { get; init; }

    public string? Region { get; init; }

    public EvolutionStage? EvolutionStage { get; init; }

    public IEnumerable<PokemonSpecies> Apply(IEnumerable<PokemonSpecies> species)
    {
        ArgumentNullException.ThrowIfNull(species);
        var query = species;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.Trim();
            query = query.Where(item => item.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (Number is { } number)
            query = query.Where(item => item.Number == number);

        if (!string.IsNullOrWhiteSpace(Type))
            query = query.Where(item => item.Types.Any(type => string.Equals(type, Type.Trim(), StringComparison.OrdinalIgnoreCase)));

        if (!string.IsNullOrWhiteSpace(Region))
            query = query.Where(item => string.Equals(item.Region, Region.Trim(), StringComparison.OrdinalIgnoreCase));

        if (EvolutionStage is { } stage)
            query = query.Where(item => item.EvolutionStage == stage);

        return query.OrderBy(item => item.Number).ThenBy(item => item.Name);
    }
}
