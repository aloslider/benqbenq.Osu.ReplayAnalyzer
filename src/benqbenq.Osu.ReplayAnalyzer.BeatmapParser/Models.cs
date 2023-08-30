using System.Numerics;

namespace benqbenq.Osu.ReplayAnalyzer.BeatmapParser.Models;

public record BeatmapInfo
{
    public required Metadata Metadata { get; init; }
    public required DifficultyInfo Difficulty { get; init; }
    public required HitObject[] HitObjects { get; init; } = Array.Empty<HitObject>();
}

public record Metadata(
    string?   TitleRomanised,
    string?   TitleUnicode,
    string?   ArtistRomanised,
    string?   ArtistUnicode,
    string?   Creator,
    string?   Version,
    string?   Source,
    string[]? Tags,
    int?      BeatmapID,
    int?      BeatmapSetID);

public record DifficultyInfo(
    double ApproachRate,
    double CircleSize,
    double HPDrainRate,
    double OverallDifficulty,
    double SliderTickRate,
    double SliderMultiplier);

public record HitObject(Vector2 Coords, double Ticks, HitObjectType Type);

public enum HitObjectType
{
    HitCircle = 0,
    Slider = 1,
    Spinner = 2,
    ManiaNote = 3
}