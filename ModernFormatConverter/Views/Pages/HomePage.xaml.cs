using Microsoft.UI.Xaml.Controls;
using ModernFormatConverter.Models;
using ModernFormatConverter.Views.Windows;
using System;
using System.Collections.Generic;

namespace ModernFormatConverter.Views.Pages
{
    /// <summary>
    /// 主页面
    /// </summary>
    public sealed partial class HomePage : Page
    {
        // 视频工具列表
        private List<ControlItemModel> VideoToolsList { get; } =
        [
        ];

        // 音频工具列表
        private List<ControlItemModel> AudioToolsList { get; } =
        [
        ];

        // 图片工具列表
        private List<ControlItemModel> PhotoToolsList { get; } =
        [
        ];

        // 文档工具列表
        private List<ControlItemModel> DocumentToolsList { get; } =
        [
        ];

        // 其他工具列表
        private List<ControlItemModel> OtherToolsList { get; } =
        [
        ];

        public HomePage()
        {
            InitializeComponent();
        }

        #region 第一部分：主页面——挂载的事件

        /// <summary>
        /// 点击条目时进入条目对应的页面
        /// </summary>
        private void OnItemClick(object sender, ItemClickEventArgs args)
        {
            if (args.ClickedItem is ControlItemModel controlItem && MainWindow.Current.NavigationItemList.Find(item => string.Equals(item.NavigationTag, controlItem.Tag, StringComparison.OrdinalIgnoreCase)) is NavigationModel navigationItem)
            {
                MainWindow.Current.NavigateTo(navigationItem.NavigationPage);
            }
        }

        #endregion 第一部分：主页面——挂载的事件
    }
}
