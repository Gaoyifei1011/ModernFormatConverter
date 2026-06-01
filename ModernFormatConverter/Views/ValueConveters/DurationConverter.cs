using Microsoft.UI.Xaml.Data;
using System;

// 抑制 CA1806 警告
#pragma warning disable CA1806

namespace ModernFormatConverter.Views.ValueConveters
{
    /// <summary>
    /// 时间值转换器
    /// </summary>
    public class DurationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is TimeSpan timeSpan ? Equals(value, TimeSpan.Zero) ? "00:00:00"   : string.Format(@"{0:00}:{1:00}:{2:00}", Math.Truncate(timeSpan.TotalHours), timeSpan.Minutes, timeSpan.Minutes): "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
