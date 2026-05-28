using Microsoft.UI.Xaml.Controls;
using ModernFormatConverter.Extensions.DataType.Enums;
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
        // 转换工具列表
        private List<ControlItemModel> ConversionToolsList { get; } =
        [
            new ControlItemModel()
            {
                Title = ResourceService.HomeResource.GetString("VideoConversion"),
                Description = ResourceService.HomeResource.GetString("VideoConversionDescription"),
                ImagePath = "ms-appx:///Assets/ControlIcon/VideoConversion.png",
                NavigationPage = typeof(VideoConversionPage)
            },
            new ControlItemModel()
            {
                Title = ResourceService.HomeResource.GetString("AudioConversion"),
                Description = ResourceService.HomeResource.GetString("AudioConversionDescription"),
                ImagePath = "ms-appx:///Assets/ControlIcon/AudioConversion.png",
                NavigationPage = typeof(AudioConversionPage)
            },
            new ControlItemModel()
            {
                Title = ResourceService.HomeResource.GetString("PhotoConversion"),
                Description = ResourceService.HomeResource.GetString("PhotoConversionDescription"),
                ImagePath = "ms-appx:///Assets/ControlIcon/PhotoConversion.png",
                NavigationPage = typeof(PhotoConversionPage)
            }
        ];

        // 其他工具列表
        private List<ControlItemModel> OtherToolsList { get; } =
        [
            new ControlItemModel()
            {
                Title = ResourceService.HomeResource.GetString("FileInformation"),
                Description = ResourceService.HomeResource.GetString("FileInformationDescription"),
                ImagePath = "ms-appx:///Assets/ControlIcon/FileInformation.png",
                NavigationPage = typeof(FileInformationPage)
            },
            new ControlItemModel()
            {
                Title = ResourceService.HomeResource.GetString("HATest"),
                Description = ResourceService.HomeResource.GetString("HATestDescription"),
                ImagePath = "ms-appx:///Assets/ControlIcon/HATest.png",
                NavigationPage = typeof(HATestPage)
            },
            new ControlItemModel()
            {
                Title = ResourceService.HomeResource.GetString("CustomCommand"),
                Description = ResourceService.HomeResource.GetString("CustomCommandDescription"),
                ImagePath = "ms-appx:///Assets/ControlIcon/CustomCommand.png",
                NavigationPage = typeof(CustomCommandPage)
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
        private async void OnItemClick(object sender, ItemClickEventArgs args)
        {
            if (args.ClickedItem is ControlItemModel controlItem && MainWindow.Current.GetSelectedItem(controlItem.NavigationPage, MainWindow.Current.NavigationViewItemMenuItemsCollection) is NavigationViewItemModel navigationViewItem)
            {
                MainWindow.Current.NavigateTo(navigationViewItem.NavigationPage);
            }
        }

        #endregion 第一部分：主页面——挂载的事件
    }
}
