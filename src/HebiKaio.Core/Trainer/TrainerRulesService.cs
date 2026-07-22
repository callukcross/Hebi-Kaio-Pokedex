using HebiKaio.Core.Profiles;

namespace HebiKaio.Core.Trainer;

public static class TrainerRulesService
{
    public const int CaughtEntriesPerLevel = 10;

    public static int CalculateMilestoneLevel(int caughtEntries) => Math.Clamp(1 + Math.Max(0, caughtEntries) / CaughtEntriesPerLevel, 1, 20);

    public static TrainerEffects Calculate(TrainerProfile profile, TrainerRulesCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(catalog);
        var levelRule = catalog.GetLevel(profile.TrainerLevel);
        var className = profile.Trainer.ClassName ?? string.Empty;
        var feats = profile.Trainer.Feats.ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new TrainerEffects
        {
            TrainerLevel = profile.TrainerLevel,
            ProficiencyBonus = levelRule.ProficiencyBonus,
            StabBonus = levelRule.StabBonus + (className == "Type Specialist" && profile.TrainerLevel >= 3 ? 1 : 0),
            AbilityScoreIncreases = levelRule.AbilityScoreIncreases,
            PokemonAttackBonus = className == "Ace Trainer" && profile.TrainerLevel >= 3 ? 1 : 0,
            PokemonDamageBonus = className == "Ace Trainer" && profile.TrainerLevel >= 7 ? 1 : 0,
            PokemonArmorClassBonus = (className == "Ranger" && profile.TrainerLevel >= 3 ? 1 : 0) + (feats.Contains("AC Up") ? 1 : 0),
            PokemonInitiativeBonus = (className == "Researcher" && profile.TrainerLevel >= 3 ? levelRule.ProficiencyBonus : 0) + (feats.Contains("Alert") ? 5 : 0),
            PokemonHitPointsPerLevel = (className == "Breeder" && profile.TrainerLevel >= 3 ? 1 : 0) + (feats.Contains("Tough") ? 2 : 0),
            ExtraMoveSlots = feats.Contains("Extra Move") ? 1 : 0,
            CatchBonus = className == "Capture Specialist" ? levelRule.ProficiencyBonus : 0
        };
    }
}
