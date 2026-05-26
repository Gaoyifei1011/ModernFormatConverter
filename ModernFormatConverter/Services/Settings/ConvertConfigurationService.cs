using ModernFormatConverter.Extensions.DataType.Constant;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.WindowsAPI.PInvoke.Shell32;
using System;
using System.Diagnostics;
using System.IO;

#pragma warning disable CA1806

namespace ModernFormatConverter.Services.Settings
{
    /// <summary>
    /// 转换配置服务
    /// </summary>
    public static class ConvertConfigurationService
    {
        private static readonly string convertedVideoSavePathKey = ConfigKey.ConvertedVideoSavePathKey;
        private static readonly string convertedAudioSavePathKey = ConfigKey.ConvertedAudioSavePathKey;
        private static readonly string convertedPhotoSavePathKey = ConfigKey.ConvertedPhotoSavePathKey;

        private static string defaultConvertedVideoSavePath;
        private static string defaultConvertedAudioSavePath;
        private static string defaultConvertedPhotoSavePath;

        public static string ConvertedVideoSavePath { get; private set; }

        public static string ConvertedAudioSavePath { get; private set; }

        public static string ConvertedPhotoSavePath { get; private set; }

        /// <summary>
        /// 应用在初始化前获取设置存储的转换配置
        /// </summary>
        public static void InitializeConvertConfiguration()
        {
            Shell32Library.SHGetKnownFolderPath(new("F1B32785-6FBA-4FCF-9D55-7B8E7F157091"), KNOWN_FOLDER_FLAG.KF_FLAG_FORCE_APP_DATA_REDIRECTION, 0, out string localAppDataPath);

            if (!string.IsNullOrEmpty(localAppDataPath))
            {
                try
                {
                    string convertedVideoSavePath = Path.Combine(localAppDataPath, "Videos");
                    string convertedAudioSavePath = Path.Combine(localAppDataPath, "Audios");
                    string convertedPictureSavePath = Path.Combine(localAppDataPath, "Pictures");
                    defaultConvertedVideoSavePath = convertedVideoSavePath;
                    defaultConvertedAudioSavePath = convertedAudioSavePath;
                    defaultConvertedPhotoSavePath = convertedPictureSavePath;
                    Directory.CreateDirectory(convertedVideoSavePath);
                    Directory.CreateDirectory(convertedAudioSavePath);
                    Directory.CreateDirectory(convertedPictureSavePath);
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(ConvertConfigurationService), nameof(InitializeConvertConfiguration), 1, e);
                }
            }
            else
            {
                defaultConvertedVideoSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                defaultConvertedAudioSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                defaultConvertedPhotoSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }

            ConvertedVideoSavePath = GetConvertedVideoSavePath();
            ConvertedAudioSavePath = GetConvertedAudioSavePath();
            ConvertedPhotoSavePath = GetConvertedPhotoSavePath();
        }

        /// <summary>
        /// 获取设置存储的视频输出文件存储路径，如果设置没有存储，使用默认值
        /// </summary>
        private static string GetConvertedVideoSavePath()
        {
            string convertedVideoSavePath = LocalSettingsService.ReadSetting<string>(convertedVideoSavePathKey);

            if (string.IsNullOrEmpty(convertedVideoSavePath))
            {
                SetConvertedVideoSavePath(defaultConvertedVideoSavePath);
                return defaultConvertedVideoSavePath;
            }

            return convertedVideoSavePath;
        }

        /// <summary>
        /// 获取设置存储的音频输出文件存储路径，如果设置没有存储，使用默认值
        /// </summary>
        private static string GetConvertedAudioSavePath()
        {
            string convertedAudioSavePath = LocalSettingsService.ReadSetting<string>(convertedAudioSavePathKey);

            if (string.IsNullOrEmpty(convertedAudioSavePath))
            {
                SetConvertedAudioSavePath(defaultConvertedAudioSavePath);
                return defaultConvertedAudioSavePath;
            }

            return convertedAudioSavePath;
        }

        /// <summary>
        /// 获取设置存储的图片输出文件存储路径，如果设置没有存储，使用默认值
        /// </summary>
        private static string GetConvertedPhotoSavePath()
        {
            string convertedPictureSavePath = LocalSettingsService.ReadSetting<string>(convertedPhotoSavePathKey);

            if (string.IsNullOrEmpty(convertedPictureSavePath))
            {
                SetConvertedPhotoSavePath(defaultConvertedPhotoSavePath);
                return defaultConvertedPhotoSavePath;
            }

            return convertedPictureSavePath;
        }

        /// <summary>
        /// 应用视频输出文件存储路径发生修改时修改设置存储的视频输出文件存储路径
        /// </summary>
        public static void SetConvertedVideoSavePath(string convertedVideoSavePath)
        {
            ConvertedVideoSavePath = convertedVideoSavePath;
            LocalSettingsService.SaveSetting(convertedVideoSavePathKey, convertedVideoSavePath);
        }

        /// <summary>
        /// 应用音频输出文件存储路径发生修改时修改设置存储的音频输出文件存储路径
        /// </summary>
        public static void SetConvertedAudioSavePath(string convertedAudioSavePath)
        {
            ConvertedAudioSavePath = convertedAudioSavePath;
            LocalSettingsService.SaveSetting(convertedAudioSavePathKey, convertedAudioSavePath);
        }

        /// <summary>
        /// 应用图片输出文件存储路径发生修改时修改设置存储的图片输出文件存储路径
        /// </summary>
        public static void SetConvertedPhotoSavePath(string convertedPictureSavePath)
        {
            ConvertedPhotoSavePath = convertedPictureSavePath;
            LocalSettingsService.SaveSetting(convertedPhotoSavePathKey, convertedPictureSavePath);
        }
    }
}
