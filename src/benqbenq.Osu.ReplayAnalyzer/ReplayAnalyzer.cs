using benqbenq.Osu.ReplayAnalyzer.BeatmapParser.Models;
using benqbenq.Osu.ReplayAnalyzer.Models;
using benqbenq.Osu.ReplayAnalyzer.ReplayParser.Models;

namespace benqbenq.Osu.ReplayAnalyzer;

public static class ReplayAnalyzer
{
    public static ReplayStats CalculateHitErrorsStats(BeatmapInfo beatmap, ReplayInfo replay)
    {
        double r  = CalculateObjectsRadius(beatmap.Difficulty.CircleSize, replay.Mods);
        double od = CalculateRealOd(beatmap.Difficulty.OverallDifficulty, replay.Mods);
        double hit50Window = 200 - 10 * od;
        Inputs prevInput = Inputs.None;

        List<double> negHitErrors = new();
        List<double> posHitErrors = new();
        List<double> allHitErrors = new();

        for (int objIndex = 0, frameIndex = 0; objIndex < beatmap.HitObjects.Length && frameIndex < replay.Frames.Length;)
        {
            HitObject obj     = beatmap.HitObjects[objIndex];
            ReplayFrame frame = replay.Frames[frameIndex];
            Inputs currInput  = frame.Inputs;

            // If somehow frame is after object hiwWindow, skip to the next obj
            if (frame.Ticks > obj.Ticks + hit50Window)
            {
                prevInput = currInput;
                objIndex++;
                continue;
            }

            if (IsNewKeyPressed(currInput, prevInput) && 
                IsOnCircle(obj, frame, r) && 
                IsInHitWindow(obj, frame, hit50Window))
            {
                double hitError = frame.Ticks - obj.Ticks;

                if (hitError < 0)
                    negHitErrors.Add(hitError);
                else
                    posHitErrors.Add(hitError);

                allHitErrors.Add(hitError);
                objIndex++;
            }

            prevInput = currInput; 
            frameIndex++;
        }

        double negHitErrorsAvg = negHitErrors.Sum() / negHitErrors.Count,
               posHitErrorsAvg = posHitErrors.Sum() / posHitErrors.Count,
               allHitErrorsAvg = allHitErrors.Sum() / allHitErrors.Count,
               variance = allHitErrors.Aggregate(0.0, (sum, err) => sum += Math.Pow(err - allHitErrorsAvg, 2)) / allHitErrors.Count,
               unstableRate = Math.Sqrt(variance) * 10;

        return new ReplayStats(negHitErrorsAvg, posHitErrorsAvg, unstableRate);
    }

    /// <summary>
    /// Calculates real OverallDifficulty based on beatmap OverallDifficulty and mods combination/>.
    /// </summary>
    /// <param name="od">Beatmap's OverallDifficulty.</param>
    /// <param name="mods">Mods used.</param>
    /// <returns>Real OverallDifficulty.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when given mods are'nt supported.
    /// </exception>
    static double CalculateRealOd(double od, Mods mods)
    {
        var odModsMask = Mods.None | Mods.DoubleTime | Mods.HalfTime | Mods.HardRock | Mods.Easy;
        (double modOffset, double modMultiplier) = (mods & odModsMask) switch
        {
            Mods.None                       => (0.0, 1.0),
            Mods.DoubleTime                 => (4.44444, 0.66667),
            Mods.HalfTime                   => (-4.44444, 1.33334),
            Mods.HardRock                   => (0.0, 1.4),
            Mods.HardRock | Mods.DoubleTime => (4.44444, 0.93334),
            Mods.HardRock | Mods.HalfTime   => (-4.44444, 1.866667),
            Mods.Easy                       => (0.0, 0.5),
            Mods.Easy | Mods.DoubleTime     => (4.44444, 0.33334),
            Mods.Easy | Mods.HalfTime       => (-4.44444, 0.66667),
            _ => throw new ArgumentException("Incorrect Mods value.", nameof(mods))
        };
        return modOffset + od * modMultiplier;
    }

    /// <summary>
    /// Calculates objects circle radius based on the beatmap CircleSize and mods combination.
    /// </summary>
    /// <param name="cs">Beatmap's CircleSize.</param>
    /// <param name="mods">Mods used.</param>
    /// <returns>Radius measured in osu!pixels.</returns>
    static double CalculateObjectsRadius(double cs, Mods mods)
    {
        var csModsMask = Mods.Easy | Mods.HardRock;
        return
            54.4 - 4.48 * (mods & csModsMask) switch
            {
                Mods.Easy => cs * 0.5,
                Mods.HardRock => Math.Max(cs * 1.3, 10.0),
                _ => cs
            };
    }

    /// <summary>
    /// Determines if the cursor of the given <paramref name="frame"/> is 
    /// within circle's radius <paramref name="r"/> of the given <paramref name="obj"/>.
    /// </summary>
    /// <param name="obj">Beatmap's object.</param>
    /// <param name="frame">Replay's frame.</param>
    /// <param name="r">Circle radius.</param>
    /// <returns></returns>
    static bool IsOnCircle(HitObject obj, ReplayFrame frame, double r) =>
        Math.Pow(obj.Coords.X - frame.Coords.X, 2) +
        Math.Pow(obj.Coords.Y - frame.Coords.Y, 2) <= Math.Pow(r, 2);

    /// <summary>
    /// Determines if any new key is pressed compared to the previous frame input.
    /// </summary>
    /// <param name="curr">Current frame inputs.</param>
    /// <param name="prev">Previous frame inputs.</param>
    /// <returns><see langword="true"/> if any new key was pressed.</returns>
    static bool IsNewKeyPressed(Inputs curr, Inputs prev) =>
        curr.HasAnyNewInput(prev) && curr != Inputs.None;

    /// <summary>
    /// Determines if the given replay frame is within object's hit-window.
    /// </summary>
    /// <param name="obj">Beatmap object.</param>
    /// <param name="frame">Replay frame.</param>
    /// <param name="border">Hit-window border's absolute value.</param>
    /// <returns><see langword="true"/> if frame is in hit-window.</returns>
    static bool IsInHitWindow(HitObject obj, ReplayFrame frame, double border) =>
        (obj.Ticks - border) <= frame.Ticks &&
        frame.Ticks <= (obj.Ticks + border);
}