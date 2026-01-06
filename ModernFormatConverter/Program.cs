using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Helpers.Root;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Services.Settings;
using ModernFormatConverter.WindowsAPI.ComTypes;
using ModernFormatConverter.WindowsAPI.PInvoke.Kernel32;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;

// 抑制 CA1806 警告
#pragma warning disable CA1806

namespace ModernFormatConverter
{
    /// <summary>
    /// 格式转换器桌面程序
    /// </summary>
    public static class Program
    {
        private static readonly Guid CLSID_ApplicationActivationManager = new("45BA127D-10A8-46EA-8AB7-56EA9078943C");
        private static readonly IApplicationActivationManager applicationActivationManager = (IApplicationActivationManager)Activator.CreateInstance(Type.GetTypeFromCLSID(CLSID_ApplicationActivationManager));

        /// <summary>
        /// 应用程序的主入口点
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            if (RuntimeHelper.IsMSIX)
            {
                if (RuntimeHelper.IsElevated && args.Length is 1 && args[0] is "--elevated")
                {
                    uint aumidLength = 260;
                    StringBuilder aumidBuilder = new((int)aumidLength);
                    Kernel32Library.GetCurrentApplicationUserModelId(ref aumidLength, aumidBuilder);
                    applicationActivationManager.ActivateApplication(Convert.ToString(aumidBuilder), string.Empty, ACTIVATEOPTIONS.AO_NONE, out uint _);
                    return;
                }
            }
            else
            {
                try
                {
                    Process.Start("modernformatconverter:");
                }
                catch (Exception)
                { }
                return;
            }

            InitializeProgramResources();
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            Application.Start((param) =>
            {
                SynchronizationContext.SetSynchronizationContext(new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread()));
                new MainApp();
            });
        }

        /// <summary>
        /// 处理应用非 UI 线程异常
        /// </summary>
        private static void OnUnhandledException(object sender, System.UnhandledExceptionEventArgs args)
        {
            LogService.WriteLog(TraceEventType.Warning, nameof(ModernFormatConverter), nameof(Program), nameof(OnUnhandledException), 1, args.ExceptionObject as Exception);
        }

        /// <summary>
        /// 加载应用程序所需的资源
        /// </summary>
        private static void InitializeProgramResources()
        {
            LogService.Initialize();
            LanguageService.InitializeLanguage();
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(LanguageService.AppLanguage.Key);
        }
    }
}
