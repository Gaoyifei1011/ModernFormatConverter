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
        private readonly string SelectFileString = ResourceService.VideoConversionResource.GetString("SelectFile");
        private readonly string SelectFolderString = ResourceService.VideoConversionResource.GetString("SelectFolder");
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
            SelectedSortRule = SortRuleList[0];
            SortWay = true;
            Shell32Library.SHGetKnownFolderPath(new("F1B32785-6FBA-4FCF-9D55-7B8E7F157091"), KNOWN_FOLDER_FLAG.KF_FLAG_FORCE_APP_DATA_REDIRECTION, 0, out string outputFolder);
            OutputFolder = outputFolder;
            // TODO：未完成，添加一个读取本地文件夹保存设置
        }

        #region 第一部分：重写父类事件

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
        /// 拖动文件完成后获取文件信息
        /// </summary>
        private async void OnVideoConversionDrop(object sender, Microsoft.UI.Xaml.DragEventArgs args)
        {
            base.OnDrop(args);
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
                List<VideoConversionFileModel> sortedVideoConversionFileList = await Task.Run(() =>
                {
                    List<VideoConversionFileModel> videoConversionFileList = [.. SelectedConversionType.VideoConversionFileCollection];
                    foreach (IStorageItem file in fileList)
                    {
                        if (GetFileInformation(file.Path) is VideoConversionFileModel videoConversionFile)
                        {
                            videoConversionFileList.Add(videoConversionFile);
                        }
                    }
                    return SortData(videoConversionFileList);
                });
                SelectedConversionType.VideoConversionFileCollection.Clear();
                foreach (VideoConversionFileModel sortedVideoConversionFile in sortedVideoConversionFileList)
                {
                    sortedVideoConversionFile.FileThumbnailSource ??= GetThumbnail(sortedVideoConversionFile.FilePath);
                    SelectedConversionType.VideoConversionFileCollection.Add(sortedVideoConversionFile);
                }
                IsGettingFileInformation = false;
            }
        }

        #endregion 第一部分：重写父类事件

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
        /// 清空列表
        /// </summary>
        private void OnClearListClicked(object sender, RoutedEventArgs args)
        {
            SelectedConversionType.VideoConversionFileCollection.Clear();
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
                if (await Task.Run(() => GetFileInformation(openFileDialog.FileName)) is VideoConversionFileModel videoConversionFile)
                {
                    videoConversionFileList.Add(videoConversionFile);
                    List<VideoConversionFileModel> sortedVideoConversionFileList = SortData(videoConversionFileList);
                    SelectedConversionType.VideoConversionFileCollection.Clear();
                    foreach (VideoConversionFileModel sortedVideoConversionFile in sortedVideoConversionFileList)
                    {
                        sortedVideoConversionFile.FileThumbnailSource ??= GetThumbnail(sortedVideoConversionFile.FilePath);
                        SelectedConversionType.VideoConversionFileCollection.Add(sortedVideoConversionFile);
                    }
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
                foreach (string filePath in filePathArray)
                {
                    if (await Task.Run(() => GetFileInformation(filePath)) is VideoConversionFileModel videoConversionFile)
                    {
                        videoConversionFileList.Add(videoConversionFile);
                    }
                }
                List<VideoConversionFileModel> sortedVideoConversionFileList = SortData(videoConversionFileList);
                SelectedConversionType.VideoConversionFileCollection.Clear();
                foreach (VideoConversionFileModel sortedVideoConversionFile in sortedVideoConversionFileList)
                {
                    sortedVideoConversionFile.FileThumbnailSource ??= GetThumbnail(sortedVideoConversionFile.FilePath);
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
                return SortWay ? videoConversionFileList.OrderBy(item => item.FileSize).ToList() : videoConversionFileList.OrderByDescending(item => item.FileSize).ToList();
            }
            else if (string.Equals(SelectedSortRule, SortRuleList[3]))
            {
                return SortWay ? videoConversionFileList.OrderBy(item => item.Duration).ToList() : videoConversionFileList.OrderByDescending(item => item.Duration).ToList();
            }
            else
            {
                return videoConversionFileList;
            }
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        private VideoFormatConversionModel GetFileInformation(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    VideoFormatConversionModel videoFormatConversion = new()
                    {
                        FileName = Path.GetFileName(filePath),
                        FilePath = filePath,
                    };
                    FileInfo fileInfo = new(filePath);
                    videoFormatConversion.FileSize = fileInfo.Length;
                    videoFormatConversion.FileSizeString = VolumeSizeHelper.ConvertVolumeSizeToString(fileInfo.Length);

                    if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                    {
                        string videoDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Duration", InfoKind.Text, InfoKind.Name));
                        if (int.TryParse(videoDuration, out int videoDurationValue))
                        {
                            TimeSpan videoDurationTimeSpan = TimeSpan.FromMilliseconds(videoDurationValue);
                            videoFormatConversion.Duration = videoDurationTimeSpan;
                            videoFormatConversion.DurationString = string.Format(@"{0:00}:{1:00}:{2:00}", videoDurationTimeSpan.TotalHours, videoDurationTimeSpan.Minutes, videoDurationTimeSpan.Minutes);
                        }
                        else
                        {
                            videoFormatConversion.Duration = TimeSpan.Zero;
                            videoFormatConversion.DurationString = string.IsNullOrEmpty(videoDuration) ? "00:00:00" : videoDuration;
                        }

                        string width = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Width", InfoKind.Text, InfoKind.Name));
                        videoFormatConversion.ScreenSizeWidth = string.IsNullOrEmpty(width) ? "0" : width;
                        string height = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, 0, "Height", InfoKind.Text, InfoKind.Name));
                        videoFormatConversion.ScreenSizeHeight = string.IsNullOrEmpty(height) ? "0" : height;
                        MediaInfoLibrary.MediaInfo_Close(handle);
                        MediaInfoLibrary.MediaInfo_Delete(handle);
                    }
                    return videoFormatConversion;
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
    }
}
