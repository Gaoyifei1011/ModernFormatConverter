using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Models;

namespace ModernFormatConverter.Views.DataTemplates
{
    /// <summary>
    /// 视频转换数据模板选择器
    /// </summary>
    public class VideoConversionDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate VideoCardDataTemplate { get; set; }

        public DataTemplate VideoListDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is VideoConversionFileModel videoConversionFile)
            {
                if (videoConversionFile.VideoConversionConfiguration.VideoConversionTypeKind is VideoConversionTypeKind.VideoFormatConversion)
                {
                    return VideoCardDataTemplate;
                }
                else if (videoConversionFile.VideoConversionConfiguration.VideoConversionTypeKind is VideoConversionTypeKind.VideoConcat)
                {
                    return VideoListDataTemplate;
                }
                else if (videoConversionFile.VideoConversionConfiguration.VideoConversionTypeKind is VideoConversionTypeKind.VideoSeparation)
                {
                    return VideoCardDataTemplate;
                }
                else
                {
                    return base.SelectTemplateCore(item, container);
                }
            }
            else
            {
                return base.SelectTemplateCore(item, container);
            }
        }
    }
}
