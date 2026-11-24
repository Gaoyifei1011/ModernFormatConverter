using Microsoft.UI.Xaml.Controls;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Views.Windows;
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
        #region 第一部分：常量、资源与状态字段

        private readonly string AudioConversionString = ResourceService.HomeResource.GetString("AudioConversion");
        private readonly string AudioConversionDescriptionString = ResourceService.HomeResource.GetString("AudioConversionDescription");
        private readonly string ConversionToolsString = ResourceService.HomeResource.GetString("ConversionTools");
        private readonly string CustomCommandString = ResourceService.HomeResource.GetString("CustomCommand");
        private readonly string CustomCommandDescriptionString = ResourceService.HomeResource.GetString("CustomCommandDescription");
        private readonly string FileInformationString = ResourceService.HomeResource.GetString("FileInformation");
        private readonly string FileInformationDescriptionString = ResourceService.HomeResource.GetString("FileInformationDescription");
        private readonly string HATestString = ResourceService.HomeResource.GetString("HATest");
        private readonly string HATestDescriptionString = ResourceService.HomeResource.GetString("HATestDescription");
        private readonly string OtherToolsString = ResourceService.HomeResource.GetString("OtherTools");
        private readonly string PhotoConversionString = ResourceService.HomeResource.GetString("PhotoConversion");
        private readonly string PhotoConversionDescriptionString = ResourceService.HomeResource.GetString("PhotoConversionDescription");
        private readonly string VideoConversionString = ResourceService.HomeResource.GetString("VideoConversion");
        private readonly string VideoConversionDescriptionString = ResourceService.HomeResource.GetString("VideoConversionDescription");

        #endregion 第一部分：常量、资源与状态字段

        #region 第二部分：属性、列表与事件

        private List<HomeItemGroupModel> HomeGroupList { get; } = [];

        #endregion 第二部分：属性、列表与事件

        #region 第三部分：构造函数

        public HomePage()
        {
            InitializeComponent();
            InitializeData();
        }

        #endregion 第三部分：构造函数

        #region 第四部分：挂载事件处理

        /// <summary>
        /// 点击条目时进入条目对应的页面
        /// </summary>
        private void OnItemClicked(object sender, ItemClickEventArgs args)
        {
            if (args.ClickedItem is HomeItemModel homeItem && MainWindow.Current.GetSelectedItem(homeItem.NavigationPage, MainWindow.Current.NavigationViewItemMenuItemsCollection) is NavigationViewItemModel navigationViewItem)
            {
                MainWindow.Current.NavigateTo(navigationViewItem.NavigationPage);
            }
        }

        #endregion 第四部分：挂载事件处理

        #region 第五部分：数据操作与业务逻辑

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData()
        {
            HomeGroupList.Add(new()
            {
                GroupName = ConversionToolsString,
                HomeItemList =
                [
                    new()
                    {
                        Title = VideoConversionString,
                        Description = VideoConversionDescriptionString,
                        ImagePath = "ms-appx:///Assets/ControlIcon/VideoConversion.png",
                        NavigationPage = typeof(VideoConversionPage)
                    },
                    new()
                    {
                        Title = AudioConversionString,
                        Description = AudioConversionDescriptionString,
                        ImagePath = "ms-appx:///Assets/ControlIcon/AudioConversion.png",
                        NavigationPage = typeof(AudioConversionPage)
                    },
                    new()
                    {
                        Title = PhotoConversionString,
                        Description = PhotoConversionDescriptionString,
                        ImagePath = "ms-appx:///Assets/ControlIcon/PhotoConversion.png",
                        NavigationPage = typeof(PhotoConversionPage)
                    }
                ]
            });
            HomeGroupList.Add(new()
            {
                GroupName = OtherToolsString,
                HomeItemList =
                [
                    new()
                    {
                        Title = FileInformationString,
                        Description = FileInformationDescriptionString,
                        ImagePath = "ms-appx:///Assets/ControlIcon/FileInformation.png",
                        NavigationPage = typeof(FileInformationPage)
                    },
                    new()
                    {
                        Title = HATestString,
                        Description = HATestDescriptionString,
                        ImagePath = "ms-appx:///Assets/ControlIcon/HATest.png",
                        NavigationPage = typeof(HATestPage)
                    },
                    new()
                    {
                        Title = CustomCommandString,
                        Description = CustomCommandDescriptionString,
                        ImagePath = "ms-appx:///Assets/ControlIcon/CustomCommand.png",
                        NavigationPage = typeof(CustomCommandPage)
                    }
                ]
            });
        }

        #endregion 第五部分：数据操作与业务逻辑
    }
}
