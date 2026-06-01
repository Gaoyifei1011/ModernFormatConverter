using Microsoft.UI.Xaml.Data;
using ModernFormatConverter.Helpers.Root;
using System;

// 抑制 CA1806 警告
#pragma warning disable CA1806

namespace ModernFormatConverter.Views.ValueConveters
{
    /// <summary>
    /// 文件大小值转换器
    /// </summary>
    public class FileSizeValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is long fileSize ? VolumeSizeHelper.ConvertVolumeSizeToString(fileSize) : "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
