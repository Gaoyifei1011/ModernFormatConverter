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

        public static ResourceManager CustomCommandResource { get; } = new("ModernFormatConverter.Strings.CustomCommand", CurrentAssembly);

        public static ResourceManager DialogResource { get; } = new("ModernFormatConverter.Strings.Dialog", CurrentAssembly);

        public static ResourceManager FileInformationResource { get; } = new("ModernFormatConverter.Strings.FileInformation", CurrentAssembly);

        public static ResourceManager HATestResource { get; } = new("ModernFormatConverter.Strings.HATest", CurrentAssembly);

        public static ResourceManager HomeResource { get; } = new("ModernFormatConverter.Strings.Home", CurrentAssembly);

        public static ResourceManager NotificationTipResource { get; } = new("ModernFormatConverter.Strings.NotificationTip", CurrentAssembly);

        public static ResourceManager SettingsAboutResource { get; } = new("ModernFormatConverter.Strings.SettingsAbout", CurrentAssembly);

        public static ResourceManager SettingsGeneralResource { get; } = new("ModernFormatConverter.Strings.SettingsGeneral", CurrentAssembly);

        public static ResourceManager SettingsResource { get; } = new("ModernFormatConverter.Strings.Settings", CurrentAssembly);

        public static ResourceManager TaskManagerResource { get; } = new("ModernFormatConverter.Strings.TaskManager", CurrentAssembly);

        public static ResourceManager WindowResource { get; } = new("ModernFormatConverter.Strings.Window", CurrentAssembly);
    }
}
