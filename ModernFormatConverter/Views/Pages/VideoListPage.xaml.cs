using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Helpers.Root;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Views.Windows;
using ModernFormatConverter.WindowsAPI.ComTypes;
using ModernFormatConverter.WindowsAPI.PInvoke.MediaInfo;
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
    /// 视频列表页面
    /// </summary>
    public sealed partial class VideoListPage : Page, INotifyPropertyChanged
    {
        private readonly string NoFolderString = ResourceService.VideoListResource.GetString("NoFolder");
        private readonly string NoMultiFileString = ResourceService.VideoListResource.GetString("NoMultiFileString");
        private readonly string SelectFileString = ResourceService.VideoListResource.GetString("SelectFile");
        private readonly string SelectFolderString = ResourceService.VideoListResource.GetString("SelectFolder");
        private readonly string VideoConcatString = ResourceService.VideoListResource.GetString("VideoConcat");
        private readonly string VideoConcatDragOverContentString = ResourceService.VideoListResource.GetString("VideoConcatDragOverContent");
        private readonly string VideoExportPictureString = ResourceService.VideoListResource.GetString("VideoExportPicture");
        private readonly string VideoExportPictureDragOverContentString = ResourceService.VideoListResource.GetString("VideoExportPictureDragOverContent");
        private readonly string VideoFormatConversionString = ResourceService.VideoListResource.GetString("VideoFormatConversion");
        private readonly string VideoFormatConversionDragOverContentString = ResourceService.VideoListResource.GetString("VideoFormatConversionDragOverContent");
        private readonly string VideoMixedFlowString = ResourceService.VideoListResource.GetString("VideoMixedFlow");
        private readonly string VideoMixedFlowAudioDragOverContentString = ResourceService.VideoListResource.GetString("VideoMixedFlowAudioDragOverContent");
        private readonly string VideoMixedFlowSubtitleDragOverContentString = ResourceService.VideoListResource.GetString("VideoMixedFlowSubtitleDragOverContent");
        private readonly string VideoMixedFlowVideoDragOverContentString = ResourceService.VideoListResource.GetString("VideoMixedFlowVideoDragOverContent");
        private readonly string VideoSeparationString = ResourceService.VideoListResource.GetString("VideoSeparation");
        private readonly string VideoSeparationDragOverContentString = ResourceService.VideoListResource.GetString("VideoSeparationDragOverContent");
        private readonly global::Windows.UI.Color accentColor = (global::Windows.UI.Color)Microsoft.UI.Xaml.Application.Current.Resources["SystemAccentColor"];
        private bool canScrollHorizontally;

        private bool _isPreviousEnabled;

        public bool IsPreviousEnabled
        {
            get { return _isPreviousEnabled; }

            set
            {
                if (!Equals(_isPreviousEnabled, value))
                {
                    _isPreviousEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPreviousEnabled)));
                }
            }
        }

        private bool _isNextEnabled;

        public bool IsNextEnabled
        {
            get { return _isNextEnabled; }

            set
            {
                if (!Equals(_isNextEnabled, value))
                {
                    _isNextEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNextEnabled)));
                }
            }
        }

        private SelectorBarItem _selectedItem;

        public SelectorBarItem SelectedItem
        {
            get { return _selectedItem; }

            set
            {
                if (!Equals(_selectedItem, value))
                {
                    _selectedItem = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectorBarItem)));
                }
            }
        }

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

        public List<string> SortRuleList { get; } = ["NotSort", "SortByFileName", "SortByFileSize", "SortByDuration"];

        public WinRTObservableCollection<VideoConversionTypeModel> VideoConversionTypeCollection { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public VideoListPage()
        {
            VideoConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoFormatConversionString,
                VideoConversionIcon = "\uE895",
                VideoConversionTypeKind = VideoConversionTypeKind.VideoFormatConversion,
                VideoFormatConversion = new()
            });
            VideoConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoConcatString,
                VideoConversionIcon = "\uEA3C",
                VideoConversionTypeKind = VideoConversionTypeKind.VideoConcat,
                VideoConcat = new()
                {
                    VideoConversionOutputConfiguration = new()
                    {
                        FormatConversionType = "MP4",
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
                        SpeedPlayback = 1.0,
                        ReverseVideo = false,
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
            VideoConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoMixedFlowString,
                VideoConversionIcon = "\uE8B1",
                VideoConversionTypeKind = VideoConversionTypeKind.VideoMixedFlow,
                VideoMixedFlow = new()
                {
                    VideoConversionOutputConfiguration = new()
                    {
                        FormatConversionType = "MP4",
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
                        SpeedPlayback = 1.0,
                        ReverseVideo = false,
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
            VideoConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoSeparationString,
                VideoConversionIcon = "\uE740",
                VideoConversionTypeKind = VideoConversionTypeKind.VideoSeparation,
                VideoSeparation = new()
            });
            VideoConversionTypeCollection.Add(new VideoConversionTypeModel
            {
                VideoConversionType = VideoExportPictureString,
                VideoConversionIcon = "\uE91B",
                VideoConversionTypeKind = VideoConversionTypeKind.VideoExportPicture,
                VideoExportPicture = new()
            });
            InitializeComponent();
            SelectedItem = VideoListSelectorBar.Items[0];
            SelectedConversionType = VideoConversionTypeCollection[0];
            SelectedSortRule = SortRuleList[0];
            SortWay = true;
        }

        #region 第一部分：ExecuteCommand 命令调用时挂载的事件

        /// <summary>
        /// 移除选中项
        /// </summary>
        private void OnRemoveExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            // 视频格式转换
            if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoFormatConversion)
            {
                SelectedConversionType.VideoFormatConversion.VideoFormatConversionFileCollection.Remove(args.Parameter as VideoFormatConversionFileModel);
            }
            // 视频合并
            else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoConcat)
            {
                SelectedConversionType.VideoConcat.VideoConcatFileCollection.Remove(args.Parameter as VideoConcatFileModel);
            }
            // 视频分离
            else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoSeparation)
            {
                SelectedConversionType.VideoSeparation.VideoSeparationFileCollection.Remove(args.Parameter as VideoSeparationFileModel);
            }
            // 视频导出图片
            else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoExportPicture)
            {
                SelectedConversionType.VideoExportPicture.VideoExportPictureFileCollection.Remove(args.Parameter as VideoExportPictureFileModel);
            }
        }

        /// <summary>
        /// 配置选中项转换参数
        /// </summary>
        private async void OnOutputConfigurationExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (MainWindow.Current.GetFrameContent() is VideoConversionPage videoConversionPage)
            {
                // 视频格式转换
                if (args.Parameter is VideoFormatConversionFileModel videoFormatConversionFile && videoFormatConversionFile.VideoConversionOutputConfiguration is not null)
                {
                    videoConversionPage.NavigateTo(videoConversionPage.PageList[2], new VideoConversionNavigationParameter()
                    {
                        VideoConversionTypeKind = VideoConversionTypeKind.VideoFormatConversion,
                        IsGlobalSettings = false,
                        VideoConversionData = videoFormatConversionFile
                    }, true);
                }
                // 视频导出图片
                else if (args.Parameter is VideoExportPictureFileModel videoExportPictureFile && videoExportPictureFile.VideoExportPictureOutputConfiguration is not null)
                {
                    videoConversionPage.NavigateTo(videoConversionPage.PageList[3], new VideoConversionNavigationParameter()
                    {
                        VideoConversionTypeKind = VideoConversionTypeKind.VideoExportPicture,
                        IsGlobalSettings = false,
                        VideoConversionData = videoExportPictureFile
                    }, true);
                }
            }
        }

        /// <summary>
        /// 视频编辑
        /// </summary>
        private async void OnVideoEditExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (MainWindow.Current.GetFrameContent() is VideoConversionPage videoConversionPage && args.Parameter is VideoFormatConversionFileModel videoFormatConversionFile && videoFormatConversionFile.VideoEdit is not null)
            {
                videoConversionPage.NavigateTo(videoConversionPage.PageList[1], videoFormatConversionFile, true);
            }
        }

        #endregion 第一部分：ExecuteCommand 命令调用时挂载的事件

        #region 第二部分：视频转换页面——挂载的事件

        /// <summary>
        /// 设置拖动的数据的可视表示形式
        /// </summary>
        private async void OnVideoListDragEnter(object sender, Microsoft.UI.Xaml.DragEventArgs args)
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
                    if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoFormatConversion)
                    {
                        args.AcceptedOperation = DataPackageOperation.Copy;
                        args.DragUIOverride.IsCaptionVisible = true;
                        args.DragUIOverride.IsContentVisible = false;
                        args.DragUIOverride.IsGlyphVisible = true;
                        args.DragUIOverride.Caption = VideoFormatConversionDragOverContentString;
                    }
                    else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoConcat)
                    {
                        args.AcceptedOperation = DataPackageOperation.Copy;
                        args.DragUIOverride.IsCaptionVisible = true;
                        args.DragUIOverride.IsContentVisible = false;
                        args.DragUIOverride.IsGlyphVisible = true;
                        args.DragUIOverride.Caption = VideoConcatDragOverContentString;
                    }
                    else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoSeparation)
                    {
                        args.AcceptedOperation = DataPackageOperation.Copy;
                        args.DragUIOverride.IsCaptionVisible = true;
                        args.DragUIOverride.IsContentVisible = false;
                        args.DragUIOverride.IsGlyphVisible = true;
                        args.DragUIOverride.Caption = VideoSeparationDragOverContentString;
                    }
                    else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoExportPicture)
                    {
                        args.AcceptedOperation = DataPackageOperation.Copy;
                        args.DragUIOverride.IsCaptionVisible = true;
                        args.DragUIOverride.IsContentVisible = false;
                        args.DragUIOverride.IsGlyphVisible = true;
                        args.DragUIOverride.Caption = VideoExportPictureDragOverContentString;
                    }
                    else
                    {
                        args.AcceptedOperation = DataPackageOperation.Copy;
                        args.DragUIOverride.IsCaptionVisible = false;
                        args.DragUIOverride.IsContentVisible = false;
                        args.DragUIOverride.IsGlyphVisible = true;
                        args.DragUIOverride.Caption = string.Empty;
                    }
                }
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoListPage), nameof(OnVideoListDragEnter), 1, e);
            }
            finally
            {
                args.Handled = true;
                dragOperationDeferral.Complete();
            }
        }

        /// <summary>
        /// 设置拖动的视频混流数据视频部分的可视表示形式
        /// </summary>
        private async void OnVideoMixedFlowVideoDragEnter(object sender, Microsoft.UI.Xaml.DragEventArgs args)
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
                        args.DragUIOverride.Caption = VideoMixedFlowVideoDragOverContentString;
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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoListPage), nameof(OnVideoMixedFlowVideoDragEnter), 1, e);
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
        private async void OnVideoListDrop(object sender, Microsoft.UI.Xaml.DragEventArgs args)
        {
            IsGettingFileInformation = true;
            DragOperationDeferral dragOperationDeferral = args.GetDeferral();
            List<string> fileList = null;

            try
            {
                DataPackageView dataPackageView = args.DataView;
                fileList = await Task.Run(async () =>
                {
                    try
                    {
                        if (dataPackageView.Contains(StandardDataFormats.StorageItems))
                        {
                            IReadOnlyList<IStorageItem> storeageItem = await dataPackageView.GetStorageItemsAsync();
                            return storeageItem.Select(item => item.Path).ToList();
                        }
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoListPage), nameof(OnVideoListDrop), 1, e);
                    }

                    return null;
                });
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoListPage), nameof(OnVideoListDrop), 2, e);
            }
            finally
            {
                dragOperationDeferral.Complete();
            }

            if (fileList is not null && fileList.Count > 0)
            {
                await AddVideoDataAsync(fileList);
            }
            IsGettingFileInformation = false;
        }

        /// <summary>
        /// 拖动文件完成后获取视频混流——视频部分文件信息
        /// </summary>
        private async void OnVideoMixedFlowVideoDrop(object sender, Microsoft.UI.Xaml.DragEventArgs args)
        {
            IsGettingFileInformation = true;
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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoListPage), nameof(OnVideoMixedFlowVideoDrop), 2, e);
            }
            finally
            {
                dragOperationDeferral.Complete();
            }

            if (File.Exists(filePath))
            {
                if (await Task.Run(() => { return GetFileInformation(filePath, StreamKind.Video); }) is VideoMixedFlowFileModel videoMixedFlowFile)
                {
                    SelectedConversionType.VideoMixedFlow.VideoFile = videoMixedFlowFile;
                    SelectedConversionType.VideoMixedFlow.IsVideoFileExisted = true;
                }
                if (Equals(SelectedConversionType.VideoConversionTypeKind, VideoConversionTypeKind.VideoMixedFlow) && SelectedConversionType.VideoMixedFlow.VideoFile.FileThumbnailSource is null)
                {
                    SelectedConversionType.VideoMixedFlow.VideoFile.FileThumbnailSource = GetThumbnail(filePath);
                }
            }
            IsGettingFileInformation = false;
        }

        /// <summary>
        /// 鼠标进入后触发的事件
        /// </summary>
        private void OnSelectorBarPointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            if (canScrollHorizontally)
            {
                if (VideoListScrollViewer.HorizontalOffset <= 0)
                {
                    IsPreviousEnabled = false;
                    IsNextEnabled = true;
                }
                else if (VideoListScrollViewer.HorizontalOffset >= VideoListScrollViewer.ScrollableWidth)
                {
                    IsPreviousEnabled = true;
                    IsNextEnabled = false;
                }
                else
                {
                    IsPreviousEnabled = true;
                    IsNextEnabled = true;
                }
            }
        }

        /// <summary>
        /// 鼠标退出后触发的事件
        /// </summary>
        private void OnSelectorBarPointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            IsPreviousEnabled = false;
            IsNextEnabled = false;
        }

        /// <summary>
        /// 大小发生变化后触发的事件
        /// </summary>
        private void OnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            canScrollHorizontally = VideoListScrollViewer.ExtentWidth > VideoListScrollViewer.ViewportWidth;
            IsPreviousEnabled = false;
            IsNextEnabled = false;
        }

        /// <summary>
        /// 当滚动和缩放等操作导致视图更改时发生的事件
        /// </summary>
        private void OnViewChanged(object sender, ScrollViewerViewChangedEventArgs args)
        {
            if (canScrollHorizontally)
            {
                if (VideoListScrollViewer.HorizontalOffset <= 0)
                {
                    IsPreviousEnabled = false;
                    IsNextEnabled = true;
                }
                else if (VideoListScrollViewer.HorizontalOffset >= VideoListScrollViewer.ScrollableWidth)
                {
                    IsPreviousEnabled = true;
                    IsNextEnabled = false;
                }
                else
                {
                    IsPreviousEnabled = true;
                    IsNextEnabled = true;
                }
            }
        }

        /// <summary>
        /// 向前移动
        /// </summary>
        private void OnPreviousClick(object sender, RoutedEventArgs args)
        {
            VideoListScrollViewer.ChangeView(VideoListScrollViewer.HorizontalOffset < 150 ? 0 : VideoListScrollViewer.HorizontalOffset - 150, null, null);
        }

        /// <summary>
        /// 向后移动
        /// </summary>
        private void OnNextClick(object sender, RoutedEventArgs args)
        {
            VideoListScrollViewer.ChangeView(VideoListScrollViewer.HorizontalOffset >= VideoListScrollViewer.ScrollableWidth - 150 ? VideoListScrollViewer.ScrollableWidth : VideoListScrollViewer.HorizontalOffset + 150, null, null);
        }

        /// <summary>
        /// 视频转换选择器栏选中项发生变化时触发的事件
        /// </summary>
        private async void OnSelectorBarSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            SelectedItem = sender.SelectedItem;
            SelectedConversionType = VideoConversionTypeCollection[sender.Items.IndexOf(SelectedItem)];
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
        /// 设置拖动的视频混流数据音频部分的可视表示形式
        /// </summary>
        private async void OnVideoMixedFlowAudioDragEnter(object sender, Microsoft.UI.Xaml.DragEventArgs args)
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
                        args.DragUIOverride.Caption = VideoMixedFlowAudioDragOverContentString;
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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoListPage), nameof(OnVideoMixedFlowAudioDragEnter), 1, e);
            }
            finally
            {
                args.Handled = true;
                dragOperationDeferral.Complete();
            }
        }

        /// <summary>
        /// 拖动文件完成后获取视频混流——音频部分文件信息
        /// </summary>
        private async void OnVideoMixedFlowAudioDrop(object sender, Microsoft.UI.Xaml.DragEventArgs args)
        {
            IsGettingFileInformation = true;
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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoListPage), nameof(OnVideoMixedFlowAudioDrop), 2, e);
            }
            finally
            {
                dragOperationDeferral.Complete();
            }

            if (File.Exists(filePath))
            {
                if (await Task.Run(() => { return GetFileInformation(filePath, StreamKind.Audio); }) is VideoMixedFlowFileModel videoMixedFlowFile)
                {
                    SelectedConversionType.VideoMixedFlow.AudioFile = videoMixedFlowFile;
                    SelectedConversionType.VideoMixedFlow.IsAudioFileExisted = true;
                }
                if (Equals(SelectedConversionType.VideoConversionTypeKind, VideoConversionTypeKind.VideoMixedFlow) && SelectedConversionType.VideoMixedFlow.AudioFile.FileThumbnailSource is null)
                {
                    SelectedConversionType.VideoMixedFlow.AudioFile.FileThumbnailSource = GetThumbnail(filePath);
                }
            }
            IsGettingFileInformation = false;
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
        /// 设置拖动的视频混流数据字幕部分的可视表示形式
        /// </summary>
        private async void OnVideoMixedFlowSubtitleDragEnter(object sender, Microsoft.UI.Xaml.DragEventArgs args)
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
                        args.DragUIOverride.Caption = VideoMixedFlowSubtitleDragOverContentString;
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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoListPage), nameof(OnVideoMixedFlowVideoDragEnter), 1, e);
            }
            finally
            {
                args.Handled = true;
                dragOperationDeferral.Complete();
            }
        }

        /// <summary>
        /// 拖动文件完成后获取视频混流——字幕部分文件信息
        /// </summary>
        private async void OnVideoMixedFlowSubtitleDrop(object sender, Microsoft.UI.Xaml.DragEventArgs args)
        {
            IsGettingFileInformation = true;
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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoListPage), nameof(OnVideoMixedFlowSubtitleDrop), 2, e);
            }
            finally
            {
                dragOperationDeferral.Complete();
            }

            if (File.Exists(filePath))
            {
                if (await Task.Run(() => { return GetFileInformation(filePath); }) is VideoMixedFlowFileModel videoMixedFlowFile)
                {
                    SelectedConversionType.VideoMixedFlow.SubtitleFile = videoMixedFlowFile;
                    SelectedConversionType.VideoMixedFlow.IsSubtitleFileExisted = true;
                }
                if (Equals(SelectedConversionType.VideoConversionTypeKind, VideoConversionTypeKind.VideoMixedFlow) && SelectedConversionType.VideoMixedFlow.SubtitleFile.FileThumbnailSource is null)
                {
                    SelectedConversionType.VideoMixedFlow.SubtitleFile.FileThumbnailSource = GetThumbnail(filePath);
                }
            }
            IsGettingFileInformation = false;
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
        /// 选择排序规则
        /// </summary>
        private async void OnSortRuleClicked(object sender, RoutedEventArgs args)
        {
            if (sender is RadioMenuFlyoutItem radioMenuFlyoutItem && radioMenuFlyoutItem.Tag is not null)
            {
                SelectedSortRule = Convert.ToString(radioMenuFlyoutItem.Tag);
                IsGettingFileInformation = true;
                await SortDataAsync();
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
                await SortDataAsync();
                IsGettingFileInformation = false;
            }
        }

        /// <summary>
        /// 清空
        /// </summary>
        private void OnClearClicked(object sender, RoutedEventArgs args)
        {
            // 视频格式转换
            if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoFormatConversion)
            {
                SelectedConversionType.VideoFormatConversion.VideoFormatConversionFileCollection.Clear();
            }
            // 视频合并
            else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoConcat)
            {
                SelectedConversionType.VideoConcat.VideoConcatFileCollection.Clear();
            }
            // 视频混流
            else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoMixedFlow)
            {
                SelectedConversionType.VideoMixedFlow.IsVideoFileExisted = false;
                SelectedConversionType.VideoMixedFlow.VideoFile = null;
                SelectedConversionType.VideoMixedFlow.IsAudioFileExisted = false;
                SelectedConversionType.VideoMixedFlow.AudioFile = null;
                SelectedConversionType.VideoMixedFlow.IsSubtitleFileExisted = false;
                SelectedConversionType.VideoMixedFlow.SubtitleFile = null;
            }
            // 视频分离
            else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoSeparation)
            {
                SelectedConversionType.VideoSeparation.VideoSeparationFileCollection.Clear();
            }
            // 视频导出图片
            else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoExportPicture)
            {
                SelectedConversionType.VideoExportPicture.VideoExportPictureFileCollection.Clear();
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
                await AddVideoDataAsync([.. openFileDialog.FileNames]);
            }
            IsGettingFileInformation = false;
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
                List<string> fileList = [.. Directory.GetFiles(openFolderDialog.SelectedPath)];
                await AddVideoDataAsync(fileList);
                IsGettingFileInformation = false;
            }
            openFolderDialog.Dispose();
        }

        /// <summary>
        /// 打开输出配置
        /// </summary>
        private async void OnOutputConfigurationClicked(object sender, RoutedEventArgs args)
        {
            if (MainWindow.Current.GetFrameContent() is VideoConversionPage videoConversionPage)
            {
                // 视频格式转换
                if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoFormatConversion)
                {
                    videoConversionPage.NavigateTo(videoConversionPage.PageList[2], new VideoConversionNavigationParameter()
                    {
                        VideoConversionTypeKind = VideoConversionTypeKind.VideoFormatConversion,
                        IsGlobalSettings = true,
                        VideoConversionData = VideoConversionTypeCollection[0].VideoFormatConversion.VideoFormatConversionFileCollection.ToList()
                    }, true);
                }
                // 视频合并
                else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoConcat)
                {
                    videoConversionPage.NavigateTo(videoConversionPage.PageList[2], new VideoConversionNavigationParameter()
                    {
                        VideoConversionTypeKind = VideoConversionTypeKind.VideoConcat,
                        IsGlobalSettings = true,
                        VideoConversionData = VideoConversionTypeCollection[1].VideoConcat
                    }, true);
                }
                // 视频混流
                else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoMixedFlow)
                {
                    videoConversionPage.NavigateTo(videoConversionPage.PageList[2], new VideoConversionNavigationParameter()
                    {
                        VideoConversionTypeKind = VideoConversionTypeKind.VideoMixedFlow,
                        IsGlobalSettings = true,
                        VideoConversionData = VideoConversionTypeCollection[2].VideoMixedFlow
                    }, true);
                }
                // 视频导出图片
                else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoExportPicture)
                {
                    videoConversionPage.NavigateTo(videoConversionPage.PageList[3], new VideoConversionNavigationParameter()
                    {
                        VideoConversionTypeKind = VideoConversionTypeKind.VideoExportPicture,
                        IsGlobalSettings = true,
                        VideoConversionData = VideoConversionTypeCollection[4].VideoExportPicture.VideoExportPictureFileCollection.ToList()
                    }, true);
                }
            }
        }

        /// <summary>
        /// 确定
        /// </summary>
        private void OnOkClicked(object sender, RoutedEventArgs args)
        {
            // TODO：未完成
        }

        #endregion 第二部分：视频转换页面——挂载的事件

        /// <summary>
        /// 添加视频数据
        /// </summary>
        private async Task AddVideoDataAsync(List<string> fileList)
        {
            // 视频格式转换
            if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoFormatConversion)
            {
                List<VideoFormatConversionFileModel> videoFormatConversionFileList = [.. SelectedConversionType.VideoFormatConversion.VideoFormatConversionFileCollection];
                List<Task> taskList = [];
                object fileInformationLock = new();
                foreach (string file in fileList)
                {
                    taskList.Add(Task.Run(async () =>
                    {
                        if (GetFileInformation(file) is VideoFormatConversionFileModel videoFormatConversionFile)
                        {
                            lock (fileInformationLock)
                            {
                                videoFormatConversionFileList.Add(videoFormatConversionFile);
                            }
                        }
                    }));
                }
                await Task.WhenAll(taskList);
                List<VideoFormatConversionFileModel> sortedVideoFormatConversionFileList = SortVideoFormatConversionFileData(videoFormatConversionFileList);
                SelectedConversionType.VideoFormatConversion.VideoFormatConversionFileCollection.Clear();
                foreach (VideoFormatConversionFileModel sortedVideoFormatConversionFile in sortedVideoFormatConversionFileList)
                {
                    sortedVideoFormatConversionFile.FileThumbnailSource ??= GetThumbnail(sortedVideoFormatConversionFile.FilePath);
                    SelectedConversionType.VideoFormatConversion.VideoFormatConversionFileCollection.Add(sortedVideoFormatConversionFile);
                }
            }
            // 视频合并
            else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoConcat)
            {
                List<VideoConcatFileModel> videoConcatFileList = [.. SelectedConversionType.VideoConcat.VideoConcatFileCollection];
                List<Task> taskList = [];
                object fileInformationLock = new();
                foreach (string file in fileList)
                {
                    taskList.Add(Task.Run(async () =>
                    {
                        if (GetFileInformation(file) is VideoConcatFileModel videoConcatFile)
                        {
                            lock (fileInformationLock)
                            {
                                videoConcatFileList.Add(videoConcatFile);
                            }
                        }
                    }));
                }
                await Task.WhenAll(taskList);
                List<VideoConcatFileModel> sortedVideoConcatFileList = SortVideoConcatFileData(videoConcatFileList);
                SelectedConversionType.VideoConcat.VideoConcatFileCollection.Clear();
                foreach (VideoConcatFileModel sortedVideoConcatFile in sortedVideoConcatFileList)
                {
                    SelectedConversionType.VideoConcat.VideoConcatFileCollection.Add(sortedVideoConcatFile);
                }
            }
            // 视频分离
            else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoSeparation)
            {
                List<VideoSeparationFileModel> videoSeparartionFileList = [.. SelectedConversionType.VideoSeparation.VideoSeparationFileCollection];
                List<Task> taskList = [];
                object fileInformationLock = new();
                foreach (string file in fileList)
                {
                    taskList.Add(Task.Run(async () =>
                    {
                        if (GetFileInformation(file) is VideoSeparationFileModel videoSeparartionFile)
                        {
                            lock (fileInformationLock)
                            {
                                videoSeparartionFileList.Add(videoSeparartionFile);
                            }
                        }
                    }));
                }
                await Task.WhenAll(taskList);
                List<VideoSeparationFileModel> sortedVideoSeparationFileList = SortVideoSeparationFileData(videoSeparartionFileList);
                SelectedConversionType.VideoSeparation.VideoSeparationFileCollection.Clear();
                foreach (VideoSeparationFileModel sortedVideoSeparationFile in sortedVideoSeparationFileList)
                {
                    sortedVideoSeparationFile.FileThumbnailSource ??= GetThumbnail(sortedVideoSeparationFile.FilePath);
                    SelectedConversionType.VideoSeparation.VideoSeparationFileCollection.Add(sortedVideoSeparationFile);
                }
            }
            // 视频导出图片
            else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoExportPicture)
            {
                List<VideoExportPictureFileModel> videoSeparartionFileList = [.. SelectedConversionType.VideoExportPicture.VideoExportPictureFileCollection];
                List<Task> taskList = [];
                object fileInformationLock = new();
                foreach (string file in fileList)
                {
                    taskList.Add(Task.Run(async () =>
                    {
                        if (GetFileInformation(file) is VideoExportPictureFileModel videoSeparartionFile)
                        {
                            lock (fileInformationLock)
                            {
                                videoSeparartionFileList.Add(videoSeparartionFile);
                            }
                        }
                    }));
                }
                await Task.WhenAll(taskList);
                List<VideoExportPictureFileModel> sortedVideoExportPictureFileList = SortVideoExportPictureFileData(videoSeparartionFileList);
                SelectedConversionType.VideoExportPicture.VideoExportPictureFileCollection.Clear();
                foreach (VideoExportPictureFileModel sortedVideoExportPictureFile in sortedVideoExportPictureFileList)
                {
                    sortedVideoExportPictureFile.FileThumbnailSource ??= GetThumbnail(sortedVideoExportPictureFile.FilePath);
                    SelectedConversionType.VideoExportPicture.VideoExportPictureFileCollection.Add(sortedVideoExportPictureFile);
                }
            }
        }

        /// <summary>
        /// 对数据进行排序
        /// </summary>
        private async Task SortDataAsync()
        {
            // 视频格式转换
            if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoFormatConversion)
            {
                List<VideoFormatConversionFileModel> sortedVideoFormatConversionFileList = await Task.Run(() =>
                {
                    List<VideoFormatConversionFileModel> sortedVideoFormatConversionFileList = [.. SelectedConversionType.VideoFormatConversion.VideoFormatConversionFileCollection];
                    return SortVideoFormatConversionFileData(sortedVideoFormatConversionFileList);
                });
                SelectedConversionType.VideoFormatConversion.VideoFormatConversionFileCollection.Clear();
                foreach (VideoFormatConversionFileModel sortedVideoFormatConversionFile in sortedVideoFormatConversionFileList)
                {
                    SelectedConversionType.VideoFormatConversion.VideoFormatConversionFileCollection.Add(sortedVideoFormatConversionFile);
                }
            }
            // 视频合并
            else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoConcat)
            {
                List<VideoConcatFileModel> sortedVideoConcatFileList = await Task.Run(() =>
                {
                    List<VideoConcatFileModel> sortedVideoConcatFileList = [.. SelectedConversionType.VideoConcat.VideoConcatFileCollection];
                    return SortVideoConcatFileData(sortedVideoConcatFileList);
                });
                SelectedConversionType.VideoConcat.VideoConcatFileCollection.Clear();
                foreach (VideoConcatFileModel sortedVideoConcatFile in sortedVideoConcatFileList)
                {
                    SelectedConversionType.VideoConcat.VideoConcatFileCollection.Add(sortedVideoConcatFile);
                }
            }
            // 视频分离
            else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoSeparation)
            {
                List<VideoSeparationFileModel> sortedVideoSeparationFileList = await Task.Run(() =>
                {
                    List<VideoSeparationFileModel> sortedVideoSeparationFileList = [.. SelectedConversionType.VideoSeparation.VideoSeparationFileCollection];
                    return SortVideoSeparationFileData(sortedVideoSeparationFileList);
                });
                SelectedConversionType.VideoSeparation.VideoSeparationFileCollection.Clear();
                foreach (VideoSeparationFileModel sortedVideoSeparationFile in sortedVideoSeparationFileList)
                {
                    SelectedConversionType.VideoSeparation.VideoSeparationFileCollection.Add(sortedVideoSeparationFile);
                }
            }
            // 视频导出图片
            else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoExportPicture)
            {
                List<VideoExportPictureFileModel> sortedVideoExportPictureFileList = await Task.Run(() =>
                {
                    List<VideoExportPictureFileModel> sortedVideoExportPictureFileList = [.. SelectedConversionType.VideoExportPicture.VideoExportPictureFileCollection];
                    return SortVideoExportPictureFileData(sortedVideoExportPictureFileList);
                });
                SelectedConversionType.VideoExportPicture.VideoExportPictureFileCollection.Clear();
                foreach (VideoExportPictureFileModel sortedVideoExportPictureFile in sortedVideoExportPictureFileList)
                {
                    SelectedConversionType.VideoExportPicture.VideoExportPictureFileCollection.Add(sortedVideoExportPictureFile);
                }
            }
        }

        /// <summary>
        /// 对视频转换文件数据进行排序
        /// </summary>
        private List<VideoFormatConversionFileModel> SortVideoFormatConversionFileData(List<VideoFormatConversionFileModel> videoFormatConversionFileList)
        {
            // 按照文件名称排序
            if (string.Equals(SelectedSortRule, SortRuleList[1]))
            {
                return SortWay ? [.. videoFormatConversionFileList.OrderBy(item => item.FileName)] : [.. videoFormatConversionFileList.OrderByDescending(item => item.FileName)];
            }
            // 按照文件大小排序
            else if (string.Equals(SelectedSortRule, SortRuleList[2]))
            {
                return SortWay ? [.. videoFormatConversionFileList.OrderBy(item => item.FileSize)] : [.. videoFormatConversionFileList.OrderByDescending(item => item.FileSize)];
            }
            // 按照视频持续时长排序
            else if (string.Equals(SelectedSortRule, SortRuleList[3]))
            {
                return SortWay ? [.. videoFormatConversionFileList.OrderBy(item => item.Duration)] : [.. videoFormatConversionFileList.OrderByDescending(item => item.Duration)];
            }
            else
            {
                return videoFormatConversionFileList;
            }
        }

        /// <summary>
        /// 对视频合并文件数据进行排序
        /// </summary>
        private List<VideoConcatFileModel> SortVideoConcatFileData(List<VideoConcatFileModel> videoConcatFileList)
        {
            // 按照文件名称排序
            if (string.Equals(SelectedSortRule, SortRuleList[1]))
            {
                return SortWay ? [.. videoConcatFileList.OrderBy(item => item.FileName)] : [.. videoConcatFileList.OrderByDescending(item => item.FileName)];
            }
            // 按照文件大小排序
            else if (string.Equals(SelectedSortRule, SortRuleList[2]))
            {
                return SortWay ? [.. videoConcatFileList.OrderBy(item => item.FileSize)] : [.. videoConcatFileList.OrderByDescending(item => item.FileSize)];
            }
            // 按照视频持续时长排序
            else if (string.Equals(SelectedSortRule, SortRuleList[3]))
            {
                return SortWay ? [.. videoConcatFileList.OrderBy(item => item.Duration)] : [.. videoConcatFileList.OrderByDescending(item => item.Duration)];
            }
            else
            {
                return videoConcatFileList;
            }
        }

        /// <summary>
        /// 对视频分离文件数据进行排序
        /// </summary>
        private List<VideoSeparationFileModel> SortVideoSeparationFileData(List<VideoSeparationFileModel> videoSeparationFileList)
        {
            // 按照文件名称排序
            if (string.Equals(SelectedSortRule, SortRuleList[1]))
            {
                return SortWay ? [.. videoSeparationFileList.OrderBy(item => item.FileName)] : [.. videoSeparationFileList.OrderByDescending(item => item.FileName)];
            }
            // 按照文件大小排序
            else if (string.Equals(SelectedSortRule, SortRuleList[2]))
            {
                return SortWay ? [.. videoSeparationFileList.OrderBy(item => item.FileSize)] : [.. videoSeparationFileList.OrderByDescending(item => item.FileSize)];
            }
            // 按照视频持续时长排序
            else if (string.Equals(SelectedSortRule, SortRuleList[3]))
            {
                return SortWay ? [.. videoSeparationFileList.OrderBy(item => item.Duration)] : [.. videoSeparationFileList.OrderByDescending(item => item.Duration)];
            }
            else
            {
                return videoSeparationFileList;
            }
        }

        /// <summary>
        /// 对视频导出图片文件数据进行排序
        /// </summary>
        private List<VideoExportPictureFileModel> SortVideoExportPictureFileData(List<VideoExportPictureFileModel> videoExportPictureFileList)
        {
            // 按照文件名称排序
            if (string.Equals(SelectedSortRule, SortRuleList[1]))
            {
                return SortWay ? [.. videoExportPictureFileList.OrderBy(item => item.FileName)] : [.. videoExportPictureFileList.OrderByDescending(item => item.FileName)];
            }
            // 按照文件大小排序
            else if (string.Equals(SelectedSortRule, SortRuleList[2]))
            {
                return SortWay ? [.. videoExportPictureFileList.OrderBy(item => item.FileSize)] : [.. videoExportPictureFileList.OrderByDescending(item => item.FileSize)];
            }
            // 按照视频持续时长排序
            else if (string.Equals(SelectedSortRule, SortRuleList[3]))
            {
                return SortWay ? [.. videoExportPictureFileList.OrderBy(item => item.Duration)] : [.. videoExportPictureFileList.OrderByDescending(item => item.Duration)];
            }
            else
            {
                return videoExportPictureFileList;
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
                    if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoFormatConversion)
                    {
                        VideoFormatConversionFileModel videoFormatConversionFile = new()
                        {
                            FileName = Path.GetFileName(filePath),
                            FilePath = filePath,
                        };
                        FileInfo fileInfo = new(filePath);
                        videoFormatConversionFile.FileSize = fileInfo.Length;

                        if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                        {
                            string videoDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Duration", InfoKind.Text, InfoKind.Name));
                            if (double.TryParse(videoDuration, out double videoDurationValue))
                            {
                                TimeSpan videoDurationTimeSpan = TimeSpan.FromMilliseconds(videoDurationValue);
                                videoFormatConversionFile.Duration = videoDurationTimeSpan;
                            }
                            else
                            {
                                videoFormatConversionFile.Duration = TimeSpan.Zero;
                            }

                            string width = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Width", InfoKind.Text, InfoKind.Name));
                            videoFormatConversionFile.ScreenSizeWidth = string.IsNullOrEmpty(width) ? "0" : width;
                            string height = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Height", InfoKind.Text, InfoKind.Name));
                            videoFormatConversionFile.ScreenSizeHeight = string.IsNullOrEmpty(height) ? "0" : height;
                            MediaInfoLibrary.MediaInfo_Close(handle);
                            MediaInfoLibrary.MediaInfo_Delete(handle);
                        }

                        videoFormatConversionFile.VideoConversionOutputConfiguration = new()
                        {
                            FormatConversionType = "MP4",
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
                            SpeedPlayback = 1.0,
                            ReverseVideo = false,
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

                        videoFormatConversionFile.VideoEdit = new()
                        {
                            StartTime = TimeSpan.Zero,
                            EndTime = TimeSpan.Zero,
                            SelectRegionOperation = "Close",
                            VideoCoverFilePath = string.Empty
                        };

                        return videoFormatConversionFile;
                    }
                    // 视频合并
                    else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoConcat)
                    {
                        VideoConcatFileModel videoConcatFile = new()
                        {
                            FileName = Path.GetFileName(filePath),
                            FilePath = filePath,
                        };
                        FileInfo fileInfo = new(filePath);
                        videoConcatFile.FileSize = fileInfo.Length;

                        if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                        {
                            string videoDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Duration", InfoKind.Text, InfoKind.Name));
                            if (double.TryParse(videoDuration, out double videoDurationValue))
                            {
                                TimeSpan videoDurationTimeSpan = TimeSpan.FromMilliseconds(videoDurationValue);
                                videoConcatFile.Duration = videoDurationTimeSpan;
                            }
                            else
                            {
                                videoConcatFile.Duration = TimeSpan.Zero;
                            }

                            string width = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Width", InfoKind.Text, InfoKind.Name));
                            videoConcatFile.ScreenSizeWidth = string.IsNullOrEmpty(width) ? "0" : width;
                            string height = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Height", InfoKind.Text, InfoKind.Name));
                            videoConcatFile.ScreenSizeHeight = string.IsNullOrEmpty(height) ? "0" : height;
                            MediaInfoLibrary.MediaInfo_Close(handle);
                            MediaInfoLibrary.MediaInfo_Delete(handle);
                        }

                        return videoConcatFile;
                    }
                    // 视频混流
                    else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoMixedFlow)
                    {
                        VideoMixedFlowFileModel videoMixedFlowFile = new()
                        {
                            FileName = Path.GetFileName(filePath),
                            FilePath = filePath,
                        };
                        FileInfo fileInfo = new(filePath);
                        videoMixedFlowFile.FileSize = VolumeSizeHelper.ConvertVolumeSizeToString(fileInfo.Length);

                        if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                        {
                            if (streamKind is StreamKind.Video)
                            {
                                string videoDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, streamKind.Value, 0, "Duration", InfoKind.Text, InfoKind.Name));
                                if (double.TryParse(videoDuration, out double videoDurationValue))
                                {
                                    TimeSpan videoDurationTimeSpan = TimeSpan.FromMilliseconds(videoDurationValue);
                                    videoMixedFlowFile.Duration = string.Format(@"{0:00}:{1:00}:{2:00}", Math.Truncate(videoDurationTimeSpan.TotalHours), videoDurationTimeSpan.Minutes, videoDurationTimeSpan.Minutes);
                                }
                                else
                                {
                                    videoMixedFlowFile.Duration = string.IsNullOrEmpty(videoDuration) ? "00:00:00" : videoDuration;
                                }

                                string width = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, streamKind.Value, 0, "Width", InfoKind.Text, InfoKind.Name));
                                videoMixedFlowFile.ScreenSizeWidth = string.IsNullOrEmpty(width) ? "0" : width;
                                string height = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, streamKind.Value, 0, "Height", InfoKind.Text, InfoKind.Name));
                                videoMixedFlowFile.ScreenSizeHeight = string.IsNullOrEmpty(height) ? "0" : height;
                            }

                            if (streamKind is StreamKind.Audio)
                            {
                                string videoDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, streamKind.Value, 0, "Duration", InfoKind.Text, InfoKind.Name));
                                if (double.TryParse(videoDuration, out double videoDurationValue))
                                {
                                    TimeSpan videoDurationTimeSpan = TimeSpan.FromMilliseconds(videoDurationValue);
                                    videoMixedFlowFile.Duration = string.Format(@"{0:00}:{1:00}:{2:00}", Math.Truncate(videoDurationTimeSpan.TotalHours), videoDurationTimeSpan.Minutes, videoDurationTimeSpan.Minutes);
                                }
                                else
                                {
                                    videoMixedFlowFile.Duration = string.IsNullOrEmpty(videoDuration) ? "00:00:00" : videoDuration;
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
                    else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoSeparation)
                    {
                        VideoSeparationFileModel videoSeparationFile = new()
                        {
                            FileName = Path.GetFileName(filePath),
                            FilePath = filePath,
                        };
                        FileInfo fileInfo = new(filePath);
                        videoSeparationFile.FileSize = fileInfo.Length;

                        if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                        {
                            string videoDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Duration", InfoKind.Text, InfoKind.Name));
                            if (double.TryParse(videoDuration, out double videoDurationValue))
                            {
                                TimeSpan videoDurationTimeSpan = TimeSpan.FromMilliseconds(videoDurationValue);
                                videoSeparationFile.Duration = videoDurationTimeSpan;
                            }
                            else
                            {
                                videoSeparationFile.Duration = TimeSpan.Zero;
                            }

                            string width = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Width", InfoKind.Text, InfoKind.Name));
                            videoSeparationFile.ScreenSizeWidth = string.IsNullOrEmpty(width) ? "0" : width;
                            string height = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Height", InfoKind.Text, InfoKind.Name));
                            videoSeparationFile.ScreenSizeHeight = string.IsNullOrEmpty(height) ? "0" : height;
                            MediaInfoLibrary.MediaInfo_Close(handle);
                            MediaInfoLibrary.MediaInfo_Delete(handle);
                        }

                        return videoSeparationFile;
                    }
                    // 视频导出图片
                    else if (SelectedConversionType.VideoConversionTypeKind is VideoConversionTypeKind.VideoExportPicture)
                    {
                        VideoExportPictureFileModel videoExportPictureFile = new()
                        {
                            FileName = Path.GetFileName(filePath),
                            FilePath = filePath,
                        };
                        FileInfo fileInfo = new(filePath);
                        videoExportPictureFile.FileSize = fileInfo.Length;

                        if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                        {
                            string videoDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Duration", InfoKind.Text, InfoKind.Name));
                            if (double.TryParse(videoDuration, out double videoDurationValue))
                            {
                                TimeSpan videoDurationTimeSpan = TimeSpan.FromMilliseconds(videoDurationValue);
                                videoExportPictureFile.Duration = videoDurationTimeSpan;
                            }
                            else
                            {
                                videoExportPictureFile.Duration = TimeSpan.Zero;
                            }

                            string width = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Width", InfoKind.Text, InfoKind.Name));
                            videoExportPictureFile.ScreenSizeWidth = string.IsNullOrEmpty(width) ? "0" : width;
                            string height = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Height", InfoKind.Text, InfoKind.Name));
                            videoExportPictureFile.ScreenSizeHeight = string.IsNullOrEmpty(height) ? "0" : height;
                            MediaInfoLibrary.MediaInfo_Close(handle);
                            MediaInfoLibrary.MediaInfo_Delete(handle);
                        }

                        videoExportPictureFile.VideoExportPictureOutputConfiguration = new()
                        {
                            VideoConversionTypeKind = VideoConversionTypeKind.VideoExportPicture,
                            SavePictureFormat = ".png",
                            VideoExportPictureKind = "TimePoint",
                            ExportTime = TimeSpan.Zero,
                            StartTime = TimeSpan.Zero,
                            EndTime = TimeSpan.Zero,
                            PictureExportPerSecond = 1
                        };

                        return videoExportPictureFile;
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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoListPage), nameof(GetFileInformation), 1, e);
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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoListPage), nameof(GetThumbnail), 1, e);
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
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(VideoListPage), nameof(GetThumbnail), 2, e);
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
