using System.Text.Json;
using System.Text.Json.Serialization;

namespace HebiKaio.Core.Profiles;

public sealed class ProfileTransferService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly IProfileRepository _repository;

    public ProfileTransferService(IProfileRepository repository) =>
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public void ExportActiveProfile(string path)
    {
        var store = _repository.Load();
        var profile = store.ActiveProfileId is { } id
            ? store.Profiles.SingleOrDefault(candidate => candidate.Id == id)
            : null;
        if (profile is null)
            throw new InvalidOperationException("An active profile is required for export.");

        var package = new ProfileTransferPackage { Profile = profile };
        WriteAtomically(path, JsonSerializer.Serialize(package, Options));
    }

    public TrainerProfile ImportProfile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("The profile package could not be found.", path);

        ProfileTransferPackage package;
        try
        {
            package = JsonSerializer.Deserialize<ProfileTransferPackage>(File.ReadAllText(path), Options)
                ?? throw new InvalidDataException("The profile package is empty.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("The profile package is not valid JSON.", exception);
        }

        if (package.FormatVersion != 1 || package.Profile is null)
            throw new InvalidDataException("This profile package version is not supported.");

        NormalizeAndValidate(package.Profile);
        var store = _repository.Load();
        package.Profile.Id = Guid.NewGuid();
        package.Profile.Name = MakeUniqueName(package.Profile.Name, store.Profiles);
        var pokemonIds = package.Profile.Pokemon.ToDictionary(pokemon => pokemon.Id, _ => Guid.NewGuid());
        package.Profile.Pokemon.ForEach(pokemon => pokemon.Id = pokemonIds[pokemon.Id]);
        package.Profile.PartyPokemonIds = package.Profile.PartyPokemonIds
            .Where(pokemonIds.ContainsKey)
            .Select(id => pokemonIds[id])
            .Distinct()
            .Take(ProfileService.MaximumPartySize)
            .ToList();
        package.Profile.Trainer.Inventory.ForEach(item => item.Id = Guid.NewGuid());
        package.Profile.CreatedAtUtc = DateTimeOffset.UtcNow;
        package.Profile.UpdatedAtUtc = package.Profile.CreatedAtUtc;
        store.Profiles.Add(package.Profile);
        store.ActiveProfileId = package.Profile.Id;
        _repository.Save(store);
        return package.Profile;
    }

    private static void NormalizeAndValidate(TrainerProfile profile)
    {
        profile.Name = profile.Name?.Trim() ?? string.Empty;
        if (profile.Name.Length is < 1 or > 40)
            throw new InvalidDataException("Imported profile names must contain between 1 and 40 characters.");
        if (profile.TrainerLevel is < 1 or > 20)
            throw new InvalidDataException("Imported trainer levels must be between 1 and 20.");
        profile.Pokedex ??= [];
        profile.Pokemon ??= [];
        profile.PartyPokemonIds ??= [];
        profile.Trainer ??= new TrainerCharacter();
        profile.Trainer.Abilities ??= new AbilityScores();
        profile.Trainer.Feats ??= [];
        profile.Trainer.Inventory ??= [];
        foreach (var pokemon in profile.Pokemon)
        {
            pokemon.AttributeIncreases ??= ZeroAbilities();
            pokemon.CustomAttributes ??= ZeroAbilities();
            pokemon.Abilities ??= [];
            pokemon.Feats ??= [];
            pokemon.Skills ??= [];
            pokemon.Moves ??= [];
            pokemon.Statuses ??= [];
        }
        if (profile.Pokedex.Keys.Any(number => number < 1) ||
            profile.Pokemon.Any(pokemon => pokemon.SpeciesNumber < 1 || pokemon.Level is < 1 or > 20) ||
            profile.Trainer.Inventory.Any(item => string.IsNullOrWhiteSpace(item.Name) || item.Quantity is < 1 or > 999))
            throw new InvalidDataException("The imported profile contains invalid game data.");
    }

    private static AbilityScores ZeroAbilities() => new() { Strength = 0, Dexterity = 0, Constitution = 0, Intelligence = 0, Wisdom = 0, Charisma = 0 };

    private static string MakeUniqueName(string requested, IEnumerable<TrainerProfile> profiles)
    {
        var names = profiles.Select(profile => profile.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!names.Contains(requested))
            return requested;
        for (var suffix = 2; suffix < 1000; suffix++)
        {
            var candidate = $"{requested} (Imported {suffix})";
            if (candidate.Length <= 40 && !names.Contains(candidate))
                return candidate;
        }
        throw new InvalidOperationException("A unique imported profile name could not be created.");
    }

    private static void WriteAtomically(string path, string contents)
    {
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporaryPath = fullPath + ".tmp";
        File.WriteAllText(temporaryPath, contents);
        File.Move(temporaryPath, fullPath, overwrite: true);
    }
}

public sealed class ProfileTransferPackage
{
    public int FormatVersion { get; init; } = 1;
    public DateTimeOffset ExportedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public TrainerProfile? Profile { get; init; }
}
