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

    public PokemonGender Gender { get; set; }

    public bool IsShiny { get; set; }

    public string Nature { get; set; } = "Hardy";

    public int Experience { get; set; }

    public int CurrentHp { get; set; } = 1;

    public int TemporaryHp { get; set; }

    public int? MaximumHpOverride { get; set; }

    public int Loyalty { get; set; }

    public string? HeldItem { get; set; }

    public AbilityScores AttributeIncreases { get; set; } = new() { Strength = 0, Dexterity = 0, Constitution = 0, Intelligence = 0, Wisdom = 0, Charisma = 0 };

    public AbilityScores CustomAttributes { get; set; } = new() { Strength = 0, Dexterity = 0, Constitution = 0, Intelligence = 0, Wisdom = 0, Charisma = 0 };

    public List<string> Abilities { get; set; } = [];

    public List<string> Feats { get; set; } = [];

    public List<string> Skills { get; set; } = [];

    public List<OwnedPokemonMove> Moves { get; set; } = [];

    public HashSet<PokemonStatus> Statuses { get; set; } = [];
}

public enum PokemonGender { Unspecified, Genderless, Male, Female }
public enum PokemonStatus { Asleep, Burned, Confused, Frozen, Paralyzed, Poisoned }

public sealed class OwnedPokemonMove
{
    public string Name { get; set; } = string.Empty;
    public int CurrentPowerPoints { get; set; }
}

public sealed class TrainerCharacter
{
    public string ClassName { get; set; } = "Ace Trainer";

    public AbilityScores Abilities { get; set; } = new();

    public List<string> Feats { get; set; } = [];

    public List<InventoryEntry> Inventory { get; set; } = [];

    public ManualTrainerModifiers ManualModifiers { get; set; } = new();
}

public sealed class ManualTrainerModifiers
{
    public int Attack { get; set; }
    public int Damage { get; set; }
    public int Stab { get; set; }
    public int MoveSlots { get; set; }
    public int AbilityScoreIncreases { get; set; }
    public int EvolutionLevel { get; set; }
    public int MaximumActivePokemon { get; set; } = 6;
    public AbilityScores PokemonAttributes { get; set; } = new() { Strength = 0, Dexterity = 0, Constitution = 0, Intelligence = 0, Wisdom = 0, Charisma = 0 };
    public Dictionary<string, int> TypeAttack { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> TypeDamage { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> TypeStab { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> AlwaysUseStabTypes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
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
