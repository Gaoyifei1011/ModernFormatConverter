using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Views.NotificationTips;
using ModernFormatConverter.Views.Windows;

// 抑制 IDE0060 警告
#pragma warning disable IDE0060

namespace ModernFormatConverter.Views.Pages
{
    /// <summary>
    /// 设置高级页面
    /// </summary>
    public sealed partial class SettingsAdvancedPage : Page
    {
        #region 第一部分：构造函数

        public SettingsAdvancedPage()
        {
            InitializeComponent();
        }

        #endregion 第一部分：构造函数

        #region 第二部分：挂载事件处理

        /// <summary>
        /// 打开日志文件夹
        /// </summary>
        private void OnOpenLogFolderClicked(object sender, RoutedEventArgs args)
        {
            LogService.OpenLogFolder();
        }

        /// <summary>
        /// 清除所有日志记录
        /// </summary>
        private async void OnClearClicked(object sender, RoutedEventArgs args)
        {
            bool result = await LogService.ClearLogAsync();
            await MainWindow.Current.ShowNotificationAsync(new OperationResultNotificationTip(OperationKind.LogClean, result));
        }

        #endregion 第二部分：挂载事件处理
    }
}
