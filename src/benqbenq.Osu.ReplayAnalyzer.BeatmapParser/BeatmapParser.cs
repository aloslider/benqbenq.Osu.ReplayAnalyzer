using benqbenq.Osu.ReplayAnalyzer.BeatmapParser.Extensions;
using benqbenq.Osu.ReplayAnalyzer.BeatmapParser.Models;
using Salaros.Configuration;
using System.Globalization;
using System.Text;

namespace benqbenq.Osu.ReplayAnalyzer.BeatmapParser;

public static class BeatmapParser
{
    const string MetadataSection   = "Metadata";
    const string DifficultySection = "Difficulty";
    const string HitObjectsSection = "HitObjects"; 
    static CultureInfo DefaultFormat => CultureInfo.InvariantCulture;

    public static BeatmapInfo Decode(StreamReader reader)
    {
        var configAsOneLine =
            reader
            .ReadLines()
            .Select(s => { var ns = s.Trim(); return ns; })
            .Where(s => s != string.Empty)
            .Aggregate(
                new StringBuilder(), 
                (sb, s) =>
                {
                    sb.Append(s);
                    sb.AppendLine();
                    return sb;
                })
            .ToString();
        ConfigParser parser = GetParser(configAsOneLine);

        // [Metadata]
        string? titleRomanised  = parser[MetadataSection][nameof(Metadata.TitleRomanised)];
        string? titleUnicode    = parser[MetadataSection][nameof(Metadata.TitleUnicode)];
        string? artistRomanised = parser[MetadataSection][nameof(Metadata.ArtistRomanised)];
        string? artistUnicode   = parser[MetadataSection][nameof(Metadata.ArtistUnicode)];
        string? creator         = parser[MetadataSection][nameof(Metadata.Creator)];
        string? version         = parser[MetadataSection][nameof(Metadata.Version)];
        string? source          = parser[MetadataSection][nameof(Metadata.Source)];
        string[]? tags          = parser[MetadataSection][nameof(Metadata.Tags)]?.Split(' ');
        int? beatmapID          = int.Parse(parser[MetadataSection][nameof(Metadata.BeatmapID)]);
        int? beatmapSetID       = int.Parse(parser[MetadataSection][nameof(Metadata.BeatmapSetID)]);
        Metadata md = new(
            titleRomanised, titleUnicode,
            artistRomanised, artistUnicode,
            creator, version,
            source, tags,
            beatmapID, beatmapSetID);

        // [Difficulty]
        double ar               = double.Parse(parser[DifficultySection][nameof(DifficultyInfo.ApproachRate)],DefaultFormat);
        double cs               = double.Parse(parser[DifficultySection][nameof(DifficultyInfo.CircleSize)], DefaultFormat);
        double hp               = double.Parse(parser[DifficultySection][nameof(DifficultyInfo.HPDrainRate)], DefaultFormat);
        double od               = double.Parse(parser[DifficultySection][nameof(DifficultyInfo.OverallDifficulty)], DefaultFormat);
        double sliderTickRate   = double.Parse(parser[DifficultySection][nameof(DifficultyInfo.SliderTickRate)], DefaultFormat);
        double sliderMultiplier = double.Parse(parser[DifficultySection][nameof(DifficultyInfo.SliderMultiplier)], DefaultFormat);
        DifficultyInfo di = new(ar, cs, hp, od, sliderTickRate, sliderMultiplier);

        // [HitObjects]
        HitObject[] ho =
            parser[HitObjectsSection].Keys
            .Select(line =>
            {
                string[] tokens = line.ToString()!.Split(',');
                int x = int.Parse(tokens[0]);
                int y = int.Parse(tokens[1]);
                double time = double.Parse(tokens[2]);
                HitObjectType type = ParseTypeField(tokens[3]);
                return new HitObject(new(x, y), time,type);
            })
            .ToArray();

        return new()
        {
            Metadata = md,
            Difficulty = di,
            HitObjects = ho
        };
    }

    static HitObjectType ParseTypeField(string field)
    {
        const int typeMask  = 0b_1000_1011;
        const int hitCircle = 1 << 0;
        const int slider    = 1 << 1;
        const int spinner   = 1 << 3;
        const int maniaNote = 1 << 7;

        int typeAsInt = int.Parse(field);
        return (typeAsInt & typeMask) switch
        {
            hitCircle => HitObjectType.HitCircle,
            slider => HitObjectType.Slider,
            spinner => HitObjectType.Spinner,
            maniaNote => HitObjectType.ManiaNote,
            _ => throw new FormatException($"Hitobject \"{field}\" info has incorrect format.")
        };
    }

    static ConfigParser GetParser(string content) =>
        new ConfigParser(
            content,
            new ConfigParserSettings()
            {
                KeyValueSeparator = ":",
                CommentCharacters = new string[] { "//" },
                MultiLineValues = 
                    MultiLineValues.Simple
                    | MultiLineValues.AllowValuelessKeys 
                    | MultiLineValues.AllowEmptyTopSection
            });
}