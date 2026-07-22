namespace HebiKaio.Core.Profiles;

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

        profile.Pokemon.Add(pokemon);
        profile.Pokedex[pokemon.SpeciesNumber] = PokedexEntryState.Caught;
        profile.UpdatedAtUtc = DateTimeOffset.UtcNow;
        _repository.Save(store);
        return ClonePokemon(pokemon);
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
        profile.Pokedex[pokemon.SpeciesNumber] = PokedexEntryState.Caught;
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
            if (profile.PartyPokemonIds.Count >= MaximumPartySize)
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
        CustomImagePath = pokemon.CustomImagePath
    };

    private static string NormalizeName(string name)
    {
        var normalized = name?.Trim() ?? string.Empty;
        if (normalized.Length is < 1 or > 40)
            throw new ArgumentException("Profile names must contain between 1 and 40 characters.", nameof(name));

        return normalized;
    }
}
