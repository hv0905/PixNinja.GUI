using System;
using System.Text.Json.Serialization;

namespace PixNinja.GUI.Models;

public record AppSettings
{
    public int Similarity { get; set; } = 2;
    public ImageHashAlgo HashAlgo { get; set; } = ImageHashAlgo.PHash;
    public int MaxThreadCount { get; set; } = Math.Max(Environment.ProcessorCount / 2, Environment.ProcessorCount - 4);
    public int MinFileSizeBytes { get; set; }


}