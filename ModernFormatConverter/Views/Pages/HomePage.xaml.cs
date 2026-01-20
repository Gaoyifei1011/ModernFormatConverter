using Microsoft.UI.Xaml.Controls;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Views.Windows;
using System;
using System.Collections.Generic;

// 抑制 IDE0060 警告
#pragma warning disable IDE0060

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
            new ControlItemModel()
            {
                Title = ResourceService.HomeResource.GetString("FileInformation"),
                Description = ResourceService.HomeResource.GetString("FileInformationDescription"),
                ImagePath = "ms-appx:///Assets/ControlIcon/FileInformation.png",
                Tag = "FileInformation"
            },
            new ControlItemModel()
            {
                Title = ResourceService.HomeResource.GetString("HATest"),
                Description = ResourceService.HomeResource.GetString("HATestDescription"),
                ImagePath = "ms-appx:///Assets/ControlIcon/HATest.png",
                Tag = "HATest"
            },
            new ControlItemModel()
            {
                Title = ResourceService.HomeResource.GetString("CustomCommand"),
                Description = ResourceService.HomeResource.GetString("CustomCommandDescription"),
                ImagePath = "ms-appx:///Assets/ControlIcon/CustomCommand.png",
                Tag = "CustomCommand"
            }
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
            if (args.ClickedItem is ControlItemModel controlItem)
            {
                if (OtherToolsList.Contains(controlItem) && MainWindow.Current.NavigationItemList.Find(item => string.Equals(item.NavigationTag, controlItem.Tag, StringComparison.OrdinalIgnoreCase)) is NavigationModel navigationItem)
                {
                    MainWindow.Current.NavigateTo(navigationItem.NavigationPage);
                }
                else
                {
                    MainWindow.Current.NavigateTo(MainWindow.Current.NavigationItemList[1].NavigationPage);
                }
            }
        }

        #endregion 第一部分：主页面——挂载的事件
    }
}
