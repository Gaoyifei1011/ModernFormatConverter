using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Helpers.Root;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Views.Dialogs;
using ModernFormatConverter.Views.Windows;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

// 抑制 CA1806，CA1822，IDE0060 警告
#pragma warning disable CA1806,CA1822,IDE0060

namespace ModernFormatConverter.Views.Pages
{
    /// <summary>
    /// 视频转换页面
    /// </summary>
    public sealed partial class VideoConversionPage : Page, INotifyPropertyChanged
    {
        private readonly string VideoAngleAdjustmentString = ResourceService.VideoConversionResource.GetString("VideoAngleAdjustment");
        private readonly string VideoConcatString = ResourceService.VideoConversionResource.GetString("VideoConcat");
        private readonly string VideoExportFrameString = ResourceService.VideoConversionResource.GetString("VideoExportFrame");
        private readonly string VideoFormatConversionString = ResourceService.VideoConversionResource.GetString("VideoFormatConversion");
        private readonly string VideoMixedFlowString = ResourceService.VideoConversionResource.GetString("VideoMixedFlow");
        private readonly string VideoSeparationString = ResourceService.VideoConversionResource.GetString("VideoSeparation");
        private readonly string VideoSplitString = ResourceService.VideoConversionResource.GetString("VideoSplit");
        private readonly string VideoSpeedAdjustmentString = ResourceService.VideoConversionResource.GetString("VideoSpeedAdjustment");
        private readonly string VideoRewindString = ResourceService.VideoConversionResource.GetString("VideoRewind");
        private readonly string VideoSplitScreenString = ResourceService.VideoConversionResource.GetString("VideoSplitScreen");

        private VideoConversionTypeModel _selectedConversionType;

