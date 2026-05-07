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

        /// <summary>
        /// 检查硬件加速测试状态
        /// </summary>
        public static Visibility CheckHATestResultKind(HATestResultKind haTestResultKind, HATestResultKind comparedHATestResultKind)
        {
            return Equals(haTestResultKind, comparedHATestResultKind) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 检查视频转换类型状态
        /// </summary>
        public static Visibility GetVideoConversionType(VideoConversionTypeKind selectedVideoConversionTypeKind, VideoConversionTypeKind comparedVideoConversionTypeKind, bool needReverse)
        {
            return needReverse ? Equals(selectedVideoConversionTypeKind, comparedVideoConversionTypeKind) ? Visibility.Collapsed : Visibility.Visible : Equals(selectedVideoConversionTypeKind, comparedVideoConversionTypeKind) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
