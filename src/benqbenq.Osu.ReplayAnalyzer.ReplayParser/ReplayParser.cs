using benqbenq.Osu.ReplayAnalyzer.ReplayParser.Models;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;

namespace benqbenq.Osu.ReplayAnalyzer.ReplayParser;

public static class ReplayParser
{
    const char PairsDelimeter = ',';
    const char ValuesDelimeter = '|';
    const string SeedId = "-12345";
    static CultureInfo DefaultFormat => CultureInfo.InvariantCulture;

    public static ReplayInfo Decode(FileStream stream)
    {
        ReplayBinaryReader reader = new(stream);
        ErrorAt error = ErrorAt.None;

        try
        {
            error = ErrorAt.Mode;
            var mode = (Mode)reader.ReadByte();

            error = ErrorAt.Version;
            int version = reader.ReadInt32();

            error = ErrorAt.MapHashMD5;
            string mapHash = reader.ReadString();

            error = ErrorAt.PlayerNickname;
            string playerNickname = reader.ReadString();

            error = ErrorAt.ReplayHashMD5;
            string replayHash = reader.ReadString();

            error = ErrorAt.Count300;
            int count300 = reader.ReadInt16();

            error = ErrorAt.Count100;
            int count100 = reader.ReadInt16();

            error = ErrorAt.Count50;
            int count50 = reader.ReadInt16();

            error = ErrorAt.CountGekis;
            int countGekis = reader.ReadInt16();

            error = ErrorAt.CountKatus;
            int countKatus = reader.ReadInt16();

            error = ErrorAt.CountMisses;
            int countMisses = reader.ReadInt16();
                
            error = ErrorAt.Score;
            int score = reader.ReadInt32();

            error = ErrorAt.Combo;
            int maxCombo = reader.ReadInt16();

            error = ErrorAt.IsPerfectCombo;
            bool isPerfectCombo = reader.ReadBoolean();

            error = ErrorAt.Mods;
            var mods = (Mods)reader.ReadInt32();

            error = ErrorAt.LifeBarData;
            string lifeBarData = reader.ReadString();
            var lifeBarGraph = 
                string.IsNullOrEmpty(lifeBarData)
                ? Array.Empty<LifeBarPoint>()
                : lifeBarData
                    .Split(PairsDelimeter)
                    .Select(pair => pair.Split(ValuesDelimeter))
                    .Where(values => values.Length is 2)
                    .Select(values =>
                    {
                        var time = TimeSpan.FromMilliseconds(int.Parse(values[0]));
                        var perc = float.Parse(values[1], DefaultFormat);
                        return new LifeBarPoint(time, perc);
                    })
                    .ToArray();

            error = ErrorAt.PlayedAt;
            DateTime playedAt = reader.ReadDateTime();

            error = ErrorAt.RawDataLength;
            int rawDataLength = reader.ReadInt32();

            error = ErrorAt.RawData;
            int seed = 0;
            int lastTime = 0;
            var frames = 
                rawDataLength <= 0
                ? Array.Empty<ReplayFrame>()
                : Encoding.ASCII.GetString(LzmaUtils.Decompress(reader.ReadBytes(rawDataLength)))
                    .Split(PairsDelimeter)
                    .Where(pair => !string.IsNullOrEmpty(pair))
                    .Select(pair => pair.Split(ValuesDelimeter))
                    .Where(values => values.Length is 4)
                    .Select(values =>
                    {
                        error = ErrorAt.Seed;

                        if (values[0] is SeedId)
                        {
                            seed = int.Parse(values[3]);
                        }

                        return values;
                    })
                    .Select(values =>
                    {
                        error = ErrorAt.RawData;
                        Vector2 coords = new()
                        {
                            X = float.Parse(values[1], DefaultFormat),
                            Y = float.Parse(values[2], DefaultFormat)
                        };
                        int time = lastTime + int.Parse(values[0], DefaultFormat);
                        lastTime = time;
                        var inputs = (Inputs)int.Parse(values[3], DefaultFormat);
                        return new ReplayFrame(coords, time, inputs);
                    })
                    .ToArray();

            error = ErrorAt.ScoreId;
            var leftDataLength = reader.BaseStream.Length - reader.BaseStream.Position;
            var scoreId =
                leftDataLength is 4
                ? reader.ReadInt32()
                : reader.ReadInt64();

            return new ReplayInfo(
                mode, version, mapHash, playerNickname, replayHash,
                count300, count100, count50, countGekis, countKatus, countMisses,
                score, maxCombo, isPerfectCombo, mods, lifeBarGraph,
                playedAt, frames, seed, scoreId);
        }
        catch (Exception ex) 
            when (ex is 
                FormatException or
                EndOfStreamException or
                ArgumentException)
        {
            throw new FormatException($"Error occured while parsing {error} file field.", ex);
        }
        finally
        {
            reader.Dispose();
        }
    }
}

internal class ReplayBinaryReader : BinaryReader
{
    public ReplayBinaryReader(FileStream input) : base(input) { }

    public override string ReadString() =>
        ReadByte() switch
        {
            0 => null,
            _ => base.ReadString()
        };

    public DateTime ReadDateTime()
    {
        long ticks = ReadInt64();
        if (ticks < 0) ticks = 0;
        return new DateTime(ticks, DateTimeKind.Utc);
    }
}

internal enum ErrorAt
{
    None,
    Mode,
    Version,
    MapHashMD5,
    PlayerNickname,
    ReplayHashMD5,
    Count300,
    Count100,
    Count50,
    CountGekis,
    CountKatus,
    CountMisses,
    Score,
    Combo,
    IsPerfectCombo,
    Mods,
    LifeBarData,
    PlayedAt,
    RawDataLength,
    RawData,
    Seed,
    ScoreId
}