using Microsoft.UI.Xaml;
using ModernFormatConverter.Extensions.DataType.Constant;
using ModernFormatConverter.Services.Root;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ModernFormatConverter.Services.Settings
{
    /// <summary>
    /// 应用主题设置服务
    /// </summary>
    public static class ThemeService
    {
        private static readonly string settingsKey = ConfigKey.ThemeKey;

        private static string defaultAppTheme;

        internal static string AppTheme
        {
            get;

            private set
            {
                if (!string.Equals(field, value))
                {
                    field = value;
                    PropertyChanged?.Invoke(null, new(nameof(AppTheme)));
                }
            }
        }

        internal static ReadOnlyCollection<string> ThemeCollection { get; } = new([nameof(ElementTheme.Default), nameof(ElementTheme.Light), nameof(ElementTheme.Dark)]);

        public static event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 应用在初始化前获取设置存储的主题值
        /// </summary>
        public static void InitializeTheme()
        {
            defaultAppTheme = ThemeCollection.FirstOrDefault(item => string.Equals(item, nameof(ElementTheme.Default), StringComparison.OrdinalIgnoreCase));
            AppTheme = GetTheme();
        }

        /// <summary>
        /// 获取设置存储的主题值，如果设置没有存储，使用默认值
        /// </summary>
        private static string GetTheme()
        {
            string theme = LocalSettingsService.ReadSetting<string>(settingsKey);

            if (string.IsNullOrEmpty(theme))
            {
                SetTheme(defaultAppTheme);
                return defaultAppTheme;
            }

            string selectedTheme = ThemeCollection.FirstOrDefault(item => string.Equals(item, theme, StringComparison.OrdinalIgnoreCase));
            return string.IsNullOrEmpty(selectedTheme) ? defaultAppTheme : selectedTheme;
        }

        /// <summary>
        /// 应用主题发生修改时修改设置存储的主题值
        /// </summary>
        public static void SetTheme(string theme)
        {
            AppTheme = theme;
            LocalSettingsService.SaveSetting(settingsKey, theme);
        }
    }
}
