using HebiKaio.Core.Pokedex;
using HebiKaio.Core.Profiles;
using HebiKaio.Core.Trainer;
using HebiKaio.Core.Content;
using HebiKaio.Core.Rules;

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
Run("trainer levels from caught-entry milestones", TrainerMilestonesLevelUp, failures);
Run("trainer feats and classes derive pokemon effects", TrainerEffectsAreDerived, failures);
Run("catalog and custom inventory items persist", InventoryPersists, failures);
Run("profiles export and import without replacing data", ProfileTransferRoundTrips, failures);
Run("validated custom modules extend trainer catalogs", CustomModulesExtendCatalog, failures);
Run("conflicting custom content is rejected", ConflictingCustomContentIsRejected, failures);
Run("complete reference rules catalog loads", CompleteReferenceRulesLoad, failures);
Run("reference pokemon initialize and battle state persists", ReferencePokemonBattleStatePersists, failures);
Run("profile management and manual trainer modifiers persist", ProfileAndManualTrainerControlsPersist, failures);
Run("bulk pokedex marking and pokemon transfer persist", BulkPokedexAndPokemonTransferPersist, failures);

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

static void TrainerMilestonesLevelUp()
{
    var service = CreateService(out _);
    service.CreateProfile("Nemona");
    for (var number = 1; number <= 10; number++)
        service.SetPokedexState(number, PokedexEntryState.Caught);

    Assert(service.GetActiveProfile()?.TrainerLevel == 2, "Ten caught entries did not advance the trainer to level two.");
    service.SetPokedexState(10, PokedexEntryState.Unknown);
    Assert(service.GetActiveProfile()?.TrainerLevel == 2, "Losing a caught mark incorrectly reduced an earned trainer level.");
}

static void TrainerEffectsAreDerived()
{
    var service = CreateService(out _);
    service.CreateProfile("Cynthia");
    for (var number = 1; number <= 60; number++)
        service.SetPokedexState(number, PokedexEntryState.Caught);
    service.UpdateTrainer(new TrainerUpdate
    {
        ClassName = "Ace Trainer",
        Feats = ["AC Up", "Extra Move", "Alert"]
    });

    var rules = TrainerRulesCatalog.Load(Path.Combine(AppContext.BaseDirectory, "data", "p5e"));
    var effects = service.GetTrainerEffects(rules);
    Assert(effects.TrainerLevel == 7 && effects.PokemonAttackBonus == 1 && effects.PokemonDamageBonus == 1, "Ace Trainer progression bonuses were incorrect.");
    Assert(effects.PokemonArmorClassBonus == 1 && effects.ExtraMoveSlots == 1 && effects.PokemonInitiativeBonus == 5, "Feat-derived effects were incorrect.");
}

static void InventoryPersists()
{
    var service = CreateService(out _);
    service.CreateProfile("Juliana");
    var potion = service.AddInventoryItem("Potion", "Restore HP", 2);
    service.AddInventoryItem("Potion", "Restore HP", 3);
    var custom = service.AddInventoryItem("Camp Kit", "A custom travel kit", 1, isCustom: true);

    var inventory = service.GetTrainer().Inventory;
    Assert(inventory.Single(item => item.Id == potion.Id).Quantity == 5, "Catalog item quantities did not stack.");
    Assert(inventory.Single(item => item.Id == custom.Id).IsCustom, "The custom item flag did not persist.");
    service.SetInventoryQuantity(custom.Id, 0);
    Assert(service.GetTrainer().Inventory.All(item => item.Id != custom.Id), "Removing an inventory item failed.");
}

static void ProfileTransferRoundTrips()
{
    var directory = Path.Combine(Path.GetTempPath(), "HebiKaio.Core.Tests", Guid.NewGuid().ToString("N"));
    var repository = new JsonProfileRepository(Path.Combine(directory, "profiles.json"));
    var service = new ProfileService(repository);
    service.CreateProfile("Gloria");
    var pokemon = service.CreatePokemon(Draft(810, "Grookey", "Twig", 5));
    service.AddToParty(pokemon.Id);
    var transfer = new ProfileTransferService(repository);
    var packagePath = Path.Combine(directory, "gloria.hkp");
    transfer.ExportActiveProfile(packagePath);
    var imported = transfer.ImportProfile(packagePath);

    Assert(imported.Name == "Gloria (Imported 2)", "A duplicate import did not receive a unique name.");
    Assert(imported.Id != service.GetProfiles().Single(profile => profile.Name == "Gloria").Id, "An import reused the source profile id.");
    Assert(imported.Pokemon.Count == 1 && imported.PartyPokemonIds.SequenceEqual(imported.Pokemon.Select(item => item.Id)), "The imported party did not map to fresh Pokémon ids.");
}

