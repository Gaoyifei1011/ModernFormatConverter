using Microsoft.UI.Xaml;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Models;

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

        /// <summary>
        /// 检查音频转换类型状态
        /// </summary>
        public static Visibility GetAudioConversionType(AudioConversionTypeKind selectedAudioConversionTypeKind, AudioConversionTypeKind comparedAudioConversionTypeKind, bool needReverse)
        {
            return needReverse ? Equals(selectedAudioConversionTypeKind, comparedAudioConversionTypeKind) ? Visibility.Collapsed : Visibility.Visible : Equals(selectedAudioConversionTypeKind, comparedAudioConversionTypeKind) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 检查是否是音频格式转换
        /// </summary>
        public static Visibility GetIsAudioFormatCovnersion(AudioConversionOutputConfigurationModel audioConversionOutputConfiguration)
        {
            return audioConversionOutputConfiguration is not null ? audioConversionOutputConfiguration.AudioConversionTypeKind is AudioConversionTypeKind.AudioFormatConversion ? Visibility.Visible : Visibility.Collapsed : Visibility.Collapsed;
        }
    }
}
