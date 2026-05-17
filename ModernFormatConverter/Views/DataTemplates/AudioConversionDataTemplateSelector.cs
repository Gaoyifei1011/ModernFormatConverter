using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Models;

namespace ModernFormatConverter.Views.DataTemplates
{
    /// <summary>
    /// 音频转换数据模板选择器
    /// </summary>
    public class AudioConversionDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AudioCardDataTemplate { get; set; }

        public DataTemplate AudioListDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is AudioConversionFileModel audioConversionFile)
            {
                if (audioConversionFile.AudioConversionOutputConfiguration is not null)
                {
                    if (audioConversionFile.AudioConversionOutputConfiguration.AudioConversionTypeKind is AudioConversionTypeKind.AudioFormatConversion)
                    {
                        return AudioCardDataTemplate;
                    }
                    else if (audioConversionFile.AudioConversionOutputConfiguration.AudioConversionTypeKind is AudioConversionTypeKind.AudioConcat)
                    {
                        return AudioListDataTemplate;
                    }
                }
            }

            return base.SelectTemplateCore(item, container);
        }
    }
}
