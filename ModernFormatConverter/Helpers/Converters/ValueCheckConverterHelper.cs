using Microsoft.UI.Xaml;
using ModernFormatConverter.Extensions.DataType.Enums;

namespace ModernFormatConverter.Helpers.Converters
{
    /// <summary>
    /// 值检查辅助类
    /// </summary>
    public static class ValueCheckConverterHelper
    {
        /// <summary>
        /// 检查更新应用状态
        /// </summary>
        public static Visibility CheckUpdateAppResultKind(UpdateAppResultKind updateAppResultKind, UpdateAppResultKind comparedUpdateAppResultKind)
        {
            return Equals(updateAppResultKind, comparedUpdateAppResultKind) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
