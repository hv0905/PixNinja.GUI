using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace PixNinja.GUI.UIUtil;

public class FileSizeValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int || value is uint || value is long || value is ulong)
        { 
            ulong len = (ulong)value;
            return len switch
            {
              > 1024 * 1024 * 1024 => $"{(len / 1024.0 / 1024.0 / 1024.0):F2}G",
              > 1024 * 1024 => $"{(len / 1024.0 / 1024.0):F2}M",
              > 1024 => $"{(len / 1024.0):F2}K",
              _ => $"{len}B"
            };
        }
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
