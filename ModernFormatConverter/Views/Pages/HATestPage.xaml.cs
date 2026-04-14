using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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
        private readonly string TestFailedString = ResourceService.HATestResource.GetString("TestFailed");
        private readonly string UserCancelTestString = ResourceService.HATestResource.GetString("UserCancelTest");
        private readonly string VP9String = ResourceService.HATestResource.GetString("VP9");
        private bool isUserCanceled;
        private Process process;

        private bool _isTesting;

        public bool IsTesting
        {
            get { return _isTesting; }

            set
            {
                _isTesting = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTesting)));
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
        private async void OnRunHATestClicked(object sender, RoutedEventArgs args)
        {
            if (!IsTesting)
            {
                IsTesting = true;
                isUserCanceled = false;

                foreach (HATestModel intelHATestItem in IntelHATestCollection)
                {
                    await RunHATestAsync(intelHATestItem);
                }
                foreach (HATestModel mediaFoundationTestItem in MediaFoundationTestCollection)
                {
                    await RunHATestAsync(mediaFoundationTestItem);
                }
                foreach (HATestModel nvidiaHATestItem in NvidiaHATestCollection)
                {
                    await RunHATestAsync(nvidiaHATestItem);
                }
                foreach (HATestModel amdHATestItem in AMDHATestCollection)
                {
                    await RunHATestAsync(amdHATestItem);
                }

                IsTesting = false;
            }
        }

        /// <summary>
        /// 停止测试
        /// </summary>
        private void OnStopHATestClicked(object sender, RoutedEventArgs args)
        {
            if (IsTesting)
            {
                IsTesting = false;
                isUserCanceled = true;

                Task.Run(() =>
                {
                    if (process is not null)
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch (Exception e)
                        {
                            LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(HATestPage), nameof(OnStopHATestClicked), 1, e);
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 使用说明
        /// </summary>
        private async void OnUseInstructionClicked(object sender, RoutedEventArgs args)
        {
            await Task.Delay(300);
            if (!HATestSplitView.IsPaneOpen)
            {
                HATestSplitView.IsPaneOpen = true;
            }
        }

        /// <summary>
        /// 点击关闭按钮关闭使用说明
        /// </summary>
        private void OnCloseClicked(object sender, RoutedEventArgs args)
        {
            if (HATestSplitView.IsPaneOpen)
            {
                HATestSplitView.IsPaneOpen = false;
            }
        }

        /// <summary>
        /// 打开设备管理器
        /// </summary>
        private void OnOpenDeviceManagementClicked(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>
            {
                try
                {
                    Process.Start("devmgmt.msc");
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(HATestPage), nameof(OnOpenDeviceManagementClicked), 1, e);
                }
            });
        }

        #endregion 第一部分：硬件加速测试页面——挂载的事件

        /// <summary>
        /// 运行硬件加速测试
        /// </summary>
        private async Task RunHATestAsync(HATestModel haTestItem)
        {
            if (!isUserCanceled)
            {
                haTestItem.HATestResultKind = HATestResultKind.Testing;
                (bool isSupported, bool isConvertFailed, string convertFailedReason) = await Task.Run(() =>
                {
                    bool isSupported = false;
                    bool isConvertFailed = false;
                    string convertFailedReason = string.Empty;
                    string output = string.Empty;
                    string temporaryFile = string.Empty;
                    string outputFile = string.Empty;

                    try
                    {
                        string inputFile = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), @"Assets\Resources\HATest.mp4");
                        temporaryFile = Path.GetTempFileName();
                        outputFile = Path.ChangeExtension(temporaryFile, ".mp4");
                        StringBuilder outputBuilder = new();
                        object outputBuilderLock = new();

                        string arguments = haTestItem.HATestKind switch
                        {
                            HATestKind.H264_QSV => string.Format(@"-y -i ""{0}"" -c:v:0 h264_qsv ""{1}""", inputFile, outputFile),
                            HATestKind.HEVC_QSV => string.Format(@"-y -i ""{0}"" -c:v:0 hevc_qsv ""{1}""", inputFile, outputFile),
                            HATestKind.AV1_QSV => string.Format(@"-y -i ""{0}"" -c:v:0 av1_qsv ""{1}""", inputFile, outputFile),
                            HATestKind.VP9_QSV => string.Format(@"-y -i ""{0}"" -c:v:0 vp9_qsv ""{1}""", inputFile, outputFile),
                            HATestKind.H264_MF => string.Format(@"-y -i ""{0}"" -c:v:0 h264_mf ""{1}""", inputFile, outputFile),
                            HATestKind.HEVC_MF => string.Format(@"-y -i ""{0}"" -c:v:0 hevc_mf ""{1}""", inputFile, outputFile),
                            HATestKind.AV1_MF => string.Format(@"-y -i ""{0}"" -c:v:0 av1_mf ""{1}""", inputFile, outputFile),
                            HATestKind.H264_NVENC => string.Format(@"-y -i ""{0}"" -c:v:0 h264_nvenc -b_ref_mode disabled ""{1}""", inputFile, outputFile),
                            HATestKind.HEVC_NVENC => string.Format(@"-y -i ""{0}"" -c:v:0 hevc_nvenc -b_ref_mode disabled ""{1}""", inputFile, outputFile),
                            HATestKind.AV1_NVENC => string.Format(@"-y -i ""{0}"" -c:v:0 av1_nvenc -b_ref_mode disabled ""{1}""", inputFile, outputFile),
                            HATestKind.H264_AMF => string.Format(@"-y -i ""{0}"" -c:v:0 h264_amf -b_ref_mode disabled ""{1}""", inputFile, outputFile),
                            HATestKind.HEVC_AMF => string.Format(@"-y -i ""{0}"" -c:v:0 hevc_amf -b_ref_mode disabled ""{1}""", inputFile, outputFile),
                            HATestKind.AV1_AMF => string.Format(@"-y -i ""{0}"" -c:v:0 av1_amf -b_ref_mode disabled ""{1}""", inputFile, outputFile),
                            _ => string.Empty
                        };

                        process = new()
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "FFmpeg.exe",
                                Arguments = arguments,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                WorkingDirectory = Environment.CurrentDirectory,
                                StandardOutputEncoding = Encoding.UTF8,
                                StandardErrorEncoding = Encoding.UTF8
                            },
                        };

                        process.OutputDataReceived += (sender, args) =>
                        {
                            lock (outputBuilderLock)
                            {
                                outputBuilder.AppendLine(args.Data);
                            }
                        };
                        process.ErrorDataReceived += (sender, args) =>
                        {
                            lock (outputBuilderLock)
                            {
                                outputBuilder.AppendLine(args.Data);
                            }
                        };
                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();
                        lock (outputBuilderLock)
                        {
                            output = outputBuilder.ToString();
                        }
                        process = null;
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(HATestPage), nameof(RunHATestAsync), 1, e);
                        convertFailedReason = e.Message;
                        process = null;
                    }

                    try
                    {
                        if (File.Exists(temporaryFile))
                        {
                            File.Delete(temporaryFile);
                        }

                        if (File.Exists(outputFile))
                        {
                            File.Delete(outputFile);
                        }
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(HATestPage), nameof(RunHATestAsync), 2, e);
                    }

                    if (!isUserCanceled && string.IsNullOrEmpty(convertFailedReason) && !output.Contains("Conversion failed"))
                    {
                        isSupported = true;
                    }

                    return ValueTuple.Create(isSupported, isConvertFailed, convertFailedReason);
                });

                if (isSupported)
                {
                    haTestItem.HATestResultKind = HATestResultKind.Supported;
                }
                else
                {
                    if (isUserCanceled)
                    {
                        haTestItem.HATestResultKind = HATestResultKind.Failed;
                        haTestItem.HATestFailedReason = string.Format(TestFailedString, UserCancelTestString);
                    }
                    else
                    {
                        if (isConvertFailed)
                        {
                            haTestItem.HATestResultKind = HATestResultKind.Failed;
                            haTestItem.HATestFailedReason = string.Format(TestFailedString, convertFailedReason);
                        }
                        else
                        {
                            haTestItem.HATestResultKind = HATestResultKind.NotSupported;
                        }
                    }
                }
            }
        }
    }
}
