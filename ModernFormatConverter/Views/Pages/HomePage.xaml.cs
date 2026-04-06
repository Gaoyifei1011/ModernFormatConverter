using Microsoft.UI.Windowing;
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
            },
            new ControlItemModel()
            {
                Title = ResourceService.HomeResource.GetString("DocumentConversion"),
                Description = ResourceService.HomeResource.GetString("DocumentConversionDescription"),
                ImagePath = "ms-appx:///Assets/ControlIcon/DocumentConversion.png",
                Tag = "DocumentConversion"
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
        private void OnItemClick(object sender, ItemClickEventArgs args)
        {
            if (args.ClickedItem is ControlItemModel controlItem)
            {
                if (ConversionToolsList.Contains(controlItem))
                {
                    if (string.Equals(controlItem.Tag, ConversionToolsList[0].Tag, StringComparison.OrdinalIgnoreCase))
                    {
                        OverlappedPresenter overlappedPresenter = OverlappedPresenter.CreateForDialog();
                        overlappedPresenter.IsResizable = true;
                        overlappedPresenter.IsMinimizable = false;
                        overlappedPresenter.IsMaximizable = false;
                        overlappedPresenter.IsModal = true;
                        AppWindow videoConversionAppWindow = AppWindow.Create(overlappedPresenter, MainWindow.Current.AppWindow.Id);
                        VideoConversionPage videoConversionPage = new(videoConversionAppWindow);
                        videoConversionAppWindow.Show();
                    }
                    else if (string.Equals(controlItem.Tag, ConversionToolsList[1].Tag, StringComparison.OrdinalIgnoreCase))
                    {
                        OverlappedPresenter overlappedPresenter = OverlappedPresenter.CreateForDialog();
                        overlappedPresenter.IsResizable = true;
                        overlappedPresenter.IsMinimizable = false;
                        overlappedPresenter.IsMaximizable = false;
                        overlappedPresenter.IsModal = true;
                        AppWindow audioConversionAppWindow = AppWindow.Create(overlappedPresenter, MainWindow.Current.AppWindow.Id);
                        AudioConversionPage audioConversionPage = new(audioConversionAppWindow);
                        audioConversionAppWindow.Show();
                    }
                    else if (string.Equals(controlItem.Tag, ConversionToolsList[2].Tag, StringComparison.OrdinalIgnoreCase))
                    {
                        OverlappedPresenter overlappedPresenter = OverlappedPresenter.CreateForDialog();
                        overlappedPresenter.IsResizable = true;
                        overlappedPresenter.IsMinimizable = false;
                        overlappedPresenter.IsMaximizable = false;
                        overlappedPresenter.IsModal = true;
                        AppWindow photoConversionAppWindow = AppWindow.Create(overlappedPresenter, MainWindow.Current.AppWindow.Id);
                        PhotoConversionPage photoConversionPage = new(photoConversionAppWindow);
                        photoConversionAppWindow.Show();
                    }
                    else if (string.Equals(controlItem.Tag, ConversionToolsList[3].Tag, StringComparison.OrdinalIgnoreCase))
                    {
                        OverlappedPresenter overlappedPresenter = OverlappedPresenter.CreateForDialog();
                        overlappedPresenter.IsResizable = true;
                        overlappedPresenter.IsMinimizable = false;
                        overlappedPresenter.IsMaximizable = false;
                        overlappedPresenter.IsModal = true;
                        AppWindow documentConversionAppWindow = AppWindow.Create(overlappedPresenter, MainWindow.Current.AppWindow.Id);
                        DocumentConversionPage documentConversionPage = new(documentConversionAppWindow);
                        documentConversionAppWindow.Show();
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
