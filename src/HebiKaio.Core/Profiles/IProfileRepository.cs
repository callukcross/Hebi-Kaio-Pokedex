namespace HebiKaio.Core.Profiles;

public interface IProfileRepository
{
    ProfileStore Load();

    void Save(ProfileStore store);
}
