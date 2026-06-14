using System;
using System.Collections.Generic;

namespace PixNinja.GUI.Models;

public record RemovalListExport(DateTimeOffset ExportedAt, int Count, List<RemovalFileExport> Files);

public record RemovalFileExport(
    string FilePath,
    string FileName,
    long FileSize,
    int Width,
    int Height,
    string Hash);
