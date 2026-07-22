namespace HebiKaio.Core.Profiles;

public sealed class PokemonDraft
{
    public int SpeciesNumber { get; init; }

    public string SpeciesName { get; init; } = string.Empty;

    public string? Nickname { get; init; }

    public string? Form { get; init; }

    public int Level { get; init; } = 1;

    public string? CustomImagePath { get; init; }

    public PokemonGender? Gender { get; init; }
    public bool? IsShiny { get; init; }
    public string? Nature { get; init; }
    public string? HeldItem { get; init; }
    public int? MaximumHpOverride { get; init; }
    public AbilityScores? AttributeIncreases { get; init; }
    public AbilityScores? CustomAttributes { get; init; }
    public IReadOnlyCollection<string>? Abilities { get; init; }
    public IReadOnlyCollection<string>? Feats { get; init; }
    public IReadOnlyCollection<string>? Skills { get; init; }
    public IReadOnlyCollection<string>? Moves { get; init; }
}
