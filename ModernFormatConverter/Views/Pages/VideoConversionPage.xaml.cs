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
        private readonly string NoMultiFileString = ResourceService.VideoConversionConfigurationResource.GetString("NoMultiFileString");
        private readonly string SelectFileString = ResourceService.VideoConversionResource.GetString("SelectFile");
        private readonly string SelectFolderString = ResourceService.VideoConversionResource.GetString("SelectFolder");
        private readonly string VideoAngleAdjustmentString = ResourceService.VideoConversionResource.GetString("VideoAngleAdjustment");
        private readonly string VideoConcatString = ResourceService.VideoConversionResource.GetString("VideoConcat");
        private readonly string VideoExportFrameString = ResourceService.VideoConversionResource.GetString("VideoExportFrame");
        private readonly string VideoFormatConversionString = ResourceService.VideoConversionResource.GetString("VideoFormatConversion");
        private readonly string VideoMixedFlowString = ResourceService.VideoConversionResource.GetString("VideoMixedFlow");
        private readonly string VideoMixedFlowDragOverContentString = ResourceService.VideoConversionResource.GetString("VideoMixedFlowDragOverContent");
        private readonly string VideoSeparationString = ResourceService.VideoConversionResource.GetString("VideoSeparation");
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
                    VideoConversionConfiguration = new()
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
                        FontColor = System.Windows.SystemParameters.WindowGlassColor.ToString(),
                        FontBorderStyle = "BorderAndShadow",
                        CounterLineSize = 0,
                        CounterLineColor = System.Windows.SystemParameters.WindowGlassColor.ToString(),
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
            if (args.Parameter is VideoConversionFileModel videoConversionFile && videoConversionFile.VideoConversionConfiguration is not null)
            {
                VideoConversionConfigurationWindow videoConversionConfigurationWindow = new(SelectedConversionType.VideoConversionTypeKind, ConversionToolsWindow.Current, videoConversionFile.VideoConversionConfiguration);
                if (await videoConversionConfigurationWindow.ShowAsync() is ContentDialogResult.Primary)
                {
                    if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoFormatConversion)
                    {
                        videoConversionFile.VideoConversionConfiguration.FormatConversionType = Convert.ToString(videoConversionConfigurationWindow.SelectedFormatConversionType.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.SizeLimitation = Convert.ToString(videoConversionConfigurationWindow.SelectedSizeLimitation.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.VideoEncoding = Convert.ToString(videoConversionConfigurationWindow.SelectedVideoEncoding.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.ScreenSize = Convert.ToString(videoConversionConfigurationWindow.SelectedScreenSize.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.VideoBitRate = Convert.ToString(videoConversionConfigurationWindow.SelectedVideoBitRate.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.CRF = videoConversionConfigurationWindow.UseCRF ? videoConversionConfigurationWindow.CRF : -1;
                        videoConversionFile.VideoConversionConfiguration.GPU = Convert.ToString(videoConversionConfigurationWindow.SelectedGPU.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.FramePerSecond = Convert.ToString(videoConversionConfigurationWindow.SelectedFramePerSecond.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.AspectRatio = Convert.ToString(videoConversionConfigurationWindow.SelectedAspectRatio.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.SecondaryEncoding = videoConversionConfigurationWindow.SecondaryEncoding;
                        videoConversionFile.VideoConversionConfiguration.KeyFrameInterval = Convert.ToString(videoConversionConfigurationWindow.SelectedKeyFrameInterval.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.DeInterlace = videoConversionConfigurationWindow.DeInterlace;
                        videoConversionFile.VideoConversionConfiguration.Rotation = (System.Windows.Media.Imaging.Rotation)videoConversionConfigurationWindow.SelectedRotation.SelectedValue;
                        videoConversionFile.VideoConversionConfiguration.MirrorReversal = videoConversionConfigurationWindow.MirrorReversal;
                        videoConversionFile.VideoConversionConfiguration.VideoFadeInEffect = Convert.ToString(videoConversionConfigurationWindow.SelectedVideoFadeInEffect);
                        videoConversionFile.VideoConversionConfiguration.VideoFadeOutEffect = Convert.ToString(videoConversionConfigurationWindow.SelectedVideoFadeOutEffect);

                        videoConversionFile.VideoConversionConfiguration.AudioEncoding = Convert.ToString(videoConversionConfigurationWindow.SelectedAudioEncoding.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.SamplingRate = Convert.ToString(videoConversionConfigurationWindow.SelectedSamplingRate.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.AudioBitRate = Convert.ToString(videoConversionConfigurationWindow.SelectedAudioBitRate.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.SoundTrack = Convert.ToString(videoConversionConfigurationWindow.SelectedSoundTrack.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.CloseSoundEffect = videoConversionConfigurationWindow.CloseSoundEffect;
                        videoConversionFile.VideoConversionConfiguration.Volume = Convert.ToString(videoConversionConfigurationWindow.SelectedVolume.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.PreserveAllSourceInputAudioStream = videoConversionConfigurationWindow.PreserveAllSourceInputAudioStream;
                        videoConversionFile.VideoConversionConfiguration.AudioFadeInEffect = Convert.ToString(videoConversionConfigurationWindow.SelectedAudioFadeInEffect.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.AudioFadeOutEffect = Convert.ToString(videoConversionConfigurationWindow.SelectedAudioFadeOutEffect.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.Echo = videoConversionConfigurationWindow.Echo;
                        videoConversionFile.VideoConversionConfiguration.DeNoise = videoConversionConfigurationWindow.DeNoise;
                        videoConversionFile.VideoConversionConfiguration.Reverse = videoConversionConfigurationWindow.Reverse;

                        videoConversionFile.VideoConversionConfiguration.PreserveAllSourceInputSubtitleStream = videoConversionConfigurationWindow.PreserveAllSourceInputSubtitleStream;
                        videoConversionFile.VideoConversionConfiguration.AdditionalSubtitlePath = videoConversionConfigurationWindow.AdditionalSubtitlePath;
                        videoConversionFile.VideoConversionConfiguration.SubtitleNestType = Convert.ToString(videoConversionConfigurationWindow.SelectedSubtitleNestType.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.FontName = videoConversionConfigurationWindow.FontName;
                        videoConversionFile.VideoConversionConfiguration.FontSize = Convert.ToInt32(videoConversionConfigurationWindow.SelectedFontSize.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.FontColor = videoConversionConfigurationWindow.FontColor;
                        videoConversionFile.VideoConversionConfiguration.FontBorderStyle = Convert.ToString(videoConversionConfigurationWindow.SelectedFontBorderStyle.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.CounterLineSize = Convert.ToInt32(videoConversionConfigurationWindow.SelectedCounterLineSize.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.CounterLineColor = videoConversionConfigurationWindow.CounterLineColor;
                        videoConversionFile.VideoConversionConfiguration.ShadowSize = Convert.ToInt32(videoConversionConfigurationWindow.SelectedShadowSize.SelectedValue);
                    }
                    else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoConcat)
                    {
                        videoConversionFile.VideoConversionConfiguration.FormatConversionType = Convert.ToString(videoConversionConfigurationWindow.SelectedFormatConversionType.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.SizeLimitation = Convert.ToString(videoConversionConfigurationWindow.SelectedSizeLimitation.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.VideoEncoding = Convert.ToString(videoConversionConfigurationWindow.SelectedVideoEncoding.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.ScreenSize = Convert.ToString(videoConversionConfigurationWindow.SelectedScreenSize.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.VideoBitRate = Convert.ToString(videoConversionConfigurationWindow.SelectedVideoBitRate.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.CRF = videoConversionConfigurationWindow.UseCRF ? videoConversionConfigurationWindow.CRF : -1;
                        videoConversionFile.VideoConversionConfiguration.GPU = Convert.ToString(videoConversionConfigurationWindow.SelectedGPU.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.FramePerSecond = Convert.ToString(videoConversionConfigurationWindow.SelectedFramePerSecond.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.AspectRatio = Convert.ToString(videoConversionConfigurationWindow.SelectedAspectRatio.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.SecondaryEncoding = videoConversionConfigurationWindow.SecondaryEncoding;
                        videoConversionFile.VideoConversionConfiguration.KeyFrameInterval = Convert.ToString(videoConversionConfigurationWindow.SelectedKeyFrameInterval.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.DeInterlace = videoConversionConfigurationWindow.DeInterlace;
                        videoConversionFile.VideoConversionConfiguration.Rotation = (System.Windows.Media.Imaging.Rotation)videoConversionConfigurationWindow.SelectedRotation.SelectedValue;
                        videoConversionFile.VideoConversionConfiguration.MirrorReversal = videoConversionConfigurationWindow.MirrorReversal;
                        videoConversionFile.VideoConversionConfiguration.VideoFadeInEffect = Convert.ToString(videoConversionConfigurationWindow.SelectedVideoFadeInEffect);
                        videoConversionFile.VideoConversionConfiguration.VideoFadeOutEffect = Convert.ToString(videoConversionConfigurationWindow.SelectedVideoFadeOutEffect);

                        videoConversionFile.VideoConversionConfiguration.AudioEncoding = Convert.ToString(videoConversionConfigurationWindow.SelectedAudioEncoding.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.SamplingRate = Convert.ToString(videoConversionConfigurationWindow.SelectedSamplingRate.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.AudioBitRate = Convert.ToString(videoConversionConfigurationWindow.SelectedAudioBitRate.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.SoundTrack = Convert.ToString(videoConversionConfigurationWindow.SelectedSoundTrack.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.CloseSoundEffect = videoConversionConfigurationWindow.CloseSoundEffect;
                        videoConversionFile.VideoConversionConfiguration.Volume = Convert.ToString(videoConversionConfigurationWindow.SelectedVolume.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.PreserveAllSourceInputAudioStream = videoConversionConfigurationWindow.PreserveAllSourceInputAudioStream;
                        videoConversionFile.VideoConversionConfiguration.AudioFadeInEffect = Convert.ToString(videoConversionConfigurationWindow.SelectedAudioFadeInEffect.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.AudioFadeOutEffect = Convert.ToString(videoConversionConfigurationWindow.SelectedAudioFadeOutEffect.SelectedValue);
                        videoConversionFile.VideoConversionConfiguration.Echo = videoConversionConfigurationWindow.Echo;
                        videoConversionFile.VideoConversionConfiguration.DeNoise = videoConversionConfigurationWindow.DeNoise;
                        videoConversionFile.VideoConversionConfiguration.Reverse = videoConversionConfigurationWindow.Reverse;
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
            if (Equals(SelectedConversionType, ConversionTypeCollection[0]))
            {
                VideoConversionConfigurationWindow videoFormatConversionWindow = new(SelectedConversionType.VideoConversionTypeKind, ConversionToolsWindow.Current);
                if (await videoFormatConversionWindow.ShowAsync() is ContentDialogResult.Primary && SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoFormatConversion)
                {
                    foreach (VideoConversionFileModel videoConversionFile in SelectedConversionType.VideoConversionFileCollection)
                    {
                        if (videoConversionFile.VideoConversionConfiguration is not null)
                        {
                            videoConversionFile.VideoConversionConfiguration.FormatConversionType = Convert.ToString(videoFormatConversionWindow.SelectedFormatConversionType.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.SizeLimitation = Convert.ToString(videoFormatConversionWindow.SelectedSizeLimitation.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.VideoEncoding = Convert.ToString(videoFormatConversionWindow.SelectedVideoEncoding.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.ScreenSize = Convert.ToString(videoFormatConversionWindow.SelectedScreenSize.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.VideoBitRate = Convert.ToString(videoFormatConversionWindow.SelectedVideoBitRate.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.CRF = videoFormatConversionWindow.UseCRF ? videoFormatConversionWindow.CRF : -1;
                            videoConversionFile.VideoConversionConfiguration.GPU = Convert.ToString(videoFormatConversionWindow.SelectedGPU.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.FramePerSecond = Convert.ToString(videoFormatConversionWindow.SelectedFramePerSecond.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.AspectRatio = Convert.ToString(videoFormatConversionWindow.SelectedAspectRatio.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.SecondaryEncoding = videoFormatConversionWindow.SecondaryEncoding;
                            videoConversionFile.VideoConversionConfiguration.KeyFrameInterval = Convert.ToString(videoFormatConversionWindow.SelectedKeyFrameInterval.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.DeInterlace = videoFormatConversionWindow.DeInterlace;
                            videoConversionFile.VideoConversionConfiguration.Rotation = (System.Windows.Media.Imaging.Rotation)videoFormatConversionWindow.SelectedRotation.SelectedValue;
                            videoConversionFile.VideoConversionConfiguration.MirrorReversal = videoFormatConversionWindow.MirrorReversal;
                            videoConversionFile.VideoConversionConfiguration.VideoFadeInEffect = Convert.ToString(videoFormatConversionWindow.SelectedVideoFadeInEffect);
                            videoConversionFile.VideoConversionConfiguration.VideoFadeOutEffect = Convert.ToString(videoFormatConversionWindow.SelectedVideoFadeOutEffect);

                            videoConversionFile.VideoConversionConfiguration.AudioEncoding = Convert.ToString(videoFormatConversionWindow.SelectedAudioEncoding.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.SamplingRate = Convert.ToString(videoFormatConversionWindow.SelectedSamplingRate.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.AudioBitRate = Convert.ToString(videoFormatConversionWindow.SelectedAudioBitRate.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.SoundTrack = Convert.ToString(videoFormatConversionWindow.SelectedSoundTrack.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.CloseSoundEffect = videoFormatConversionWindow.CloseSoundEffect;
                            videoConversionFile.VideoConversionConfiguration.Volume = Convert.ToString(videoFormatConversionWindow.SelectedVolume.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.PreserveAllSourceInputAudioStream = videoFormatConversionWindow.PreserveAllSourceInputAudioStream;
                            videoConversionFile.VideoConversionConfiguration.AudioFadeInEffect = Convert.ToString(videoFormatConversionWindow.SelectedAudioFadeInEffect.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.AudioFadeOutEffect = Convert.ToString(videoFormatConversionWindow.SelectedAudioFadeOutEffect.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.Echo = videoFormatConversionWindow.Echo;
                            videoConversionFile.VideoConversionConfiguration.DeNoise = videoFormatConversionWindow.DeNoise;
                            videoConversionFile.VideoConversionConfiguration.Reverse = videoFormatConversionWindow.Reverse;

                            videoConversionFile.VideoConversionConfiguration.PreserveAllSourceInputSubtitleStream = videoFormatConversionWindow.PreserveAllSourceInputSubtitleStream;
                            videoConversionFile.VideoConversionConfiguration.AdditionalSubtitlePath = videoFormatConversionWindow.AdditionalSubtitlePath;
                            videoConversionFile.VideoConversionConfiguration.SubtitleNestType = Convert.ToString(videoFormatConversionWindow.SelectedSubtitleNestType.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.FontName = videoFormatConversionWindow.FontName;
                            videoConversionFile.VideoConversionConfiguration.FontSize = Convert.ToInt32(videoFormatConversionWindow.SelectedFontSize.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.FontColor = videoFormatConversionWindow.FontColor;
                            videoConversionFile.VideoConversionConfiguration.FontBorderStyle = Convert.ToString(videoFormatConversionWindow.SelectedFontBorderStyle.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.CounterLineSize = Convert.ToInt32(videoFormatConversionWindow.SelectedCounterLineSize.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.CounterLineColor = videoFormatConversionWindow.CounterLineColor;
                            videoConversionFile.VideoConversionConfiguration.ShadowSize = Convert.ToInt32(videoFormatConversionWindow.SelectedShadowSize.SelectedValue);
                        }
                    }
                }
            }
            else if (Equals(SelectedConversionType, ConversionTypeCollection[1]))
            {
                VideoConversionConfigurationWindow videoFormatConversionWindow = new(SelectedConversionType.VideoConversionTypeKind, ConversionToolsWindow.Current);
                if (await videoFormatConversionWindow.ShowAsync() is ContentDialogResult.Primary && SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoFormatConversion)
                {
                    foreach (VideoConversionFileModel videoConversionFile in SelectedConversionType.VideoConversionFileCollection)
                    {
                        if (videoConversionFile.VideoConversionConfiguration is not null)
                        {
                            videoConversionFile.VideoConversionConfiguration.FormatConversionType = Convert.ToString(videoFormatConversionWindow.SelectedFormatConversionType.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.SizeLimitation = Convert.ToString(videoFormatConversionWindow.SelectedSizeLimitation.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.VideoEncoding = Convert.ToString(videoFormatConversionWindow.SelectedVideoEncoding.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.ScreenSize = Convert.ToString(videoFormatConversionWindow.SelectedScreenSize.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.VideoBitRate = Convert.ToString(videoFormatConversionWindow.SelectedVideoBitRate.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.CRF = videoFormatConversionWindow.UseCRF ? videoFormatConversionWindow.CRF : -1;
                            videoConversionFile.VideoConversionConfiguration.GPU = Convert.ToString(videoFormatConversionWindow.SelectedGPU.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.FramePerSecond = Convert.ToString(videoFormatConversionWindow.SelectedFramePerSecond.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.AspectRatio = Convert.ToString(videoFormatConversionWindow.SelectedAspectRatio.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.SecondaryEncoding = videoFormatConversionWindow.SecondaryEncoding;
                            videoConversionFile.VideoConversionConfiguration.KeyFrameInterval = Convert.ToString(videoFormatConversionWindow.SelectedKeyFrameInterval.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.DeInterlace = videoFormatConversionWindow.DeInterlace;
                            videoConversionFile.VideoConversionConfiguration.Rotation = (System.Windows.Media.Imaging.Rotation)videoFormatConversionWindow.SelectedRotation.SelectedValue;
                            videoConversionFile.VideoConversionConfiguration.MirrorReversal = videoFormatConversionWindow.MirrorReversal;
                            videoConversionFile.VideoConversionConfiguration.VideoFadeInEffect = Convert.ToString(videoFormatConversionWindow.SelectedVideoFadeInEffect);
                            videoConversionFile.VideoConversionConfiguration.VideoFadeOutEffect = Convert.ToString(videoFormatConversionWindow.SelectedVideoFadeOutEffect);

                            videoConversionFile.VideoConversionConfiguration.AudioEncoding = Convert.ToString(videoFormatConversionWindow.SelectedAudioEncoding.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.SamplingRate = Convert.ToString(videoFormatConversionWindow.SelectedSamplingRate.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.AudioBitRate = Convert.ToString(videoFormatConversionWindow.SelectedAudioBitRate.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.SoundTrack = Convert.ToString(videoFormatConversionWindow.SelectedSoundTrack.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.CloseSoundEffect = videoFormatConversionWindow.CloseSoundEffect;
                            videoConversionFile.VideoConversionConfiguration.Volume = Convert.ToString(videoFormatConversionWindow.SelectedVolume.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.PreserveAllSourceInputAudioStream = videoFormatConversionWindow.PreserveAllSourceInputAudioStream;
                            videoConversionFile.VideoConversionConfiguration.AudioFadeInEffect = Convert.ToString(videoFormatConversionWindow.SelectedAudioFadeInEffect.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.AudioFadeOutEffect = Convert.ToString(videoFormatConversionWindow.SelectedAudioFadeOutEffect.SelectedValue);
                            videoConversionFile.VideoConversionConfiguration.Echo = videoFormatConversionWindow.Echo;
                            videoConversionFile.VideoConversionConfiguration.DeNoise = videoFormatConversionWindow.DeNoise;
                            videoConversionFile.VideoConversionConfiguration.Reverse = videoFormatConversionWindow.Reverse;
                        }
                    }
                }
            }
            else if (Equals(SelectedConversionType, ConversionTypeCollection[2]))
            {
                VideoConversionConfigurationWindow videoFormatConversionWindow = new(SelectedConversionType.VideoConversionTypeKind, ConversionToolsWindow.Current);
                if (await videoFormatConversionWindow.ShowAsync() is ContentDialogResult.Primary && SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoFormatConversion)
                {
                    if (SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration is not null)
                    {
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.FormatConversionType = Convert.ToString(videoFormatConversionWindow.SelectedFormatConversionType.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.SizeLimitation = Convert.ToString(videoFormatConversionWindow.SelectedSizeLimitation.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.VideoEncoding = Convert.ToString(videoFormatConversionWindow.SelectedVideoEncoding.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.ScreenSize = Convert.ToString(videoFormatConversionWindow.SelectedScreenSize.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.VideoBitRate = Convert.ToString(videoFormatConversionWindow.SelectedVideoBitRate.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.CRF = videoFormatConversionWindow.UseCRF ? videoFormatConversionWindow.CRF : -1;
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.GPU = Convert.ToString(videoFormatConversionWindow.SelectedGPU.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.FramePerSecond = Convert.ToString(videoFormatConversionWindow.SelectedFramePerSecond.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.AspectRatio = Convert.ToString(videoFormatConversionWindow.SelectedAspectRatio.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.SecondaryEncoding = videoFormatConversionWindow.SecondaryEncoding;
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.KeyFrameInterval = Convert.ToString(videoFormatConversionWindow.SelectedKeyFrameInterval.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.DeInterlace = videoFormatConversionWindow.DeInterlace;
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.Rotation = (System.Windows.Media.Imaging.Rotation)videoFormatConversionWindow.SelectedRotation.SelectedValue;
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.MirrorReversal = videoFormatConversionWindow.MirrorReversal;
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.VideoFadeInEffect = Convert.ToString(videoFormatConversionWindow.SelectedVideoFadeInEffect);
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.VideoFadeOutEffect = Convert.ToString(videoFormatConversionWindow.SelectedVideoFadeOutEffect);

                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.AudioEncoding = Convert.ToString(videoFormatConversionWindow.SelectedAudioEncoding.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.SamplingRate = Convert.ToString(videoFormatConversionWindow.SelectedSamplingRate.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.AudioBitRate = Convert.ToString(videoFormatConversionWindow.SelectedAudioBitRate.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.SoundTrack = Convert.ToString(videoFormatConversionWindow.SelectedSoundTrack.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.CloseSoundEffect = videoFormatConversionWindow.CloseSoundEffect;
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.Volume = Convert.ToString(videoFormatConversionWindow.SelectedVolume.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.PreserveAllSourceInputAudioStream = videoFormatConversionWindow.PreserveAllSourceInputAudioStream;
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.AudioFadeInEffect = Convert.ToString(videoFormatConversionWindow.SelectedAudioFadeInEffect.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.AudioFadeOutEffect = Convert.ToString(videoFormatConversionWindow.SelectedAudioFadeOutEffect.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.Echo = videoFormatConversionWindow.Echo;
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.DeNoise = videoFormatConversionWindow.DeNoise;
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.Reverse = videoFormatConversionWindow.Reverse;

                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.PreserveAllSourceInputSubtitleStream = videoFormatConversionWindow.PreserveAllSourceInputSubtitleStream;
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.AdditionalSubtitlePath = videoFormatConversionWindow.AdditionalSubtitlePath;
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.SubtitleNestType = Convert.ToString(videoFormatConversionWindow.SelectedSubtitleNestType.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.FontName = videoFormatConversionWindow.FontName;
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.FontSize = Convert.ToInt32(videoFormatConversionWindow.SelectedFontSize.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.FontColor = videoFormatConversionWindow.FontColor;
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.FontBorderStyle = Convert.ToString(videoFormatConversionWindow.SelectedFontBorderStyle.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.CounterLineSize = Convert.ToInt32(videoFormatConversionWindow.SelectedCounterLineSize.SelectedValue);
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.CounterLineColor = videoFormatConversionWindow.CounterLineColor;
                        SelectedConversionType.VideoMixedFlow.VideoConversionConfiguration.ShadowSize = Convert.ToInt32(videoFormatConversionWindow.SelectedShadowSize.SelectedValue);
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
                    case "Download":
                        {
                            Shell32Library.SHGetKnownFolderPath(new("374DE290-123F-4565-9164-39C4925E467B"), KNOWN_FOLDER_FLAG.KF_FLAG_DEFAULT, 0, out string downloadFolder);
                            OutputFolder = downloadFolder;
                            // TODO：未完成，添加一个文件夹保存到本地设置中
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
            if (string.Equals(SelectedSortRule, SortRuleList[1]))
            {
                return SortWay ? [.. videoConversionFileList.OrderBy(item => item.FileName)] : [.. videoConversionFileList.OrderByDescending(item => item.FileName)];
            }
            else if (string.Equals(SelectedSortRule, SortRuleList[2]))
            {
                return SortWay ? [.. videoConversionFileList.OrderBy(item => item.FileSize)] : [.. videoConversionFileList.OrderByDescending(item => item.FileSize)];
            }
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

                        videoConversionFile.VideoConversionConfiguration = new()
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
                            FontColor = System.Windows.SystemParameters.WindowGlassColor.ToString(),
                            FontBorderStyle = "BorderAndShadow",
                            CounterLineSize = 0,
                            CounterLineColor = System.Windows.SystemParameters.WindowGlassColor.ToString(),
                            ShadowSize = 0
                        };

                        return videoConversionFile;
                    }
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

                        videoConversionFile.VideoConversionConfiguration = new()
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

        private Visibility GetVideoConversionType(VideoConversionTypeKind selectedVideoConversionTypeKind, VideoConversionTypeKind comparedVideoConversionTypeKind, bool needReverse)
        {
            return needReverse ? Equals(selectedVideoConversionTypeKind, comparedVideoConversionTypeKind) ? Visibility.Collapsed : Visibility.Visible : Equals(selectedVideoConversionTypeKind, comparedVideoConversionTypeKind) ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool GetAllowDropVideoConversionFile(VideoConversionTypeKind videoConversionTypeKind)
        {
            return videoConversionTypeKind is not VideoConversionTypeKind.VideoMixedFlow;
        }
    }
}
