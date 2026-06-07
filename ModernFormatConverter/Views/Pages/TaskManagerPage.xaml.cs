using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Services.Root;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace ModernFormatConverter.Views.Pages
{
    /// <summary>
    /// 任务管理页面
    /// TODO：未完成
    /// </summary>
    public sealed partial class TaskManagerPage : Page
    {
        private readonly string OutputConfigurationString = ResourceService.TaskManagerResource.GetString("OutputConfiguration");
        private readonly string TaskListString = ResourceService.TaskManagerResource.GetString("TaskList");

        public List<Type> PageList { get; } = [typeof(TaskListPage), typeof(VideoConversionOutputConfigurationPage), typeof(VideoExportPictureOutputConfigurationPage), typeof(AudioConversionOutputConfigurationPage), typeof(TextToAudioOutputConfigurationPage), typeof(PhotoConversionOutputConfigurationPage)];

        public WinRTObservableCollection<DictionaryEntry> BreadCollection { get; } = [];

        public TaskManagerPage()
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
            TaskManagerFrame.ContentTransitions = SuppressNavigationTransitionCollection;

            // 第一次导航
            if (GetCurrentPageType() is null)
            {
                NavigateTo(PageList[0], null, null);
            }
        }

        #endregion 第一部分：重写父类事件

        #region 第二部分：视频转换页面——挂载的事件

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
                BreadCollection.Add(new DictionaryEntry
                {
                    Key = "TaskList",
                    Value = TaskListString
                });
            }
            else if (BreadCollection.Count is 1 && Equals(GetCurrentPageType(), PageList[1]))
            {
                BreadCollection.Add(new DictionaryEntry()
                {
                    Key = "OutputConfiguration",
                    Value = OutputConfigurationString
                });
            }
            else if (BreadCollection.Count is 1 && Equals(GetCurrentPageType(), PageList[2]))
            {
                BreadCollection.Add(new DictionaryEntry()
                {
                    Key = "OutputConfiguration",
                    Value = OutputConfigurationString
                });
            }
            else if (BreadCollection.Count is 1 && Equals(GetCurrentPageType(), PageList[3]))
            {
                BreadCollection.Add(new DictionaryEntry()
                {
                    Key = "OutputConfiguration",
                    Value = OutputConfigurationString
                });
            }
            else if (BreadCollection.Count is 1 && Equals(GetCurrentPageType(), PageList[4]))
            {
                BreadCollection.Add(new DictionaryEntry()
                {
                    Key = "OutputConfiguration",
                    Value = OutputConfigurationString
                });
            }
            else if (BreadCollection.Count is 1 && Equals(GetCurrentPageType(), PageList[5]))
            {
                BreadCollection.Add(new DictionaryEntry()
                {
                    Key = "OutputConfiguration",
                    Value = OutputConfigurationString
                });
            }
            else if (BreadCollection.Count is 2 && Equals(GetCurrentPageType(), PageList[0]))
            {
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

        #endregion 第二部分：视频转换页面——挂载的事件

        /// <summary>
        /// 页面向前导航
        /// </summary>
        public void NavigateTo(Type navigationPageType, object parameter = null, bool? slideDirection = null)
        {
            try
            {
                TaskManagerFrame.ContentTransitions = slideDirection.HasValue ? slideDirection.Value ? RightSlideNavigationTransitionCollection : LeftSlideNavigationTransitionCollection : SuppressNavigationTransitionCollection;

                // 导航到该项目对应的页面
                TaskManagerFrame.Navigate(navigationPageType, parameter);
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionPage), nameof(NavigateTo), 1, e);
            }
        }

        /// <summary>
        /// 获取当前导航到的页
        /// </summary>
        public Type GetCurrentPageType()
        {
            return TaskManagerFrame.CurrentSourcePageType;
        }
    }
}
