using benqbenq.Osu.ReplayAnalyzer;
using benqbenq.Osu.ReplayAnalyzer.BeatmapParser;
using benqbenq.Osu.ReplayAnalyzer.BeatmapParser.Models;
using benqbenq.Osu.ReplayAnalyzer.ReplayParser;
using benqbenq.Osu.ReplayAnalyzer.ReplayParser.Models;

string cd = Directory.GetCurrentDirectory();

string replaysFolderPath = Path.Combine(cd, "replays");
string beatmapsFolderPath = Path.Combine(cd, "beatmaps");

FileInfo[] replayFiles = new DirectoryInfo(replaysFolderPath).GetFiles();
FileInfo[] beatmapFiles = new DirectoryInfo(beatmapsFolderPath).GetFiles();

Console.WriteLine("Replays:\n");

for (int i = 0; i < replayFiles.Length; i++)
{
    Console.WriteLine($"{i}. {replayFiles[i].Name}");
}

var replayIndex = int.Parse(Console.ReadLine());
FileStream replayStream = File.OpenRead(replayFiles[replayIndex].FullName);
ReplayInfo replay = ReplayParser.Decode(replayStream);

Console.WriteLine("Beatmaps:\n");

for (int i = 0; i < beatmapFiles.Length; i++)
{
    Console.WriteLine($"{i}. {beatmapFiles[i].Name}");
}

var beatmapIndex = int.Parse(Console.ReadLine());
FileStream beatmapStream = File.OpenRead(beatmapFiles[beatmapIndex].FullName);
BeatmapInfo beatmap = BeatmapParser.Decode(new StreamReader(beatmapStream));

PrintReplay(replay);
PrintBeatmap(beatmap);
CalculateStatistics(beatmap, replay);

Console.ReadKey();

static void CalculateStatistics(BeatmapInfo beatmap, ReplayInfo replay)
{
    var stats = ReplayAnalyzer.CalculateHitErrorsStats(beatmap, replay);

    Console.WriteLine($"""
        NegHitErrorAvg:   {stats.NegativeHitErrorAvg}
        PosHitErrorAvg:   {stats.PositiveHitErrorAvg}
        Unstable rate:    {stats.UnstableRate}
        """);
}

static void PrintBeatmap(BeatmapInfo b)
{
    Console.WriteLine($"""
        Beatmap info

        [Metadata]
        Song:       {b.Metadata.ArtistRomanised} - {b.Metadata.TitleRomanised}
        DiffName:   {b.Metadata.Version}
        Mapper:     {b.Metadata.Creator}

        [Difficulty]
        AR: {b.Difficulty.ApproachRate}
        CS: {b.Difficulty.CircleSize}
        HP: {b.Difficulty.HPDrainRate}
        OD: {b.Difficulty.OverallDifficulty}

        [HitObjects]
        Circles:    {b.HitObjects.Where(x => x.Type == HitObjectType.HitCircle).Count()}
        Sliders:    {b.HitObjects.Where(x => x.Type == HitObjectType.Slider).Count()}
        Spinners:   {b.HitObjects.Where(x => x.Type == HitObjectType.Spinner).Count()}
        
        """);
}

static void PrintReplay(ReplayInfo r)
{
    Console.WriteLine($"""
        Replay info

        Mode:        {r.Mode}
        Ver:         {r.OsuVersion}
        Map hash:    {r.MapMD5Hash}
        Player:      {r.PlayerNickname}
        Replay hash: {r.ReplayMD5Hash}
        Result:      
                     Score: {r.Score} Combo: {r.MaxCombo}x({r.IsPerfectCombo})
                     {r.Count300} {r.CountGekis}
                     {r.Count100} {r.CountKatus}
                     {r.Count50} {r.CountMisses}

        Mods:        {r.Mods}
        Life bar:    {r.LifeBarGraph.Length}
        Played at:   {r.PlayedAt}
        Frames:      {r.Frames.Length}
        Seed:        {r.Seed}
        ScoreId:     {r.ScoreId}

        """);
}