using Microsoft.UI.Xaml.Controls;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Services.Root;
using System.ComponentModel;

namespace ModernFormatConverter.Views.NotificationTips
{
    /// <summary>
    /// 操作完成后应用内通知
    /// </summary>
    public sealed partial class OperationResultNotificationTip : TeachingTip, INotifyPropertyChanged
    {
        private bool _isSuccessOperation;

        public bool IsSuccessOperation
        {
            get { return _isSuccessOperation; }

            set
            {
                if (!Equals(_isSuccessOperation, value))
                {
                    _isSuccessOperation = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSuccessOperation)));
                }
            }
        }

        private string _operationContent;

        public string OperationContent
        {
            get { return _operationContent; }

            set
            {
                if (!string.Equals(_operationContent, value))
                {
                    _operationContent = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OperationContent)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public OperationResultNotificationTip(OperationKind operationKind)
        {
            InitializeComponent();

            if (operationKind is OperationKind.LanguageChange)
            {
                IsSuccessOperation = true;
                OperationContent = ResourceService.NotificationTipResource.GetString("LanguageChange");
            }
        }

        public OperationResultNotificationTip(OperationKind operationKind, bool operationResult)
        {
            InitializeComponent();

            if (operationKind is OperationKind.Desktop)
            {
                if (operationResult)
                {
                    IsSuccessOperation = true;
                    OperationContent = ResourceService.NotificationTipResource.GetString("DesktopShortcutSuccessfully");
                }
                else
                {
                    IsSuccessOperation = false;
                    OperationContent = ResourceService.NotificationTipResource.GetString("DesktopShortcutFailed");
                }
            }
            else if (operationKind is OperationKind.StartScreen)
            {
                if (operationResult)
                {
                    IsSuccessOperation = true;
                    OperationContent = ResourceService.NotificationTipResource.GetString("StartScreenSuccessfully");
                }
                else
                {
                    IsSuccessOperation = false;
                    OperationContent = ResourceService.NotificationTipResource.GetString("StartScreenFailed");
                }
            }
            else if (operationKind is OperationKind.Taskbar)
            {
                if (operationResult)
                {
                    IsSuccessOperation = true;
                    OperationContent = ResourceService.NotificationTipResource.GetString("TaskbarSuccessfully");
                }
                else
                {
                    IsSuccessOperation = false;
                    OperationContent = ResourceService.NotificationTipResource.GetString("TaskbarFailed");
                }
            }
        }

        public OperationResultNotificationTip(OperationKind operationKind, int statusKind)
        {
            InitializeComponent();

            if (operationKind is OperationKind.CheckUpdate)
            {
                if (statusKind is 0)
                {
                    IsSuccessOperation = false;
                    OperationContent = ResourceService.NotificationTipResource.GetString("NotNewestVersion");
                }
                else if (statusKind is 1)
                {
                    IsSuccessOperation = true;
                    OperationContent = ResourceService.NotificationTipResource.GetString("NewestVersion");
                }
                else if (statusKind is 2)
                {
                    IsSuccessOperation = false;
                    OperationContent = ResourceService.NotificationTipResource.GetString("UpdateCheckFailed");
                }
            }
        }
    }
}
