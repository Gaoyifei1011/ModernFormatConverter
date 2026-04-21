using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Helpers.Root;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Services.Settings;
using ModernFormatConverter.Views.NotificationTips;
using ModernFormatConverter.Views.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

// 抑制 CA1822，IDE0060 警告
#pragma warning disable CA1822,IDE0060

namespace ModernFormatConverter.Views.Pages
{
    /// <summary>
    /// 设置通用选项页面
    /// </summary>
    public sealed partial class SettingsGeneralPage : Page, INotifyPropertyChanged
    {
        private readonly string BackdropAcrylicString = ResourceService.SettingsGeneralResource.GetString("BackdropAcrylic");
        private readonly string BackdropAcrylicBaseString = ResourceService.SettingsGeneralResource.GetString("BackdropAcrylicBase");
        private readonly string BackdropAcrylicThinString = ResourceService.SettingsGeneralResource.GetString("BackdropAcrylicThin");
        private readonly string BackdropDefaultString = ResourceService.SettingsGeneralResource.GetString("BackdropDefault");
        private readonly string BackdropMicaString = ResourceService.SettingsGeneralResource.GetString("BackdropMica");
        private readonly string BackdropMicaAltString = ResourceService.SettingsGeneralResource.GetString("BackdropMicaAlt");
        private readonly string DesktopAcrylicString = ResourceService.SettingsGeneralResource.GetString("DesktopAcrylic");
        private readonly string MicaString = ResourceService.SettingsGeneralResource.GetString("Mica");
        private readonly string ThemeDarkString = ResourceService.SettingsGeneralResource.GetString("ThemeDark");
        private readonly string ThemeDefaultString = ResourceService.SettingsGeneralResource.GetString("ThemeDefault");
        private readonly string ThemeLightAltString = ResourceService.SettingsGeneralResource.GetString("ThemeLight");
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;

        private DictionaryEntry _theme;

