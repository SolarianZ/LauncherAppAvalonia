using System;
using System.Globalization;
using Avalonia.Data.Converters;
using LauncherAppAvalonia.Models;

namespace LauncherAppAvalonia.Converters;

public class LauncherItemTypeToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is LauncherItemType type)
        {
            return type switch
            {
                LauncherItemType.File => "文件",
                LauncherItemType.Folder => "文件夹",
                LauncherItemType.Url => "网址",
                LauncherItemType.Command => "命令",
                _ => value.ToString()
            };
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}