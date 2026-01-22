using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Views.NotificationTips;
using ModernFormatConverter.Views.Windows;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// 抑制 IDE0060 警告
#pragma warning disable IDE0060

namespace ModernFormatConverter.Views.Pages
{
    /// <summary>
    /// 自定义命令运行页面
    /// </summary>
    public sealed partial class CustomCommandPage : Page, INotifyPropertyChanged
    {
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        private readonly StringBuilder commandResultBuilder = new();

        private bool _isRunning;

        public bool IsRunning
        {
            get { return _isRunning; }

            set
            {
                if (!Equals(_isRunning, value))
                {
                    _isRunning = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
                }
            }
        }

        private string _commandText;

        public string CommandText
        {
            get { return _commandText; }

            set
            {
                if (!string.Equals(_commandText, value))
                {
                    _commandText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CommandText)));
                }
            }
        }

        private string _commandResultText;

        public string CommandResultText
        {
            get { return _commandResultText; }

            set
            {
                if (!string.Equals(_commandResultText, value))
                {
                    _commandResultText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CommandResultText)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CustomCommandPage()
        {
            InitializeComponent();
        }

        #region 第一部分：自定义命令运行页面——挂载的事件

        /// <summary>
        /// 运行命令
        /// </summary>
        private async void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (string.IsNullOrEmpty(CommandText))
            {
                await MainWindow.Current.ShowNotificationAsync(new OperationResultNotificationTip(OperationKind.CommandEmpty));
            }
        }

        /// <summary>
        /// 命令内容发生变化时出发的事件
        /// </summary>
        private void OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            CommandText = sender.Text;
        }

        /// <summary>
        /// 运行命令
        /// </summary>
        private async void OnRunClicked(object sender, RoutedEventArgs args)
        {
            if (string.IsNullOrEmpty(CommandText))
            {
                await MainWindow.Current.ShowNotificationAsync(new OperationResultNotificationTip(OperationKind.CommandEmpty));
            }
        }

        /// <summary>
        /// 停止运行命令
        /// </summary>
        private void OnStopClicked(object sender, RoutedEventArgs args)
        {
            // TODO：未完成
        }

        /// <summary>
        /// 测试
        /// </summary>
        private void OnTestClicked(object sender, RoutedEventArgs args)
        {
            commandResultBuilder.Clear();
            CommandResultText = string.Empty;

            Task.Run(() =>
            {
                try
                {
                    Process process = new()
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "FFmpeg.exe",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            WorkingDirectory = Environment.CurrentDirectory,
                            StandardOutputEncoding = Encoding.UTF8,
                            StandardErrorEncoding = Encoding.UTF8
                        }
                    };

                    process.OutputDataReceived += OnOutputDataReceived;
                    process.ErrorDataReceived += OnErrorDataReceived;
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                    process.OutputDataReceived -= OnOutputDataReceived;
                    process.ErrorDataReceived -= OnErrorDataReceived;
                    process.Dispose();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            });
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs args)
        {
            synchronizationContext.Post((_) =>
            {
                commandResultBuilder.AppendLine(args.Data);
                CommandResultText = commandResultBuilder.ToString();
            }, null);
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            synchronizationContext.Post((_) =>
            {
                commandResultBuilder.AppendLine(args.Data);
                CommandResultText = commandResultBuilder.ToString();
            }, null);
        }

        /// <summary>
        /// 清空输出结果
        /// </summary>
        private void OnClearResultClicked(object sender, RoutedEventArgs args)
        {
            commandResultBuilder.Clear();
            CommandResultText = string.Empty;
        }

        /// <summary>
        /// 命令输出结果内容发生变化时触发的事件
        /// </summary>
        private void OnCommandResultTextChanged(object sender, TextChangedEventArgs args)
        {
        }

        #endregion 第一部分：自定义命令运行页面——挂载的事件
    }
}
