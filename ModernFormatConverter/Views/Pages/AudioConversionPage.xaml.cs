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
    /// 音频转换页面
    /// </summary>
    public sealed partial class AudioConversionPage : Page, INotifyPropertyChanged
    {
        private readonly string ParameterSettingsString = ResourceService.AudioConversionResource.GetString("ParameterSettings");
        private readonly string SelectFileString = ResourceService.AudioConversionResource.GetString("SelectFile");

        private bool _isBackEnabled;

        public bool IsBackEnabled
        {
            get { return _isBackEnabled; }

            set
            {
                if (!Equals(_isBackEnabled, value))
                {
                    _isBackEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBackEnabled)));
                }
            }
        }

        public List<Type> PageList { get; } = [typeof(AudioConversionParameterSettingsPage), typeof(AudioConversionSelectFilePage)];

        public WinRTObservableCollection<DictionaryEntry> BreadCollection { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public AudioConversionPage()
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
            if (BreadCollection.Count is 2 && Equals(GetCurrentPageType(), typeof(AudioConversionSelectFilePage)))
            {
                NavigateTo(PageList[0], null, false);
            }
        }

        /// <summary>
        /// 单击痕迹栏条目时发生的事件
        /// </summary>
        private void OnItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {
            if (args.Item is DictionaryEntry bread && BreadCollection.Count is 2 && Equals(bread.Key, BreadCollection[0].Key))
            {
                NavigateTo(PageList[0], null, false);
            }
        }

        /// <summary>
        /// 导航完成后发生
        /// </summary>
        private void OnNavigated(object sender, NavigationEventArgs args)
        {
            if (BreadCollection.Count is 0 && Equals(GetCurrentPageType(), PageList[0]))
            {
                IsBackEnabled = false;
                BreadCollection.Add(new DictionaryEntry
                {
                    Key = "ParameterSettings",
                    Value = ParameterSettingsString
                });
            }
            else if (BreadCollection.Count is 1 && Equals(GetCurrentPageType(), PageList[1]))
            {
                IsBackEnabled = true;
                BreadCollection.Add(new DictionaryEntry()
                {
                    Key = "SelectFile",
                    Value = SelectFileString
                });
            }
            else if (BreadCollection.Count is 2 && Equals(GetCurrentPageType(), PageList[0]))
            {
                IsBackEnabled = false;
                BreadCollection.RemoveAt(1);
            }
        }

        /// <summary>
        /// 导航失败时发生
        /// </summary>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs args)
        {
            args.Handled = true;
        }

        /// <summary>
        /// 下一步
        /// </summary>
        private void OnNextStepClicked(object sender, RoutedEventArgs args)
        {
            if (BreadCollection.Count is 1 && Equals(GetCurrentPageType(), typeof(AudioConversionParameterSettingsPage)))
            {
                NavigateTo(PageList[1], null, true);
            }
        }

        /// <summary>
        /// 确定
        /// </summary>
        private void OnOkClicked(object sender, RoutedEventArgs args)
        {
            ConversionToolsPage.Current?.Close();
        }

        #endregion 第二部分：视频转换页面——挂载的事件

        /// <summary>
        /// 页面向前导航
        /// </summary>
        private void NavigateTo(Type navigationPageType, object parameter = null, bool? slideDirection = null)
        {
            try
            {
                AudioConversionFrame.ContentTransitions = slideDirection.HasValue ? slideDirection.Value ? RightSlideNavigationTransitionCollection : LeftSlideNavigationTransitionCollection : SuppressNavigationTransitionCollection;

                // 导航到该项目对应的页面
                AudioConversionFrame.Navigate(navigationPageType, parameter);
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioConversionPage), nameof(NavigateTo), 1, e);
            }
        }

        /// <summary>
        /// 获取当前导航到的页
        /// </summary>
        private Type GetCurrentPageType()
        {
            return AudioConversionFrame.CurrentSourcePageType;
        }

        /// <summary>
        /// 获取选中的步骤
        /// </summary>
        private Visibility GetSelectedStep(int selectedStep, int comparedSelectedStep)
        {
            return Equals(selectedStep, comparedSelectedStep) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
