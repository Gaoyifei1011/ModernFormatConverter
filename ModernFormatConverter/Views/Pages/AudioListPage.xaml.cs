using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Helpers.Reflection;
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
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

// 抑制 CA1806，CA1822，IDE0060 警告
#pragma warning disable CA1806,CA1822,IDE0060

namespace ModernFormatConverter.Views.Pages
{
    /// <summary>
    /// 音频列表页面
    /// </summary>
    public sealed partial class AudioListPage : Page, INotifyPropertyChanged
    {
        private readonly string AudioConcatString = ResourceService.AudioListResource.GetString("AudioConcat");
        private readonly string AudioConcatDragOverContentString = ResourceService.AudioListResource.GetString("AudioConcatDragOverContent");
        private readonly string AudioFormatConversionString = ResourceService.AudioListResource.GetString("AudioFormatConversion");
        private readonly string AudioFormatConversionDragOverContentString = ResourceService.AudioListResource.GetString("AudioFormatConversionDragOverContent");
        private readonly string NoFolderString = ResourceService.AudioListResource.GetString("NoFolder");
        private readonly string NoMultiFileString = ResourceService.AudioListResource.GetString("NoMultiFile");
        private readonly string SelectFileString = ResourceService.AudioListResource.GetString("SelectFile");
        private readonly string SelectFolderString = ResourceService.AudioListResource.GetString("SelectFolder");
        private readonly string TextFileDragOverContentString = ResourceService.AudioListResource.GetString("TextFileDragOverContent");
        private readonly string TextToAudioString = ResourceService.AudioListResource.GetString("TextToAudio");
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        private bool canScrollHorizontally;
        private int textToAudioSelectorBarIndex = -1;

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

        private AudioConversionTypeModel _selectedConversionType;

        public AudioConversionTypeModel SelectedConversionType
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

        private bool _isVoiceExisted;

        public bool IsVoiceExisted
        {
            get { return _isVoiceExisted; }

            set
            {
                if (!Equals(_isVoiceExisted, value))
                {
                    _isVoiceExisted = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVoiceExisted)));
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

        private SelectorBarItem _textToAudioSelectedItem;