static void CustomModulesExtendCatalog()
{
    var directory = Path.Combine(Path.GetTempPath(), "HebiKaio.Core.Tests", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(directory);
    var source = Path.Combine(directory, "source.json");
    File.WriteAllText(source, """{"formatVersion":1,"id":"table.homebrew","name":"Table Homebrew","author":"Test","feats":{"Quick Study":"Learn quickly."},"items":{"Field Tent":"Portable shelter."}}""");
    var installed = Path.Combine(directory, "installed");
    var service = new CustomContentService(installed);
    service.Install(source);
    var rules = TrainerRulesCatalog.Load(Path.Combine(AppContext.BaseDirectory, "data", "p5e"), installed);

    Assert(rules.Feats.ContainsKey("Quick Study") && rules.Items.ContainsKey("Field Tent"), "Installed module entries were not merged into the rules catalog.");
    service.Remove("table.homebrew");
    Assert(service.GetInstalledModules().Count == 0, "Removing a custom module failed.");
}

static void ConflictingCustomContentIsRejected()
{
    var directory = Path.Combine(Path.GetTempPath(), "HebiKaio.Core.Tests", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(directory);
    File.WriteAllText(Path.Combine(directory, "conflict.json"), """{"formatVersion":1,"id":"table.conflict","name":"Conflict","feats":{"Alert":"Replacement"},"items":{}}""");
    try
    {
        TrainerRulesCatalog.Load(Path.Combine(AppContext.BaseDirectory, "data", "p5e"), directory);
        throw new Exception("A module overwrote built-in content.");
    }
    catch (InvalidDataException)
    {
    }
}

static void CompleteReferenceRulesLoad()
{
    var rules = ReferenceRulesCatalog.Load(Path.Combine(AppContext.BaseDirectory, "data", "p5e"));
    var bulbasaur = rules.FindPokemon("Bulbasaur");
    var tackle = rules.FindMove("Tackle");
    Assert(rules.Pokemon.Count == 810 && rules.Moves.Count == 675, "The complete Pokémon or move dataset was not loaded.");
    Assert(bulbasaur?.Attributes["STR"] == 13 && bulbasaur.StartingMoves.Contains("Tackle"), "Complete Pokémon combat data was not parsed.");
    Assert(tackle?.PowerPoints == 20 && tackle.IsAttack && tackle.PowerAttributes.Contains("STR"), "Move combat data was not parsed.");
    Assert(rules.Abilities.ContainsKey("Overgrow") && rules.Natures.ContainsKey("Hardy") && rules.PokedexDetails[1].Genus.Contains("Seed"), "Supporting reference rule data was not parsed.");
}

static void ReferencePokemonBattleStatePersists()
{
    var service = CreateService(out _);
    service.CreateProfile("Ash");
    var rules = ReferenceRulesCatalog.Load(Path.Combine(AppContext.BaseDirectory, "data", "p5e"));
    var pokemon = service.CreatePokemon(Draft(1, "Bulbasaur", "Buddy", 5), rules);
    Assert(pokemon.Abilities.Contains("Overgrow") && pokemon.Moves.Any(move => move.Name == "Tackle" && move.CurrentPowerPoints == 20), "Reference abilities or moves were not initialized.");
    service.UpdateBattleState(pokemon.Id, new PokemonBattleUpdate
    {
        CurrentHp = 2,
        TemporaryHp = 4,
        Loyalty = 3,
        Statuses = [PokemonStatus.Poisoned],
        MovePowerPoints = new Dictionary<string, int> { ["Tackle"] = 7 }
    }, rules);
    var updated = service.GetOwnedPokemon().Single();
    Assert(updated.CurrentHp == 2 && updated.TemporaryHp == 4 && updated.Statuses.Contains(PokemonStatus.Poisoned), "Battle meters or status did not persist.");
    Assert(updated.Moves.Single(move => move.Name == "Tackle").CurrentPowerPoints == 7, "Move PP did not persist.");
    service.AddToParty(updated.Id);
    service.HealParty(rules);
    var healed = service.GetOwnedPokemon().Single();
    Assert(healed.CurrentHp > 2 && healed.TemporaryHp == 0 && healed.Statuses.Count == 0 && healed.Moves.Single(move => move.Name == "Tackle").CurrentPowerPoints == 20, "Full rest did not restore party battle state.");
    service.UpdatePokemon(healed.Id, new PokemonDraft
    {
        SpeciesNumber = 1, SpeciesName = "Bulbasaur", Nickname = "Buddy", Level = 5,
        Gender = PokemonGender.Male, IsShiny = true, Nature = "Brave", HeldItem = "Potion",
        Abilities = ["Overgrow"], Feats = ["Tough"], Skills = ["Nature"], Moves = ["Tackle", "Growl"],
        AttributeIncreases = new AbilityScores { Strength = 2, Dexterity = 0, Constitution = 0, Intelligence = 0, Wisdom = 0, Charisma = 0 },
        CustomAttributes = new AbilityScores { Strength = 0, Dexterity = 1, Constitution = 0, Intelligence = 0, Wisdom = 0, Charisma = 0 }
    }, rules);
    var edited = service.GetOwnedPokemon().Single();
    Assert(edited.Gender == PokemonGender.Male && edited.IsShiny && edited.Nature == "Brave" && edited.HeldItem == "Potion", "Advanced identity fields did not persist.");
    Assert(edited.Feats.Contains("Tough") && edited.Moves.Count == 2 && edited.Moves.Single(move => move.Name == "Tackle").CurrentPowerPoints == 20, "Advanced selections or move PP did not persist.");
}

static void ProfileAndManualTrainerControlsPersist()
{
    var service = CreateService(out _);
    var first = service.CreateProfile("Old Name");
    service.RenameProfile(first.Id, "New Name");
    service.UpdateTrainer(new TrainerUpdate
    {
        ClassName = "Ace Trainer",
        ManualModifiers = new ManualTrainerModifiers
        {
            Attack = 2, Damage = 3, Stab = 1, MoveSlots = 2, AbilityScoreIncreases = 4, MaximumActivePokemon = 8,
            PokemonAttributes = new AbilityScores { Strength = 1, Dexterity = 2, Constitution = 0, Intelligence = 0, Wisdom = 0, Charisma = 0 },
            TypeAttack = new Dictionary<string, int> { ["Fire"] = 2 },
            AlwaysUseStabTypes = new HashSet<string> { "Fire" }
        }
    });
    var rules = TrainerRulesCatalog.Load(Path.Combine(AppContext.BaseDirectory, "data", "p5e"));
    var effects = service.GetTrainerEffects(rules);
    Assert(service.GetActiveProfile()?.Name == "New Name" && effects.PokemonAttackBonus == 2 && effects.PokemonDamageBonus == 3 && effects.ExtraMoveSlots == 2, "Manual trainer modifiers or rename did not persist.");
    Assert(service.GetTrainer().ManualModifiers.TypeAttack["Fire"] == 2 && service.GetTrainer().ManualModifiers.AlwaysUseStabTypes.Contains("Fire"), "Per-type trainer controls did not persist.");
    var second = service.CreateProfile("Temporary");
    service.DeleteProfile(second.Id);
    Assert(service.GetActiveProfile()?.Id == first.Id, "Deleting the active profile did not select the remaining profile.");
}

static void BulkPokedexAndPokemonTransferPersist()
{
    var directory = Path.Combine(Path.GetTempPath(), "HebiKaio.Core.Tests", Guid.NewGuid().ToString("N"));
    var repository = new JsonProfileRepository(Path.Combine(directory, "profiles.json"));
    var service = new ProfileService(repository);
    service.CreateProfile("Serena");
    service.SetPokedexStates([1, 2, 3], PokedexEntryState.Seen);
    Assert(service.GetPokedexStates().Count == 3 && service.GetPokedexStates().Values.All(state => state == PokedexEntryState.Seen), "Bulk Pokédex marking failed.");
    var pokemon = service.CreatePokemon(Draft(25, "Pikachu", "Sparky", 5));
    var transfer = new ProfileTransferService(repository);
    var path = Path.Combine(directory, "sparky.hkpokemon");
    transfer.ExportPokemon(pokemon.Id, path);
    var imported = transfer.ImportPokemon(path);
    Assert(imported.Id != pokemon.Id && service.GetOwnedPokemon().Count == 2 && imported.Nickname == "Sparky", "Individual Pokémon transfer did not create an independent stored Pokémon.");
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
