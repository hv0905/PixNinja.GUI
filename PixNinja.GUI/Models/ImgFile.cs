using CoenM.ImageHash;
using PixNinja.GUI.Util;

namespace PixNinja.GUI.Models;

public record class ImgFile(string FilePath, int Width, int Height, ulong Hash)
{
    public int Id { get; set; } = -1;
    // The difference bit of the image hash
    // Greater value means less similar
    public uint ImageDiff(ImgFile other)
    {
        return BitComparer.BitCount(Hash ^ other.Hash);
    }
}
