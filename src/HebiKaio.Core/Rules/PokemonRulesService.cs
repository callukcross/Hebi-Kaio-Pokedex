using HebiKaio.Core.Profiles;
using HebiKaio.Core.Trainer;

namespace HebiKaio.Core.Rules;

public static class PokemonRulesService
{
    public static void Initialize(OwnedPokemon pokemon, ReferenceRulesCatalog rules)
    {
        var species = rules.FindPokemon(pokemon.SpeciesName)
            ?? throw new KeyNotFoundException($"No complete rules record exists for '{pokemon.SpeciesName}'.");
        pokemon.Abilities = species.Abilities.ToList();
        pokemon.Skills = species.Skills.ToList();
        pokemon.Nature = rules.Natures.ContainsKey(pokemon.Nature) ? pokemon.Nature : "Hardy";
        pokemon.Moves = AvailableMoves(species, pokemon.Level)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(4)
            .Select(name => new OwnedPokemonMove { Name = name, CurrentPowerPoints = rules.FindMove(name)?.PowerPoints ?? 0 })
            .ToList();
        pokemon.CurrentHp = GetMaximumHp(pokemon, species, rules);
    }

    public static IReadOnlyList<string> AvailableMoves(PokemonRule species, int level) =>
        species.StartingMoves.Concat(species.LevelMoves.Where(entry => entry.Key <= level).OrderBy(entry => entry.Key).SelectMany(entry => entry.Value)).ToList();

    public static AbilityScores GetAttributes(OwnedPokemon pokemon, PokemonRule species, ReferenceRulesCatalog rules)
    {
        var nature = rules.Natures.GetValueOrDefault(pokemon.Nature) ?? new Dictionary<string, int>();
        return new AbilityScores
        {
            Strength = Score("STR", pokemon.AttributeIncreases.Strength, pokemon.CustomAttributes.Strength),
            Dexterity = Score("DEX", pokemon.AttributeIncreases.Dexterity, pokemon.CustomAttributes.Dexterity),
            Constitution = Score("CON", pokemon.AttributeIncreases.Constitution, pokemon.CustomAttributes.Constitution),
            Intelligence = Score("INT", pokemon.AttributeIncreases.Intelligence, pokemon.CustomAttributes.Intelligence),
            Wisdom = Score("WIS", pokemon.AttributeIncreases.Wisdom, pokemon.CustomAttributes.Wisdom),
            Charisma = Score("CHA", pokemon.AttributeIncreases.Charisma, pokemon.CustomAttributes.Charisma)
        };

        int Score(string name, int increase, int custom) => species.Attributes.GetValueOrDefault(name) + nature.GetValueOrDefault(name) + increase + custom;
    }

    public static int GetMaximumHp(OwnedPokemon pokemon, PokemonRule species, ReferenceRulesCatalog rules, TrainerEffects? effects = null)
    {
        if (pokemon.Abilities.Contains("Paper Thin", StringComparer.OrdinalIgnoreCase)) return 1;
        var baseHp = pokemon.MaximumHpOverride ?? species.BaseHp + Math.Max(0, pokemon.Level - species.MinimumLevel) * (int)Math.Ceiling((species.HitDie + 1) / 2d);
        var constitution = GetAttributes(pokemon, species, rules).Constitution;
        var constitutionModifier = (int)Math.Floor((constitution - 10) / 2d);
        var loyaltyHp = pokemon.Loyalty switch { 2 => (int)Math.Ceiling(pokemon.Level / 2d), 3 => pokemon.Level, _ => 0 };
        var toughHp = pokemon.Feats.Contains("Tough", StringComparer.OrdinalIgnoreCase) ? pokemon.Level * 2 : 0;
        return Math.Max(1, baseHp + pokemon.Level * constitutionModifier + loyaltyHp + toughHp + pokemon.Level * (effects?.PokemonHitPointsPerLevel ?? 0));
    }

    public static int GetArmorClass(OwnedPokemon pokemon, PokemonRule species, TrainerEffects? effects = null) =>
        species.ArmorClass + (pokemon.Feats.Contains("AC Up", StringComparer.OrdinalIgnoreCase) ? 1 : 0) + (effects?.PokemonArmorClassBonus ?? 0);

    public static int Modifier(int score) => (int)Math.Floor((score - 10) / 2d);
}