        public SelectorBarItem TextToAudioSelectedItem
        {
            get { return _textToAudioSelectedItem; }

            set
            {
                if (!Equals(_textToAudioSelectedItem, value))
                {
                    _textToAudioSelectedItem = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextToAudioSelectedItem)));
                }
            }
        }

        public List<string> SortRuleList { get; } = ["NotSort", "SortByFileName", "SortByFileSize", "SortByDuration"];

        public WinRTObservableCollection<AudioConversionTypeModel> AudioConversionTypeCollection { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public AudioListPage()
        {
            AudioConversionTypeCollection.Add(new AudioConversionTypeModel
            {
                AudioConversionType = AudioFormatConversionString,
                AudioConversionIcon = "\uE895",
                AudioConversionTypeKind = AudioConversionTypeKind.AudioFormatConversion,
                AudioFormatConversion = new()
            });
            AudioConversionTypeCollection.Add(new AudioConversionTypeModel
            {
                AudioConversionType = AudioConcatString,
                AudioConversionIcon = "\uEA3C",
                AudioConversionTypeKind = AudioConversionTypeKind.AudioConcat,
                AudioConcat = new()
                {
                    AudioConversionOutputConfiguration = new()
                    {
                        FormatConversionType = ".mp3",
                        AudioEncoding = "Copy",
                        SamplingRate = "Default",
                        AudioBitRate = "Default",
                        SoundTrack = "Default",
                        CloseSoundEffect = false,
                        Volume = "100%",
                        VariableBitRate = "Close",
                        SamplingFormat = "Default",
                        AudioFadeInEffect = "None",
                        AudioFadeOutEffect = "None",
                        Echo = false,
                        DeNoise = false,
                        Reverse = false
                    }
                }
            });
            AudioConversionTypeCollection.Add(new AudioConversionTypeModel
            {
                AudioConversionType = TextToAudioString,
                AudioConversionIcon = "\uE720",
                AudioConversionTypeKind = AudioConversionTypeKind.TextToAudio,
                TextToAudio = new()
                {
                    TextToAudioType = TextToAudioType.Text,
                    FileThumbnailSource = null,
                    InputText = string.Empty,
                    FileName = string.Empty,
                    FilePath = string.Empty,
                    FileSize = string.Empty,
                    FileCharacterSize = string.Empty,
                    TextToAudioOutputConfiguration = new()
                    {
                        VoiceType = string.Empty,
                        ReadingSpeed = 0,
                        Volume = 100
                    }
                }
            });
            InitializeComponent();
            SelectedItem = AudioListSelectorBar.Items[0];
            SelectedConversionType = AudioConversionTypeCollection[0];
            SelectedSortRule = SortRuleList[0];
            SortWay = true;
            InitializeVoiceInformation();
        }

        #region 第一部分：ExecuteCommand 命令调用时挂载的事件

        /// <summary>
        /// 移除选中项
        /// </summary>
        private void OnRemoveExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            // 音频格式转换
            if (SelectedConversionType.AudioConversionTypeKind is AudioConversionTypeKind.AudioFormatConversion)
            {
                SelectedConversionType.AudioFormatConversion.AudioFormatConversionFileCollection.Remove(args.Parameter as AudioFormatConversionFileModel);
            }
            // 音频合并
            else if (SelectedConversionType.AudioConversionTypeKind is AudioConversionTypeKind.AudioConcat)
            {
                SelectedConversionType.AudioConcat.AudioConcatFileCollection.Remove(args.Parameter as AudioConcatFileModel);
            }
        }

        /// <summary>
        /// 配置选中项转换参数
        /// </summary>
        private async void OnOutputConfigurationExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (MainWindow.Current.GetFrameContent() is AudioConversionPage audioConversionPage)
            {
                // 音频格式转换
                if (args.Parameter is AudioFormatConversionFileModel audioFormatConversionFile && audioFormatConversionFile.AudioConversionOutputConfiguration is not null)
                {
                    audioConversionPage.NavigateTo(audioConversionPage.PageList[2], new AudioConversionNavigationParameter()
                    {
                        AudioConversionTypeKind = AudioConversionTypeKind.AudioFormatConversion,
                        IsGlobalSettings = false,
                        AudioConversionData = audioFormatConversionFile
                    }, true);
                }
            }
        }

        /// <summary>
        /// 音频编辑
        /// </summary>
        private async void OnAudioEditExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (MainWindow.Current.GetFrameContent() is AudioConversionPage audioConversionPage && args.Parameter is AudioFormatConversionFileModel audioFormatConversionFile && audioFormatConversionFile.AudioEdit is not null)
            {
                audioConversionPage.NavigateTo(audioConversionPage.PageList[1], audioFormatConversionFile, true);
            }
        }

        #endregion 第一部分：ExecuteCommand 命令调用时挂载的事件

        #region 第二部分：音频列表页面——挂载的事件

        /// <summary>
        /// 设置拖动的数据的可视表示形式
        /// </summary>
        private async void OnAudioListDragEnter(object sender, Microsoft.UI.Xaml.DragEventArgs args)
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
                    if (SelectedConversionType.AudioConversionTypeKind is AudioConversionTypeKind.AudioFormatConversion)
                    {
                        args.AcceptedOperation = DataPackageOperation.Copy;
                        args.DragUIOverride.IsCaptionVisible = true;
                        args.DragUIOverride.IsContentVisible = false;
                        args.DragUIOverride.IsGlyphVisible = true;
                        args.DragUIOverride.Caption = AudioFormatConversionDragOverContentString;
                    }
                    else if (SelectedConversionType.AudioConversionTypeKind is AudioConversionTypeKind.AudioConcat)
                    {
                        args.AcceptedOperation = DataPackageOperation.Copy;
                        args.DragUIOverride.IsCaptionVisible = true;
                        args.DragUIOverride.IsContentVisible = false;
                        args.DragUIOverride.IsGlyphVisible = true;
                        args.DragUIOverride.Caption = AudioConcatDragOverContentString;
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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioConversionPage), nameof(OnAudioListDragEnter), 1, e);
            }
            finally
            {
                args.Handled = true;
                dragOperationDeferral.Complete();
            }
        }

        /// <summary>
        /// 设置拖动的文本文件的可视表示形式
        /// </summary>
        private async void OnTextFileDragEnter(object sender, Microsoft.UI.Xaml.DragEventArgs args)
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
                        args.DragUIOverride.Caption = TextFileDragOverContentString;
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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioConversionPage), nameof(OnTextFileDragEnter), 1, e);
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
        private async void OnAudioListDrop(object sender, Microsoft.UI.Xaml.DragEventArgs args)
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
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioConversionPage), nameof(OnAudioListDrop), 1, e);
                    }

                    return null;
                });
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioConversionPage), nameof(OnAudioListDrop), 2, e);
            }
            finally
            {
                dragOperationDeferral.Complete();
            }

            if (fileList is not null && fileList.Count > 0)
            {
                await AddAudioDataAsync(fileList);
            }
            IsGettingFileInformation = false;
        }

        /// <summary>
        /// 拖动文件完成后获取文本文件信息
        /// </summary>
        private async void OnTextFileDrop(object sender, Microsoft.UI.Xaml.DragEventArgs args)
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
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(FileInformationPage), nameof(OnTextFileDrop), 1, e);
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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioConversionPage), nameof(OnTextFileDrop), 2, e);
            }
            finally
            {
                dragOperationDeferral.Complete();
            }

            if (File.Exists(filePath))
            {
                if (await Task.Run(() => { return GetFileInformation(filePath); }) is TextToAudioModel textToAudio && SelectedConversionType.TextToAudio is not null && textToAudio.TextToAudioType is TextToAudioType.File)
                {
                    SelectedConversionType.TextToAudio.FileName = textToAudio.FileName;
                    SelectedConversionType.TextToAudio.FilePath = textToAudio.FilePath;
                    SelectedConversionType.TextToAudio.FileCharacterSize = textToAudio.FileCharacterSize;
                    SelectedConversionType.TextToAudio.FileSize = textToAudio.FileSize;
                    SelectedConversionType.TextToAudio.IsTextFileSelected = true;
                }
                if (Equals(SelectedConversionType.AudioConversionTypeKind, AudioConversionTypeKind.TextToAudio) && SelectedConversionType.TextToAudio.FileThumbnailSource is null)
                {
                    SelectedConversionType.TextToAudio.FileThumbnailSource = GetThumbnail(filePath);
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
                if (AudioListScrollViewer.HorizontalOffset <= 0)
                {
                    IsPreviousEnabled = false;
                    IsNextEnabled = true;
                }
                else if (AudioListScrollViewer.HorizontalOffset >= AudioListScrollViewer.ScrollableWidth)
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
            canScrollHorizontally = AudioListScrollViewer.ExtentWidth > AudioListScrollViewer.ViewportWidth;
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
                if (AudioListScrollViewer.HorizontalOffset <= 0)
                {
                    IsPreviousEnabled = false;
                    IsNextEnabled = true;
                }
                else if (AudioListScrollViewer.HorizontalOffset >= AudioListScrollViewer.ScrollableWidth)
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
            AudioListScrollViewer.ChangeView(AudioListScrollViewer.HorizontalOffset < 150 ? 0 : AudioListScrollViewer.HorizontalOffset - 150, null, null);
        }

        /// <summary>
        /// 向后移动
        /// </summary>
        private void OnNextClick(object sender, RoutedEventArgs args)
        {
            AudioListScrollViewer.ChangeView(AudioListScrollViewer.HorizontalOffset >= AudioListScrollViewer.ScrollableWidth - 150 ? AudioListScrollViewer.ScrollableWidth : AudioListScrollViewer.HorizontalOffset + 150, null, null);
        }

        /// <summary>
        /// 音频转换选择器栏选中项发生变化时触发的事件
        /// </summary>
        private async void OnSelectorBarSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            SelectedItem = sender.SelectedItem;
            SelectedConversionType = AudioConversionTypeCollection[sender.Items.IndexOf(SelectedItem)];
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
            // 音频格式转换
            if (SelectedConversionType.AudioConversionTypeKind is AudioConversionTypeKind.AudioFormatConversion)
            {
                SelectedConversionType.AudioFormatConversion.AudioFormatConversionFileCollection.Clear();
            }
            // 音频合并
            else if (SelectedConversionType.AudioConversionTypeKind is AudioConversionTypeKind.AudioConcat)
            {
                SelectedConversionType.AudioConcat.AudioConcatFileCollection.Clear();
            }
            // 文本转语音
            else if (SelectedConversionType.AudioConversionTypeKind is AudioConversionTypeKind.TextToAudio && SelectedConversionType.TextToAudio is not null)
            {
                SelectedConversionType.TextToAudio.FileThumbnailSource = null;
                SelectedConversionType.TextToAudio.InputText = string.Empty;
                SelectedConversionType.TextToAudio.IsTextFileSelected = false;
                SelectedConversionType.TextToAudio.FileName = string.Empty;
                SelectedConversionType.TextToAudio.FilePath = string.Empty;
                SelectedConversionType.TextToAudio.FileSize = string.Empty;
                SelectedConversionType.TextToAudio.FileCharacterSize = string.Empty;
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
                await AddAudioDataAsync([.. openFileDialog.FileNames]);
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
                await AddAudioDataAsync(fileList);
                IsGettingFileInformation = false;
            }
            openFolderDialog.Dispose();
        }

        /// <summary>
        /// 打开输出配置
        /// </summary>
        private async void OnOutputConfigurationClicked(object sender, RoutedEventArgs args)
        {
            if (MainWindow.Current.GetFrameContent() is AudioConversionPage audioConversionPage)
            {
                // 音频格式转换
                if (SelectedConversionType.AudioConversionTypeKind is AudioConversionTypeKind.AudioFormatConversion)
                {
                    audioConversionPage.NavigateTo(audioConversionPage.PageList[2], new AudioConversionNavigationParameter()
                    {
                        AudioConversionTypeKind = AudioConversionTypeKind.AudioFormatConversion,
                        IsGlobalSettings = true,
                        AudioConversionData = AudioConversionTypeCollection[0].AudioFormatConversion.AudioFormatConversionFileCollection.ToList()
                    }, true);
                }
                // 音频合并
                else if (SelectedConversionType.AudioConversionTypeKind is AudioConversionTypeKind.AudioConcat)
                {
                    audioConversionPage.NavigateTo(audioConversionPage.PageList[2], new AudioConversionNavigationParameter()
                    {
                        AudioConversionTypeKind = AudioConversionTypeKind.AudioConcat,
                        IsGlobalSettings = true,
                        AudioConversionData = AudioConversionTypeCollection[1].AudioConcat
                    }, true);
                }
                // 文本转语音
                else if (SelectedConversionType.AudioConversionTypeKind is AudioConversionTypeKind.TextToAudio)
                {
                    audioConversionPage.NavigateTo(audioConversionPage.PageList[3], new AudioConversionNavigationParameter()
                    {
                        AudioConversionTypeKind = AudioConversionTypeKind.TextToAudio,
                        IsGlobalSettings = true,
                        AudioConversionData = AudioConversionTypeCollection[2].TextToAudio
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

        /// <summary>
        /// 文本转语音选择栏选中项加载后触发的事件
        /// </summary>
        private void OnTextToAudioSelectorBarLoaded(object sender, RoutedEventArgs args)
        {
            if (textToAudioSelectorBarIndex is -1 && TextToAudioSelectedItem is null)
            {
                textToAudioSelectorBarIndex = 0;
            }

            TextToAudioSelectedItem = TextToAudioSelectorBar.Items[textToAudioSelectorBarIndex];
        }

        /// <summary>
        /// 文本转语音选择栏选中项取消加载后触发的事件
        /// </summary>
        private void OnTextToAudioSelectorBarUnloaded(object sender, RoutedEventArgs args)
        {
            TextToAudioSelectedItem = null;
        }

        /// <summary>
        /// 文本转语音选中项发生变化时触发的事件
        /// </summary>
        private void OnTextToAudioSelectorBarSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            if (!Equals(sender.SelectedItem, TextToAudioSelectedItem) && SelectedConversionType.TextToAudio is TextToAudioModel textToAudio)
            {
                TextToAudioSelectedItem = sender.SelectedItem;
                textToAudioSelectorBarIndex = sender.Items.IndexOf(TextToAudioSelectedItem);
                textToAudio.TextToAudioType = (TextToAudioType)TextToAudioSelectorBar.Items.IndexOf(TextToAudioSelectedItem);
            }
        }

        /// <summary>
        /// 添加文本文件
        /// </summary>
        private async void OnAddTextFileClicked(object sender, RoutedEventArgs args)
        {
            OpenFileDialog openFileDialog = new()
            {
                Multiselect = false,
                Title = SelectFileString
            };
            if (openFileDialog.ShowDialog() is DialogResult.OK)
            {
                IsGettingFileInformation = true;
                if (await Task.Run(() => { return GetFileInformation(openFileDialog.FileName); }) is TextToAudioModel textToAudio && SelectedConversionType.AudioConversionTypeKind is AudioConversionTypeKind.TextToAudio && SelectedConversionType.TextToAudio is not null && textToAudio.TextToAudioType is TextToAudioType.File)
                {
                    SelectedConversionType.TextToAudio.FileName = textToAudio.FileName;
                    SelectedConversionType.TextToAudio.FilePath = textToAudio.FilePath;
                    SelectedConversionType.TextToAudio.FileCharacterSize = textToAudio.FileCharacterSize;
                    SelectedConversionType.TextToAudio.FileSize = textToAudio.FileSize;
                    SelectedConversionType.TextToAudio.IsTextFileSelected = true;
                }
                if (Equals(SelectedConversionType.AudioConversionTypeKind, AudioConversionTypeKind.TextToAudio) && SelectedConversionType.TextToAudio.FileThumbnailSource is null)
                {
                    SelectedConversionType.TextToAudio.FileThumbnailSource = GetThumbnail(openFileDialog.FileName);
                }
                IsGettingFileInformation = false;
            }
            openFileDialog.Dispose();
        }

        /// <summary>
        /// 移除文本文件
        /// </summary>
        private void OnRemoveTextFileClicked(object sender, RoutedEventArgs args)
        {
            if (SelectedConversionType.TextToAudio is not null && SelectedConversionType.AudioConversionTypeKind is AudioConversionTypeKind.TextToAudio && SelectedConversionType.TextToAudio.TextToAudioType is TextToAudioType.File)
            {
                SelectedConversionType.TextToAudio.FileThumbnailSource = null;
                SelectedConversionType.TextToAudio.IsTextFileSelected = false;
                SelectedConversionType.TextToAudio.FileName = string.Empty;
                SelectedConversionType.TextToAudio.FilePath = string.Empty;
                SelectedConversionType.TextToAudio.FileSize = string.Empty;
                SelectedConversionType.TextToAudio.FileCharacterSize = string.Empty;
            }
        }

        /// <summary>
        /// 打开系统设置
        /// </summary>
        private void OnSystemSettingsClicked(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>
            {
                try
                {
                    Process.Start("ms-settings:speech");
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioConversionPage), nameof(OnSystemSettingsClicked), 1, e);
                }
            });
        }

        /// <summary>
        /// 转换文本内容发生改变时触发的事件
        /// </summary>
        private void OnConvertInputTextChanged(object sender, TextChangedEventArgs args)
        {
            if (sender is Microsoft.UI.Xaml.Controls.TextBox textBox && SelectedConversionType.TextToAudio is TextToAudioModel textToAudio && textToAudio.TextToAudioType is TextToAudioType.Text)
            {
                textToAudio.InputText = textBox.Text;
            }
        }

        #endregion 第二部分：音频列表页面——挂载的事件

        /// <summary>
        /// 添加音频数据
        /// </summary>
        private async Task AddAudioDataAsync(List<string> fileList)
        {
            // 音频格式转换
            if (SelectedConversionType.AudioConversionTypeKind is AudioConversionTypeKind.AudioFormatConversion)
            {
                List<AudioFormatConversionFileModel> audioFormatConversionFileList = [.. SelectedConversionType.AudioFormatConversion.AudioFormatConversionFileCollection];
                List<Task> taskList = [];
                object fileInformationLock = new();
                foreach (string file in fileList)
                {
                    taskList.Add(Task.Run(async () =>
                    {
                        if (GetFileInformation(file) is AudioFormatConversionFileModel audioFormatConversionFile)
                        {
                            lock (fileInformationLock)
                            {
                                audioFormatConversionFileList.Add(audioFormatConversionFile);
                            }
                        }
                    }));
                }
                await Task.WhenAll(taskList);
                List<AudioFormatConversionFileModel> sortedAudioFormatConversionFileList = SortAudioFormatConversionFileData(audioFormatConversionFileList);
                SelectedConversionType.AudioFormatConversion.AudioFormatConversionFileCollection.Clear();
                foreach (AudioFormatConversionFileModel sortedAudioFormatConversionFile in sortedAudioFormatConversionFileList)
                {
                    sortedAudioFormatConversionFile.FileThumbnailSource ??= GetThumbnail(sortedAudioFormatConversionFile.FilePath);
                    SelectedConversionType.AudioFormatConversion.AudioFormatConversionFileCollection.Add(sortedAudioFormatConversionFile);
                }
            }
            // 音频合并
            else if (SelectedConversionType.AudioConversionTypeKind is AudioConversionTypeKind.AudioConcat)
            {
                List<AudioConcatFileModel> audioConcatFileList = [.. SelectedConversionType.AudioConcat.AudioConcatFileCollection];
                List<Task> taskList = [];
                object fileInformationLock = new();
                foreach (string file in fileList)
                {
                    taskList.Add(Task.Run(async () =>
                    {
                        if (GetFileInformation(file) is AudioConcatFileModel audioConcatFile)
                        {
                            lock (fileInformationLock)
                            {
                                audioConcatFileList.Add(audioConcatFile);
                            }
                        }
                    }));
                }
                await Task.WhenAll(taskList);
                List<AudioConcatFileModel> sortedAudioConcatFileList = SortAudioConcatFileData(audioConcatFileList);
                SelectedConversionType.AudioConcat.AudioConcatFileCollection.Clear();
                foreach (AudioConcatFileModel sortedAudioConcatFile in sortedAudioConcatFileList)
                {
                    SelectedConversionType.AudioConcat.AudioConcatFileCollection.Add(sortedAudioConcatFile);
                }
            }
        }

        /// <summary>
        /// 对数据进行排序
        /// </summary>
        private async Task SortDataAsync()
        {
            // 音频格式转换
            if (SelectedConversionType.AudioConversionTypeKind is AudioConversionTypeKind.AudioFormatConversion)
            {
                List<AudioFormatConversionFileModel> sortedAudioFormatConversionFileList = await Task.Run(() =>
                {
                    List<AudioFormatConversionFileModel> sortedAudioFormatConversionFileList = [.. SelectedConversionType.AudioFormatConversion.AudioFormatConversionFileCollection];
                    return SortAudioFormatConversionFileData(sortedAudioFormatConversionFileList);
                });
                SelectedConversionType.AudioFormatConversion.AudioFormatConversionFileCollection.Clear();
                foreach (AudioFormatConversionFileModel sortedAudioFormatConversionFile in sortedAudioFormatConversionFileList)
                {
                    SelectedConversionType.AudioFormatConversion.AudioFormatConversionFileCollection.Add(sortedAudioFormatConversionFile);
                }
            }
            // 音频合并
            else if (SelectedConversionType.AudioConversionTypeKind is AudioConversionTypeKind.AudioConcat)
            {
                List<AudioConcatFileModel> sortedAudioConcatFileList = await Task.Run(() =>
                {
                    List<AudioConcatFileModel> sortedAudioConcatFileList = [.. SelectedConversionType.AudioConcat.AudioConcatFileCollection];
                    return SortAudioConcatFileData(sortedAudioConcatFileList);
                });
                SelectedConversionType.AudioConcat.AudioConcatFileCollection.Clear();
                foreach (AudioConcatFileModel sortedAudioConcatFile in sortedAudioConcatFileList)
                {
                    SelectedConversionType.AudioConcat.AudioConcatFileCollection.Add(sortedAudioConcatFile);
                }
            }
        }

        /// <summary>
        /// 对音频转换文件数据进行排序
        /// </summary>
        private List<AudioFormatConversionFileModel> SortAudioFormatConversionFileData(List<AudioFormatConversionFileModel> audioFormatConversionFileList)
        {
            // 按照文件名称排序
            if (string.Equals(SelectedSortRule, SortRuleList[1]))
            {
                return SortWay ? [.. audioFormatConversionFileList.OrderBy(item => item.FileName)] : [.. audioFormatConversionFileList.OrderByDescending(item => item.FileName)];
            }
            // 按照文件大小排序
            else if (string.Equals(SelectedSortRule, SortRuleList[2]))
            {
                return SortWay ? [.. audioFormatConversionFileList.OrderBy(item => item.FileSize)] : [.. audioFormatConversionFileList.OrderByDescending(item => item.FileSize)];
            }
            // 按照音频持续时长排序
            else if (string.Equals(SelectedSortRule, SortRuleList[3]))
            {
                return SortWay ? [.. audioFormatConversionFileList.OrderBy(item => item.Duration)] : [.. audioFormatConversionFileList.OrderByDescending(item => item.Duration)];
            }
            else
            {
                return audioFormatConversionFileList;
            }
        }

        /// <summary>
        /// 对音频合并文件数据进行排序
        /// </summary>
        private List<AudioConcatFileModel> SortAudioConcatFileData(List<AudioConcatFileModel> audioConcatFileList)
        {
            // 按照文件名称排序
            if (string.Equals(SelectedSortRule, SortRuleList[1]))
            {
                return SortWay ? [.. audioConcatFileList.OrderBy(item => item.FileName)] : [.. audioConcatFileList.OrderByDescending(item => item.FileName)];
            }
            // 按照文件大小排序
            else if (string.Equals(SelectedSortRule, SortRuleList[2]))
            {
                return SortWay ? [.. audioConcatFileList.OrderBy(item => item.FileSize)] : [.. audioConcatFileList.OrderByDescending(item => item.FileSize)];
            }
            // 按照音频持续时长排序
            else if (string.Equals(SelectedSortRule, SortRuleList[3]))
            {
                return SortWay ? [.. audioConcatFileList.OrderBy(item => item.Duration)] : [.. audioConcatFileList.OrderByDescending(item => item.Duration)];
            }
            else
            {
                return audioConcatFileList;
            }
        }

        /// <summary>
        /// 初始化语音信息
        /// </summary>
        private void InitializeVoiceInformation()
        {
            Task.Run(() =>
            {
                try
                {
                    bool isVoiceExisted = false;
                    string voiceType = string.Empty;
                    SpeechSynthesizer speechSynthesizer = new();
                    speechSynthesizer.InjectOneCoreVoices();
                    foreach (InstalledVoice installedVoice in speechSynthesizer.GetInstalledVoices())
                    {
                        if (installedVoice.Enabled && AudioConversionTypeCollection[2].TextToAudio is not null)
                        {
                            isVoiceExisted = true;
                            voiceType = installedVoice.VoiceInfo.Id;
                            break;
                        }
                    }

                    speechSynthesizer.Dispose();

                    synchronizationContext.Post((_) =>
                    {
                        IsVoiceExisted = isVoiceExisted;
                        AudioConversionTypeCollection[2].TextToAudio.TextToAudioOutputConfiguration.VoiceType = voiceType;
                    }, null);
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioConversionPage), nameof(InitializeVoiceInformation), 1, e);
                }
            });
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        private object GetFileInformation(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    // 音频格式转换
                    if (SelectedConversionType.AudioConversionTypeKind is AudioConversionTypeKind.AudioFormatConversion)
                    {
                        AudioFormatConversionFileModel audioFormatConversionFile = new()
                        {
                            FileName = Path.GetFileName(filePath),
                            FilePath = filePath,
                        };
                        FileInfo fileInfo = new(filePath);
                        audioFormatConversionFile.FileSize = fileInfo.Length;

                        if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                        {
                            string audioDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, 0, "Duration", InfoKind.Text, InfoKind.Name));
                            if (double.TryParse(audioDuration, out double audioDurationValue))
                            {
                                TimeSpan audioDurationTimeSpan = TimeSpan.FromMilliseconds(audioDurationValue);
                                audioFormatConversionFile.Duration = audioDurationTimeSpan;
                            }
                            else
                            {
                                audioFormatConversionFile.Duration = TimeSpan.Zero;
                            }
                            string channel = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, 0, "Channel(s)/String", InfoKind.Text, InfoKind.Name));
                            audioFormatConversionFile.Channel = string.IsNullOrEmpty(channel) ? "0" : channel;
                            string samplingRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, 0, "SamplingRate/String", InfoKind.Text, InfoKind.Name));
                            audioFormatConversionFile.SamplingRate = string.IsNullOrEmpty(samplingRate) ? "0" : samplingRate;
                            string bitRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, 0, "BitRate/String", InfoKind.Text, InfoKind.Name));
                            audioFormatConversionFile.BitRate = string.IsNullOrEmpty(bitRate) ? "0" : bitRate;
                            MediaInfoLibrary.MediaInfo_Close(handle);
                            MediaInfoLibrary.MediaInfo_Delete(handle);
                        }

                        audioFormatConversionFile.AudioConversionOutputConfiguration = new()
                        {
                            FormatConversionType = ".mp3",
                            AudioEncoding = "Copy",
                            SamplingRate = "Default",
                            AudioBitRate = "Default",
                            SoundTrack = "Default",
                            CloseSoundEffect = false,
                            Volume = "100%",
                            VariableBitRate = "Close",
                            SamplingFormat = "Default",
                            AudioFadeInEffect = "None",
                            AudioFadeOutEffect = "None",
                            Echo = false,
                            DeNoise = false,
                            Reverse = false
                        };

                        audioFormatConversionFile.AudioEdit = new()
                        {
                            StartTime = TimeSpan.Zero,
                            EndTime = TimeSpan.Zero,
                            AudioCoverFilePath = string.Empty
                        };

                        return audioFormatConversionFile;
                    }
                    // 音频合并
                    else if (SelectedConversionType.AudioConversionTypeKind is AudioConversionTypeKind.AudioConcat)
                    {
                        AudioConcatFileModel audioConcatFile = new()
                        {
                            FileName = Path.GetFileName(filePath),
                            FilePath = filePath,
                        };
                        FileInfo fileInfo = new(filePath);
                        audioConcatFile.FileSize = fileInfo.Length;

                        if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                        {
                            string audioDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, 0, "Duration", InfoKind.Text, InfoKind.Name));
                            if (double.TryParse(audioDuration, out double audioDurationValue))
                            {
                                TimeSpan audioDurationTimeSpan = TimeSpan.FromMilliseconds(audioDurationValue);
                                audioConcatFile.Duration = audioDurationTimeSpan;
                            }
                            else
                            {
                                audioConcatFile.Duration = TimeSpan.Zero;
                            }
                            string channel = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, 0, "Channel(s)/String", InfoKind.Text, InfoKind.Name));
                            audioConcatFile.Channel = string.IsNullOrEmpty(channel) ? "0" : channel;
                            string samplingRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, 0, "SamplingRate/String", InfoKind.Text, InfoKind.Name));
                            audioConcatFile.SamplingRate = string.IsNullOrEmpty(samplingRate) ? "0" : samplingRate;
                            string bitRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, 0, "BitRate/String", InfoKind.Text, InfoKind.Name));
                            audioConcatFile.BitRate = string.IsNullOrEmpty(bitRate) ? "0" : bitRate;
                            MediaInfoLibrary.MediaInfo_Close(handle);
                            MediaInfoLibrary.MediaInfo_Delete(handle);
                        }

                        return audioConcatFile;
                    }
                    // 文本转语音
                    else if (Equals(SelectedConversionType, AudioConversionTypeCollection[2]))
                    {
                        TextToAudioModel textToAudio = new()
                        {
                            TextToAudioType = TextToAudioType.File,
                            FileName = Path.GetFileName(filePath),
                            FilePath = filePath
                        };
                        FileInfo fileInfo = new(filePath);
                        textToAudio.FileSize = VolumeSizeHelper.ConvertVolumeSizeToString(fileInfo.Length); ;
                        textToAudio.FileCharacterSize = Convert.ToString(File.ReadAllText(filePath).Length);
                        return textToAudio;
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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioConversionPage), nameof(GetFileInformation), 1, e);
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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioConversionPage), nameof(GetThumbnail), 1, e);
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
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioConversionPage), nameof(GetThumbnail), 2, e);
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

        private bool GetAllowDropAudioConversionFile(AudioConversionTypeKind audioConversionTypeKind)
        {
            return audioConversionTypeKind is not AudioConversionTypeKind.TextToAudio;
        }

        private Visibility GetSelectedTextToAudioType(TextToAudioType selectedTextToAudioType, TextToAudioType comparedSelectedTextToAudioType)
        {
            return Equals(selectedTextToAudioType, comparedSelectedTextToAudioType) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
