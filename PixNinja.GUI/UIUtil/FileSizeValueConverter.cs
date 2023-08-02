using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace PixNinja.GUI.UIUtil;

public class FileSizeValueConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not (int or uint or long or ulong))
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        var len = System.Convert.ToUInt64(value);
        return len switch
        {
            > 1024 * 1024 * 1024 => $"{len / 1024.0 / 1024.0 / 1024.0:F2}G",
            > 1024 * 1024 => $"{len / 1024.0 / 1024.0:F2}M",
            > 1024 => $"{len / 1024.0:F2}K",
            _ => $"{len}B"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
