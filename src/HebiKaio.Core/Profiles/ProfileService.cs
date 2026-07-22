namespace HebiKaio.Core.Profiles;

public sealed class ProfileService
{
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

    private static TrainerProfile GetRequiredActiveProfile(ProfileStore store)
    {
        if (store.ActiveProfileId is not { } activeId)
            throw new InvalidOperationException("Create or select a trainer profile first.");

        return store.Profiles.SingleOrDefault(profile => profile.Id == activeId)
            ?? throw new InvalidOperationException("The active trainer profile no longer exists.");
    }

    private static string NormalizeName(string name)
    {
        var normalized = name?.Trim() ?? string.Empty;
        if (normalized.Length is < 1 or > 40)
            throw new ArgumentException("Profile names must contain between 1 and 40 characters.", nameof(name));

        return normalized;
    }
}
