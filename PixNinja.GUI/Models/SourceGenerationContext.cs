using System.Text.Json.Serialization;

namespace PixNinja.GUI.Models;

// Required in Trimming
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(RemovalListExport))]
public partial class SourceGenerationContext : JsonSerializerContext {}
