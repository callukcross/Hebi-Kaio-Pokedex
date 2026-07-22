using HebiKaio.Core.Pokedex;
using HebiKaio.Core.Profiles;

var failures = new List<string>();
Run("profiles persist and become active", ProfilesPersist, failures);
Run("duplicate profile names are rejected", DuplicateNamesAreRejected, failures);
Run("a good backup survives primary-save corruption", BackupSurvivesCorruption, failures);
Run("pokedex state persists per active profile", PokedexStatePersists, failures);
Run("the reference pokedex catalog loads", CatalogLoads, failures);
Run("pokedex filters compose", FiltersCompose, failures);
Run("pokemon creation and editing persist", PokemonLifecyclePersists, failures);
Run("party membership and ordering persist", PartyManagementPersists, failures);
Run("party size is limited to six", PartySizeIsLimited, failures);

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

static void BackupSurvivesCorruption()
{
    var service = CreateService(out var path);
    service.CreateProfile("Red");
    service.CreateProfile("Blue");
    File.WriteAllText(path, "{ corrupt json");

    service.CreateProfile("Leaf");

    var backupService = new ProfileService(new JsonProfileRepository(path + ".bak"));
    var backupNames = backupService.GetProfiles().Select(profile => profile.Name).ToList();
    Assert(backupNames.Contains("Red"), "The last known-good backup was overwritten by corrupt data.");
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

static void PokedexStatePersists()
{
    var service = CreateService(out _);
    service.CreateProfile("May");
    service.SetPokedexState(252, PokedexEntryState.Caught);
    Assert(service.GetPokedexState(252) == PokedexEntryState.Caught, "The caught state was not persisted.");

    service.SetPokedexState(252, PokedexEntryState.Unknown);
    Assert(service.GetPokedexState(252) == PokedexEntryState.Unknown, "Clearing the state did not persist.");
}

static void CatalogLoads()
{
    var dataPath = Path.Combine(AppContext.BaseDirectory, "data", "p5e");
    var catalog = JsonPokemonCatalog.Load(dataPath);
    var bulbasaur = catalog.FindByNumber(1);
    Assert(catalog.GetAll().Count == 810, "The canonical catalog did not load every species/form entry.");
    Assert(bulbasaur?.Name == "Bulbasaur" && bulbasaur.Types.Contains("Grass"), "Bulbasaur data was not mapped correctly.");
}

static void PokemonLifecyclePersists()
{
    var service = CreateService(out _);
    service.CreateProfile("Dawn");
    var created = service.CreatePokemon(Draft(393, "Piplup", "Pip", 4));

    Assert(service.GetPokedexState(393) == PokedexEntryState.Caught, "Creating a Pokémon did not mark the species caught.");
    var updated = service.UpdatePokemon(created.Id, Draft(393, "Piplup", "Emperor", 7));
    Assert(updated.Nickname == "Emperor" && updated.Level == 7, "The Pokémon edits were not returned.");
    Assert(service.GetOwnedPokemon().Single().Nickname == "Emperor", "The Pokémon edits did not persist.");

    service.DeletePokemon(created.Id);
    Assert(service.GetOwnedPokemon().Count == 0, "The Pokémon was not deleted from storage.");
}

static void PartyManagementPersists()
{
    var service = CreateService(out _);
    service.CreateProfile("Serena");
    var first = service.CreatePokemon(Draft(650, "Chespin", null, 3));
    var second = service.CreatePokemon(Draft(653, "Fennekin", null, 3));
    service.AddToParty(first.Id);
    service.AddToParty(second.Id);
    service.ReorderPartyPokemon(second.Id, 0);

    var party = service.GetPartyPokemon();
    Assert(party.Count == 2 && party[0].Id == second.Id && party[1].Id == first.Id, "Party order did not persist.");

    service.RemoveFromParty(second.Id);
    Assert(service.GetPartyPokemon().Single().Id == first.Id, "Removing a Pokémon from the party failed.");
}

static void PartySizeIsLimited()
{
    var service = CreateService(out _);
    service.CreateProfile("Lillie");
    for (var index = 1; index <= ProfileService.MaximumPartySize; index++)
    {
        var pokemon = service.CreatePokemon(Draft(index, $"Species {index}", null, 1));
        service.AddToParty(pokemon.Id);
    }

    var extra = service.CreatePokemon(Draft(7, "Species 7", null, 1));
    try
    {
        service.AddToParty(extra.Id);
        throw new Exception("A seventh party Pokémon was accepted.");
    }
    catch (InvalidOperationException)
    {
    }
}

static PokemonDraft Draft(int number, string species, string? nickname, int level) => new()
{
    SpeciesNumber = number,
    SpeciesName = species,
    Nickname = nickname,
    Level = level
};

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
