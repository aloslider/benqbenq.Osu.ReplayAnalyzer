using System.Numerics;

namespace benqbenq.Osu.ReplayAnalyzer.ReplayParser.Models;

public record ReplayInfo(
    Mode Mode,
    int OsuVersion,
    string MapMD5Hash,
    string PlayerNickname,
    string ReplayMD5Hash,
    int Count300,
    int Count100,
    int Count50,
    int CountGekis,
    int CountKatus,
    int CountMisses,
    int Score,
    int MaxCombo,
    bool IsPerfectCombo,
    Mods Mods,
    LifeBarPoint[] LifeBarGraph,
    DateTimeOffset PlayedAt,
    ReplayFrame[] Frames,
    int Seed,
    long ScoreId);

public enum Mode
{
    Std = 0,
    Taiko = 1,
    Ctb = 2,
    Mania = 3
}

[Flags]
public enum Mods
{
    None              = 0,
    NoFail            = 1 << 0,
    Easy              = 1 << 1,
    TouchDevice       = 1 << 2,
    Hidden            = 1 << 3,
    HardRock          = 1 << 4,
    SuddenDeath       = 1 << 5,
    DoubleTime        = 1 << 6,
    Relax             = 1 << 7,
    HalfTime          = 1 << 8,
    Nightcore         = 1 << 9,
    Flashlight        = 1 << 10,
    Autoplay          = 1 << 11,
    SpunOut           = 1 << 12,
    Relax2            = 1 << 13,
    Perfect           = 1 << 14,
    Key4              = 1 << 15,
    Key5              = 1 << 16,
    Key6              = 1 << 17,
    Key7              = 1 << 18,
    Key8              = 1 << 19,
    FadeIn            = 1 << 20,
    Random            = 1 << 21,
    Cinema            = 1 << 21,
    Target            = 1 << 22,
    Key9              = 1 << 23,
    KeyCoop           = 1 << 24,
    Key1              = 1 << 25,
    Key3              = 1 << 26,
    Key2              = 1 << 27,
    ScoreV2           = 1 << 28,
    Mirror            = 1 << 29,
    KeyMod            = Key1 | Key2 | Key3 | Key4 | Key5 | Key6 | Key7 | Key8 | Key9 | KeyCoop,
    FreeModAllowed    = NoFail | Easy | Hidden | HardRock | SuddenDeath | Flashlight | FadeIn | Relax | Relax2 | SpunOut | KeyMod,
    ScoreIncreaseMods = Hidden | HardRock | DoubleTime | Flashlight | FadeIn
}

public record LifeBarPoint(TimeSpan Time, float Percentage);

public record ReplayFrame(Vector2 Coords, double Ticks, Inputs Inputs);

[Flags]
public enum Inputs : byte
{
    None = 0,
    M1 = 1,
    M2 = 2,
    K1 = 4,
    K2 = 8,
    Smoke = 16
}

public static class InputsMethods
{
    public static bool HasAnyNewInput(this Inputs currInputs, Inputs prevInputs) =>
        (currInputs & ~prevInputs) != Inputs.None;
}