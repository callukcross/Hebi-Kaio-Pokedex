namespace HebiKaio.Core.Profiles;

public sealed class PokemonBattleUpdate
{
    public int? CurrentHp { get; init; }
    public int? TemporaryHp { get; init; }
    public int? Loyalty { get; init; }
    public int? Experience { get; init; }
    public IReadOnlyCollection<PokemonStatus>? Statuses { get; init; }
    public IReadOnlyDictionary<string, int>? MovePowerPoints { get; init; }
}
