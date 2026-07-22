namespace HebiKaio.Core.Profiles;

public sealed class TrainerProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public int TrainerLevel { get; set; } = 1;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public Dictionary<int, PokedexEntryState> Pokedex { get; set; } = [];

    public List<OwnedPokemon> Pokemon { get; set; } = [];

    public List<Guid> PartyPokemonIds { get; set; } = [];
}

public enum PokedexEntryState
{
    Unknown = 0,
    Seen = 1,
    Caught = 2
}

public sealed class OwnedPokemon
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int SpeciesNumber { get; set; }

    public string SpeciesName { get; set; } = string.Empty;

    public string? Nickname { get; set; }

    public string? Form { get; set; }

    public int Level { get; set; } = 1;

    public string? CustomImagePath { get; set; }
}
