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
using ModernFormatConverter.WindowsAPI.ComTypes;
using ModernFormatConverter.WindowsAPI.PInvoke.MediaInfo;
using ModernFormatConverter.WindowsAPI.PInvoke.Shell32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

// 抑制 CA1806，CA1822，IDE0060 警告
#pragma warning disable CA1806,CA1822,IDE0060

namespace ModernFormatConverter.Views.Pages
{
    /// <summary>
    /// 视频转换页面
    /// </summary>
    public sealed partial class VideoConversionPage : Page, INotifyPropertyChanged
    {
        private readonly string DragOverContentString = ResourceService.VideoConversionResource.GetString("DragOverContent");
        private readonly string NoFolderString = ResourceService.VideoConversionResource.GetString("NoFolder");
        private readonly string NoMultiFileString = ResourceService.VideoConversionOutputConfigurationResource.GetString("NoMultiFileString");
        private readonly string SelectFileString = ResourceService.VideoConversionResource.GetString("SelectFile");
        private readonly string SelectFolderString = ResourceService.VideoConversionResource.GetString("SelectFolder");
        private readonly string VideoAngleAdjustmentString = ResourceService.VideoConversionResource.GetString("VideoAngleAdjustment");
        private readonly string VideoConcatString = ResourceService.VideoConversionResource.GetString("VideoConcat");
        private readonly string VideoExportPictureString = ResourceService.VideoConversionResource.GetString("VideoExportPicture");
        private readonly string VideoFormatConversionString = ResourceService.VideoConversionResource.GetString("VideoFormatConversion");
        private readonly string VideoMixedFlowString = ResourceService.VideoConversionResource.GetString("VideoMixedFlow");
        private readonly string VideoMixedFlowDragOverContentString = ResourceService.VideoConversionResource.GetString("VideoMixedFlowDragOverContent");
        private readonly string VideoSeparationString = ResourceService.VideoConversionResource.GetString("VideoSeparation");
        private readonly string VideoSpeedAdjustmentString = ResourceService.VideoConversionResource.GetString("VideoSpeedAdjustment");
        private readonly string VideoRewindString = ResourceService.VideoConversionResource.GetString("VideoRewind");
        private readonly string VideoSplitScreenString = ResourceService.VideoConversionResource.GetString("VideoSplitScreen");
        private global::Windows.UI.Color accentColor = global::Windows.UI.Color.FromArgb(System.Windows.SystemParameters.WindowGlassColor.A, System.Windows.SystemParameters.WindowGlassColor.R, System.Windows.SystemParameters.WindowGlassColor.G, System.Windows.SystemParameters.WindowGlassColor.B);

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

        private bool _isGettingFileInformation;

