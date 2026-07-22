namespace HebiKaio.Core.Profiles;

public sealed class PokemonDraft
{
    public int SpeciesNumber { get; init; }

    public string SpeciesName { get; init; } = string.Empty;

    public string? Nickname { get; init; }

    public string? Form { get; init; }

    public int Level { get; init; } = 1;

    public string? CustomImagePath { get; init; }
}
