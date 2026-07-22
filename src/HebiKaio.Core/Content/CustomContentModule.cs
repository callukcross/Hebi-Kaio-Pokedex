namespace HebiKaio.Core.Content;

public sealed class CustomContentModule
{
    public int FormatVersion { get; init; } = 1;
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Author { get; init; }
    public Dictionary<string, string>? Feats { get; init; } = [];
    public Dictionary<string, string>? Items { get; init; } = [];
}

public sealed record ContentModuleSummary(string Id, string Name, string? Author, int FeatCount, int ItemCount, string FilePath);
