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
        private static readonly string convertedPictureSavePathKey = ConfigKey.ConvertedPictureSavePathKey;
        private static readonly string convertedDocumentSavePathKey = ConfigKey.ConvertedDocumentSavePathKey;

        private static string defaultConvertedVideoSavePath;
        private static string defaultConvertedAudioSavePath;
        private static string defaultConvertedPictureSavePath;
        private static string defaultConvertedDocumentSavePath;

        public static string ConvertedVideoSavePath { get; private set; }

        public static string ConvertedAudioSavePath { get; private set; }

        public static string ConvertedPictureSavePath { get; private set; }

        public static string ConvertedDocumentSavePath { get; private set; }

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
                    string convertedDocumentsSavePath = Path.Combine(localAppDataPath, "Documents");
                    defaultConvertedVideoSavePath = convertedVideoSavePath;
                    defaultConvertedAudioSavePath = convertedAudioSavePath;
                    defaultConvertedPictureSavePath = convertedPictureSavePath;
                    defaultConvertedDocumentSavePath = convertedDocumentsSavePath;
                    Directory.CreateDirectory(convertedVideoSavePath);
                    Directory.CreateDirectory(convertedAudioSavePath);
                    Directory.CreateDirectory(convertedPictureSavePath);
                    Directory.CreateDirectory(convertedDocumentsSavePath);
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
                defaultConvertedPictureSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                defaultConvertedDocumentSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            ConvertedVideoSavePath = GetConvertedVideoSavePath();
            ConvertedAudioSavePath = GetConvertedAudioSavePath();
            ConvertedPictureSavePath = GetConvertedPictureSavePath();
            ConvertedDocumentSavePath = GetConvertedDocumentSavePath();
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
        private static string GetConvertedPictureSavePath()
        {
            string convertedPictureSavePath = LocalSettingsService.ReadSetting<string>(convertedPictureSavePathKey);

            if (string.IsNullOrEmpty(convertedPictureSavePath))
            {
                SetConvertedPictureSavePath(defaultConvertedPictureSavePath);
                return defaultConvertedPictureSavePath;
            }

            return convertedPictureSavePath;
        }

        /// <summary>
        /// 获取设置存储的文档输出文件存储路径，如果设置没有存储，使用默认值
        /// </summary>
        private static string GetConvertedDocumentSavePath()
        {
            string convertedDocumentSavePath = LocalSettingsService.ReadSetting<string>(convertedDocumentSavePathKey);

            if (string.IsNullOrEmpty(convertedDocumentSavePath))
            {
                SetConvertedDocumentSavePath(defaultConvertedDocumentSavePath);
                return defaultConvertedDocumentSavePath;
            }

            return convertedDocumentSavePath;
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
        public static void SetConvertedPictureSavePath(string convertedPictureSavePath)
        {
            ConvertedPictureSavePath = convertedPictureSavePath;
            LocalSettingsService.SaveSetting(convertedPictureSavePathKey, convertedPictureSavePath);
        }

        /// <summary>
        /// 应用文档输出文件存储路径发生修改时修改设置存储的文档输出文件存储路径
        /// </summary>
        public static void SetConvertedDocumentSavePath(string convertedDocumentSavePath)
        {
            ConvertedDocumentSavePath = convertedDocumentSavePath;
            LocalSettingsService.SaveSetting(convertedDocumentSavePathKey, convertedDocumentSavePath);
        }
    }
}
