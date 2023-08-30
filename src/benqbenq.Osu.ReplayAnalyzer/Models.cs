namespace benqbenq.Osu.ReplayAnalyzer.Models;

public readonly record struct ReplayStats(
    double NegativeHitErrorAvg,
    double PositiveHitErrorAvg,
    double UnstableRate);