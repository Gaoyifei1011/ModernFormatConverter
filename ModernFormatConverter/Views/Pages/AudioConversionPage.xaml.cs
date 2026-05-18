using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Helpers.Root;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Services.Settings;
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
    /// 音频转换页面
    /// </summary>
    public sealed partial class AudioConversionPage : Page, INotifyPropertyChanged
    {
        private readonly string AudioConcatString = ResourceService.AudioConversionResource.GetString("AudioConcat");
        private readonly string AudioFormatConversionString = ResourceService.AudioConversionResource.GetString("AudioFormatConversion");
        private readonly string DragOverContentString = ResourceService.AudioConversionResource.GetString("DragOverContent");
        private readonly string NoFolderString = ResourceService.AudioConversionResource.GetString("NoFolder");
        private readonly string SelectFileString = ResourceService.AudioConversionResource.GetString("SelectFile");
        private readonly string SelectFolderString = ResourceService.AudioConversionResource.GetString("SelectFolder");
        private readonly string TextToAudioString = ResourceService.AudioConversionResource.GetString("TextToAudio");

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

        public WinRTObservableCollection<AudioConversionTypeModel> AudioConversionTypeCollection { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public AudioConversionPage()
        {
            InitializeComponent();
            AudioConversionTypeCollection.Add(new AudioConversionTypeModel
            {
                AudioConversionType = AudioFormatConversionString,
                AudioConversionIcon = "\uE895",
                AudioConversionTypeKind = AudioConversionTypeKind.AudioFormatConversion
            });
            AudioConversionTypeCollection.Add(new AudioConversionTypeModel
            {
                AudioConversionType = AudioConcatString,
                AudioConversionIcon = "\uEA3C",
                AudioConversionTypeKind = AudioConversionTypeKind.AudioConcat
            });
            AudioConversionTypeCollection.Add(new AudioConversionTypeModel
            {
                AudioConversionType = TextToAudioString,
                AudioConversionIcon = "\uE720",
                AudioConversionTypeKind = AudioConversionTypeKind.TextToAudio
            });
            SelectedConversionType = AudioConversionTypeCollection[0];
            SelectedSortRule = SortRuleList[0];
            SortWay = true;
            OutputFolder = ConvertConfigurationService.ConvertedAudioSavePath;
        }

        #region 第一部分：ExecuteCommand 命令调用时挂载的事件

        /// <summary>
        /// 移除选中项
        /// </summary>
        private void OnRemoveExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            SelectedConversionType.AudioConversionFileCollection.Remove(args.Parameter as AudioConversionFileModel);
        }

        /// <summary>
        /// 配置选中项转换参数
        /// </summary>
        private async void OnOutputConfigurationExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (args.Parameter is AudioConversionFileModel audioConversionFile)
            {
                // 音频格式转换输出配置 & 音频合并输出配置
                if (Equals(SelectedConversionType, AudioConversionTypeCollection[0]) || Equals(SelectedConversionType, AudioConversionTypeCollection[1]))
                {
                    AudioConversionOutputConfigurationWindow audioConversionOutputConfigurationWindow = new(SelectedConversionType.AudioConversionTypeKind, ConversionToolsWindow.Current, audioConversionFile.AudioConversionOutputConfiguration);
                    if (await audioConversionOutputConfigurationWindow.ShowAsync() is ContentDialogResult.Primary && SelectedConversionType.AudioConversionTypeKind is AudioConversionTypeKind.AudioFormatConversion && audioConversionFile.AudioConversionOutputConfiguration is not null)
                    {
                        audioConversionFile.AudioConversionOutputConfiguration.FormatConversionType = Convert.ToString(audioConversionOutputConfigurationWindow.SelectedFormatConversionType.SelectedValue);
                        audioConversionFile.AudioConversionOutputConfiguration.AudioEncoding = Convert.ToString(audioConversionOutputConfigurationWindow.SelectedAudioEncoding.SelectedValue);
                        audioConversionFile.AudioConversionOutputConfiguration.SamplingRate = Convert.ToString(audioConversionOutputConfigurationWindow.SelectedSamplingRate.SelectedValue);
                        audioConversionFile.AudioConversionOutputConfiguration.AudioBitRate = Convert.ToString(audioConversionOutputConfigurationWindow.SelectedAudioBitRate.SelectedValue);
                        audioConversionFile.AudioConversionOutputConfiguration.SoundTrack = Convert.ToString(audioConversionOutputConfigurationWindow.SelectedSoundTrack.SelectedValue);
                        audioConversionFile.AudioConversionOutputConfiguration.CloseSoundEffect = audioConversionOutputConfigurationWindow.CloseSoundEffect;
                        audioConversionFile.AudioConversionOutputConfiguration.Volume = Convert.ToString(audioConversionOutputConfigurationWindow.SelectedVolume.SelectedValue);
                        audioConversionFile.AudioConversionOutputConfiguration.VariableBitRate = audioConversionOutputConfigurationWindow.IsVariableBitRateSupported ? Convert.ToString(audioConversionOutputConfigurationWindow.SelectedVariableBitRate.SelectedValue) : string.Empty;
                        audioConversionFile.AudioConversionOutputConfiguration.SamplingFormat = Convert.ToString(audioConversionOutputConfigurationWindow.SelectedSamplingFormat);
                        audioConversionFile.AudioConversionOutputConfiguration.AudioFadeInEffect = Convert.ToString(audioConversionOutputConfigurationWindow.SelectedAudioFadeInEffect.SelectedValue);
                        audioConversionFile.AudioConversionOutputConfiguration.AudioFadeOutEffect = Convert.ToString(audioConversionOutputConfigurationWindow.SelectedAudioFadeOutEffect.SelectedValue);
                        audioConversionFile.AudioConversionOutputConfiguration.Echo = audioConversionOutputConfigurationWindow.Echo;
                        audioConversionFile.AudioConversionOutputConfiguration.DeNoise = audioConversionOutputConfigurationWindow.DeNoise;
                        audioConversionFile.AudioConversionOutputConfiguration.Reverse = audioConversionOutputConfigurationWindow.Reverse;
                    }
                }
                // 文本转音频输出配置
                else if (Equals(SelectedConversionType, AudioConversionTypeCollection[2]))
                {
                    // TODO：未完成
                }
            }
        }

        /// <summary>
        /// 剪辑音频
        /// </summary>
        private async void OnCutAudioExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (args.Parameter is AudioConversionFileModel audioConversionFile && audioConversionFile.CutAudio is not null)
            {
                CutAudioWindow cutAudioWindow = new(ConversionToolsWindow.Current, audioConversionFile.CutAudio, audioConversionFile.FilePath);
                if (await cutAudioWindow.ShowAsync() is ContentDialogResult.Primary)
                {
                    audioConversionFile.CutAudio.StartTime = new(0, cutAudioWindow.TimeStartHours, cutAudioWindow.TimeStartMinutes, cutAudioWindow.TimeStartSeconds, cutAudioWindow.TimeStartMillseconds);
                    audioConversionFile.CutAudio.EndTime = new(0, cutAudioWindow.TimeEndHours, cutAudioWindow.TimeEndMinutes, cutAudioWindow.TimeEndSeconds, cutAudioWindow.TimeEndMillseconds);
                    audioConversionFile.CutAudio.AudioCoverFilePath = cutAudioWindow.AudioCoverFilePath;
                }
            }
        }

        #endregion 第一部分：ExecuteCommand 命令调用时挂载的事件

        #region 第二部分：音频转换页面——挂载的事件

        /// <summary>
        /// 设置拖动的数据的可视表示形式
        /// </summary>
        private async void OnAudioConversionDragEnter(object sender, Microsoft.UI.Xaml.DragEventArgs args)
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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioConversionPage), nameof(OnAudioConversionDragEnter), 1, e);
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
        private async void OnAudioConversionDrop(object sender, Microsoft.UI.Xaml.DragEventArgs args)
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
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioConversionPage), nameof(OnAudioConversionDrop), 1, e);
                    }

                    return null;
                });
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(AudioConversionPage), nameof(OnAudioConversionDrop), 2, e);
            }
            finally
            {
                dragOperationDeferral.Complete();
            }

            if (fileList is not null && fileList.Count > 0)
            {
                IsGettingFileInformation = true;
                List<AudioConversionFileModel> audioConversionFileList = [.. SelectedConversionType.AudioConversionFileCollection];
                List<Task> taskList = [];
                object fileInformationLock = new();
                foreach (IStorageItem file in fileList)
                {
                    taskList.Add(Task.Run(async () =>
                    {
                        if (GetFileInformation(file.Path) is AudioConversionFileModel audioConversionFile)
                        {
                            lock (fileInformationLock)
                            {
                                audioConversionFileList.Add(audioConversionFile);
                            }
                        }
                    }));
                }
                await Task.WhenAll(taskList);
                List<AudioConversionFileModel> sortedAudioConversionFileList = SortData(audioConversionFileList);
                SelectedConversionType.AudioConversionFileCollection.Clear();
                foreach (AudioConversionFileModel sortedAudioConversionFile in sortedAudioConversionFileList)
                {
                    if (!Equals(SelectedConversionType.AudioConversionTypeKind, AudioConversionTypeKind.AudioConcat) && sortedAudioConversionFile.FileThumbnailSource is null)
                    {
                        sortedAudioConversionFile.FileThumbnailSource = GetThumbnail(sortedAudioConversionFile.FilePath);
                    }
                    SelectedConversionType.AudioConversionFileCollection.Add(sortedAudioConversionFile);
                }
                IsGettingFileInformation = false;
            }
        }

        /// <summary>
        /// 音频转换列表选中项发生变化时触发的事件
        /// </summary>
        private async void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            SelectedConversionType = args.SelectedItem as AudioConversionTypeModel;
            IsGettingFileInformation = true;
            List<AudioConversionFileModel> sortedAudioConversionFileList = await Task.Run(() =>
            {
                List<AudioConversionFileModel> audioConversionFileList = [.. SelectedConversionType.AudioConversionFileCollection];
                return SortData(audioConversionFileList);
            });
            SelectedConversionType.AudioConversionFileCollection.Clear();
            foreach (AudioConversionFileModel sortedAudioConversionFile in sortedAudioConversionFileList)
            {
                SelectedConversionType.AudioConversionFileCollection.Add(sortedAudioConversionFile);
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
                List<AudioConversionFileModel> sortedAudioConversionFileList = await Task.Run(() =>
                {
                    List<AudioConversionFileModel> audioConversionFileList = [.. SelectedConversionType.AudioConversionFileCollection];
                    return SortData(audioConversionFileList);
                });
                SelectedConversionType.AudioConversionFileCollection.Clear();
                foreach (AudioConversionFileModel sortedAudioConversionFile in sortedAudioConversionFileList)
                {
                    SelectedConversionType.AudioConversionFileCollection.Add(sortedAudioConversionFile);
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
                List<AudioConversionFileModel> sortedAudioConversionFileList = await Task.Run(() =>
                {
                    List<AudioConversionFileModel> audioConversionFileList = [.. SelectedConversionType.AudioConversionFileCollection];
                    return SortData(audioConversionFileList);
                });
                SelectedConversionType.AudioConversionFileCollection.Clear();
                foreach (AudioConversionFileModel sortedAudioConversionFile in sortedAudioConversionFileList)
                {
                    SelectedConversionType.AudioConversionFileCollection.Add(sortedAudioConversionFile);
                }
                IsGettingFileInformation = false;
            }
        }

        /// <summary>
        /// 清空
        /// </summary>
        private void OnClearClicked(object sender, RoutedEventArgs args)
        {
            SelectedConversionType.AudioConversionFileCollection.Clear();
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
                List<AudioConversionFileModel> audioConversionFileList = [.. SelectedConversionType.AudioConversionFileCollection];
                List<Task> taskList = [];
                object fileInformationLock = new();
                foreach (string filePath in openFileDialog.FileNames)
                {
                    taskList.Add(Task.Run(async () =>
                    {
                        if (GetFileInformation(filePath) is AudioConversionFileModel audioConversionFile)
                        {
                            lock (fileInformationLock)
                            {
                                audioConversionFileList.Add(audioConversionFile);
                            }
                        }
                    }));
                }
                await Task.WhenAll(taskList);
                List<AudioConversionFileModel> sortedAudioConversionFileList = SortData(audioConversionFileList);
                SelectedConversionType.AudioConversionFileCollection.Clear();
                foreach (AudioConversionFileModel sortedAudioConversionFile in sortedAudioConversionFileList)
                {
                    if (!Equals(SelectedConversionType.AudioConversionTypeKind, AudioConversionTypeKind.AudioConcat) && sortedAudioConversionFile.FileThumbnailSource is null)
                    {
                        sortedAudioConversionFile.FileThumbnailSource = GetThumbnail(sortedAudioConversionFile.FilePath);
                    }
                    SelectedConversionType.AudioConversionFileCollection.Add(sortedAudioConversionFile);
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
                List<AudioConversionFileModel> audioConversionFileList = [.. SelectedConversionType.AudioConversionFileCollection];
                string[] filePathArray = Directory.GetFiles(openFolderDialog.SelectedPath);
                List<Task> taskList = [];
                object fileInformationLock = new();
                foreach (string filePath in filePathArray)
                {
                    taskList.Add(Task.Run(async () =>
                    {
                        if (GetFileInformation(filePath) is AudioConversionFileModel audioConversionFile)
                        {
                            lock (fileInformationLock)
                            {
                                audioConversionFileList.Add(audioConversionFile);
                            }
                        }
                    }));
                }
                await Task.WhenAll(taskList);
                List<AudioConversionFileModel> sortedAudioConversionFileList = SortData(audioConversionFileList);
                SelectedConversionType.AudioConversionFileCollection.Clear();
                foreach (AudioConversionFileModel sortedAudioConversionFile in sortedAudioConversionFileList)
                {
                    if (!Equals(SelectedConversionType.AudioConversionTypeKind, AudioConversionTypeKind.AudioConcat) && sortedAudioConversionFile.FileThumbnailSource is null)
                    {
                        sortedAudioConversionFile.FileThumbnailSource = GetThumbnail(sortedAudioConversionFile.FilePath);
                    }
                    SelectedConversionType.AudioConversionFileCollection.Add(sortedAudioConversionFile);
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
            // 音频格式转换输出配置 & 音频合并输出配置
            if (Equals(SelectedConversionType, AudioConversionTypeCollection[0]) || Equals(SelectedConversionType, AudioConversionTypeCollection[1]))
            {
                AudioConversionOutputConfigurationWindow audioConversionOutputConfigurationWindow = new(SelectedConversionType.AudioConversionTypeKind, ConversionToolsWindow.Current);
                if (await audioConversionOutputConfigurationWindow.ShowAsync() is ContentDialogResult.Primary && SelectedConversionType.AudioConversionTypeKind is AudioConversionTypeKind.AudioFormatConversion)
                {
                    foreach (AudioConversionFileModel audioConversionFile in SelectedConversionType.AudioConversionFileCollection)
                    {
                        if (audioConversionFile.AudioConversionOutputConfiguration is not null)
                        {
                            audioConversionFile.AudioConversionOutputConfiguration.FormatConversionType = Convert.ToString(audioConversionOutputConfigurationWindow.SelectedFormatConversionType.SelectedValue);
                            audioConversionFile.AudioConversionOutputConfiguration.AudioEncoding = Convert.ToString(audioConversionOutputConfigurationWindow.SelectedAudioEncoding.SelectedValue);
                            audioConversionFile.AudioConversionOutputConfiguration.SamplingRate = Convert.ToString(audioConversionOutputConfigurationWindow.SelectedSamplingRate.SelectedValue);
                            audioConversionFile.AudioConversionOutputConfiguration.AudioBitRate = Convert.ToString(audioConversionOutputConfigurationWindow.SelectedAudioBitRate.SelectedValue);
                            audioConversionFile.AudioConversionOutputConfiguration.SoundTrack = Convert.ToString(audioConversionOutputConfigurationWindow.SelectedSoundTrack.SelectedValue);
                            audioConversionFile.AudioConversionOutputConfiguration.CloseSoundEffect = audioConversionOutputConfigurationWindow.CloseSoundEffect;
                            audioConversionFile.AudioConversionOutputConfiguration.Volume = Convert.ToString(audioConversionOutputConfigurationWindow.SelectedVolume.SelectedValue);
                            audioConversionFile.AudioConversionOutputConfiguration.VariableBitRate = audioConversionOutputConfigurationWindow.IsVariableBitRateSupported ? Convert.ToString(audioConversionOutputConfigurationWindow.SelectedVariableBitRate.SelectedValue) : string.Empty;
                            audioConversionFile.AudioConversionOutputConfiguration.SamplingFormat = Convert.ToString(audioConversionOutputConfigurationWindow.SelectedSamplingFormat);
                            audioConversionFile.AudioConversionOutputConfiguration.AudioFadeInEffect = Convert.ToString(audioConversionOutputConfigurationWindow.SelectedAudioFadeInEffect.SelectedValue);
                            audioConversionFile.AudioConversionOutputConfiguration.AudioFadeOutEffect = Convert.ToString(audioConversionOutputConfigurationWindow.SelectedAudioFadeOutEffect.SelectedValue);
                            audioConversionFile.AudioConversionOutputConfiguration.Echo = audioConversionOutputConfigurationWindow.Echo;
                            audioConversionFile.AudioConversionOutputConfiguration.DeNoise = audioConversionOutputConfigurationWindow.DeNoise;
                            audioConversionFile.AudioConversionOutputConfiguration.Reverse = audioConversionOutputConfigurationWindow.Reverse;
                        }
                    }
                }
                // 文本转音频输出配置
                else if (Equals(SelectedConversionType, AudioConversionTypeCollection[2]))
                {
                    // TODO：未完成
                }
            }
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
                            ConvertConfigurationService.SetConvertedAudioSavePath(OutputFolder);
                            break;
                        }
                    case "Music":
                        {
                            string musicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                            OutputFolder = musicFolder;
                            ConvertConfigurationService.SetConvertedAudioSavePath(OutputFolder);
                            break;
                        }
                    case "Desktop":
                        {
                            OutputFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                            ConvertConfigurationService.SetConvertedAudioSavePath(OutputFolder);
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
                                ConvertConfigurationService.SetConvertedAudioSavePath(OutputFolder);
                            }
                            openFolderDialog.Dispose();
                            break;
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
        }

        #endregion 第二部分：音频转换页面——挂载的事件

        /// <summary>
        /// 对数据进行排序
        /// </summary>
        private List<AudioConversionFileModel> SortData(List<AudioConversionFileModel> audioConversionFileList)
        {
            // 按照文件名称排序
            if (string.Equals(SelectedSortRule, SortRuleList[1]))
            {
                return SortWay ? [.. audioConversionFileList.OrderBy(item => item.FileName)] : [.. audioConversionFileList.OrderByDescending(item => item.FileName)];
            }
            // 按照文件大小排序
            else if (string.Equals(SelectedSortRule, SortRuleList[2]))
            {
                return SortWay ? [.. audioConversionFileList.OrderBy(item => item.FileSize)] : [.. audioConversionFileList.OrderByDescending(item => item.FileSize)];
            }
            // 按照音频持续时长排序
            else if (string.Equals(SelectedSortRule, SortRuleList[3]))
            {
                return SortWay ? [.. audioConversionFileList.OrderBy(item => item.Duration)] : [.. audioConversionFileList.OrderByDescending(item => item.Duration)];
            }
            else
            {
                return audioConversionFileList;
            }
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
                    // 音频格式转换 & 音频合并
                    if (Equals(SelectedConversionType, AudioConversionTypeCollection[0]) || Equals(SelectedConversionType, AudioConversionTypeCollection[1]))
                    {
                        AudioConversionFileModel audioConversionFile = new()
                        {
                            FileName = Path.GetFileName(filePath),
                            FilePath = filePath,
                        };
                        FileInfo fileInfo = new(filePath);
                        audioConversionFile.FileSize = fileInfo.Length;
                        audioConversionFile.FileSizeString = VolumeSizeHelper.ConvertVolumeSizeToString(fileInfo.Length);

                        if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                        {
                            string audioDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, 0, "Duration", InfoKind.Text, InfoKind.Name));
                            if (double.TryParse(audioDuration, out double audioDurationValue))
                            {
                                TimeSpan audioDurationTimeSpan = TimeSpan.FromMilliseconds(audioDurationValue);
                                audioConversionFile.Duration = audioDurationTimeSpan;
                                audioConversionFile.DurationString = string.Format(@"{0:00}:{1:00}:{2:00}", Math.Truncate(audioDurationTimeSpan.TotalHours), audioDurationTimeSpan.Minutes, audioDurationTimeSpan.Minutes);
                            }
                            else
                            {
                                audioConversionFile.Duration = TimeSpan.Zero;
                                audioConversionFile.DurationString = string.IsNullOrEmpty(audioDuration) ? "00:00:00" : audioDuration;
                            }
                            string channel = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, 0, "Channel(s)/String", InfoKind.Text, InfoKind.Name));
                            audioConversionFile.Channel = string.IsNullOrEmpty(channel) ? "0" : channel;
                            string samplingRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, 0, "SamplingRate/String", InfoKind.Text, InfoKind.Name));
                            audioConversionFile.SamplingRate = string.IsNullOrEmpty(samplingRate) ? "0" : samplingRate;
                            string bitRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, 0, "BitRate/String", InfoKind.Text, InfoKind.Name));
                            audioConversionFile.BitRate = string.IsNullOrEmpty(bitRate) ? "0" : bitRate;
                            MediaInfoLibrary.MediaInfo_Close(handle);
                            MediaInfoLibrary.MediaInfo_Delete(handle);
                        }

                        audioConversionFile.AudioConversionOutputConfiguration = new()
                        {
                            AudioConversionTypeKind = SelectedConversionType.AudioConversionTypeKind,

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

                        audioConversionFile.CutAudio = new()
                        {
                            StartTime = TimeSpan.Zero,
                            EndTime = TimeSpan.Zero,
                            AudioCoverFilePath = string.Empty
                        };

                        return audioConversionFile;
                    }
                    // 文本转语音
                    else if (Equals(SelectedConversionType, AudioConversionTypeCollection[2]))
                    {
                        // TODO：未完成
                        return null;
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
    }
}