        public VideoConversionTypeModel SelectedConversionType
        {
            get { return _selectedConversionType; }

            set
            {
                if (!Equals(_selectedConversionType, value))
                {
                    _selectedConversionType = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedConversionType)));
                }
            }
        }

        public WinRTObservableCollection<VideoConversionTypeModel> ConversionTypeCollection { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public VideoConversionPage()
        {
            InitializeComponent();
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoFormatConversionString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoFormatConversion
            });
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoConcatString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoConcat
            });
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoMixedFlowString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoMixedFlow
            });
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoSplitString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoSplit
            });
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoSeparationString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoSeparation
            });
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoExportFrameString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoExportFrame
            });
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoSpeedAdjustmentString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoSpeedAdjustment
            });
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoAngleAdjustmentString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoAngleAdjustment
            });
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoRewindString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoRewind
            });
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoSplitScreenString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoSplitScreen
            });
            SelectedConversionType = ConversionTypeCollection[0];
        }

        #region 第一部分：ExecuteCommand 命令调用时挂载的事件

        /// <summary>
        /// 移除选中项
        /// </summary>
        private void OnRemoveExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            SelectedConversionType.VideoConversionFileCollection.Remove(args.Parameter as VideoConversionFileModel);
        }

        /// <summary>
        /// 配置选中项转换参数
        /// </summary>
        private async void OnOutputConfigurationExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (args.Parameter is VideoFormatConversionModel videoFormatConversion)
            {
                VideoFormatConversionWindow videoFormatConversionWindow = new(ConversionToolsWindow.Current, videoFormatConversion);
                if (await videoFormatConversionWindow.ShowAsync() is ContentDialogResult.Primary)
                {
                    videoFormatConversion.FormatConversionType = Convert.ToString(videoFormatConversionWindow.SelectedFormatConversionType.SelectedValue);
                    videoFormatConversion.SizeLimitation = Convert.ToString(videoFormatConversionWindow.SelectedSizeLimitation.SelectedValue);
                    videoFormatConversion.VideoEncoding = Convert.ToString(videoFormatConversionWindow.SelectedVideoEncoding.SelectedValue);
                    videoFormatConversion.ScreenSize = Convert.ToString(videoFormatConversionWindow.SelectedScreenSize.SelectedValue);
                    videoFormatConversion.VideoBitRate = Convert.ToString(videoFormatConversionWindow.SelectedVideoBitRate.SelectedValue);
                    videoFormatConversion.CRF = videoFormatConversionWindow.UseCRF ? videoFormatConversionWindow.CRF : -1;
                    videoFormatConversion.GPU = Convert.ToString(videoFormatConversionWindow.SelectedGPU.SelectedValue);
                    videoFormatConversion.FramePerSecond = Convert.ToString(videoFormatConversionWindow.SelectedFramePerSecond.SelectedValue);
                    videoFormatConversion.AspectRatio = Convert.ToString(videoFormatConversionWindow.SelectedAspectRatio.SelectedValue);
                    videoFormatConversion.SecondaryEncoding = videoFormatConversionWindow.SecondaryEncoding;
                    videoFormatConversion.KeyFrameInterval = Convert.ToString(videoFormatConversionWindow.SelectedKeyFrameInterval.SelectedValue);
                    videoFormatConversion.DeInterlace = videoFormatConversionWindow.DeInterlace;
                    videoFormatConversion.Rotation = (System.Windows.Media.Imaging.Rotation)videoFormatConversionWindow.SelectedRotation.SelectedValue;
                    videoFormatConversion.MirrorReversal = videoFormatConversionWindow.MirrorReversal;
                    videoFormatConversion.VideoFadeInEffect = Convert.ToString(videoFormatConversionWindow.SelectedVideoFadeInEffect);
                    videoFormatConversion.VideoFadeOutEffect = Convert.ToString(videoFormatConversionWindow.SelectedVideoFadeOutEffect);

                    videoFormatConversion.AudioEncoding = Convert.ToString(videoFormatConversionWindow.SelectedAudioEncoding.SelectedValue);
                    videoFormatConversion.SamplingRate = Convert.ToString(videoFormatConversionWindow.SelectedSamplingRate.SelectedValue);
                    videoFormatConversion.AudioBitRate = Convert.ToString(videoFormatConversionWindow.SelectedAudioBitRate.SelectedValue);
                    videoFormatConversion.SoundTrack = Convert.ToString(videoFormatConversionWindow.SelectedSoundTrack.SelectedValue);
                    videoFormatConversion.CloseSoundEffect = videoFormatConversionWindow.CloseSoundEffect;
                    videoFormatConversion.Volume = Convert.ToString(videoFormatConversionWindow.SelectedVolume.SelectedValue);
                    videoFormatConversion.PreserveAllSourceInputAudioStream = videoFormatConversionWindow.PreserveAllSourceInputAudioStream;
                    videoFormatConversion.AudioFadeInEffect = Convert.ToString(videoFormatConversionWindow.SelectedAudioFadeInEffect.SelectedValue);
                    videoFormatConversion.AudioFadeOutEffect = Convert.ToString(videoFormatConversionWindow.SelectedAudioFadeOutEffect.SelectedValue);
                    videoFormatConversion.Echo = videoFormatConversionWindow.Echo;
                    videoFormatConversion.DeNoise = videoFormatConversionWindow.DeNoise;
                    videoFormatConversion.Reverse = videoFormatConversionWindow.Reverse;

                    videoFormatConversion.PreserveAllSourceInputSubtitleStream = videoFormatConversionWindow.PreserveAllSourceInputSubtitleStream;
                    videoFormatConversion.AdditionalSubtitlePath = videoFormatConversionWindow.AdditionalSubtitlePath;
                    videoFormatConversion.SubtitleNestType = Convert.ToString(videoFormatConversionWindow.SelectedSubtitleNestType.SelectedValue);
                    videoFormatConversion.FontName = videoFormatConversionWindow.FontName;
                    videoFormatConversion.FontSize = Convert.ToInt32(videoFormatConversionWindow.SelectedFontSize.SelectedValue);
                    videoFormatConversion.FontColor = videoFormatConversionWindow.FontColor;
                    videoFormatConversion.FontBorderStyle = Convert.ToString(videoFormatConversionWindow.SelectedFontBorderStyle.SelectedValue);
                    videoFormatConversion.CounterLineSize = Convert.ToInt32(videoFormatConversionWindow.SelectedCounterLineSize.SelectedValue);
                    videoFormatConversion.CounterLineColor = videoFormatConversionWindow.CounterLineColor;
                    videoFormatConversion.ShadowSize = Convert.ToInt32(videoFormatConversionWindow.SelectedShadowSize.SelectedValue);
                }
            }
        }

        #endregion 第一部分：ExecuteCommand 命令调用时挂载的事件

        #region 第二部分：视频转换页面——挂载的事件

        /// <summary>
        /// 视频转换列表选中项发生变化时触发的事件
        /// </summary>
        private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            SelectedConversionType = args.SelectedItem as VideoConversionTypeModel;
        }

        /// <summary>
        /// 打开输出配置
        /// </summary>
        private async void OnOutputConfigurationClicked(object sender, RoutedEventArgs args)
        {
            VideoFormatConversionWindow videoFormatConversionWindow = new(ConversionToolsWindow.Current);
            if (await videoFormatConversionWindow.ShowAsync() is ContentDialogResult.Primary && SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoFormatConversion)
            {
                foreach (VideoConversionFileModel videoConversionFile in SelectedConversionType.VideoConversionFileCollection)
                {
                    if (videoConversionFile is VideoFormatConversionModel videoFormatConversion)
                    {
                        videoFormatConversion.FormatConversionType = Convert.ToString(videoFormatConversionWindow.SelectedFormatConversionType.SelectedValue);
                        videoFormatConversion.SizeLimitation = Convert.ToString(videoFormatConversionWindow.SelectedSizeLimitation.SelectedValue);
                        videoFormatConversion.VideoEncoding = Convert.ToString(videoFormatConversionWindow.SelectedVideoEncoding.SelectedValue);
                        videoFormatConversion.ScreenSize = Convert.ToString(videoFormatConversionWindow.SelectedScreenSize.SelectedValue);
                        videoFormatConversion.VideoBitRate = Convert.ToString(videoFormatConversionWindow.SelectedVideoBitRate.SelectedValue);
                        videoFormatConversion.CRF = videoFormatConversionWindow.UseCRF ? videoFormatConversionWindow.CRF : -1;
                        videoFormatConversion.GPU = Convert.ToString(videoFormatConversionWindow.SelectedGPU.SelectedValue);
                        videoFormatConversion.FramePerSecond = Convert.ToString(videoFormatConversionWindow.SelectedFramePerSecond.SelectedValue);
                        videoFormatConversion.AspectRatio = Convert.ToString(videoFormatConversionWindow.SelectedAspectRatio.SelectedValue);
                        videoFormatConversion.SecondaryEncoding = videoFormatConversionWindow.SecondaryEncoding;
                        videoFormatConversion.KeyFrameInterval = Convert.ToString(videoFormatConversionWindow.SelectedKeyFrameInterval.SelectedValue);
                        videoFormatConversion.DeInterlace = videoFormatConversionWindow.DeInterlace;
                        videoFormatConversion.Rotation = (System.Windows.Media.Imaging.Rotation)videoFormatConversionWindow.SelectedRotation.SelectedValue;
                        videoFormatConversion.MirrorReversal = videoFormatConversionWindow.MirrorReversal;
                        videoFormatConversion.VideoFadeInEffect = Convert.ToString(videoFormatConversionWindow.SelectedVideoFadeInEffect);
                        videoFormatConversion.VideoFadeOutEffect = Convert.ToString(videoFormatConversionWindow.SelectedVideoFadeOutEffect);

                        videoFormatConversion.AudioEncoding = Convert.ToString(videoFormatConversionWindow.SelectedAudioEncoding.SelectedValue);
                        videoFormatConversion.SamplingRate = Convert.ToString(videoFormatConversionWindow.SelectedSamplingRate.SelectedValue);
                        videoFormatConversion.AudioBitRate = Convert.ToString(videoFormatConversionWindow.SelectedAudioBitRate.SelectedValue);
                        videoFormatConversion.SoundTrack = Convert.ToString(videoFormatConversionWindow.SelectedSoundTrack.SelectedValue);
                        videoFormatConversion.CloseSoundEffect = videoFormatConversionWindow.CloseSoundEffect;
                        videoFormatConversion.Volume = Convert.ToString(videoFormatConversionWindow.SelectedVolume.SelectedValue);
                        videoFormatConversion.PreserveAllSourceInputAudioStream = videoFormatConversionWindow.PreserveAllSourceInputAudioStream;
                        videoFormatConversion.AudioFadeInEffect = Convert.ToString(videoFormatConversionWindow.SelectedAudioFadeInEffect.SelectedValue);
                        videoFormatConversion.AudioFadeOutEffect = Convert.ToString(videoFormatConversionWindow.SelectedAudioFadeOutEffect.SelectedValue);
                        videoFormatConversion.Echo = videoFormatConversionWindow.Echo;
                        videoFormatConversion.DeNoise = videoFormatConversionWindow.DeNoise;
                        videoFormatConversion.Reverse = videoFormatConversionWindow.Reverse;

                        videoFormatConversion.PreserveAllSourceInputSubtitleStream = videoFormatConversionWindow.PreserveAllSourceInputSubtitleStream;
                        videoFormatConversion.AdditionalSubtitlePath = videoFormatConversionWindow.AdditionalSubtitlePath;
                        videoFormatConversion.SubtitleNestType = Convert.ToString(videoFormatConversionWindow.SelectedSubtitleNestType.SelectedValue);
                        videoFormatConversion.FontName = videoFormatConversionWindow.FontName;
                        videoFormatConversion.FontSize = Convert.ToInt32(videoFormatConversionWindow.SelectedFontSize.SelectedValue);
                        videoFormatConversion.FontColor = videoFormatConversionWindow.FontColor;
                        videoFormatConversion.FontBorderStyle = Convert.ToString(videoFormatConversionWindow.SelectedFontBorderStyle.SelectedValue);
                        videoFormatConversion.CounterLineSize = Convert.ToInt32(videoFormatConversionWindow.SelectedCounterLineSize.SelectedValue);
                        videoFormatConversion.CounterLineColor = videoFormatConversionWindow.CounterLineColor;
                        videoFormatConversion.ShadowSize = Convert.ToInt32(videoFormatConversionWindow.SelectedShadowSize.SelectedValue);
                    }
                }
            }
        }

        /// <summary>
        /// 确定
        /// </summary>
        private void OnOkClicked(object sender, RoutedEventArgs args)
        {
            ConversionToolsWindow.Current.Close();
            // TODO：未完成
        }

        #endregion 第二部分：视频转换页面——挂载的事件

        /// <summary>
        /// 获取文件缩略图
        /// </summary>
        private async Task<BitmapImage> GetThumbnailAsync(string filePath)
        {
            MemoryStream memoryStream = null;
            try
            {
                Bitmap thumbnailBitmap = ThumbnailHelper.GetThumbnailBitmap(filePath, 100);

                if (thumbnailBitmap is not null)
                {
                    memoryStream = new();
                    thumbnailBitmap.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    thumbnailBitmap.Dispose();
                }
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionPage), nameof(GetThumbnailAsync), 1, e);
            }

            if (memoryStream is not null)
            {
                try
                {
                    BitmapImage bitmapImage = new();
                    bitmapImage.SetSource(memoryStream.AsRandomAccessStream());
                    return bitmapImage;
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionPage), nameof(GetThumbnailAsync), 2, e);
                    return null;
                }
                finally
                {
                    memoryStream?.Dispose();
                }
            }
            else
            {
                return null;
            }
        }
    }
}
