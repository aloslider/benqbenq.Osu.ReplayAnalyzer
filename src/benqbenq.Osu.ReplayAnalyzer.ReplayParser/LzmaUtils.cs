using SevenZip.Compression.LZMA;

namespace benqbenq.Osu.ReplayAnalyzer.ReplayParser;

internal static class LzmaUtils
{
    public static byte[] Decompress(byte[] inBytes)
    {
        using var ms = new MemoryStream(inBytes, false);
        return Decompress(ms).ToArray();
    }

    public static MemoryStream Decompress(Stream inStream)
    {
        var decoder = new Decoder();

        byte[] properties = new byte[5];
        if (inStream.Read(properties, 0, 5) != 5)
            throw new Exception("input .lzma is too short");
        decoder.SetDecoderProperties(properties);

        long outSize = 0;
        for (int i = 0; i < 8; i++)
        {
            int v = inStream.ReadByte();
            if (v < 0)
                break;
            outSize |= ((long)(byte)v) << (8 * i);
        }
        long compressedSize = inStream.Length - inStream.Position;

        var outStream = new MemoryStream();
        decoder.Code(inStream, outStream, compressedSize, outSize, null);
        outStream.Flush();
        outStream.Position = 0;
        return outStream;
    }
}
