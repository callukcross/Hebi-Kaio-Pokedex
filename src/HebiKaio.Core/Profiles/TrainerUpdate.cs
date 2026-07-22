namespace HebiKaio.Core.Profiles;

public sealed class TrainerUpdate
{
    public string ClassName { get; init; } = "Ace Trainer";

    public AbilityScores Abilities { get; init; } = new();

    public IReadOnlyList<string> Feats { get; init; } = [];
}
