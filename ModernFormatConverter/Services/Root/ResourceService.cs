using System.Reflection;
using System.Resources;

namespace ModernFormatConverter.Services.Root
{
    /// <summary>
    /// 应用资源服务
    /// </summary>
    public static class ResourceService
    {
        private static Assembly CurrentAssembly { get; } = Assembly.GetExecutingAssembly();

        public static ResourceManager AppInformationResource { get; } = new("ModernFormatConverter.Strings.AppInformation", CurrentAssembly);

        public static ResourceManager AudioConversionResource { get; } = new("ModernFormatConverter.Strings.AudioConversion", CurrentAssembly);

        public static ResourceManager ConversionToolsResource { get; } = new("ModernFormatConverter.Strings.ConversionTools", CurrentAssembly);

        public static ResourceManager CustomCommandResource { get; } = new("ModernFormatConverter.Strings.CustomCommand", CurrentAssembly);

        public static ResourceManager DocumentConversionResource { get; } = new("ModernFormatConverter.Strings.DocumentConversion", CurrentAssembly);

        public static ResourceManager FileInformationResource { get; } = new("ModernFormatConverter.Strings.FileInformation", CurrentAssembly);

        public static ResourceManager HATestResource { get; } = new("ModernFormatConverter.Strings.HATest", CurrentAssembly);

        public static ResourceManager HomeResource { get; } = new("ModernFormatConverter.Strings.Home", CurrentAssembly);

        public static ResourceManager LicenseResource { get; } = new("ModernFormatConverter.Strings.License", CurrentAssembly);

        public static ResourceManager NotificationTipResource { get; } = new("ModernFormatConverter.Strings.NotificationTip", CurrentAssembly);

        public static ResourceManager PhotoConversionResource { get; } = new("ModernFormatConverter.Strings.PhotoConversion", CurrentAssembly);

        public static ResourceManager RestartAppsResource { get; } = new("ModernFormatConverter.Strings.RestartApps", CurrentAssembly);

        public static ResourceManager SettingsAboutResource { get; } = new("ModernFormatConverter.Strings.SettingsAbout", CurrentAssembly);

        public static ResourceManager SettingsAdvancedResource { get; } = new("ModernFormatConverter.Strings.SettingsAdvanced", CurrentAssembly);

        public static ResourceManager SettingsGeneralResource { get; } = new("ModernFormatConverter.Strings.SettingsGeneral", CurrentAssembly);

        public static ResourceManager SettingsResource { get; } = new("ModernFormatConverter.Strings.Settings", CurrentAssembly);

        public static ResourceManager TaskManagerResource { get; } = new("ModernFormatConverter.Strings.TaskManager", CurrentAssembly);

        public static ResourceManager UpdateAppResource { get; } = new("ModernFormatConverter.Strings.UpdateApp", CurrentAssembly);

        public static ResourceManager VideoConversionResource { get; } = new("ModernFormatConverter.Strings.VideoConversion", CurrentAssembly);

        public static ResourceManager VideoConversionOutputConfigurationResource { get; } = new("ModernFormatConverter.Strings.VideoConversionOutputConfiguration", CurrentAssembly);

        public static ResourceManager VideoExportPictureOutputConfigurationResource { get; } = new("ModernFormatConverter.Strings.VideoExportPictureOutputConfiguration", CurrentAssembly);

        public static ResourceManager WindowResource { get; } = new("ModernFormatConverter.Strings.Window", CurrentAssembly);
    }
}
