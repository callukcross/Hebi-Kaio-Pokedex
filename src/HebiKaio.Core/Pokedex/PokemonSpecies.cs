namespace HebiKaio.Core.Pokedex;

public sealed class PokemonSpecies
{
    public int Number { get; init; }

    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<string> Types { get; init; } = [];

    public string Region { get; init; } = string.Empty;

    public EvolutionStage EvolutionStage { get; init; }
}

public enum EvolutionStage
{
    Basic,
    StageOne,
    StageTwo,
    Standalone
}