        public bool IsGettingFileInformation
        {
            get { return _isGettingFileInformation; }

            set
            {
                if (!Equals(_isGettingFileInformation, value))
                {
                    _isGettingFileInformation = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsGettingFileInformation)));
                }
            }
        }

        private string _selectedSortRule;

        public string SelectedSortRule
        {
            get { return _selectedSortRule; }

            set
            {
                if (!string.Equals(_selectedSortRule, value))
                {
                    _selectedSortRule = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSortRule)));
                }
            }
        }

        private bool _sortWay;

        public bool SortWay
        {
            get { return _sortWay; }

            set
            {
                if (!Equals(_sortWay, value))
                {
                    _sortWay = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SortWay)));
                }
            }
        }

        private string _outputFolder;

        public string OutputFolder
        {
            get { return _outputFolder; }

            set
            {
                if (!string.Equals(_outputFolder, value))
                {
                    _outputFolder = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OutputFolder)));
                }
            }
        }

        public List<string> SortRuleList { get; } = ["NotSort", "SortByFileName", "SortByFileSize", "SortByDuration"];

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
                VideoConversionTypeKind = VideoConversionTypeKind.VideoMixedFlow,
                VideoMixedFlow = new()
                {
                    VideoConversionOutputConfiguration = new()
                    {
                        VideoConversionTypeKind = VideoConversionTypeKind.VideoMixedFlow,

                        FormatConversionType = ".mp4",
                        SizeLimitation = "Copy",
                        VideoEncoding = "None",
                        ScreenSize = "DefaultSize",
                        VideoBitRate = "Default",
                        CRF = -1,
                        GPU = "None",
                        FramePerSecond = "Default",
                        AspectRatio = "Default",
                        SecondaryEncoding = false,
                        KeyFrameInterval = "Default",
                        DeInterlace = false,
                        Rotation = System.Windows.Media.Imaging.Rotation.Rotate0,
                        MirrorReversal = false,
                        VideoFadeInEffect = "None",
                        VideoFadeOutEffect = "None",

                        AudioEncoding = "Copy",
                        SamplingRate = "Default",
                        AudioBitRate = "Default",
                        SoundTrack = "Default",
                        CloseSoundEffect = false,
                        Volume = "100%",
                        PreserveAllSourceInputAudioStream = false,
                        AudioFadeInEffect = "None",
                        AudioFadeOutEffect = "None",
                        Echo = false,
                        DeNoise = false,
                        Reverse = false,

                        PreserveAllSourceInputSubtitleStream = false,
                        AdditionalSubtitlePath = string.Empty,
                        SubtitleNestType = "Default",
                        FontName = SystemFonts.DefaultFont.Name,
                        FontSize = 1,
                        FontColor = accentColor,
                        FontBorderStyle = "BorderAndShadow",
                        CounterLineSize = 0,
                        CounterLineColor = accentColor,
                        ShadowSize = 0
                    }
                }
            });
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoSeparationString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoSeparation
            });
            ConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoExportPictureString,
                VideoConversionTypeKind = VideoConversionTypeKind.VideoExportPicture
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
            SelectedSortRule = SortRuleList[0];
            SortWay = true;
            Shell32Library.SHGetKnownFolderPath(new("F1B32785-6FBA-4FCF-9D55-7B8E7F157091"), KNOWN_FOLDER_FLAG.KF_FLAG_FORCE_APP_DATA_REDIRECTION, 0, out string outputFolder);
            OutputFolder = outputFolder;
            // TODO：未完成，添加一个读取本地文件夹保存设置
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
            if (args.Parameter is VideoConversionFileModel videoConversionFile && videoConversionFile.VideoConversionOutputConfiguration is not null)
            {
                VideoConversionOutputConfigurationWindow videoConversionOutputConfigurationWindow = new(SelectedConversionType.VideoConversionTypeKind, ConversionToolsWindow.Current, videoConversionFile.VideoConversionOutputConfiguration);
                if (await videoConversionOutputConfigurationWindow.ShowAsync() is ContentDialogResult.Primary)
                {
                    if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoFormatConversion)
                    {
                        videoConversionFile.VideoConversionOutputConfiguration.FormatConversionType = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedFormatConversionType.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.SizeLimitation = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedSizeLimitation.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.VideoEncoding = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedVideoEncoding.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.ScreenSize = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedScreenSize.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.VideoBitRate = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedVideoBitRate.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.CRF = videoConversionOutputConfigurationWindow.UseCRF ? videoConversionOutputConfigurationWindow.CRF : -1;
                        videoConversionFile.VideoConversionOutputConfiguration.GPU = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedGPU.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.FramePerSecond = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedFramePerSecond.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.AspectRatio = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedAspectRatio.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.SecondaryEncoding = videoConversionOutputConfigurationWindow.SecondaryEncoding;
                        videoConversionFile.VideoConversionOutputConfiguration.KeyFrameInterval = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedKeyFrameInterval.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.DeInterlace = videoConversionOutputConfigurationWindow.DeInterlace;
                        videoConversionFile.VideoConversionOutputConfiguration.Rotation = (System.Windows.Media.Imaging.Rotation)videoConversionOutputConfigurationWindow.SelectedRotation.SelectedValue;
                        videoConversionFile.VideoConversionOutputConfiguration.MirrorReversal = videoConversionOutputConfigurationWindow.MirrorReversal;
                        videoConversionFile.VideoConversionOutputConfiguration.VideoFadeInEffect = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedVideoFadeInEffect);
                        videoConversionFile.VideoConversionOutputConfiguration.VideoFadeOutEffect = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedVideoFadeOutEffect);

                        videoConversionFile.VideoConversionOutputConfiguration.AudioEncoding = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedAudioEncoding.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.SamplingRate = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedSamplingRate.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.AudioBitRate = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedAudioBitRate.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.SoundTrack = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedSoundTrack.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.CloseSoundEffect = videoConversionOutputConfigurationWindow.CloseSoundEffect;
                        videoConversionFile.VideoConversionOutputConfiguration.Volume = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedVolume.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.PreserveAllSourceInputAudioStream = videoConversionOutputConfigurationWindow.PreserveAllSourceInputAudioStream;
                        videoConversionFile.VideoConversionOutputConfiguration.AudioFadeInEffect = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedAudioFadeInEffect.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.AudioFadeOutEffect = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedAudioFadeOutEffect.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.Echo = videoConversionOutputConfigurationWindow.Echo;
                        videoConversionFile.VideoConversionOutputConfiguration.DeNoise = videoConversionOutputConfigurationWindow.DeNoise;
                        videoConversionFile.VideoConversionOutputConfiguration.Reverse = videoConversionOutputConfigurationWindow.Reverse;

                        videoConversionFile.VideoConversionOutputConfiguration.PreserveAllSourceInputSubtitleStream = videoConversionOutputConfigurationWindow.PreserveAllSourceInputSubtitleStream;
                        videoConversionFile.VideoConversionOutputConfiguration.AdditionalSubtitlePath = videoConversionOutputConfigurationWindow.AdditionalSubtitlePath;
                        videoConversionFile.VideoConversionOutputConfiguration.SubtitleNestType = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedSubtitleNestType.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.FontName = videoConversionOutputConfigurationWindow.FontName;
                        videoConversionFile.VideoConversionOutputConfiguration.FontSize = Convert.ToInt32(videoConversionOutputConfigurationWindow.SelectedFontSize.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.FontColor = videoConversionOutputConfigurationWindow.FontColor;
                        videoConversionFile.VideoConversionOutputConfiguration.FontBorderStyle = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedFontBorderStyle.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.CounterLineSize = Convert.ToInt32(videoConversionOutputConfigurationWindow.SelectedCounterLineSize.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.CounterLineColor = videoConversionOutputConfigurationWindow.CounterLineColor;
                        videoConversionFile.VideoConversionOutputConfiguration.ShadowSize = Convert.ToInt32(videoConversionOutputConfigurationWindow.SelectedShadowSize.SelectedValue);
                    }
                    else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoConcat)
                    {
                        videoConversionFile.VideoConversionOutputConfiguration.FormatConversionType = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedFormatConversionType.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.SizeLimitation = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedSizeLimitation.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.VideoEncoding = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedVideoEncoding.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.ScreenSize = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedScreenSize.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.VideoBitRate = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedVideoBitRate.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.CRF = videoConversionOutputConfigurationWindow.UseCRF ? videoConversionOutputConfigurationWindow.CRF : -1;
                        videoConversionFile.VideoConversionOutputConfiguration.GPU = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedGPU.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.FramePerSecond = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedFramePerSecond.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.AspectRatio = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedAspectRatio.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.SecondaryEncoding = videoConversionOutputConfigurationWindow.SecondaryEncoding;
                        videoConversionFile.VideoConversionOutputConfiguration.KeyFrameInterval = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedKeyFrameInterval.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.DeInterlace = videoConversionOutputConfigurationWindow.DeInterlace;
                        videoConversionFile.VideoConversionOutputConfiguration.Rotation = (System.Windows.Media.Imaging.Rotation)videoConversionOutputConfigurationWindow.SelectedRotation.SelectedValue;
                        videoConversionFile.VideoConversionOutputConfiguration.MirrorReversal = videoConversionOutputConfigurationWindow.MirrorReversal;
                        videoConversionFile.VideoConversionOutputConfiguration.VideoFadeInEffect = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedVideoFadeInEffect);
                        videoConversionFile.VideoConversionOutputConfiguration.VideoFadeOutEffect = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedVideoFadeOutEffect);

                        videoConversionFile.VideoConversionOutputConfiguration.AudioEncoding = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedAudioEncoding.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.SamplingRate = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedSamplingRate.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.AudioBitRate = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedAudioBitRate.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.SoundTrack = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedSoundTrack.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.CloseSoundEffect = videoConversionOutputConfigurationWindow.CloseSoundEffect;
                        videoConversionFile.VideoConversionOutputConfiguration.Volume = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedVolume.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.PreserveAllSourceInputAudioStream = videoConversionOutputConfigurationWindow.PreserveAllSourceInputAudioStream;
                        videoConversionFile.VideoConversionOutputConfiguration.AudioFadeInEffect = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedAudioFadeInEffect.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.AudioFadeOutEffect = Convert.ToString(videoConversionOutputConfigurationWindow.SelectedAudioFadeOutEffect.SelectedValue);
                        videoConversionFile.VideoConversionOutputConfiguration.Echo = videoConversionOutputConfigurationWindow.Echo;
                        videoConversionFile.VideoConversionOutputConfiguration.DeNoise = videoConversionOutputConfigurationWindow.DeNoise;
                        videoConversionFile.VideoConversionOutputConfiguration.Reverse = videoConversionOutputConfigurationWindow.Reverse;
                    }
                }
            }
        }

        /// <summary>
        /// 剪辑视频
        /// </summary>
        private void OnCutVideoExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (args.Parameter is VideoConversionFileModel videoConversionFileModel)
            {
                // TODO：未完成
            }
        }

        #endregion 第一部分：ExecuteCommand 命令调用时挂载的事件

        #region 第二部分：视频转换页面——挂载的事件

        /// <summary>
        /// 设置拖动的数据的可视表示形式
        /// </summary>
        private async void OnVideoConversionDragEnter(object sender, Microsoft.UI.Xaml.DragEventArgs args)
        {
            DragOperationDeferral dragOperationDeferral = args.GetDeferral();

            try
            {
                IReadOnlyList<IStorageItem> dragItemsList = await args.DataView.GetStorageItemsAsync();
                bool containsFolder = dragItemsList.Any(item => item.IsOfType(StorageItemTypes.Folder));

                if (containsFolder)
                {
                    args.AcceptedOperation = DataPackageOperation.None;
                    args.DragUIOverride.IsCaptionVisible = true;
                    args.DragUIOverride.IsContentVisible = false;
                    args.DragUIOverride.IsGlyphVisible = true;
                    args.DragUIOverride.Caption = NoFolderString;
                }
                else
                {
                    args.AcceptedOperation = DataPackageOperation.Copy;
                    args.DragUIOverride.IsCaptionVisible = true;
                    args.DragUIOverride.IsContentVisible = false;
                    args.DragUIOverride.IsGlyphVisible = true;
                    args.DragUIOverride.Caption = DragOverContentString;
                }
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionPage), nameof(OnVideoConversionDragEnter), 1, e);
            }
            finally
            {
                args.Handled = true;
                dragOperationDeferral.Complete();
            }
        }

        /// <summary>
        /// 设置拖动的视频混流数据的可视表示形式
        /// </summary>
        private async void OnVideoMixedFlowDragEnter(object sender, Microsoft.UI.Xaml.DragEventArgs args)
        {
            DragOperationDeferral dragOperationDeferral = args.GetDeferral();

            try
            {
                IReadOnlyList<IStorageItem> dragItemsList = await args.DataView.GetStorageItemsAsync();
                bool containsFolder = dragItemsList.Any(item => item.IsOfType(StorageItemTypes.Folder));

                if (containsFolder)
                {
                    args.AcceptedOperation = DataPackageOperation.None;
                    args.DragUIOverride.IsCaptionVisible = true;
                    args.DragUIOverride.IsContentVisible = false;
                    args.DragUIOverride.IsGlyphVisible = true;
                    args.DragUIOverride.Caption = NoFolderString;
                }
                else
                {
                    if (dragItemsList.Count is 1)
                    {
                        args.AcceptedOperation = DataPackageOperation.Copy;
                        args.DragUIOverride.IsCaptionVisible = true;
                        args.DragUIOverride.IsContentVisible = false;
                        args.DragUIOverride.IsGlyphVisible = true;
                        args.DragUIOverride.Caption = VideoMixedFlowDragOverContentString;
                    }
                    else
                    {
                        args.AcceptedOperation = DataPackageOperation.None;
                        args.DragUIOverride.IsCaptionVisible = true;
                        args.DragUIOverride.IsContentVisible = false;
                        args.DragUIOverride.IsGlyphVisible = true;
                        args.DragUIOverride.Caption = NoMultiFileString;
                    }
                }
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionPage), nameof(OnVideoMixedFlowDragEnter), 1, e);
            }
            finally
            {
                args.Handled = true;
                dragOperationDeferral.Complete();
            }
        }

        /// <summary>
        /// 拖动文件完成后获取文件信息
        /// </summary>
        private async void OnVideoConversionDrop(object sender, Microsoft.UI.Xaml.DragEventArgs args)
        {
            DragOperationDeferral dragOperationDeferral = args.GetDeferral();
            IReadOnlyList<IStorageItem> fileList = null;

            try
            {
                DataPackageView dataPackageView = args.DataView;
                fileList = await Task.Run(async () =>
                {
                    try
                    {
                        if (dataPackageView.Contains(StandardDataFormats.StorageItems))
                        {
                            return await dataPackageView.GetStorageItemsAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionPage), nameof(OnVideoConversionDrop), 1, e);
                    }

                    return null;
                });
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionPage), nameof(OnVideoConversionDrop), 2, e);
            }
            finally
            {
                dragOperationDeferral.Complete();
            }

            if (fileList is not null && fileList.Count > 0)
            {
                IsGettingFileInformation = true;
                List<VideoConversionFileModel> videoConversionFileList = [.. SelectedConversionType.VideoConversionFileCollection];
                List<Task> taskList = [];
                object fileInformationLock = new();
                foreach (IStorageItem file in fileList)
                {
                    taskList.Add(Task.Run(async () =>
                    {
                        if (GetFileInformation(file.Path) is VideoConversionFileModel videoConversionFile)
                        {
                            lock (fileInformationLock)
                            {
                                videoConversionFileList.Add(videoConversionFile);
                            }
                        }
                    }));
                }
                await Task.WhenAll(taskList);
                List<VideoConversionFileModel> sortedVideoConversionFileList = SortData(videoConversionFileList);
                SelectedConversionType.VideoConversionFileCollection.Clear();
                foreach (VideoConversionFileModel sortedVideoConversionFile in sortedVideoConversionFileList)
                {
                    if (!Equals(SelectedConversionType.VideoConversionTypeKind, VideoConversionTypeKind.VideoConcat) && sortedVideoConversionFile.FileThumbnailSource is null)
                    {
                        sortedVideoConversionFile.FileThumbnailSource = GetThumbnail(sortedVideoConversionFile.FilePath);
                    }
                    SelectedConversionType.VideoConversionFileCollection.Add(sortedVideoConversionFile);
                }
                IsGettingFileInformation = false;
            }
        }

        /// <summary>
        /// 拖动文件完成后获取视频混流——视频部分文件信息
        /// </summary>
        private async void OnVideoMixedFlowVideoDrop(object sender, Microsoft.UI.Xaml.DragEventArgs args)
        {
            DragOperationDeferral dragOperationDeferral = args.GetDeferral();
            string filePath = string.Empty;

            try
            {
                DataPackageView dataPackageView = args.DataView;
                IReadOnlyList<IStorageItem> filesList = await Task.Run(async () =>
                {
                    try
                    {
                        if (dataPackageView.Contains(StandardDataFormats.StorageItems))
                        {
                            return await dataPackageView.GetStorageItemsAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(FileInformationPage), nameof(OnVideoMixedFlowVideoDrop), 1, e);
                    }

                    return null;
                });

                if (filesList is not null && filesList.Count is 1)
                {
                    filePath = filesList[0].Path;
                }
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionPage), nameof(OnVideoMixedFlowVideoDrop), 2, e);
            }
            finally
            {
                dragOperationDeferral.Complete();
            }

            if (File.Exists(filePath))
            {
                IsGettingFileInformation = true;
                if (await Task.Run(() => { return GetFileInformation(filePath, StreamKind.Video); }) is VideoMixedFlowFileModel videoMixedFlowFile)
                {
                    SelectedConversionType.VideoMixedFlow.VideoFile = videoMixedFlowFile;
                    SelectedConversionType.VideoMixedFlow.IsVideoFileExisted = true;
                }
                if (Equals(SelectedConversionType.VideoConversionTypeKind, VideoConversionTypeKind.VideoMixedFlow) && SelectedConversionType.VideoMixedFlow.VideoFile.FileThumbnailSource is null)
                {
                    SelectedConversionType.VideoMixedFlow.VideoFile.FileThumbnailSource = GetThumbnail(filePath);
                }
                IsGettingFileInformation = false;
            }
        }

        /// <summary>
        /// 移除视频混流——视频部分文件
        /// </summary>
        private void OnRemoveVideoMixedFlowVideoFileClicked(object sender, RoutedEventArgs args)
        {
            if (SelectedConversionType.VideoMixedFlow is not null)
            {
                SelectedConversionType.VideoMixedFlow.VideoFile = null;
                SelectedConversionType.VideoMixedFlow.IsVideoFileExisted = false;
            }
        }

        /// <summary>
        /// 拖动文件完成后获取视频混流——音频部分文件信息
        /// </summary>
        private async void OnVideoMixedFlowAudioDrop(object sender, Microsoft.UI.Xaml.DragEventArgs args)
        {
            DragOperationDeferral dragOperationDeferral = args.GetDeferral();
            string filePath = string.Empty;

            try
            {
                DataPackageView dataPackageView = args.DataView;
                IReadOnlyList<IStorageItem> filesList = await Task.Run(async () =>
                {
                    try
                    {
                        if (dataPackageView.Contains(StandardDataFormats.StorageItems))
                        {
                            return await dataPackageView.GetStorageItemsAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(FileInformationPage), nameof(OnVideoMixedFlowAudioDrop), 1, e);
                    }

                    return null;
                });

                if (filesList is not null && filesList.Count is 1)
                {
                    filePath = filesList[0].Path;
                }
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionPage), nameof(OnVideoMixedFlowAudioDrop), 2, e);
            }
            finally
            {
                dragOperationDeferral.Complete();
            }

            if (File.Exists(filePath))
            {
                IsGettingFileInformation = true;
                if (await Task.Run(() => { return GetFileInformation(filePath, StreamKind.Audio); }) is VideoMixedFlowFileModel videoMixedFlowFile)
                {
                    SelectedConversionType.VideoMixedFlow.AudioFile = videoMixedFlowFile;
                    SelectedConversionType.VideoMixedFlow.IsAudioFileExisted = true;
                }
                if (Equals(SelectedConversionType.VideoConversionTypeKind, VideoConversionTypeKind.VideoMixedFlow) && SelectedConversionType.VideoMixedFlow.AudioFile.FileThumbnailSource is null)
                {
                    SelectedConversionType.VideoMixedFlow.AudioFile.FileThumbnailSource = GetThumbnail(filePath);
                }
                IsGettingFileInformation = false;
            }
        }

        /// <summary>
        /// 移除视频混流——音频部分文件
        /// </summary>
        private void OnRemoveVideoMixedFlowAudioFileClicked(object sender, RoutedEventArgs args)
        {
            if (SelectedConversionType.VideoMixedFlow is not null)
            {
                SelectedConversionType.VideoMixedFlow.AudioFile = null;
                SelectedConversionType.VideoMixedFlow.IsAudioFileExisted = false;
            }
        }

        /// <summary>
        /// 拖动文件完成后获取视频混流——字幕部分文件信息
        /// </summary>
        private async void OnVideoMixedFlowSubtitleDrop(object sender, Microsoft.UI.Xaml.DragEventArgs args)
        {
            DragOperationDeferral dragOperationDeferral = args.GetDeferral();
            string filePath = string.Empty;

            try
            {
                DataPackageView dataPackageView = args.DataView;
                IReadOnlyList<IStorageItem> filesList = await Task.Run(async () =>
                {
                    try
                    {
                        if (dataPackageView.Contains(StandardDataFormats.StorageItems))
                        {
                            return await dataPackageView.GetStorageItemsAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(FileInformationPage), nameof(OnVideoMixedFlowSubtitleDrop), 1, e);
                    }

                    return null;
                });

                if (filesList is not null && filesList.Count is 1)
                {
                    filePath = filesList[0].Path;
                }
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionPage), nameof(OnVideoMixedFlowSubtitleDrop), 2, e);
            }
            finally
            {
                dragOperationDeferral.Complete();
            }

            if (File.Exists(filePath))
            {
                IsGettingFileInformation = true;
                if (await Task.Run(() => { return GetFileInformation(filePath); }) is VideoMixedFlowFileModel videoMixedFlowFile)
                {
                    SelectedConversionType.VideoMixedFlow.SubtitleFile = videoMixedFlowFile;
                    SelectedConversionType.VideoMixedFlow.IsSubtitleFileExisted = true;
                }
                if (Equals(SelectedConversionType.VideoConversionTypeKind, VideoConversionTypeKind.VideoMixedFlow) && SelectedConversionType.VideoMixedFlow.SubtitleFile.FileThumbnailSource is null)
                {
                    SelectedConversionType.VideoMixedFlow.SubtitleFile.FileThumbnailSource = GetThumbnail(filePath);
                }
                IsGettingFileInformation = false;
            }
        }

        /// <summary>
        /// 移除视频混流——字幕部分文件
        /// </summary>
        private void OnRemoveVideoMixedFlowSubtitleFileClicked(object sender, RoutedEventArgs args)
        {
            if (SelectedConversionType.VideoMixedFlow is not null)
            {
                SelectedConversionType.VideoMixedFlow.SubtitleFile = null;
                SelectedConversionType.VideoMixedFlow.IsSubtitleFileExisted = false;
            }
        }

        /// <summary>
        /// 视频转换列表选中项发生变化时触发的事件
        /// </summary>
        private async void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            SelectedConversionType = args.SelectedItem as VideoConversionTypeModel;
            IsGettingFileInformation = true;
            List<VideoConversionFileModel> sortedVideoConversionFileList = await Task.Run(() =>
            {
                List<VideoConversionFileModel> videoConversionFileList = [.. SelectedConversionType.VideoConversionFileCollection];
                return SortData(videoConversionFileList);
            });
            SelectedConversionType.VideoConversionFileCollection.Clear();
            foreach (VideoConversionFileModel sortedVideoConversionFile in sortedVideoConversionFileList)
            {
                SelectedConversionType.VideoConversionFileCollection.Add(sortedVideoConversionFile);
            }
            IsGettingFileInformation = false;
        }

        /// <summary>
        /// 选择排序规则
        /// </summary>
        private async void OnSortRuleClicked(object sender, RoutedEventArgs args)
        {
            if (sender is RadioMenuFlyoutItem radioMenuFlyoutItem && radioMenuFlyoutItem.Tag is not null)
            {
                SelectedSortRule = Convert.ToString(radioMenuFlyoutItem.Tag);
                IsGettingFileInformation = true;
                List<VideoConversionFileModel> sortedVideoConversionFileList = await Task.Run(() =>
                {
                    List<VideoConversionFileModel> videoConversionFileList = [.. SelectedConversionType.VideoConversionFileCollection];
                    return SortData(videoConversionFileList);
                });
                SelectedConversionType.VideoConversionFileCollection.Clear();
                foreach (VideoConversionFileModel sortedVideoConversionFile in sortedVideoConversionFileList)
                {
                    SelectedConversionType.VideoConversionFileCollection.Add(sortedVideoConversionFile);
                }
                IsGettingFileInformation = false;
            }
        }

        /// <summary>
        /// 选择排序方式
        /// </summary>
        private async void OnSortWayClicked(object sender, RoutedEventArgs args)
        {
            if (sender is RadioMenuFlyoutItem radioMenuFlyoutItem && radioMenuFlyoutItem.Tag is not null)
            {
                SortWay = Convert.ToBoolean(radioMenuFlyoutItem.Tag);
                IsGettingFileInformation = true;
                List<VideoConversionFileModel> sortedVideoConversionFileList = await Task.Run(() =>
                {
                    List<VideoConversionFileModel> videoConversionFileList = [.. SelectedConversionType.VideoConversionFileCollection];
                    return SortData(videoConversionFileList);
                });
                SelectedConversionType.VideoConversionFileCollection.Clear();
                foreach (VideoConversionFileModel sortedVideoConversionFile in sortedVideoConversionFileList)
                {
                    SelectedConversionType.VideoConversionFileCollection.Add(sortedVideoConversionFile);
                }
                IsGettingFileInformation = false;
            }
        }

        /// <summary>
        /// 清空
        /// </summary>
        private void OnClearClicked(object sender, RoutedEventArgs args)
        {
            if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoMixedFlow)
            {
                SelectedConversionType.VideoMixedFlow.IsVideoFileExisted = false;
                SelectedConversionType.VideoMixedFlow.VideoFile = null;
                SelectedConversionType.VideoMixedFlow.IsAudioFileExisted = false;
                SelectedConversionType.VideoMixedFlow.AudioFile = null;
                SelectedConversionType.VideoMixedFlow.IsSubtitleFileExisted = false;
                SelectedConversionType.VideoMixedFlow.SubtitleFile = null;
            }
            else
            {
                SelectedConversionType.VideoConversionFileCollection.Clear();
            }
        }

        /// <summary>
        /// 添加文件
        /// </summary>
        private async void OnAddFileClicked(object sender, RoutedEventArgs args)
        {
            OpenFileDialog openFileDialog = new()
            {
                Multiselect = true,
                Title = SelectFileString
            };
            if (openFileDialog.ShowDialog() is DialogResult.OK)
            {
                IsGettingFileInformation = true;
                List<VideoConversionFileModel> videoConversionFileList = [.. SelectedConversionType.VideoConversionFileCollection];
                List<Task> taskList = [];
                object fileInformationLock = new();
                foreach (string filePath in openFileDialog.FileNames)
                {
                    taskList.Add(Task.Run(async () =>
                    {
                        if (GetFileInformation(filePath) is VideoConversionFileModel videoConversionFile)
                        {
                            lock (fileInformationLock)
                            {
                                videoConversionFileList.Add(videoConversionFile);
                            }
                        }
                    }));
                }
                await Task.WhenAll(taskList);
                List<VideoConversionFileModel> sortedVideoConversionFileList = SortData(videoConversionFileList);
                SelectedConversionType.VideoConversionFileCollection.Clear();
                foreach (VideoConversionFileModel sortedVideoConversionFile in sortedVideoConversionFileList)
                {
                    if (!Equals(SelectedConversionType.VideoConversionTypeKind, VideoConversionTypeKind.VideoConcat) && sortedVideoConversionFile.FileThumbnailSource is null)
                    {
                        sortedVideoConversionFile.FileThumbnailSource = GetThumbnail(sortedVideoConversionFile.FilePath);
                    }
                    SelectedConversionType.VideoConversionFileCollection.Add(sortedVideoConversionFile);
                }
                IsGettingFileInformation = false;
            }
            openFileDialog.Dispose();
        }

        /// <summary>
        /// 添加视频文件
        /// </summary>
        private async void OnAddVideoFileClicked(object sender, RoutedEventArgs args)
        {
            OpenFileDialog openFileDialog = new()
            {
                Multiselect = false,
                Title = SelectFileString
            };
            if (openFileDialog.ShowDialog() is DialogResult.OK)
            {
                IsGettingFileInformation = true;
                if (await Task.Run(() => { return GetFileInformation(openFileDialog.FileName, StreamKind.Video); }) is VideoMixedFlowFileModel videoMixedFlowFile)
                {
                    SelectedConversionType.VideoMixedFlow.VideoFile = videoMixedFlowFile;
                    SelectedConversionType.VideoMixedFlow.IsVideoFileExisted = true;
                }
                if (Equals(SelectedConversionType.VideoConversionTypeKind, VideoConversionTypeKind.VideoMixedFlow) && SelectedConversionType.VideoMixedFlow.VideoFile.FileThumbnailSource is null)
                {
                    SelectedConversionType.VideoMixedFlow.VideoFile.FileThumbnailSource = GetThumbnail(openFileDialog.FileName);
                }
                IsGettingFileInformation = false;
            }
            openFileDialog.Dispose();
        }

        /// <summary>
        /// 添加音频文件
        /// </summary>
        private async void OnAddAudiofileClicked(object sender, RoutedEventArgs args)
        {
            OpenFileDialog openFileDialog = new()
            {
                Multiselect = false,
                Title = SelectFileString
            };
            if (openFileDialog.ShowDialog() is DialogResult.OK)
            {
                IsGettingFileInformation = true;
                if (await Task.Run(() => { return GetFileInformation(openFileDialog.FileName, StreamKind.Audio); }) is VideoMixedFlowFileModel videoMixedFlowFile)
                {
                    SelectedConversionType.VideoMixedFlow.AudioFile = videoMixedFlowFile;
                    SelectedConversionType.VideoMixedFlow.IsAudioFileExisted = true;
                }
                if (Equals(SelectedConversionType.VideoConversionTypeKind, VideoConversionTypeKind.VideoMixedFlow) && SelectedConversionType.VideoMixedFlow.AudioFile.FileThumbnailSource is null)
                {
                    SelectedConversionType.VideoMixedFlow.AudioFile.FileThumbnailSource = GetThumbnail(openFileDialog.FileName);
                }
                IsGettingFileInformation = false;
            }
            openFileDialog.Dispose();
        }

        /// <summary>
        /// 添加字幕文件
        /// </summary>
        private async void OnAddSubtitlefileClicked(object sender, RoutedEventArgs args)
        {
            OpenFileDialog openFileDialog = new()
            {
                Multiselect = false,
                Title = SelectFileString
            };
            if (openFileDialog.ShowDialog() is DialogResult.OK)
            {
                IsGettingFileInformation = true;
                if (await Task.Run(() => { return GetFileInformation(openFileDialog.FileName); }) is VideoMixedFlowFileModel videoMixedFlowFile)
                {
                    SelectedConversionType.VideoMixedFlow.SubtitleFile = videoMixedFlowFile;
                    SelectedConversionType.VideoMixedFlow.IsSubtitleFileExisted = true;
                }
                if (Equals(SelectedConversionType.VideoConversionTypeKind, VideoConversionTypeKind.VideoMixedFlow) && SelectedConversionType.VideoMixedFlow.SubtitleFile.FileThumbnailSource is null)
                {
                    SelectedConversionType.VideoMixedFlow.SubtitleFile.FileThumbnailSource = GetThumbnail(openFileDialog.FileName);
                }
                IsGettingFileInformation = false;
            }
            openFileDialog.Dispose();
        }

        /// <summary>
        /// 从文件夹中添加
        /// </summary>
        private async void OnAddFromFolderClicked(object sender, RoutedEventArgs args)
        {
            OpenFolderDialog openFolderDialog = new((nint)MainWindow.Current.AppWindow.Id.Value)
            {
                Description = SelectFolderString,
                RootFolder = Environment.SpecialFolder.Desktop
            };
            DialogResult dialogResult = openFolderDialog.ShowDialog();
            if (dialogResult is DialogResult.OK || dialogResult is DialogResult.Yes)
            {
                IsGettingFileInformation = true;
                List<VideoConversionFileModel> videoConversionFileList = [.. SelectedConversionType.VideoConversionFileCollection];
                string[] filePathArray = Directory.GetFiles(openFolderDialog.SelectedPath);
                List<Task> taskList = [];
                object fileInformationLock = new();
                foreach (string filePath in filePathArray)
                {
                    taskList.Add(Task.Run(async () =>
                    {
                        if (GetFileInformation(filePath) is VideoConversionFileModel videoConversionFile)
                        {
                            lock (fileInformationLock)
                            {
                                videoConversionFileList.Add(videoConversionFile);
                            }
                        }
                    }));
                }
                await Task.WhenAll(taskList);
                List<VideoConversionFileModel> sortedVideoConversionFileList = SortData(videoConversionFileList);
                SelectedConversionType.VideoConversionFileCollection.Clear();
                foreach (VideoConversionFileModel sortedVideoConversionFile in sortedVideoConversionFileList)
                {
                    if (!Equals(SelectedConversionType.VideoConversionTypeKind, VideoConversionTypeKind.VideoConcat) && sortedVideoConversionFile.FileThumbnailSource is null)
                    {
                        sortedVideoConversionFile.FileThumbnailSource = GetThumbnail(sortedVideoConversionFile.FilePath);
                    }
                    SelectedConversionType.VideoConversionFileCollection.Add(sortedVideoConversionFile);
                }
                IsGettingFileInformation = false;
            }
            openFolderDialog.Dispose();
        }

        /// <summary>
        /// 打开输出配置
        /// </summary>
        private async void OnOutputConfigurationClicked(object sender, RoutedEventArgs args)
        {
            // 视频格式转换输出配置
            if (Equals(SelectedConversionType, ConversionTypeCollection[0]))
            {
                VideoConversionOutputConfigurationWindow videoFormatOutputConversionWindow = new(SelectedConversionType.VideoConversionTypeKind, ConversionToolsWindow.Current);
                if (await videoFormatOutputConversionWindow.ShowAsync() is ContentDialogResult.Primary && SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoFormatConversion)
                {
                    foreach (VideoConversionFileModel videoConversionFile in SelectedConversionType.VideoConversionFileCollection)
                    {
                        if (videoConversionFile.VideoConversionOutputConfiguration is not null)
                        {
                            videoConversionFile.VideoConversionOutputConfiguration.FormatConversionType = Convert.ToString(videoFormatOutputConversionWindow.SelectedFormatConversionType.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.SizeLimitation = Convert.ToString(videoFormatOutputConversionWindow.SelectedSizeLimitation.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.VideoEncoding = Convert.ToString(videoFormatOutputConversionWindow.SelectedVideoEncoding.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.ScreenSize = Convert.ToString(videoFormatOutputConversionWindow.SelectedScreenSize.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.VideoBitRate = Convert.ToString(videoFormatOutputConversionWindow.SelectedVideoBitRate.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.CRF = videoFormatOutputConversionWindow.UseCRF ? videoFormatOutputConversionWindow.CRF : -1;
                            videoConversionFile.VideoConversionOutputConfiguration.GPU = Convert.ToString(videoFormatOutputConversionWindow.SelectedGPU.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.FramePerSecond = Convert.ToString(videoFormatOutputConversionWindow.SelectedFramePerSecond.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.AspectRatio = Convert.ToString(videoFormatOutputConversionWindow.SelectedAspectRatio.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.SecondaryEncoding = videoFormatOutputConversionWindow.SecondaryEncoding;
                            videoConversionFile.VideoConversionOutputConfiguration.KeyFrameInterval = Convert.ToString(videoFormatOutputConversionWindow.SelectedKeyFrameInterval.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.DeInterlace = videoFormatOutputConversionWindow.DeInterlace;
                            videoConversionFile.VideoConversionOutputConfiguration.Rotation = (System.Windows.Media.Imaging.Rotation)videoFormatOutputConversionWindow.SelectedRotation.SelectedValue;
                            videoConversionFile.VideoConversionOutputConfiguration.MirrorReversal = videoFormatOutputConversionWindow.MirrorReversal;
                            videoConversionFile.VideoConversionOutputConfiguration.VideoFadeInEffect = Convert.ToString(videoFormatOutputConversionWindow.SelectedVideoFadeInEffect);
                            videoConversionFile.VideoConversionOutputConfiguration.VideoFadeOutEffect = Convert.ToString(videoFormatOutputConversionWindow.SelectedVideoFadeOutEffect);

                            videoConversionFile.VideoConversionOutputConfiguration.AudioEncoding = Convert.ToString(videoFormatOutputConversionWindow.SelectedAudioEncoding.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.SamplingRate = Convert.ToString(videoFormatOutputConversionWindow.SelectedSamplingRate.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.AudioBitRate = Convert.ToString(videoFormatOutputConversionWindow.SelectedAudioBitRate.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.SoundTrack = Convert.ToString(videoFormatOutputConversionWindow.SelectedSoundTrack.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.CloseSoundEffect = videoFormatOutputConversionWindow.CloseSoundEffect;
                            videoConversionFile.VideoConversionOutputConfiguration.Volume = Convert.ToString(videoFormatOutputConversionWindow.SelectedVolume.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.PreserveAllSourceInputAudioStream = videoFormatOutputConversionWindow.PreserveAllSourceInputAudioStream;
                            videoConversionFile.VideoConversionOutputConfiguration.AudioFadeInEffect = Convert.ToString(videoFormatOutputConversionWindow.SelectedAudioFadeInEffect.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.AudioFadeOutEffect = Convert.ToString(videoFormatOutputConversionWindow.SelectedAudioFadeOutEffect.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.Echo = videoFormatOutputConversionWindow.Echo;
                            videoConversionFile.VideoConversionOutputConfiguration.DeNoise = videoFormatOutputConversionWindow.DeNoise;
                            videoConversionFile.VideoConversionOutputConfiguration.Reverse = videoFormatOutputConversionWindow.Reverse;

                            videoConversionFile.VideoConversionOutputConfiguration.PreserveAllSourceInputSubtitleStream = videoFormatOutputConversionWindow.PreserveAllSourceInputSubtitleStream;
                            videoConversionFile.VideoConversionOutputConfiguration.AdditionalSubtitlePath = videoFormatOutputConversionWindow.AdditionalSubtitlePath;
                            videoConversionFile.VideoConversionOutputConfiguration.SubtitleNestType = Convert.ToString(videoFormatOutputConversionWindow.SelectedSubtitleNestType.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.FontName = videoFormatOutputConversionWindow.FontName;
                            videoConversionFile.VideoConversionOutputConfiguration.FontSize = Convert.ToInt32(videoFormatOutputConversionWindow.SelectedFontSize.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.FontColor = videoFormatOutputConversionWindow.FontColor;
                            videoConversionFile.VideoConversionOutputConfiguration.FontBorderStyle = Convert.ToString(videoFormatOutputConversionWindow.SelectedFontBorderStyle.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.CounterLineSize = Convert.ToInt32(videoFormatOutputConversionWindow.SelectedCounterLineSize.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.CounterLineColor = videoFormatOutputConversionWindow.CounterLineColor;
                            videoConversionFile.VideoConversionOutputConfiguration.ShadowSize = Convert.ToInt32(videoFormatOutputConversionWindow.SelectedShadowSize.SelectedValue);
                        }
                    }
                }
            }
            // 视频合并输出配置
            else if (Equals(SelectedConversionType, ConversionTypeCollection[1]))
            {
                VideoConversionOutputConfigurationWindow videoFormatConversionWindow = new(SelectedConversionType.VideoConversionTypeKind, ConversionToolsWindow.Current);
                if (await videoFormatConversionWindow.ShowAsync() is ContentDialogResult.Primary && SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoFormatConversion)
                {
                    foreach (VideoConversionFileModel videoConversionFile in SelectedConversionType.VideoConversionFileCollection)
                    {
                        if (videoConversionFile.VideoConversionOutputConfiguration is not null)
                        {
                            videoConversionFile.VideoConversionOutputConfiguration.FormatConversionType = Convert.ToString(videoFormatConversionWindow.SelectedFormatConversionType.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.SizeLimitation = Convert.ToString(videoFormatConversionWindow.SelectedSizeLimitation.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.VideoEncoding = Convert.ToString(videoFormatConversionWindow.SelectedVideoEncoding.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.ScreenSize = Convert.ToString(videoFormatConversionWindow.SelectedScreenSize.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.VideoBitRate = Convert.ToString(videoFormatConversionWindow.SelectedVideoBitRate.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.CRF = videoFormatConversionWindow.UseCRF ? videoFormatConversionWindow.CRF : -1;
                            videoConversionFile.VideoConversionOutputConfiguration.GPU = Convert.ToString(videoFormatConversionWindow.SelectedGPU.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.FramePerSecond = Convert.ToString(videoFormatConversionWindow.SelectedFramePerSecond.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.AspectRatio = Convert.ToString(videoFormatConversionWindow.SelectedAspectRatio.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.SecondaryEncoding = videoFormatConversionWindow.SecondaryEncoding;
                            videoConversionFile.VideoConversionOutputConfiguration.KeyFrameInterval = Convert.ToString(videoFormatConversionWindow.SelectedKeyFrameInterval.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.DeInterlace = videoFormatConversionWindow.DeInterlace;
                            videoConversionFile.VideoConversionOutputConfiguration.Rotation = (System.Windows.Media.Imaging.Rotation)videoFormatConversionWindow.SelectedRotation.SelectedValue;
                            videoConversionFile.VideoConversionOutputConfiguration.MirrorReversal = videoFormatConversionWindow.MirrorReversal;
                            videoConversionFile.VideoConversionOutputConfiguration.VideoFadeInEffect = Convert.ToString(videoFormatConversionWindow.SelectedVideoFadeInEffect);
                            videoConversionFile.VideoConversionOutputConfiguration.VideoFadeOutEffect = Convert.ToString(videoFormatConversionWindow.SelectedVideoFadeOutEffect);

                            videoConversionFile.VideoConversionOutputConfiguration.AudioEncoding = Convert.ToString(videoFormatConversionWindow.SelectedAudioEncoding.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.SamplingRate = Convert.ToString(videoFormatConversionWindow.SelectedSamplingRate.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.AudioBitRate = Convert.ToString(videoFormatConversionWindow.SelectedAudioBitRate.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.SoundTrack = Convert.ToString(videoFormatConversionWindow.SelectedSoundTrack.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.CloseSoundEffect = videoFormatConversionWindow.CloseSoundEffect;
                            videoConversionFile.VideoConversionOutputConfiguration.Volume = Convert.ToString(videoFormatConversionWindow.SelectedVolume.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.PreserveAllSourceInputAudioStream = videoFormatConversionWindow.PreserveAllSourceInputAudioStream;
                            videoConversionFile.VideoConversionOutputConfiguration.AudioFadeInEffect = Convert.ToString(videoFormatConversionWindow.SelectedAudioFadeInEffect.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.AudioFadeOutEffect = Convert.ToString(videoFormatConversionWindow.SelectedAudioFadeOutEffect.SelectedValue);
                            videoConversionFile.VideoConversionOutputConfiguration.Echo = videoFormatConversionWindow.Echo;
                            videoConversionFile.VideoConversionOutputConfiguration.DeNoise = videoFormatConversionWindow.DeNoise;
                            videoConversionFile.VideoConversionOutputConfiguration.Reverse = videoFormatConversionWindow.Reverse;
                        }
                    }
                }
            }
            // 视频混流输出配置
            else if (Equals(SelectedConversionType, ConversionTypeCollection[2]))
            {
                VideoConversionOutputConfigurationWindow videoFormatConversionWindow = new(SelectedConversionType.VideoConversionTypeKind, ConversionToolsWindow.Current);
                if (await videoFormatConversionWindow.ShowAsync() is ContentDialogResult.Primary && SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoFormatConversion)
                {
                    if (SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration is not null)
                    {
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.FormatConversionType = Convert.ToString(videoFormatConversionWindow.SelectedFormatConversionType.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.SizeLimitation = Convert.ToString(videoFormatConversionWindow.SelectedSizeLimitation.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.VideoEncoding = Convert.ToString(videoFormatConversionWindow.SelectedVideoEncoding.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.ScreenSize = Convert.ToString(videoFormatConversionWindow.SelectedScreenSize.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.VideoBitRate = Convert.ToString(videoFormatConversionWindow.SelectedVideoBitRate.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.CRF = videoFormatConversionWindow.UseCRF ? videoFormatConversionWindow.CRF : -1;
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.GPU = Convert.ToString(videoFormatConversionWindow.SelectedGPU.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.FramePerSecond = Convert.ToString(videoFormatConversionWindow.SelectedFramePerSecond.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.AspectRatio = Convert.ToString(videoFormatConversionWindow.SelectedAspectRatio.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.SecondaryEncoding = videoFormatConversionWindow.SecondaryEncoding;
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.KeyFrameInterval = Convert.ToString(videoFormatConversionWindow.SelectedKeyFrameInterval.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.DeInterlace = videoFormatConversionWindow.DeInterlace;
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.Rotation = (System.Windows.Media.Imaging.Rotation)videoFormatConversionWindow.SelectedRotation.SelectedValue;
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.MirrorReversal = videoFormatConversionWindow.MirrorReversal;
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.VideoFadeInEffect = Convert.ToString(videoFormatConversionWindow.SelectedVideoFadeInEffect);
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.VideoFadeOutEffect = Convert.ToString(videoFormatConversionWindow.SelectedVideoFadeOutEffect);

                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.AudioEncoding = Convert.ToString(videoFormatConversionWindow.SelectedAudioEncoding.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.SamplingRate = Convert.ToString(videoFormatConversionWindow.SelectedSamplingRate.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.AudioBitRate = Convert.ToString(videoFormatConversionWindow.SelectedAudioBitRate.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.SoundTrack = Convert.ToString(videoFormatConversionWindow.SelectedSoundTrack.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.CloseSoundEffect = videoFormatConversionWindow.CloseSoundEffect;
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.Volume = Convert.ToString(videoFormatConversionWindow.SelectedVolume.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.PreserveAllSourceInputAudioStream = videoFormatConversionWindow.PreserveAllSourceInputAudioStream;
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.AudioFadeInEffect = Convert.ToString(videoFormatConversionWindow.SelectedAudioFadeInEffect.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.AudioFadeOutEffect = Convert.ToString(videoFormatConversionWindow.SelectedAudioFadeOutEffect.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.Echo = videoFormatConversionWindow.Echo;
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.DeNoise = videoFormatConversionWindow.DeNoise;
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.Reverse = videoFormatConversionWindow.Reverse;

                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.PreserveAllSourceInputSubtitleStream = videoFormatConversionWindow.PreserveAllSourceInputSubtitleStream;
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.AdditionalSubtitlePath = videoFormatConversionWindow.AdditionalSubtitlePath;
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.SubtitleNestType = Convert.ToString(videoFormatConversionWindow.SelectedSubtitleNestType.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.FontName = videoFormatConversionWindow.FontName;
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.FontSize = Convert.ToInt32(videoFormatConversionWindow.SelectedFontSize.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.FontColor = videoFormatConversionWindow.FontColor;
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.FontBorderStyle = Convert.ToString(videoFormatConversionWindow.SelectedFontBorderStyle.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.CounterLineSize = Convert.ToInt32(videoFormatConversionWindow.SelectedCounterLineSize.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.CounterLineColor = videoFormatConversionWindow.CounterLineColor;
                        SelectedConversionType.VideoMixedFlow.VideoConversionOutputConfiguration.ShadowSize = Convert.ToInt32(videoFormatConversionWindow.SelectedShadowSize.SelectedValue);
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

        /// <summary>
        /// 修改输出的文件夹
        /// </summary>
        private void OnChangeOutputFolderClicked(object sender, RoutedEventArgs args)
        {
            if (sender is MenuFlyoutItem menuFlyoutItem && menuFlyoutItem.Tag is string tag)
            {
                switch (tag)
                {
                    case "AppCache":
                        {
                            Shell32Library.SHGetKnownFolderPath(new("F1B32785-6FBA-4FCF-9D55-7B8E7F157091"), KNOWN_FOLDER_FLAG.KF_FLAG_FORCE_APP_DATA_REDIRECTION, 0, out string localAppDataPath);
                            OutputFolder = localAppDataPath;
                            // TODO：未完成，添加一个文件夹保存到本地设置中
                            break;
                        }
                    case "Video":
                        {
                            string videoFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                            OutputFolder = videoFolder;
                            break;
                        }
                    case "Desktop":
                        {
                            OutputFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                            // TODO：未完成，添加一个文件夹保存到本地设置中
                            break;
                        }
                    case "Custom":
                        {
                            OpenFolderDialog openFolderDialog = new((nint)MainWindow.Current.AppWindow.Id.Value)
                            {
                                Description = SelectFolderString,
                                RootFolder = Environment.SpecialFolder.Desktop
                            };
                            DialogResult dialogResult = openFolderDialog.ShowDialog();
                            if (dialogResult is DialogResult.OK || dialogResult is DialogResult.Yes)
                            {
                                OutputFolder = openFolderDialog.SelectedPath;
                                // TODO：未完成，添加一个文件夹保存到本地设置中
                            }
                            openFolderDialog.Dispose();
                            break;
                        }
                }
            }
        }

        #endregion 第二部分：视频转换页面——挂载的事件

        /// <summary>
        /// 对数据进行排序
        /// </summary>
        private List<VideoConversionFileModel> SortData(List<VideoConversionFileModel> videoConversionFileList)
        {
            // 按照文件名称排序
            if (string.Equals(SelectedSortRule, SortRuleList[1]))
            {
                return SortWay ? [.. videoConversionFileList.OrderBy(item => item.FileName)] : [.. videoConversionFileList.OrderByDescending(item => item.FileName)];
            }
            // 按照文件大小排序
            else if (string.Equals(SelectedSortRule, SortRuleList[2]))
            {
                return SortWay ? [.. videoConversionFileList.OrderBy(item => item.FileSize)] : [.. videoConversionFileList.OrderByDescending(item => item.FileSize)];
            }
            // 按照视频持续时长排序
            else if (string.Equals(SelectedSortRule, SortRuleList[3]))
            {
                return SortWay ? [.. videoConversionFileList.OrderBy(item => item.Duration)] : [.. videoConversionFileList.OrderByDescending(item => item.Duration)];
            }
            else
            {
                return videoConversionFileList;
            }
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        private object GetFileInformation(string filePath, StreamKind? streamKind = null)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    // 视频格式转换
                    if (Equals(SelectedConversionType, ConversionTypeCollection[0]))
                    {
                        VideoConversionFileModel videoConversionFile = new()
                        {
                            FileName = Path.GetFileName(filePath),
                            FilePath = filePath,
                        };
                        FileInfo fileInfo = new(filePath);
                        videoConversionFile.FileSize = fileInfo.Length;
                        videoConversionFile.FileSizeString = VolumeSizeHelper.ConvertVolumeSizeToString(fileInfo.Length);

                        if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                        {
                            string videoDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Duration", InfoKind.Text, InfoKind.Name));
                            if (int.TryParse(videoDuration, out int videoDurationValue))
                            {
                                TimeSpan videoDurationTimeSpan = TimeSpan.FromMilliseconds(videoDurationValue);
                                videoConversionFile.Duration = videoDurationTimeSpan;
                                videoConversionFile.DurationString = string.Format(@"{0:00}:{1:00}:{2:00}", Math.Truncate(videoDurationTimeSpan.TotalHours), videoDurationTimeSpan.Minutes, videoDurationTimeSpan.Minutes);
                            }
                            else
                            {
                                videoConversionFile.Duration = TimeSpan.Zero;
                                videoConversionFile.DurationString = string.IsNullOrEmpty(videoDuration) ? "00:00:00" : videoDuration;
                            }

                            string width = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Width", InfoKind.Text, InfoKind.Name));
                            videoConversionFile.ScreenSizeWidth = string.IsNullOrEmpty(width) ? "0" : width;
                            string height = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Height", InfoKind.Text, InfoKind.Name));
                            videoConversionFile.ScreenSizeHeight = string.IsNullOrEmpty(height) ? "0" : height;
                            MediaInfoLibrary.MediaInfo_Close(handle);
                            MediaInfoLibrary.MediaInfo_Delete(handle);
                        }

                        videoConversionFile.VideoConversionOutputConfiguration = new()
                        {
                            VideoConversionTypeKind = SelectedConversionType.VideoConversionTypeKind,

                            FormatConversionType = ".mp4",
                            SizeLimitation = "Copy",
                            VideoEncoding = "None",
                            ScreenSize = "DefaultSize",
                            VideoBitRate = "Default",
                            CRF = -1,
                            GPU = "None",
                            FramePerSecond = "Default",
                            AspectRatio = "Default",
                            SecondaryEncoding = false,
                            KeyFrameInterval = "Default",
                            DeInterlace = false,
                            Rotation = System.Windows.Media.Imaging.Rotation.Rotate0,
                            MirrorReversal = false,
                            VideoFadeInEffect = "None",
                            VideoFadeOutEffect = "None",

                            AudioEncoding = "Copy",
                            SamplingRate = "Default",
                            AudioBitRate = "Default",
                            SoundTrack = "Default",
                            CloseSoundEffect = false,
                            Volume = "100%",
                            PreserveAllSourceInputAudioStream = false,
                            AudioFadeInEffect = "None",
                            AudioFadeOutEffect = "None",
                            Echo = false,
                            DeNoise = false,
                            Reverse = false,

                            PreserveAllSourceInputSubtitleStream = false,
                            AdditionalSubtitlePath = string.Empty,
                            SubtitleNestType = "Default",
                            FontName = SystemFonts.DefaultFont.Name,
                            FontSize = 1,
                            FontColor = accentColor,
                            FontBorderStyle = "BorderAndShadow",
                            CounterLineSize = 0,
                            CounterLineColor = accentColor,
                            ShadowSize = 0
                        };

                        return videoConversionFile;
                    }
                    // 视频合并
                    else if (Equals(SelectedConversionType, ConversionTypeCollection[1]))
                    {
                        VideoConversionFileModel videoConversionFile = new()
                        {
                            FileName = Path.GetFileName(filePath),
                            FilePath = filePath,
                        };
                        FileInfo fileInfo = new(filePath);
                        videoConversionFile.FileSize = fileInfo.Length;
                        videoConversionFile.FileSizeString = VolumeSizeHelper.ConvertVolumeSizeToString(fileInfo.Length);

                        if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                        {
                            string videoDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Duration", InfoKind.Text, InfoKind.Name));
                            if (int.TryParse(videoDuration, out int videoDurationValue))
                            {
                                TimeSpan videoDurationTimeSpan = TimeSpan.FromMilliseconds(videoDurationValue);
                                videoConversionFile.Duration = videoDurationTimeSpan;
                                videoConversionFile.DurationString = string.Format(@"{0:00}:{1:00}:{2:00}", Math.Truncate(videoDurationTimeSpan.TotalHours), videoDurationTimeSpan.Minutes, videoDurationTimeSpan.Minutes);
                            }
                            else
                            {
                                videoConversionFile.Duration = TimeSpan.Zero;
                                videoConversionFile.DurationString = string.IsNullOrEmpty(videoDuration) ? "00:00:00" : videoDuration;
                            }

                            string width = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Width", InfoKind.Text, InfoKind.Name));
                            videoConversionFile.ScreenSizeWidth = string.IsNullOrEmpty(width) ? "0" : width;
                            string height = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Height", InfoKind.Text, InfoKind.Name));
                            videoConversionFile.ScreenSizeHeight = string.IsNullOrEmpty(height) ? "0" : height;
                            MediaInfoLibrary.MediaInfo_Close(handle);
                            MediaInfoLibrary.MediaInfo_Delete(handle);
                        }

                        videoConversionFile.VideoConversionOutputConfiguration = new()
                        {
                            VideoConversionTypeKind = SelectedConversionType.VideoConversionTypeKind,

                            FormatConversionType = ".mp4",
                            SizeLimitation = "Copy",
                            VideoEncoding = "None",
                            ScreenSize = "DefaultSize",
                            VideoBitRate = "Default",
                            CRF = -1,
                            GPU = "None",
                            FramePerSecond = "Default",
                            AspectRatio = "Default",
                            SecondaryEncoding = false,
                            KeyFrameInterval = "Default",
                            DeInterlace = false,
                            Rotation = System.Windows.Media.Imaging.Rotation.Rotate0,
                            MirrorReversal = false,
                            VideoFadeInEffect = "None",
                            VideoFadeOutEffect = "None",

                            AudioEncoding = "Copy",
                            SamplingRate = "Default",
                            AudioBitRate = "Default",
                            SoundTrack = "Default",
                            CloseSoundEffect = false,
                            Volume = "100%",
                            PreserveAllSourceInputAudioStream = false,
                            AudioFadeInEffect = "None",
                            AudioFadeOutEffect = "None",
                            Echo = false,
                            DeNoise = false,
                            Reverse = false
                        };

                        return videoConversionFile;
                    }
                    // 视频混流
                    else if (Equals(SelectedConversionType, ConversionTypeCollection[2]))
                    {
                        VideoMixedFlowFileModel videoMixedFlowFile = new()
                        {
                            FileName = Path.GetFileName(filePath),
                            FilePath = filePath,
                        };
                        FileInfo fileInfo = new(filePath);
                        videoMixedFlowFile.FileSize = fileInfo.Length;
                        videoMixedFlowFile.FileSizeString = VolumeSizeHelper.ConvertVolumeSizeToString(fileInfo.Length);

                        if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                        {
                            if (streamKind is StreamKind.Video)
                            {
                                string videoDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, streamKind.Value, 0, "Duration", InfoKind.Text, InfoKind.Name));
                                if (int.TryParse(videoDuration, out int videoDurationValue))
                                {
                                    TimeSpan videoDurationTimeSpan = TimeSpan.FromMilliseconds(videoDurationValue);
                                    videoMixedFlowFile.Duration = videoDurationTimeSpan;
                                    videoMixedFlowFile.DurationString = string.Format(@"{0:00}:{1:00}:{2:00}", Math.Truncate(videoDurationTimeSpan.TotalHours), videoDurationTimeSpan.Minutes, videoDurationTimeSpan.Minutes);
                                }
                                else
                                {
                                    videoMixedFlowFile.Duration = TimeSpan.Zero;
                                    videoMixedFlowFile.DurationString = string.IsNullOrEmpty(videoDuration) ? "00:00:00" : videoDuration;
                                }

                                string width = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, streamKind.Value, 0, "Width", InfoKind.Text, InfoKind.Name));
                                videoMixedFlowFile.ScreenSizeWidth = string.IsNullOrEmpty(width) ? "0" : width;
                                string height = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, streamKind.Value, 0, "Height", InfoKind.Text, InfoKind.Name));
                                videoMixedFlowFile.ScreenSizeHeight = string.IsNullOrEmpty(height) ? "0" : height;
                            }

                            if (streamKind is StreamKind.Audio)
                            {
                                string videoDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, streamKind.Value, 0, "Duration", InfoKind.Text, InfoKind.Name));
                                if (int.TryParse(videoDuration, out int videoDurationValue))
                                {
                                    TimeSpan videoDurationTimeSpan = TimeSpan.FromMilliseconds(videoDurationValue);
                                    videoMixedFlowFile.Duration = videoDurationTimeSpan;
                                    videoMixedFlowFile.DurationString = string.Format(@"{0:00}:{1:00}:{2:00}", Math.Truncate(videoDurationTimeSpan.TotalHours), videoDurationTimeSpan.Minutes, videoDurationTimeSpan.Minutes);
                                }
                                else
                                {
                                    videoMixedFlowFile.Duration = TimeSpan.Zero;
                                    videoMixedFlowFile.DurationString = string.IsNullOrEmpty(videoDuration) ? "00:00:00" : videoDuration;
                                }

                                string channel = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, streamKind.Value, 0, "Channel(s)/String", InfoKind.Text, InfoKind.Name));
                                videoMixedFlowFile.Channel = string.IsNullOrEmpty(channel) ? "0" : channel;
                                string samplingRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, streamKind.Value, 0, "SamplingRate/String", InfoKind.Text, InfoKind.Name));
                                videoMixedFlowFile.SamplingRate = string.IsNullOrEmpty(samplingRate) ? "0" : samplingRate;
                                string bitRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, streamKind.Value, 0, "BitRate/String", InfoKind.Text, InfoKind.Name));
                                videoMixedFlowFile.BitRate = string.IsNullOrEmpty(bitRate) ? "0" : bitRate;
                            }

                            MediaInfoLibrary.MediaInfo_Close(handle);
                            MediaInfoLibrary.MediaInfo_Delete(handle);
                        }

                        return videoMixedFlowFile;
                    }
                    // 视频分离
                    else if (Equals(SelectedConversionType, ConversionTypeCollection[3]))
                    {
                        VideoConversionFileModel videoConversionFile = new()
                        {
                            FileName = Path.GetFileName(filePath),
                            FilePath = filePath,
                        };
                        FileInfo fileInfo = new(filePath);
                        videoConversionFile.FileSize = fileInfo.Length;
                        videoConversionFile.FileSizeString = VolumeSizeHelper.ConvertVolumeSizeToString(fileInfo.Length);

                        if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                        {
                            string videoDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Duration", InfoKind.Text, InfoKind.Name));
                            if (int.TryParse(videoDuration, out int videoDurationValue))
                            {
                                TimeSpan videoDurationTimeSpan = TimeSpan.FromMilliseconds(videoDurationValue);
                                videoConversionFile.Duration = videoDurationTimeSpan;
                                videoConversionFile.DurationString = string.Format(@"{0:00}:{1:00}:{2:00}", Math.Truncate(videoDurationTimeSpan.TotalHours), videoDurationTimeSpan.Minutes, videoDurationTimeSpan.Minutes);
                            }
                            else
                            {
                                videoConversionFile.Duration = TimeSpan.Zero;
                                videoConversionFile.DurationString = string.IsNullOrEmpty(videoDuration) ? "00:00:00" : videoDuration;
                            }

                            string width = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Width", InfoKind.Text, InfoKind.Name));
                            videoConversionFile.ScreenSizeWidth = string.IsNullOrEmpty(width) ? "0" : width;
                            string height = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Height", InfoKind.Text, InfoKind.Name));
                            videoConversionFile.ScreenSizeHeight = string.IsNullOrEmpty(height) ? "0" : height;
                            MediaInfoLibrary.MediaInfo_Close(handle);
                            MediaInfoLibrary.MediaInfo_Delete(handle);
                        }

                        videoConversionFile.VideoConversionOutputConfiguration = new()
                        {
                            VideoConversionTypeKind = VideoConversionTypeKind.VideoSeparation
                        };

                        return videoConversionFile;
                    }
                    // 视频导出图片
                    else if (Equals(SelectedConversionType, ConversionTypeCollection[4]))
                    {
                        VideoConversionFileModel videoConversionFile = new()
                        {
                            FileName = Path.GetFileName(filePath),
                            FilePath = filePath,
                        };
                        FileInfo fileInfo = new(filePath);
                        videoConversionFile.FileSize = fileInfo.Length;
                        videoConversionFile.FileSizeString = VolumeSizeHelper.ConvertVolumeSizeToString(fileInfo.Length);

                        if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                        {
                            string videoDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Duration", InfoKind.Text, InfoKind.Name));
                            if (int.TryParse(videoDuration, out int videoDurationValue))
                            {
                                TimeSpan videoDurationTimeSpan = TimeSpan.FromMilliseconds(videoDurationValue);
                                videoConversionFile.Duration = videoDurationTimeSpan;
                                videoConversionFile.DurationString = string.Format(@"{0:00}:{1:00}:{2:00}", Math.Truncate(videoDurationTimeSpan.TotalHours), videoDurationTimeSpan.Minutes, videoDurationTimeSpan.Minutes);
                            }
                            else
                            {
                                videoConversionFile.Duration = TimeSpan.Zero;
                                videoConversionFile.DurationString = string.IsNullOrEmpty(videoDuration) ? "00:00:00" : videoDuration;
                            }

                            string width = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Width", InfoKind.Text, InfoKind.Name));
                            videoConversionFile.ScreenSizeWidth = string.IsNullOrEmpty(width) ? "0" : width;
                            string height = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Height", InfoKind.Text, InfoKind.Name));
                            videoConversionFile.ScreenSizeHeight = string.IsNullOrEmpty(height) ? "0" : height;
                            MediaInfoLibrary.MediaInfo_Close(handle);
                            MediaInfoLibrary.MediaInfo_Delete(handle);
                        }

                        videoConversionFile.VideoConversionOutputConfiguration = new()
                        {
                            VideoConversionTypeKind = VideoConversionTypeKind.VideoSeparation
                        };

                        return videoConversionFile;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionPage), nameof(GetFileInformation), 1, e);
                return null;
            }
        }

        /// <summary>
        /// 获取文件缩略图
        /// </summary>
        private BitmapImage GetThumbnail(string filePath)
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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionPage), nameof(GetThumbnail), 1, e);
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
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoConversionPage), nameof(GetThumbnail), 2, e);
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

        private bool GetAllowDropVideoConversionFile(VideoConversionTypeKind videoConversionTypeKind)
        {
            return videoConversionTypeKind is not VideoConversionTypeKind.VideoMixedFlow;
        }
    }
}
