using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Services.Root;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

// 抑制 CA1806，CA1822，IDE0060 警告
#pragma warning disable CA1806,CA1822,IDE0060

namespace ModernFormatConverter.Views.Pages
{
    /// <summary>
    /// 图片转换页面
    /// </summary>
    public sealed partial class PhotoConversionPage : Page, INotifyPropertyChanged
    {
        private readonly string ParameterSettingsString = ResourceService.PhotoConversionResource.GetString("ParameterSettings");
        private readonly string SelectFileString = ResourceService.PhotoConversionResource.GetString("SelectFile");

        public List<Type> PageList { get; } = [typeof(PhotoConversionParameterSettingsPage), typeof(PhotoConversionSelectFilePage)];

        public WinRTObservableCollection<DictionaryEntry> BreadCollection { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public PhotoConversionPage()
        {
            InitializeComponent();
        }

        #region 第一部分：重写父类事件

        /// <summary>
        /// 导航到该页面触发的事件
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);
            PhotoConversionFrame.ContentTransitions = SuppressNavigationTransitionCollection;

            // 第一次导航
            if (GetCurrentPageType() is null)
            {
                NavigateTo(PageList[0], null, null);
            }
        }

        #endregion 第一部分：重写父类事件

        #region 第二部分：视频转换页面——挂载的事件

        /// <summary>
        /// 当后退按钮收到交互（如单击或点击）时发生
        /// </summary>
        private void OnBackClicked(object sender, RoutedEventArgs args)
        {
        }

        /// <summary>
        /// 单击痕迹栏条目时发生的事件
        /// </summary>
        private void OnItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {
        }

        /// <summary>
        /// 导航完成后发生
        /// </summary>
        private void OnNavigated(object sender, NavigationEventArgs args)
        {
            if (BreadCollection.Count is 0 && Equals(GetCurrentPageType(), PageList[0]))
            {
                BreadCollection.Add(new DictionaryEntry
                {
                    Key = "ParameterSettings",
                    Value = ParameterSettingsString
                });
            }
            else if (BreadCollection.Count is 1 && Equals(GetCurrentPageType(), PageList[1]))
            {
                BreadCollection.Add(new DictionaryEntry()
                {
                    Key = "SelectFile",
                    Value = SelectFileString
                });
            }
        }

        /// <summary>
        /// 导航失败时发生
        /// </summary>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs args)
        {
            args.Handled = true;
        }

        #endregion 第二部分：视频转换页面——挂载的事件

        /// <summary>
        /// 页面向前导航
        /// </summary>
        public void NavigateTo(Type navigationPageType, object parameter = null, bool? slideDirection = null)
        {
            try
            {
                if (slideDirection.HasValue)
                {
                    PhotoConversionFrame.ContentTransitions = slideDirection.Value ? RightSlideNavigationTransitionCollection : LeftSlideNavigationTransitionCollection;
                }

                // 导航到该项目对应的页面
                PhotoConversionFrame.Navigate(navigationPageType, parameter);
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionPage), nameof(NavigateTo), 1, e);
            }
        }

        /// <summary>
        /// 获取当前导航到的页
        /// </summary>
        public Type GetCurrentPageType()
        {
            return PhotoConversionFrame.CurrentSourcePageType;
        }
    }
}
