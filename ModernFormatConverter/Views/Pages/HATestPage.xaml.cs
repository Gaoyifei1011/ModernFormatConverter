using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using System.ComponentModel;

// 抑制 IDE0060 警告
#pragma warning disable IDE0060

namespace ModernFormatConverter.Views.Pages
{
    /// <summary>
    /// 硬件加速测试页面
    /// </summary>
    public sealed partial class HATestPage : Page, INotifyPropertyChanged
    {
        private readonly string AV1String = ResourceService.HATestResource.GetString("AV1");
        private readonly string H264String = ResourceService.HATestResource.GetString("H264");
        private readonly string HevcString = ResourceService.HATestResource.GetString("Hevc");
        private readonly string VP9String = ResourceService.HATestResource.GetString("VP9");

        private bool _isTesting;

        public bool IsTesting
        {
            get { return _isTesting; }

            set
            {
                if (!Equals(_isTesting, value))
                {
                    _isTesting = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTesting)));
                }
            }
        }

        private WinRTObservableCollection<HATestModel> IntelHATestCollection { get; } = [];

        private WinRTObservableCollection<HATestModel> MediaFoundationTestCollection { get; } = [];

        private WinRTObservableCollection<HATestModel> NvidiaHATestCollection { get; } = [];

        private WinRTObservableCollection<HATestModel> AMDHATestCollection { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public HATestPage()
        {
            InitializeComponent();
            IntelHATestCollection.Add(new HATestModel()
            {
                HATestName = H264String,
                HATestKind = HATestKind.H264_QSV,
                HATestResultKind = HATestResultKind.NotTest,
            });
            IntelHATestCollection.Add(new HATestModel()
            {
                HATestName = HevcString,
                HATestKind = HATestKind.HEVC_QSV,
                HATestResultKind = HATestResultKind.NotTest,
            });
            IntelHATestCollection.Add(new HATestModel()
            {
                HATestName = AV1String,
                HATestKind = HATestKind.AV1_QSV,
                HATestResultKind = HATestResultKind.NotTest,
            });
            IntelHATestCollection.Add(new HATestModel()
            {
                HATestName = VP9String,
                HATestKind = HATestKind.VP9_QSV,
                HATestResultKind = HATestResultKind.NotTest,
            });
            MediaFoundationTestCollection.Add(new HATestModel()
            {
                HATestName = H264String,
                HATestKind = HATestKind.H264_MF,
                HATestResultKind = HATestResultKind.NotTest,
            });
            MediaFoundationTestCollection.Add(new HATestModel()
            {
                HATestName = HevcString,
                HATestKind = HATestKind.HEVC_MF,
                HATestResultKind = HATestResultKind.NotTest,
            });
            MediaFoundationTestCollection.Add(new HATestModel()
            {
                HATestName = AV1String,
                HATestKind = HATestKind.AV1_MF,
                HATestResultKind = HATestResultKind.NotTest,
            });
            NvidiaHATestCollection.Add(new HATestModel()
            {
                HATestName = H264String,
                HATestKind = HATestKind.H264_NVENC,
                HATestResultKind = HATestResultKind.NotTest,
            });
            NvidiaHATestCollection.Add(new HATestModel()
            {
                HATestName = HevcString,
                HATestKind = HATestKind.HEVC_NVENC,
                HATestResultKind = HATestResultKind.NotTest,
            });
            NvidiaHATestCollection.Add(new HATestModel()
            {
                HATestName = AV1String,
                HATestKind = HATestKind.AV1_NVENC,
                HATestResultKind = HATestResultKind.NotTest,
            });
            AMDHATestCollection.Add(new HATestModel()
            {
                HATestName = H264String,
                HATestKind = HATestKind.H264_AMF,
                HATestResultKind = HATestResultKind.NotTest,
            });
            AMDHATestCollection.Add(new HATestModel()
            {
                HATestName = HevcString,
                HATestKind = HATestKind.HEVC_AMF,
                HATestResultKind = HATestResultKind.NotTest,
            });
            AMDHATestCollection.Add(new HATestModel()
            {
                HATestName = AV1String,
                HATestKind = HATestKind.AV1_AMF,
                HATestResultKind = HATestResultKind.NotTest,
            });
        }

        #region 第一部分：硬件加速测试页面——挂载的事件

        /// <summary>
        /// 运行测试
        /// </summary>
        private void OnRunHATestClicked(object sender, RoutedEventArgs args)
        {
        }

        /// <summary>
        /// 停止测试
        /// </summary>
        private void OnStopHATestClicked(object sender, RoutedEventArgs args)
        {
        }

        /// <summary>
        /// 使用说明
        /// </summary>
        private void OnUseInstructionClicked(object sender, RoutedEventArgs args)
        {
        }

        #endregion 第一部分：硬件加速测试页面——挂载的事件
    }
}
