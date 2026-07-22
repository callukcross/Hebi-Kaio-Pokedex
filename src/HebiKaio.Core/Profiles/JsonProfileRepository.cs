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

        if (File.Exists(_filePath))
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
        return store;
    }

    private string GetBackupPath() => _filePath + ".bak";
}
