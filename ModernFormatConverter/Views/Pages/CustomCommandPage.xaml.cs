using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

namespace ModernFormatConverter.Views.Pages
{
    /// <summary>
    /// 自定义命令运行页面
    /// </summary>
    public sealed partial class CustomCommandPage : Page, INotifyPropertyChanged
    {
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
            CommandResultText =
                """
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
                private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
                {
                    if (string.IsNullOrEmpty(CommandText))
                    {
                        // TODO：显示通知，请输入命令
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
                private void OnRunClicked(object sender, RoutedEventArgs args)
                {
                    if (string.IsNullOrEmpty(CommandText))
                    {
                        // TODO：显示通知，请输入命令
                    }
                }

                /// <summary>
                /// 停止运行命令
                /// </summary>
                private void OnStopClicked(object sender, RoutedEventArgs args)
                {
                    // TODO：未完成
                }
                """;
        }

        #region 第一部分：自定义命令运行页面——挂载的事件

        /// <summary>
        /// 运行命令
        /// </summary>
        private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (string.IsNullOrEmpty(CommandText))
            {
                // TODO：显示通知，请输入命令
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
        private void OnRunClicked(object sender, RoutedEventArgs args)
        {
            if (string.IsNullOrEmpty(CommandText))
            {
                // TODO：显示通知，请输入命令
            }
        }

        /// <summary>
        /// 停止运行命令
        /// </summary>
        private void OnStopClicked(object sender, RoutedEventArgs args)
        {
            // TODO：未完成
        }

        #endregion 第一部分：自定义命令运行页面——挂载的事件
    }
}
