using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace benqbenq.Osu.ReplayAnalyzer.BeatmapParser.Extensions;

static class StreamReaderExtensions
{
    public static IEnumerable<string> ReadLines(this StreamReader reader)
    {
        while (!reader.EndOfStream)
        {
            yield return reader.ReadLine()!;
        }
    }
}