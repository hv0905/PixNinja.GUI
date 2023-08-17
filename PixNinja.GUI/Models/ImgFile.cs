using System.IO;
using System.Threading.Tasks;
using Force.Crc32;
using PixNinja.GUI.Util;

namespace PixNinja.GUI.Models;

public record ImgFile(string FilePath, int Width, int Height, ulong Hash, long FileSize)
{
    public int Id { get; set; } = -1;
    // The difference bit of the image hash
    // Greater value means less similar

    public byte[]? FileHash { get; set; }
    public long Length { get; set; }
    public bool MarkedForRemoval { get; set; } = false;
    public uint ImageDiff(ImgFile other)
    {
        return BitComparer.BitCount(Hash ^ other.Hash);
    }

    public double ImageSimilarityRatio(ImgFile other)
    {
        return (64 - ImageDiff(other)) / 64.0;
    }

    public async Task ComputeFileHash()
    {
        using var crc32CComp = new Crc32CAlgorithm();
        await using var fs = File.OpenRead(FilePath);
        FileHash = await crc32CComp.ComputeHashAsync(fs);
    }
}
