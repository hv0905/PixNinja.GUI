using CoenM.ImageHash;
using PixNinja.GUI.Util;

namespace PixNinja.GUI.Models;

public record class ImgFile(string FilePath, int Width, int Height, ulong Hash, long FileSize)
{
    public int Id { get; set; } = -1;
    // The difference bit of the image hash
    // Greater value means less similar

    public byte[]? Sha1Hash { get; set; }
    
    public long Length { get; set; }
    public uint ImageDiff(ImgFile other)
    {
        return BitComparer.BitCount(Hash ^ other.Hash);
    }

    public double ImageSimilarityRatio(ImgFile other)
    {
        return (64 - ImageDiff(other)) / 64.0;
    }
}
