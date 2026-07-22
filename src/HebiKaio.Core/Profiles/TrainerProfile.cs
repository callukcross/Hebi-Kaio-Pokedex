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

    public TrainerCharacter Trainer { get; set; } = new();
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

public sealed class TrainerCharacter
{
    public string ClassName { get; set; } = "Ace Trainer";

    public AbilityScores Abilities { get; set; } = new();

    public List<string> Feats { get; set; } = [];

    public List<InventoryEntry> Inventory { get; set; } = [];
}

public sealed class AbilityScores
{
    public int Strength { get; set; } = 10;
    public int Dexterity { get; set; } = 10;
    public int Constitution { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Charisma { get; set; } = 10;
}

public sealed class InventoryEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public bool IsCustom { get; set; }
}
