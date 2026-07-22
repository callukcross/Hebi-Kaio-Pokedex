namespace HebiKaio.Core.Profiles;

public sealed class ProfileStore
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    public Guid? ActiveProfileId { get; set; }

    public List<TrainerProfile> Profiles { get; set; } = [];
}