        public DictionaryEntry Theme
        {
            get { return _theme; }

            set
            {
                _theme = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Theme)));
            }
        }

        private DictionaryEntry _backdrop = default;

        public DictionaryEntry Backdrop
        {
            get { return _backdrop; }

            set
            {
                _backdrop = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Backdrop)));
            }
        }

        private bool _alwaysShowBackdropValue = AlwaysShowBackdropService.AlwaysShowBackdropValue;

        public bool AlwaysShowBackdropValue
        {
            get { return _alwaysShowBackdropValue; }

            set
            {
                _alwaysShowBackdropValue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AlwaysShowBackdropValue)));
            }
        }

        private bool _alwaysShowBackdropEnabled;

        public bool AlwaysShowBackdropEnabled
        {
            get { return _alwaysShowBackdropEnabled; }

            set
            {
                _alwaysShowBackdropEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AlwaysShowBackdropEnabled)));
            }
        }

        private bool _advancedEffectsEnabled;

        public bool AdvancedEffectsEnabled
        {
            get { return _advancedEffectsEnabled; }

            set
            {
                _advancedEffectsEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AdvancedEffectsEnabled)));
            }
        }

        private DictionaryEntry _appLanguage = LanguageService.AppLanguage;

        public DictionaryEntry AppLanguage
        {
            get { return _appLanguage; }

            set
            {
                _appLanguage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AppLanguage)));
            }
        }

        private List<DictionaryEntry> ThemeList { get; } = [];

        private List<DictionaryEntry> BackdropList { get; } = [];

        private WinRTObservableCollection<DictionaryEntry> LanguageCollection { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsGeneralPage()
        {
            InitializeComponent();

            AdvancedEffectsEnabled = IsAdvancedEffectsEnabled();
            ThemeList.Add(new DictionaryEntry(ThemeService.ThemeList[0], ThemeDefaultString));
            ThemeList.Add(new DictionaryEntry(ThemeService.ThemeList[1], ThemeLightAltString));
            ThemeList.Add(new DictionaryEntry(ThemeService.ThemeList[2], ThemeDarkString));
            Theme = ThemeList.Find(item => Equals(item.Key, ThemeService.AppTheme));

            BackdropList.Add(new DictionaryEntry(BackdropService.BackdropList[0], BackdropDefaultString));
            if (MicaController.IsSupported())
            {
                BackdropList.Add(new DictionaryEntry(BackdropService.BackdropList[1], string.Format("{0} {1}", MicaString, BackdropMicaString)));
                BackdropList.Add(new DictionaryEntry(BackdropService.BackdropList[2], string.Format("{0} {1}", MicaString, BackdropMicaAltString)));
            }
            if (DesktopAcrylicController.IsSupported())
            {
                BackdropList.Add(new DictionaryEntry(BackdropService.BackdropList[3], string.Format("{0} {1}", DesktopAcrylicString, BackdropAcrylicString)));
                BackdropList.Add(new DictionaryEntry(BackdropService.BackdropList[4], string.Format("{0} {1}", DesktopAcrylicString, BackdropAcrylicBaseString)));
                BackdropList.Add(new DictionaryEntry(BackdropService.BackdropList[5], string.Format("{0} {1}", DesktopAcrylicString, BackdropAcrylicThinString)));
            }
            Backdrop = BackdropList.Find(item => Equals(item.Key, BackdropService.AppBackdrop));

            foreach (DictionaryEntry languageItem in LanguageService.LanguageList)
            {
                LanguageCollection.Add(languageItem);
                if (Equals(LanguageService.AppLanguage.Key, languageItem.Key))
                {
                    AppLanguage = languageItem;
                }
            }

            AlwaysShowBackdropEnabled = IsAdvancedEffectsEnabled() && !string.Equals(Backdrop.Key, BackdropList[0].Key);
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
            GlobalNotificationService.ApplicationExit += OnApplicationExit;
        }

        #region 第一部分：设置通用选项页面——挂载的事件

        /// <summary>
        /// 打开系统主题设置
        /// </summary>
        private void OnSystemThemeSettingsClicked(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>
            {
                try
                {
                    Process.Start("ms-settings:colors");
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(SettingsGeneralPage), nameof(OnSystemThemeSettingsClicked), 1, e);
                }
            });
        }

        /// <summary>
        /// 主题选项修改后触发的事件
        /// </summary>
        private void OnThemeSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry theme && !Equals(Theme, theme))
            {
                Theme = theme;
                ThemeService.SetTheme(Convert.ToString(Theme.Key));
            }
        }

        /// <summary>
        /// 背景色选项修改后触发的事件
        /// </summary>
        private void OnBackdropSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry backdrop && !Equals(Backdrop, backdrop))
            {
                Backdrop = backdrop;
                BackdropService.SetBackdrop(Convert.ToString(Backdrop.Key));
                AlwaysShowBackdropEnabled = IsAdvancedEffectsEnabled() && !string.Equals(Backdrop.Key, BackdropList[0].Key);

                if (Equals(Backdrop, BackdropList[0]))
                {
                    AlwaysShowBackdropService.SetAlwaysShowBackdropValue(false);
                    AlwaysShowBackdropValue = false;
                }
            }
        }

        /// <summary>
        /// 打开系统主题色设置
        /// </summary>
        private void OnSystemBackdropSettingsClicked(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>
            {
                try
                {
                    Process.Start("ms-settings:easeofaccess-visualeffects");
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(SettingsGeneralPage), nameof(OnSystemBackdropSettingsClicked), 1, e);
                }
            });
        }

        /// <summary>
        /// 打开系统语言设置
        /// </summary>
        private void OnSystemLanguageSettingsClicked(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>
            {
                try
                {
                    Process.Start("ms-settings:regionlanguage-languageoptions");
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(SettingsGeneralPage), nameof(OnSystemLanguageSettingsClicked), 1, e);
                }
            });
        }

        /// <summary>
        /// 是否开启始终显示背景色
        /// </summary>
        private void OnAlwaysShowBackdropToggled(object sender, RoutedEventArgs args)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                AlwaysShowBackdropService.SetAlwaysShowBackdropValue(toggleSwitch.IsOn);
                AlwaysShowBackdropValue = toggleSwitch.IsOn;
            }
        }

        /// <summary>
        /// 语言设置选项修改后触发的事件
        /// </summary>
        private async void OnLanguageSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is DictionaryEntry language && !Equals(AppLanguage, language))
            {
                AppLanguage = language;

                LanguageService.SetLanguage(AppLanguage);
                await MainWindow.Current.ShowNotificationAsync(new OperationResultNotificationTip(OperationKind.LanguageChange));
            }
        }

        #endregion 第一部分：设置通用选项页面——挂载的事件

        #region 第三部分：自定义事件

        /// <summary>
        /// 在用户首选项发生更改时触发的事件
        /// </summary>
        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs args)
        {
            synchronizationContext.Post(_ =>
            {
                bool isAdvancedEffectsEnabled = IsAdvancedEffectsEnabled();
                AdvancedEffectsEnabled = isAdvancedEffectsEnabled;
                AlwaysShowBackdropEnabled = isAdvancedEffectsEnabled && !string.Equals(Backdrop.Key, BackdropList[0].Key);
            }, null);
        }

        /// <summary>
        /// 应用程序即将关闭时发生的事件
        /// </summary>
        private void OnApplicationExit()
        {
            try
            {
                GlobalNotificationService.ApplicationExit -= OnApplicationExit;
                SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(SettingsGeneralPage), nameof(OnApplicationExit), 1, e);
            }
        }

        #endregion 第三部分：自定义事件

        /// <summary>
        /// 检查是否启用系统透明度效果设置
        /// </summary>
        private bool IsAdvancedEffectsEnabled()
        {
            return RegistryHelper.ReadRegistryKey<bool>(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency");
        }
    }
}
