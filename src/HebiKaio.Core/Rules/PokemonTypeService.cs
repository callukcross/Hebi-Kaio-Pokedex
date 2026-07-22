namespace HebiKaio.Core.Rules;

public static class PokemonTypeService
{
    private static readonly IReadOnlyDictionary<string, TypeMatchup> Matchups = Build();

    public static TypeDefenses Calculate(IEnumerable<string> defendingTypes)
    {
        var defenders = defendingTypes.ToList();
        var vulnerabilities = new List<string>();
        var resistances = new List<string>();
        var immunities = new List<string>();
        foreach (var attack in Matchups.Keys.OrderBy(value => value))
        {
            var multiplier = defenders.Aggregate(1d, (current, defense) => current * Multiplier(attack, defense));
            if (multiplier == 0) immunities.Add(attack);
            else if (multiplier > 1) vulnerabilities.Add(multiplier >= 4 ? $"{attack} x4" : attack);
            else if (multiplier < 1) resistances.Add(multiplier <= .25 ? $"{attack} x1/4" : attack);
        }
        return new(vulnerabilities, resistances, immunities);
    }

    private static double Multiplier(string attack, string defense)
    {
        var matchup = Matchups[attack];
        if (matchup.Immune.Contains(defense)) return 0;
        if (matchup.Strong.Contains(defense)) return 2;
        if (matchup.Weak.Contains(defense)) return .5;
        return 1;
    }

    private static IReadOnlyDictionary<string, TypeMatchup> Build() => new Dictionary<string, TypeMatchup>(StringComparer.OrdinalIgnoreCase)
    {
        ["Normal"] = M([], ["Rock", "Steel"], ["Ghost"]),
        ["Fire"] = M(["Grass", "Ice", "Bug", "Steel"], ["Fire", "Water", "Rock", "Dragon"]),
        ["Water"] = M(["Fire", "Ground", "Rock"], ["Water", "Grass", "Dragon"]),
        ["Electric"] = M(["Water", "Flying"], ["Electric", "Grass", "Dragon"], ["Ground"]),
        ["Grass"] = M(["Water", "Ground", "Rock"], ["Fire", "Grass", "Poison", "Flying", "Bug", "Dragon", "Steel"]),
        ["Ice"] = M(["Grass", "Ground", "Flying", "Dragon"], ["Fire", "Water", "Ice", "Steel"]),
        ["Fighting"] = M(["Normal", "Ice", "Rock", "Dark", "Steel"], ["Poison", "Flying", "Psychic", "Bug", "Fairy"], ["Ghost"]),
        ["Poison"] = M(["Grass", "Fairy"], ["Poison", "Ground", "Rock", "Ghost"], ["Steel"]),
        ["Ground"] = M(["Fire", "Electric", "Poison", "Rock", "Steel"], ["Grass", "Bug"], ["Flying"]),
        ["Flying"] = M(["Grass", "Fighting", "Bug"], ["Electric", "Rock", "Steel"]),
        ["Psychic"] = M(["Fighting", "Poison"], ["Psychic", "Steel"], ["Dark"]),
        ["Bug"] = M(["Grass", "Psychic", "Dark"], ["Fire", "Fighting", "Poison", "Flying", "Ghost", "Steel", "Fairy"]),
        ["Rock"] = M(["Fire", "Ice", "Flying", "Bug"], ["Fighting", "Ground", "Steel"]),
        ["Ghost"] = M(["Psychic", "Ghost"], ["Dark"], ["Normal"]),
        ["Dragon"] = M(["Dragon"], ["Steel"], ["Fairy"]),
        ["Dark"] = M(["Psychic", "Ghost"], ["Fighting", "Dark", "Fairy"]),
        ["Steel"] = M(["Ice", "Rock", "Fairy"], ["Fire", "Water", "Electric", "Steel"]),
        ["Fairy"] = M(["Fighting", "Dragon", "Dark"], ["Fire", "Poison", "Steel"])
    };

    private static TypeMatchup M(IEnumerable<string> strong, IEnumerable<string> weak, IEnumerable<string>? immune = null) => new(strong.ToHashSet(StringComparer.OrdinalIgnoreCase), weak.ToHashSet(StringComparer.OrdinalIgnoreCase), (immune ?? []).ToHashSet(StringComparer.OrdinalIgnoreCase));
    private sealed record TypeMatchup(IReadOnlySet<string> Strong, IReadOnlySet<string> Weak, IReadOnlySet<string> Immune);
}

public sealed record TypeDefenses(IReadOnlyList<string> Vulnerabilities, IReadOnlyList<string> Resistances, IReadOnlyList<string> Immunities);
