using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;

// 抑制 CA1806，CA1822，IDE0060 警告
#pragma warning disable CA1806,CA1822,IDE0060

namespace ModernFormatConverter.Views.Pages
{
    /// <summary>
    /// 图片转换页面
    /// </summary>
    public sealed partial class PhotoConversionPage : Page, INotifyPropertyChanged
    {
        public ObservableCollection<DictionaryEntry> BreadCollection { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public PhotoConversionPage()
        {
            InitializeComponent();
        }

        #region 第一部分：图片转换页面——挂载的事件

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

        #endregion 第一部分：图片转换页面——挂载的事件
    }
}
