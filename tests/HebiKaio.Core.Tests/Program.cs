using HebiKaio.Core.Pokedex;
using HebiKaio.Core.Profiles;

var failures = new List<string>();
Run("profiles persist and become active", ProfilesPersist, failures);
Run("duplicate profile names are rejected", DuplicateNamesAreRejected, failures);
Run("pokedex filters compose", FiltersCompose, failures);

if (failures.Count > 0)
{
    Console.Error.WriteLine(string.Join(Environment.NewLine, failures));
    return 1;
}

Console.WriteLine("All HebiKaio.Core checks passed.");
return 0;

static void ProfilesPersist()
{
    var service = CreateService(out _);
    var created = service.CreateProfile("  Misty  ");
    var reloaded = service.GetActiveProfile();

    Assert(reloaded?.Id == created.Id, "The created profile was not reloaded as active.");
    Assert(reloaded?.Name == "Misty", "The profile name was not normalized.");
}

static void DuplicateNamesAreRejected()
{
    var service = CreateService(out _);
    service.CreateProfile("Brock");

    try
    {
        service.CreateProfile("brock");
        throw new Exception("A case-insensitive duplicate name was accepted.");
    }
    catch (InvalidOperationException)
    {
    }
}

static void FiltersCompose()
{
    PokemonSpecies[] species =
    [
        new() { Number = 1, Name = "Bulbasaur", Types = ["Grass", "Poison"], Region = "Kanto", EvolutionStage = EvolutionStage.Basic },
        new() { Number = 2, Name = "Ivysaur", Types = ["Grass", "Poison"], Region = "Kanto", EvolutionStage = EvolutionStage.StageOne },
        new() { Number = 152, Name = "Chikorita", Types = ["Grass"], Region = "Johto", EvolutionStage = EvolutionStage.Basic }
    ];

    var result = new PokedexFilter { Type = "grass", Region = "kanto", EvolutionStage = EvolutionStage.Basic }
        .Apply(species)
        .ToList();

    Assert(result.Count == 1 && result[0].Name == "Bulbasaur", "The composed filter returned the wrong species.");
}

static ProfileService CreateService(out string path)
{
    var directory = Path.Combine(Path.GetTempPath(), "HebiKaio.Core.Tests", Guid.NewGuid().ToString("N"));
    path = Path.Combine(directory, "profiles.json");
    return new ProfileService(new JsonProfileRepository(path));
}

static void Run(string name, Action check, ICollection<string> failures)
{
    try
    {
        check();
        Console.WriteLine($"PASS: {name}");
    }
    catch (Exception exception)
    {
        failures.Add($"FAIL: {name} - {exception.Message}");
    }
}

static void Assert(bool condition, string message)
{
    if (!condition)
        throw new Exception(message);
}
