using Microsoft.UI.Xaml.Data;
using System;

// 抑制 CA1806 警告
#pragma warning disable CA1806

namespace ModernFormatConverter.Views.Converters
{
    /// <summary>
    /// 滑动条数字格式化显示
    /// </summary>
    public class SliderThumbnailToolTipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is double doubleValue ? doubleValue.ToString("0.####") : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
