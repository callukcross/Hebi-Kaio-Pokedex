namespace HebiKaio.Core.Profiles;

using HebiKaio.Core.Trainer;
using HebiKaio.Core.Rules;

public sealed class ProfileService
{
    public const int MaximumPartySize = 6;

    private readonly IProfileRepository _repository;

    public ProfileService(IProfileRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public IReadOnlyList<TrainerProfile> GetProfiles() =>
        _repository.Load().Profiles
            .OrderByDescending(profile => profile.UpdatedAtUtc)
            .ToList();

    public TrainerProfile? GetActiveProfile()
    {
        var store = _repository.Load();
        return store.ActiveProfileId is { } id
            ? store.Profiles.SingleOrDefault(profile => profile.Id == id)
            : null;
    }

    public TrainerProfile CreateProfile(string name)
    {
        var normalizedName = NormalizeName(name);
        var store = _repository.Load();

        if (store.Profiles.Any(profile =>
                string.Equals(profile.Name, normalizedName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"A profile named '{normalizedName}' already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        var profile = new TrainerProfile
        {
            Name = normalizedName,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        store.Profiles.Add(profile);
        store.ActiveProfileId = profile.Id;
        _repository.Save(store);
        return profile;
    }

    public void SetActiveProfile(Guid profileId)
    {
        var store = _repository.Load();
        if (store.Profiles.All(profile => profile.Id != profileId))
            throw new KeyNotFoundException("The requested profile does not exist.");

        store.ActiveProfileId = profileId;
        _repository.Save(store);
    }

    public void RenameProfile(Guid profileId, string name)
    {
        var normalized = NormalizeName(name);
        var store = _repository.Load();
        var profile = store.Profiles.SingleOrDefault(item => item.Id == profileId)
            ?? throw new KeyNotFoundException("The requested profile does not exist.");
        if (store.Profiles.Any(item => item.Id != profileId && string.Equals(item.Name, normalized, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"A profile named '{normalized}' already exists.");
        profile.Name = normalized;
        profile.UpdatedAtUtc = DateTimeOffset.UtcNow;
        _repository.Save(store);
    }

    public void DeleteProfile(Guid profileId)
    {
        var store = _repository.Load();
        var profile = store.Profiles.SingleOrDefault(item => item.Id == profileId)
            ?? throw new KeyNotFoundException("The requested profile does not exist.");
        store.Profiles.Remove(profile);
        if (store.ActiveProfileId == profileId)
            store.ActiveProfileId = store.Profiles.OrderByDescending(item => item.UpdatedAtUtc).FirstOrDefault()?.Id;
        _repository.Save(store);
    }

    public PokedexEntryState GetPokedexState(int speciesNumber)
    {
        var profile = GetRequiredActiveProfile(_repository.Load());
        return profile.Pokedex.TryGetValue(speciesNumber, out var state)
            ? state
            : PokedexEntryState.Unknown;
    }

    public IReadOnlyDictionary<int, PokedexEntryState> GetPokedexStates()
    {
        var profile = GetRequiredActiveProfile(_repository.Load());
        return new Dictionary<int, PokedexEntryState>(profile.Pokedex);
    }

    public void SetPokedexStates(IEnumerable<int> speciesNumbers, PokedexEntryState state)
    {
        ArgumentNullException.ThrowIfNull(speciesNumbers);
        var numbers = speciesNumbers.Distinct().ToList();
        if (numbers.Any(number => number < 1)) throw new ArgumentOutOfRangeException(nameof(speciesNumbers));
        var store = _repository.Load();
        var profile = GetRequiredActiveProfile(store);
        foreach (var number in numbers)
        {
            if (state == PokedexEntryState.Unknown) profile.Pokedex.Remove(number); else profile.Pokedex[number] = state;
        }
        RefreshTrainerLevel(profile);
        profile.UpdatedAtUtc = DateTimeOffset.UtcNow;
        _repository.Save(store);
    }

    public void SetPokedexState(int speciesNumber, PokedexEntryState state)
    {
        if (speciesNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(speciesNumber));

        var store = _repository.Load();
        var profile = GetRequiredActiveProfile(store);
        if (state == PokedexEntryState.Unknown)
            profile.Pokedex.Remove(speciesNumber);
        else
            profile.Pokedex[speciesNumber] = state;

        profile.UpdatedAtUtc = DateTimeOffset.UtcNow;
        RefreshTrainerLevel(profile);
        _repository.Save(store);
    }

    public IReadOnlyList<OwnedPokemon> GetOwnedPokemon()
    {
        var profile = GetRequiredActiveProfile(_repository.Load());
        return profile.Pokemon.Select(ClonePokemon).ToList();
    }

    public IReadOnlyList<OwnedPokemon> GetPartyPokemon()
    {
        var profile = GetRequiredActiveProfile(_repository.Load());
        var byId = profile.Pokemon.ToDictionary(pokemon => pokemon.Id);
        return profile.PartyPokemonIds
            .Where(byId.ContainsKey)
            .Select(id => ClonePokemon(byId[id]))
            .ToList();
    }

    public OwnedPokemon CreatePokemon(PokemonDraft draft)
    {
        ValidateDraft(draft);
        var store = _repository.Load();
        var profile = GetRequiredActiveProfile(store);
        var pokemon = new OwnedPokemon
        {
            SpeciesNumber = draft.SpeciesNumber,
            SpeciesName = draft.SpeciesName.Trim(),
            Nickname = NormalizeOptional(draft.Nickname, 40, nameof(draft.Nickname)),
            Form = NormalizeOptional(draft.Form, 40, nameof(draft.Form)),
            Level = draft.Level,
            CustomImagePath = NormalizeOptional(draft.CustomImagePath, 500, nameof(draft.CustomImagePath))
        };
        ApplyAdvancedDraft(pokemon, draft);

        profile.Pokemon.Add(pokemon);
        profile.Pokedex[pokemon.SpeciesNumber] = PokedexEntryState.Caught;
        RefreshTrainerLevel(profile);
        profile.UpdatedAtUtc = DateTimeOffset.UtcNow;
        _repository.Save(store);
        return ClonePokemon(pokemon);
    }

    public OwnedPokemon CreatePokemon(PokemonDraft draft, ReferenceRulesCatalog rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        var created = CreatePokemon(draft);
        var store = _repository.Load();
        var profile = GetRequiredActiveProfile(store);
        var pokemon = profile.Pokemon.Single(item => item.Id == created.Id);
        PokemonRulesService.Initialize(pokemon, rules);
        ApplyAdvancedDraft(pokemon, draft, rules);
        profile.UpdatedAtUtc = DateTimeOffset.UtcNow;
        _repository.Save(store);
        return ClonePokemon(pokemon);
    }

    public OwnedPokemon UpdateBattleState(Guid pokemonId, PokemonBattleUpdate update, ReferenceRulesCatalog rules)
    {
        ArgumentNullException.ThrowIfNull(update);
        ArgumentNullException.ThrowIfNull(rules);
        var store = _repository.Load();
        var profile = GetRequiredActiveProfile(store);
        var pokemon = profile.Pokemon.SingleOrDefault(item => item.Id == pokemonId)
            ?? throw new KeyNotFoundException("The requested Pokémon does not exist.");
        var species = rules.FindPokemon(pokemon.SpeciesName)
            ?? throw new KeyNotFoundException("The Pokémon rules record does not exist.");
        var maximumHp = PokemonRulesService.GetMaximumHp(pokemon, species, rules);
        if (update.CurrentHp is { } hp) pokemon.CurrentHp = Math.Clamp(hp, 0, maximumHp);
        if (update.TemporaryHp is { } temporaryHp) pokemon.TemporaryHp = Math.Max(0, temporaryHp);
        if (update.Loyalty is { } loyalty) pokemon.Loyalty = Math.Clamp(loyalty, -3, 3);
        if (update.Experience is { } experience) pokemon.Experience = Math.Max(0, experience);
        if (update.Statuses is not null) pokemon.Statuses = update.Statuses.ToHashSet();
        if (update.MovePowerPoints is not null)
            foreach (var move in pokemon.Moves)
                if (update.MovePowerPoints.TryGetValue(move.Name, out var pp))
                    move.CurrentPowerPoints = Math.Clamp(pp, 0, rules.FindMove(move.Name)?.PowerPoints ?? Math.Max(0, pp));
        profile.UpdatedAtUtc = DateTimeOffset.UtcNow;
        _repository.Save(store);
        return ClonePokemon(pokemon);
    }

    public void HealParty(ReferenceRulesCatalog rules)
    {
        var store = _repository.Load();
        var profile = GetRequiredActiveProfile(store);
        foreach (var pokemon in profile.Pokemon.Where(item => profile.PartyPokemonIds.Contains(item.Id)))
        {
            var species = rules.FindPokemon(pokemon.SpeciesName);
            if (species is null) continue;
            pokemon.CurrentHp = PokemonRulesService.GetMaximumHp(pokemon, species, rules);
            pokemon.TemporaryHp = 0;
            pokemon.Statuses.Clear();
            foreach (var move in pokemon.Moves)
                move.CurrentPowerPoints = rules.FindMove(move.Name)?.PowerPoints ?? move.CurrentPowerPoints;
        }
        profile.UpdatedAtUtc = DateTimeOffset.UtcNow;
        _repository.Save(store);
    }

    public OwnedPokemon UpdatePokemon(Guid pokemonId, PokemonDraft draft)
    {
        ValidateDraft(draft);
        var store = _repository.Load();
        var profile = GetRequiredActiveProfile(store);
        var pokemon = profile.Pokemon.SingleOrDefault(item => item.Id == pokemonId)
            ?? throw new KeyNotFoundException("The requested Pokémon does not exist.");

        pokemon.SpeciesNumber = draft.SpeciesNumber;
        pokemon.SpeciesName = draft.SpeciesName.Trim();
        pokemon.Nickname = NormalizeOptional(draft.Nickname, 40, nameof(draft.Nickname));
        pokemon.Form = NormalizeOptional(draft.Form, 40, nameof(draft.Form));
        pokemon.Level = draft.Level;
        pokemon.CustomImagePath = NormalizeOptional(draft.CustomImagePath, 500, nameof(draft.CustomImagePath));
        ApplyAdvancedDraft(pokemon, draft);
        profile.Pokedex[pokemon.SpeciesNumber] = PokedexEntryState.Caught;
        profile.UpdatedAtUtc = DateTimeOffset.UtcNow;
        _repository.Save(store);
        return ClonePokemon(pokemon);
    }

    public OwnedPokemon UpdatePokemon(Guid pokemonId, PokemonDraft draft, ReferenceRulesCatalog rules)
    {
        var updated = UpdatePokemon(pokemonId, draft);
        var store = _repository.Load();
        var profile = GetRequiredActiveProfile(store);
        var pokemon = profile.Pokemon.Single(item => item.Id == updated.Id);
        ApplyAdvancedDraft(pokemon, draft, rules);
        profile.UpdatedAtUtc = DateTimeOffset.UtcNow;
        _repository.Save(store);
        return ClonePokemon(pokemon);
    }

    public void DeletePokemon(Guid pokemonId)
    {
        var store = _repository.Load();
        var profile = GetRequiredActiveProfile(store);
        var pokemon = profile.Pokemon.SingleOrDefault(item => item.Id == pokemonId)
            ?? throw new KeyNotFoundException("The requested Pokémon does not exist.");

        profile.Pokemon.Remove(pokemon);
        profile.PartyPokemonIds.RemoveAll(id => id == pokemonId);
        profile.UpdatedAtUtc = DateTimeOffset.UtcNow;
        _repository.Save(store);
    }

    public void AddToParty(Guid pokemonId, int? targetIndex = null)
    {
        var store = _repository.Load();
        var profile = GetRequiredActiveProfile(store);
        EnsurePokemonExists(profile, pokemonId);

        if (profile.PartyPokemonIds.Contains(pokemonId))
        {
            if (targetIndex is { } existingTarget)
                MovePartyPokemon(profile.PartyPokemonIds, pokemonId, existingTarget);
        }
        else
        {
            var maximumPartySize = Math.Clamp(profile.Trainer.ManualModifiers?.MaximumActivePokemon ?? MaximumPartySize, 1, 12);
            if (profile.PartyPokemonIds.Count >= maximumPartySize)
                throw new InvalidOperationException($"A party can contain at most {MaximumPartySize} Pokémon.");

            var insertAt = Math.Clamp(targetIndex ?? profile.PartyPokemonIds.Count, 0, profile.PartyPokemonIds.Count);
            profile.PartyPokemonIds.Insert(insertAt, pokemonId);
        }

        profile.UpdatedAtUtc = DateTimeOffset.UtcNow;
        _repository.Save(store);
    }

    public void RemoveFromParty(Guid pokemonId)
    {
        var store = _repository.Load();
        var profile = GetRequiredActiveProfile(store);
        if (profile.PartyPokemonIds.Remove(pokemonId))
        {
            profile.UpdatedAtUtc = DateTimeOffset.UtcNow;
            _repository.Save(store);
        }
    }

    public void ReorderPartyPokemon(Guid pokemonId, int targetIndex)
    {
        var store = _repository.Load();
        var profile = GetRequiredActiveProfile(store);
        if (!profile.PartyPokemonIds.Contains(pokemonId))
            throw new InvalidOperationException("Only party Pokémon can be reordered.");

        MovePartyPokemon(profile.PartyPokemonIds, pokemonId, targetIndex);
        profile.UpdatedAtUtc = DateTimeOffset.UtcNow;
        _repository.Save(store);
    }

    public TrainerCharacter GetTrainer()
    {
        var profile = GetRequiredActiveProfile(_repository.Load());
        return CloneTrainer(profile.Trainer);
    }

    public void UpdateTrainer(TrainerUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);
        if (string.IsNullOrWhiteSpace(update.ClassName))
            throw new ArgumentException("A trainer class is required.", nameof(update.ClassName));
        ValidateAbilities(update.Abilities);

        var store = _repository.Load();
        var profile = GetRequiredActiveProfile(store);
        profile.Trainer.ClassName = update.ClassName.Trim();
        profile.Trainer.Abilities = CloneAbilities(update.Abilities);
        profile.Trainer.Feats = update.Feats
            .Where(feat => !string.IsNullOrWhiteSpace(feat))
            .Select(feat => feat.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(feat => feat)
            .ToList();
        if (update.ManualModifiers is not null)
            profile.Trainer.ManualModifiers = CloneManualModifiers(update.ManualModifiers);
        profile.UpdatedAtUtc = DateTimeOffset.UtcNow;
        _repository.Save(store);
    }

    public InventoryEntry AddInventoryItem(string name, string description, int quantity = 1, bool isCustom = false)
    {
        var normalizedName = name?.Trim() ?? string.Empty;
        if (normalizedName.Length is < 1 or > 80)
            throw new ArgumentException("Item names must contain between 1 and 80 characters.", nameof(name));
        if (quantity is < 1 or > 999)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        var store = _repository.Load();
        var profile = GetRequiredActiveProfile(store);
        var existing = profile.Trainer.Inventory.FirstOrDefault(item =>
            string.Equals(item.Name, normalizedName, StringComparison.OrdinalIgnoreCase) && item.IsCustom == isCustom);
        if (existing != null)
        {
            existing.Quantity = Math.Min(999, existing.Quantity + quantity);
            profile.UpdatedAtUtc = DateTimeOffset.UtcNow;
            _repository.Save(store);
            return CloneInventoryEntry(existing);
        }

        var item = new InventoryEntry
        {
            Name = normalizedName,
            Description = description?.Trim() ?? string.Empty,
            Quantity = quantity,
            IsCustom = isCustom
        };
        profile.Trainer.Inventory.Add(item);
        profile.UpdatedAtUtc = DateTimeOffset.UtcNow;
        _repository.Save(store);
        return CloneInventoryEntry(item);
    }

    public void SetInventoryQuantity(Guid itemId, int quantity)
    {
        if (quantity is < 0 or > 999)
            throw new ArgumentOutOfRangeException(nameof(quantity));
        var store = _repository.Load();
        var profile = GetRequiredActiveProfile(store);
        var item = profile.Trainer.Inventory.SingleOrDefault(entry => entry.Id == itemId)
            ?? throw new KeyNotFoundException("The requested inventory item does not exist.");
        if (quantity == 0)
            profile.Trainer.Inventory.Remove(item);
        else
            item.Quantity = quantity;
        profile.UpdatedAtUtc = DateTimeOffset.UtcNow;
        _repository.Save(store);
    }

    public TrainerEffects GetTrainerEffects(TrainerRulesCatalog catalog)
    {
        var profile = GetRequiredActiveProfile(_repository.Load());
        return TrainerRulesService.Calculate(profile, catalog);
    }

    private static TrainerProfile GetRequiredActiveProfile(ProfileStore store)
    {
        if (store.ActiveProfileId is not { } activeId)
            throw new InvalidOperationException("Create or select a trainer profile first.");

        return store.Profiles.SingleOrDefault(profile => profile.Id == activeId)
            ?? throw new InvalidOperationException("The active trainer profile no longer exists.");
    }

    private static void ValidateDraft(PokemonDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        if (draft.SpeciesNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(draft.SpeciesNumber));
        if (string.IsNullOrWhiteSpace(draft.SpeciesName))
            throw new ArgumentException("A species is required.", nameof(draft.SpeciesName));
        if (draft.Level is < 1 or > 20)
            throw new ArgumentOutOfRangeException(nameof(draft.Level), "Pokémon levels must be between 1 and 20.");
    }

    private static void ValidateAbilities(AbilityScores abilities)
    {
        ArgumentNullException.ThrowIfNull(abilities);
        var scores = new[] { abilities.Strength, abilities.Dexterity, abilities.Constitution, abilities.Intelligence, abilities.Wisdom, abilities.Charisma };
        if (scores.Any(score => score is < 1 or > 20))
            throw new ArgumentOutOfRangeException(nameof(abilities), "Ability scores must be between 1 and 20.");
    }

    private static void RefreshTrainerLevel(TrainerProfile profile)
    {
        var caught = profile.Pokedex.Values.Count(state => state == PokedexEntryState.Caught);
        profile.TrainerLevel = Math.Max(profile.TrainerLevel, TrainerRulesService.CalculateMilestoneLevel(caught));
    }

    private static void ApplyAdvancedDraft(OwnedPokemon pokemon, PokemonDraft draft, ReferenceRulesCatalog? rules = null)
    {
        if (draft.Gender is { } gender) pokemon.Gender = gender;
        if (draft.IsShiny is { } shiny) pokemon.IsShiny = shiny;
        if (!string.IsNullOrWhiteSpace(draft.Nature)) pokemon.Nature = draft.Nature.Trim();
        if (draft.HeldItem is not null) pokemon.HeldItem = NormalizeOptional(draft.HeldItem, 80, nameof(draft.HeldItem));
        if (draft.MaximumHpOverride is { } maximumHp) pokemon.MaximumHpOverride = maximumHp <= 0 ? null : Math.Clamp(maximumHp, 1, 9999);
        if (draft.AttributeIncreases is not null) pokemon.AttributeIncreases = CloneAbilities(draft.AttributeIncreases);
        if (draft.CustomAttributes is not null) pokemon.CustomAttributes = CloneAbilities(draft.CustomAttributes);
        if (draft.Abilities is not null) pokemon.Abilities = NormalizeList(draft.Abilities, 80);
        if (draft.Feats is not null) pokemon.Feats = NormalizeList(draft.Feats, 80);
        if (draft.Skills is not null) pokemon.Skills = NormalizeList(draft.Skills, 80);
        if (draft.Moves is not null)
            pokemon.Moves = draft.Moves.Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.OrdinalIgnoreCase).Take(12)
                .Select(name => name.Trim()).Select(name => new OwnedPokemonMove { Name = name, CurrentPowerPoints = rules?.FindMove(name)?.PowerPoints ?? pokemon.Moves.FirstOrDefault(move => string.Equals(move.Name, name, StringComparison.OrdinalIgnoreCase))?.CurrentPowerPoints ?? 0 }).ToList();
    }

    private static List<string> NormalizeList(IEnumerable<string> values, int maxLength) => values
        .Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).Where(value => value.Length <= maxLength)
        .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

    private static string? NormalizeOptional(string? value, int maximumLength, string parameterName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrEmpty(normalized))
            return null;
        if (normalized.Length > maximumLength)
            throw new ArgumentException($"The value cannot exceed {maximumLength} characters.", parameterName);
        return normalized;
    }

    private static void EnsurePokemonExists(TrainerProfile profile, Guid pokemonId)
    {
        if (profile.Pokemon.All(pokemon => pokemon.Id != pokemonId))
            throw new KeyNotFoundException("The requested Pokémon does not exist.");
    }

    private static void MovePartyPokemon(List<Guid> party, Guid pokemonId, int targetIndex)
    {
        party.Remove(pokemonId);
        party.Insert(Math.Clamp(targetIndex, 0, party.Count), pokemonId);
    }

    private static OwnedPokemon ClonePokemon(OwnedPokemon pokemon) => new()
    {
        Id = pokemon.Id,
        SpeciesNumber = pokemon.SpeciesNumber,
        SpeciesName = pokemon.SpeciesName,
        Nickname = pokemon.Nickname,
        Form = pokemon.Form,
        Level = pokemon.Level,
        CustomImagePath = pokemon.CustomImagePath,
        Gender = pokemon.Gender,
        IsShiny = pokemon.IsShiny,
        Nature = pokemon.Nature,
        Experience = pokemon.Experience,
        CurrentHp = pokemon.CurrentHp,
        TemporaryHp = pokemon.TemporaryHp,
        MaximumHpOverride = pokemon.MaximumHpOverride,
        Loyalty = pokemon.Loyalty,
        HeldItem = pokemon.HeldItem,
        AttributeIncreases = CloneAbilities(pokemon.AttributeIncreases),
        CustomAttributes = CloneAbilities(pokemon.CustomAttributes),
        Abilities = pokemon.Abilities.ToList(),
        Feats = pokemon.Feats.ToList(),
        Skills = pokemon.Skills.ToList(),
        Moves = pokemon.Moves.Select(move => new OwnedPokemonMove { Name = move.Name, CurrentPowerPoints = move.CurrentPowerPoints }).ToList(),
        Statuses = pokemon.Statuses.ToHashSet()
    };

    private static TrainerCharacter CloneTrainer(TrainerCharacter trainer) => new()
    {
        ClassName = trainer.ClassName,
        Abilities = CloneAbilities(trainer.Abilities),
        Feats = trainer.Feats.ToList(),
        Inventory = trainer.Inventory.Select(CloneInventoryEntry).ToList(),
        ManualModifiers = CloneManualModifiers(trainer.ManualModifiers)
    };

    private static ManualTrainerModifiers CloneManualModifiers(ManualTrainerModifiers modifiers) => new()
    {
        Attack = modifiers.Attack,
        Damage = modifiers.Damage,
        Stab = modifiers.Stab,
        MoveSlots = modifiers.MoveSlots,
        AbilityScoreIncreases = modifiers.AbilityScoreIncreases,
        EvolutionLevel = modifiers.EvolutionLevel,
        MaximumActivePokemon = Math.Clamp(modifiers.MaximumActivePokemon, 1, 12),
        PokemonAttributes = CloneAbilities(modifiers.PokemonAttributes),
        TypeAttack = new Dictionary<string, int>(modifiers.TypeAttack, StringComparer.OrdinalIgnoreCase),
        TypeDamage = new Dictionary<string, int>(modifiers.TypeDamage, StringComparer.OrdinalIgnoreCase),
        TypeStab = new Dictionary<string, int>(modifiers.TypeStab, StringComparer.OrdinalIgnoreCase),
        AlwaysUseStabTypes = new HashSet<string>(modifiers.AlwaysUseStabTypes, StringComparer.OrdinalIgnoreCase)
    };

    private static AbilityScores CloneAbilities(AbilityScores abilities) => new()
    {
        Strength = abilities.Strength,
        Dexterity = abilities.Dexterity,
        Constitution = abilities.Constitution,
        Intelligence = abilities.Intelligence,
        Wisdom = abilities.Wisdom,
        Charisma = abilities.Charisma
    };

    private static InventoryEntry CloneInventoryEntry(InventoryEntry item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        Description = item.Description,
        Quantity = item.Quantity,
        IsCustom = item.IsCustom
    };

    private static string NormalizeName(string name)
    {
        var normalized = name?.Trim() ?? string.Empty;
        if (normalized.Length is < 1 or > 40)
            throw new ArgumentException("Profile names must contain between 1 and 40 characters.", nameof(name));

        return normalized;
    }
}
