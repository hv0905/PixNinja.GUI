using System.Text.Json.Serialization;

namespace PixNinja.GUI.Models;

// Required in Trimming
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AppSettings))]
public partial class SourceGenerationContext : JsonSerializerContext {}
