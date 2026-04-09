using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Models;
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
        private ConversionTypeModel _selectedConversionType;

        public ConversionTypeModel SelectedConversionType
        {
            get { return _selectedConversionType; }

            set
            {
                if (!Equals(_selectedConversionType, value))
                {
                    _selectedConversionType = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedConversionType)));
                }
            }
        }

        public WinRTObservableCollection<ConversionTypeModel> ConversionTypeCollection { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public PhotoConversionPage()
        {
            InitializeComponent();
        }

        #region 第一部分：图片转换页面——挂载的事件

        /// <summary>
        /// 图片转换列表选中项发生变化时触发的事件
        /// </summary>
        private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            SelectedConversionType = args.SelectedItem as ConversionTypeModel;
        }

        /// <summary>
        /// 打开输出配置
        /// </summary>
        private void OnOutputConfigurationClicked(object sender, RoutedEventArgs args)
        {
            // TODO：未完成
        }

        /// <summary>
        /// 确定
        /// </summary>
        private void OnOkClicked(object sender, RoutedEventArgs args)
        {
            ConversionToolsPage.Current?.Close();
        }

        #endregion 第一部分：图片转换页面——挂载的事件
    }
}
