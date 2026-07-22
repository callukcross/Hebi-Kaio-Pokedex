using System.Text.Json;
using System.Text.Json.Serialization;

namespace HebiKaio.Core.Profiles;

public sealed class JsonProfileRepository : IProfileRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string _filePath;

    public JsonProfileRepository(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("A save-file path is required.", nameof(filePath));

        _filePath = Path.GetFullPath(filePath);
    }

    public ProfileStore Load()
    {
        if (!File.Exists(_filePath))
            return new ProfileStore();

        try
        {
            return Read(_filePath);
        }
        catch (Exception exception) when (exception is JsonException or IOException)
        {
            var backupPath = GetBackupPath();
            if (!File.Exists(backupPath))
                throw new InvalidDataException("The profile save is invalid and no backup is available.", exception);

            return Read(backupPath);
        }
    }

    public void Save(ProfileStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        store.SchemaVersion = ProfileStore.CurrentSchemaVersion;
        var directory = Path.GetDirectoryName(_filePath)!;
        Directory.CreateDirectory(directory);

        var temporaryPath = _filePath + ".tmp";
        var json = JsonSerializer.Serialize(store, SerializerOptions);
        File.WriteAllText(temporaryPath, json);

        if (File.Exists(_filePath) && IsValidSave(_filePath))
            File.Copy(_filePath, GetBackupPath(), overwrite: true);

        File.Move(temporaryPath, _filePath, overwrite: true);
    }

    private static ProfileStore Read(string path)
    {
        var store = JsonSerializer.Deserialize<ProfileStore>(File.ReadAllText(path), SerializerOptions)
            ?? throw new InvalidDataException("The profile save did not contain a profile store.");

        if (store.SchemaVersion > ProfileStore.CurrentSchemaVersion)
            throw new InvalidDataException($"Save schema {store.SchemaVersion} is newer than this app supports.");

        store.Profiles ??= [];
        foreach (var profile in store.Profiles)
        {
            profile.Pokedex ??= [];
            profile.Pokemon ??= [];
            profile.PartyPokemonIds ??= [];
            profile.Trainer ??= new TrainerCharacter();
            profile.Trainer.Abilities ??= new AbilityScores();
            profile.Trainer.Feats ??= [];
            profile.Trainer.Inventory ??= [];
            foreach (var pokemon in profile.Pokemon)
            {
                pokemon.Nature = string.IsNullOrWhiteSpace(pokemon.Nature) ? "Hardy" : pokemon.Nature;
                pokemon.AttributeIncreases ??= ZeroAbilities();
                pokemon.CustomAttributes ??= ZeroAbilities();
                pokemon.Abilities ??= [];
                pokemon.Feats ??= [];
                pokemon.Skills ??= [];
                pokemon.Moves ??= [];
                pokemon.Statuses ??= [];
                pokemon.CurrentHp = Math.Max(0, pokemon.CurrentHp);
                pokemon.TemporaryHp = Math.Max(0, pokemon.TemporaryHp);
            }
        }
        return store;
    }

    private static AbilityScores ZeroAbilities() => new() { Strength = 0, Dexterity = 0, Constitution = 0, Intelligence = 0, Wisdom = 0, Charisma = 0 };

    private static bool IsValidSave(string path)
    {
        try
        {
            Read(path);
            return true;
        }
        catch (Exception exception) when (exception is JsonException or IOException)
        {
            return false;
        }
    }

    private string GetBackupPath() => _filePath + ".bak";
}
