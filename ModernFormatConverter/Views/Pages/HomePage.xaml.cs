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
                Tag = "VideoConversion"
            },
            new ControlItemModel()
            {
                Title = ResourceService.HomeResource.GetString("AudioConversion"),
                Description = ResourceService.HomeResource.GetString("AudioConversionDescription"),
                ImagePath = "ms-appx:///Assets/ControlIcon/AudioConversion.png",
                Tag = "AudioConversion"
            },
            new ControlItemModel()
            {
                Title = ResourceService.HomeResource.GetString("PhotoConversion"),
                Description = ResourceService.HomeResource.GetString("PhotoConversionDescription"),
                ImagePath = "ms-appx:///Assets/ControlIcon/PhotoConversion.png",
                Tag = "PhotoConversion"
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
        private async void OnItemClick(object sender, ItemClickEventArgs args)
        {
            if (args.ClickedItem is ControlItemModel controlItem)
            {
                if (ConversionToolsList.Contains(controlItem))
                {
                    if (string.Equals(controlItem.Tag, ConversionToolsList[0].Tag, StringComparison.OrdinalIgnoreCase))
                    {
                        ConversionToolsWindow conversionToolsWindow = new(MainWindow.Current, ConversionToolsKind.VideoConversion);
                        if (await conversionToolsWindow.ShowAsync() is ContentDialogResult.Primary)
                        {
                            // TODO：未完成
                        }
                    }
                    else if (string.Equals(controlItem.Tag, ConversionToolsList[1].Tag, StringComparison.OrdinalIgnoreCase))
                    {
                        ConversionToolsWindow conversionToolsWindow = new(MainWindow.Current, ConversionToolsKind.AudioConversion);
                        if (await conversionToolsWindow.ShowAsync() is ContentDialogResult.Primary)
                        {
                            // TODO：未完成
                        }
                    }
                    else if (string.Equals(controlItem.Tag, ConversionToolsList[2].Tag, StringComparison.OrdinalIgnoreCase))
                    {
                        ConversionToolsWindow conversionToolsWindow = new(MainWindow.Current, ConversionToolsKind.PhotoConversion);
                        if (await conversionToolsWindow.ShowAsync() is ContentDialogResult.Primary)
                        {
                            // TODO：未完成
                        }
                    }
                }
                else if (OtherToolsList.Contains(controlItem))
                {
                    if (MainWindow.Current.NavigationItemList.Find(item => string.Equals(item.NavigationTag, controlItem.Tag, StringComparison.OrdinalIgnoreCase)) is NavigationModel navigationItem)
                    {
                        MainWindow.Current.NavigateTo(navigationItem.NavigationPage);
                    }
                    else
                    {
                        MainWindow.Current.NavigateTo(MainWindow.Current.NavigationItemList[1].NavigationPage);
                    }
                }
            }
        }

        #endregion 第一部分：主页面——挂载的事件
    }
}
