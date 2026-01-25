using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Helpers.Controls;
using ModernFormatConverter.Services.Root;
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
        private readonly string ProcessExitString = ResourceService.CustomCommandResource.GetString("ProcessExit");
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        private readonly StringBuilder commandResultBuilder = new();
        private ScrollViewer customCommandResultTextBoxScrollViewer;
        private Process process;
        private bool isLoaded;

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
        /// 加载完成后触发的事件
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            if (!isLoaded)
            {
                isLoaded = true;
                (CommandResultTextBox as WindowsAPI.ComTypes.IUIElementProtected).ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
                if (XamlTreeHelper.FindDescendant<ScrollViewer>(CommandResultTextBox, "ContentElement") is ScrollViewer scrollViewer)
                {
                    customCommandResultTextBoxScrollViewer = scrollViewer;
                }
                if (XamlTreeHelper.FindDescendant<ScrollContentPresenter>(CommandResultTextBox, "ScrollContentPresenter") is ScrollContentPresenter scrollContentPresenter)
                {
                    (scrollContentPresenter.Content as WindowsAPI.ComTypes.IUIElementProtected).ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
                }
            }
        }

        /// <summary>
        /// 运行命令
        /// </summary>
        private async void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (string.IsNullOrEmpty(CommandText))
            {
                await MainWindow.Current.ShowNotificationAsync(new OperationResultNotificationTip(OperationKind.CommandEmpty));
                return;
            }

            if (!IsRunning)
            {
                IsRunning = true;
                commandResultBuilder.Clear();
                CommandResultText = string.Empty;
                customCommandResultTextBoxScrollViewer.ChangeView(null, 0, null, true);

                await Task.Run(() =>
                {
                    try
                    {
                        process = new()
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "FFmpeg.exe",
                                Arguments = string.Join(" ", "-nostdin", CommandText),
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                WorkingDirectory = Environment.CurrentDirectory,
                                StandardOutputEncoding = Encoding.UTF8,
                                StandardErrorEncoding = Encoding.UTF8
                            },
                            EnableRaisingEvents = true
                        };
                        process.OutputDataReceived += OnOutputDataReceived;
                        process.ErrorDataReceived += OnErrorDataReceived;
                        process.Exited += OnExited;
                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();
                        process.OutputDataReceived -= OnOutputDataReceived;
                        process.ErrorDataReceived -= OnErrorDataReceived;
                        process.Exited -= OnExited;
                        process = null;
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CustomCommandPage), nameof(OnTestClicked), 1, e);
                        process.OutputDataReceived -= OnOutputDataReceived;
                        process.ErrorDataReceived -= OnErrorDataReceived;
                        process.Exited -= OnExited;
                        process.Dispose();
                        process = null;
                    }
                });
                IsRunning = false;
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
                return;
            }

            if (!IsRunning)
            {
                IsRunning = true;
                commandResultBuilder.Clear();
                CommandResultText = string.Empty;
                customCommandResultTextBoxScrollViewer.ChangeView(null, 0, null, true);

                await Task.Run(() =>
                {
                    try
                    {
                        process = new()
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "FFmpeg.exe",
                                Arguments = string.Join(" ", "-nostdin", CommandText),
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                WorkingDirectory = Environment.CurrentDirectory,
                                StandardOutputEncoding = Encoding.UTF8,
                                StandardErrorEncoding = Encoding.UTF8
                            },
                            EnableRaisingEvents = true
                        };
                        process.OutputDataReceived += OnOutputDataReceived;
                        process.ErrorDataReceived += OnErrorDataReceived;
                        process.Exited += OnExited;
                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();
                        process.OutputDataReceived -= OnOutputDataReceived;
                        process.ErrorDataReceived -= OnErrorDataReceived;
                        process.Exited -= OnExited;
                        process = null;
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CustomCommandPage), nameof(OnTestClicked), 1, e);
                        process.OutputDataReceived -= OnOutputDataReceived;
                        process.ErrorDataReceived -= OnErrorDataReceived;
                        process.Exited -= OnExited;
                        process.Dispose();
                        process = null;
                    }
                });
                IsRunning = false;
            }
        }

        /// <summary>
        /// 停止运行命令
        /// </summary>
        private async void OnStopClicked(object sender, RoutedEventArgs args)
        {
            if (IsRunning)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        if (process is not null && !process.HasExited)
                        {
                            process.Kill();
                        }
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CustomCommandPage), nameof(OnStopClicked), 1, e);
                    }
                });

                IsRunning = false;
            }
        }

        /// <summary>
        /// 测试
        /// </summary>
        private async void OnTestClicked(object sender, RoutedEventArgs args)
        {
            if (!IsRunning)
            {
                IsRunning = true;
                commandResultBuilder.Clear();
                CommandResultText = string.Empty;
                customCommandResultTextBoxScrollViewer.ChangeView(null, 0, null, true);

                await Task.Run(() =>
                {
                    try
                    {
                        Process process = new()
                        {
                            EnableRaisingEvents = true,
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "FFmpeg.exe",
                                Arguments = "-nostdin",
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                WorkingDirectory = Environment.CurrentDirectory,
                                StandardOutputEncoding = Encoding.UTF8,
                                StandardErrorEncoding = Encoding.UTF8
                            },
                        };
                        process.OutputDataReceived += OnOutputDataReceived;
                        process.ErrorDataReceived += OnErrorDataReceived;
                        process.Exited += OnExited;
                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();
                        process.OutputDataReceived -= OnOutputDataReceived;
                        process.ErrorDataReceived -= OnErrorDataReceived;
                        process.Exited -= OnExited;
                        process = null;
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CustomCommandPage), nameof(OnTestClicked), 1, e);
                        process.OutputDataReceived -= OnOutputDataReceived;
                        process.ErrorDataReceived -= OnErrorDataReceived;
                        process.Exited -= OnExited;
                        process.Dispose();
                        process = null;
                    }
                });
                IsRunning = false;
            }
        }

        /// <summary>
        /// 清空输出结果
        /// </summary>
        private void OnClearResultClicked(object sender, RoutedEventArgs args)
        {
            commandResultBuilder.Clear();
            CommandResultText = string.Empty;
            customCommandResultTextBoxScrollViewer.ChangeView(null, 0, null, true);
        }

        /// <summary>
        /// 命令输出结果内容发生变化时触发的事件
        /// </summary>
        private void OnCommandResultTextChanged(object sender, TextChangedEventArgs args)
        {
            if (customCommandResultTextBoxScrollViewer.ComputedVerticalScrollBarVisibility is Visibility.Visible)
            {
                customCommandResultTextBoxScrollViewer.ChangeView(null, customCommandResultTextBoxScrollViewer.ExtentHeight, null, true);
            }
        }

        #endregion 第一部分：自定义命令运行页面——挂载的事件

        #region 第二部分：自定义命令运行页面——自定义事件

        /// <summary>
        /// 当应用程序向其重定向 StandardOutput 流中写入行时发生的事件
        /// </summary>
        private void OnOutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            synchronizationContext.Post((_) =>
            {
                commandResultBuilder.AppendLine(args.Data);
                CommandResultText = commandResultBuilder.ToString();
            }, null);
        }

        /// <summary>
        /// 当应用程序写入其重定向 StandardError 流中时发生的事件
        /// </summary>
        private void OnErrorDataReceived(object sender, DataReceivedEventArgs args)
        {
            synchronizationContext.Post((_) =>
            {
                commandResultBuilder.AppendLine(args.Data);
                CommandResultText = commandResultBuilder.ToString();
            }, null);
        }

        /// <summary>
        /// 在进程退出时发生的事件
        /// </summary>
        private void OnExited(object sender, EventArgs args)
        {
            Process process = sender as Process;
            synchronizationContext.Post((_) =>
            {
                try
                {
                    commandResultBuilder.Append(string.Format(ProcessExitString, process.Id, "FFmpeg.exe", process.ExitCode, process.ExitCode));
                    CommandResultText = commandResultBuilder.ToString();
                    process.Dispose();
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(CustomCommandPage), nameof(OnExited), 1, e);
                }
            }, null);
        }

        #endregion 第二部分：自定义命令运行页面——自定义事件
    }
}
