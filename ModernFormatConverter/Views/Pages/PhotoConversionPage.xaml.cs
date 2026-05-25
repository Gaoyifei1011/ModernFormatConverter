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
    /// 图片转换页面
    /// TODO：未完成
    /// </summary>
    public sealed partial class PhotoConversionPage : Page, INotifyPropertyChanged
    {
        private readonly string DragOverContentString = ResourceService.PhotoConversionResource.GetString("DragOverContent");
        private readonly string NoFolderString = ResourceService.PhotoConversionResource.GetString("NoFolder");
        private readonly string PhotoFormatConversionString = ResourceService.PhotoConversionResource.GetString("PhotoFormatConversion");
        private readonly string SelectFileString = ResourceService.PhotoConversionResource.GetString("SelectFile");
        private readonly string SelectFolderString = ResourceService.PhotoConversionResource.GetString("SelectFolder");
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;

        private PhotoConversionTypeModel _selectedConversionType;

        public PhotoConversionTypeModel SelectedConversionType
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

        public List<string> SortRuleList { get; } = ["NotSort", "SortByFileName", "SortByFileSize", "SortByWidth", "SortByHeight"];

        public WinRTObservableCollection<PhotoConversionTypeModel> PhotoConversionTypeCollection { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public PhotoConversionPage()
        {
            InitializeComponent();
            PhotoConversionTypeCollection.Add(new PhotoConversionTypeModel
            {
                PhotoConversionType = PhotoFormatConversionString,
                PhotoConversionIcon = "\uE895",
                PhotoConversionTypeKind = PhotoConversionTypeKind.PhotoFormatConversion
            });
            SelectedConversionType = PhotoConversionTypeCollection[0];
            SelectedSortRule = SortRuleList[0];
            SortWay = true;
            OutputFolder = ConvertConfigurationService.ConvertedPhotoSavePath;
        }

        #region 第一部分：ExecuteCommand 命令调用时挂载的事件

        /// <summary>
        /// 移除选中项
        /// </summary>
        private void OnRemoveExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            SelectedConversionType.PhotoConversionFileCollection.Remove(args.Parameter as PhotoConversionFileModel);
        }

        /// <summary>
        /// 配置选中项转换参数
        /// </summary>
        private async void OnOutputConfigurationExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (args.Parameter is PhotoConversionFileModel photoConversionFile)
            {
                // 图片格式转换输出配置
                if (Equals(SelectedConversionType, PhotoConversionTypeCollection[0]) || Equals(SelectedConversionType, PhotoConversionTypeCollection[1]))
                {
                    PhotoConversionOutputConfigurationWindow photoConversionOutputConfigurationWindow = new(ConversionToolsWindow.Current, photoConversionFile);
                    if (await photoConversionOutputConfigurationWindow.ShowAsync() is ContentDialogResult.Primary && SelectedConversionType.PhotoConversionTypeKind is PhotoConversionTypeKind.PhotoFormatConversion)
                    {
                        photoConversionFile.PhotoConversionOutputConfiguration.FormatConversionType = Convert.ToString(photoConversionOutputConfigurationWindow.SelectedFormatConversionType.SelectedValue);
                        photoConversionFile.PhotoConversionOutputConfiguration.IsImageCropped = photoConversionOutputConfigurationWindow.IsImageCropped;
                        photoConversionFile.PhotoConversionOutputConfiguration.ImageWidth = photoConversionOutputConfigurationWindow.ImageWidth;
                        photoConversionFile.PhotoConversionOutputConfiguration.ImageHeight = photoConversionOutputConfigurationWindow.ImageHeight;
                        photoConversionFile.PhotoConversionOutputConfiguration.XCoordinate = photoConversionOutputConfigurationWindow.XCoordinate;
                        photoConversionFile.PhotoConversionOutputConfiguration.YCoordinate = photoConversionOutputConfigurationWindow.YCoordinate;
                        photoConversionFile.PhotoConversionOutputConfiguration.ClipWidth = photoConversionOutputConfigurationWindow.ClipWidth;
                        photoConversionFile.PhotoConversionOutputConfiguration.ClipHeight = photoConversionOutputConfigurationWindow.ClipHeight;
                        photoConversionFile.PhotoConversionOutputConfiguration.ConstrastRatio = photoConversionOutputConfigurationWindow.ConstrastRatio;
                        photoConversionFile.PhotoConversionOutputConfiguration.Exposure = photoConversionOutputConfigurationWindow.Exposure;
                        photoConversionFile.PhotoConversionOutputConfiguration.Saturation = photoConversionOutputConfigurationWindow.Saturation;
                        photoConversionFile.PhotoConversionOutputConfiguration.ColorTemperature = photoConversionOutputConfigurationWindow.ColorTemperature;
                        photoConversionFile.PhotoConversionOutputConfiguration.Tone = photoConversionOutputConfigurationWindow.Tone;
                        photoConversionFile.PhotoConversionOutputConfiguration.Blur = photoConversionOutputConfigurationWindow.Blur;
                        photoConversionFile.PhotoConversionOutputConfiguration.GrayScale = photoConversionOutputConfigurationWindow.GrayScale;
                        photoConversionFile.PhotoConversionOutputConfiguration.Reversal = photoConversionOutputConfigurationWindow.Reversal;
                    }
                }
            }
        }

        #endregion 第一部分：ExecuteCommand 命令调用时挂载的事件

        #region 第二部分：图片转换页面——挂载的事件

        /// <summary>
        /// 设置拖动的数据的可视表示形式
        /// </summary>
        private async void OnPhotoConversionDragEnter(object sender, Microsoft.UI.Xaml.DragEventArgs args)
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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionPage), nameof(OnPhotoConversionDragEnter), 1, e);
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
        private async void OnPhotoConversionDrop(object sender, Microsoft.UI.Xaml.DragEventArgs args)
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
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionPage), nameof(OnPhotoConversionDrop), 1, e);
                    }

                    return null;
                });
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionPage), nameof(OnPhotoConversionDrop), 2, e);
            }
            finally
            {
                dragOperationDeferral.Complete();
            }

            if (fileList is not null && fileList.Count > 0)
            {
                IsGettingFileInformation = true;
                List<PhotoConversionFileModel> photoConversionFileList = [.. SelectedConversionType.PhotoConversionFileCollection];
                List<Task> taskList = [];
                object fileInformationLock = new();
                foreach (IStorageItem file in fileList)
                {
                    taskList.Add(Task.Run(async () =>
                    {
                        if (GetFileInformation(file.Path) is PhotoConversionFileModel photoConversionFile)
                        {
                            lock (fileInformationLock)
                            {
                                photoConversionFileList.Add(photoConversionFile);
                            }
                        }
                    }));
                }
                await Task.WhenAll(taskList);
                List<PhotoConversionFileModel> sortedPhotoConversionFileList = SortData(photoConversionFileList);
                SelectedConversionType.PhotoConversionFileCollection.Clear();
                foreach (PhotoConversionFileModel sortedPhotoConversionFile in sortedPhotoConversionFileList)
                {
                    sortedPhotoConversionFile.FileThumbnailSource = GetThumbnail(sortedPhotoConversionFile.FilePath);
                    SelectedConversionType.PhotoConversionFileCollection.Add(sortedPhotoConversionFile);
                }
                IsGettingFileInformation = false;
            }
        }

        /// <summary>
        /// 图片转换列表选中项发生变化时触发的事件
        /// </summary>
        private async void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            SelectedConversionType = args.SelectedItem as PhotoConversionTypeModel;
            IsGettingFileInformation = true;
            List<PhotoConversionFileModel> sortedPhotoConversionFileList = await Task.Run(() =>
            {
                List<PhotoConversionFileModel> photoConversionFileList = [.. SelectedConversionType.PhotoConversionFileCollection];
                return SortData(photoConversionFileList);
            });
            SelectedConversionType.PhotoConversionFileCollection.Clear();
            foreach (PhotoConversionFileModel sortedPhotoConversionFile in sortedPhotoConversionFileList)
            {
                SelectedConversionType.PhotoConversionFileCollection.Add(sortedPhotoConversionFile);
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
                List<PhotoConversionFileModel> sortedPhotoConversionFileList = await Task.Run(() =>
                {
                    List<PhotoConversionFileModel> photoConversionFileList = [.. SelectedConversionType.PhotoConversionFileCollection];
                    return SortData(photoConversionFileList);
                });
                SelectedConversionType.PhotoConversionFileCollection.Clear();
                foreach (PhotoConversionFileModel sortedPhotoConversionFile in sortedPhotoConversionFileList)
                {
                    SelectedConversionType.PhotoConversionFileCollection.Add(sortedPhotoConversionFile);
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
                List<PhotoConversionFileModel> sortedPhotoConversionFileList = await Task.Run(() =>
                {
                    List<PhotoConversionFileModel> photoConversionFileList = [.. SelectedConversionType.PhotoConversionFileCollection];
                    return SortData(photoConversionFileList);
                });
                SelectedConversionType.PhotoConversionFileCollection.Clear();
                foreach (PhotoConversionFileModel sortedPhotoConversionFile in sortedPhotoConversionFileList)
                {
                    SelectedConversionType.PhotoConversionFileCollection.Add(sortedPhotoConversionFile);
                }
                IsGettingFileInformation = false;
            }
        }

        /// <summary>
        /// 清空
        /// </summary>
        private void OnClearClicked(object sender, RoutedEventArgs args)
        {
            SelectedConversionType.PhotoConversionFileCollection.Clear();
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
                List<PhotoConversionFileModel> photoConversionFileList = [.. SelectedConversionType.PhotoConversionFileCollection];
                List<Task> taskList = [];
                object fileInformationLock = new();
                foreach (string filePath in openFileDialog.FileNames)
                {
                    taskList.Add(Task.Run(async () =>
                    {
                        if (GetFileInformation(filePath) is PhotoConversionFileModel photoConversionFile)
                        {
                            lock (fileInformationLock)
                            {
                                photoConversionFileList.Add(photoConversionFile);
                            }
                        }
                    }));
                }
                await Task.WhenAll(taskList);
                List<PhotoConversionFileModel> sortedPhotoConversionFileList = SortData(photoConversionFileList);
                SelectedConversionType.PhotoConversionFileCollection.Clear();
                foreach (PhotoConversionFileModel sortedPhotoConversionFile in sortedPhotoConversionFileList)
                {
                    sortedPhotoConversionFile.FileThumbnailSource = GetThumbnail(sortedPhotoConversionFile.FilePath);
                    SelectedConversionType.PhotoConversionFileCollection.Add(sortedPhotoConversionFile);
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
                List<PhotoConversionFileModel> photoConversionFileList = [.. SelectedConversionType.PhotoConversionFileCollection];
                string[] filePathArray = Directory.GetFiles(openFolderDialog.SelectedPath);
                List<Task> taskList = [];
                object fileInformationLock = new();
                foreach (string filePath in filePathArray)
                {
                    taskList.Add(Task.Run(async () =>
                    {
                        if (GetFileInformation(filePath) is PhotoConversionFileModel photoConversionFile)
                        {
                            lock (fileInformationLock)
                            {
                                photoConversionFileList.Add(photoConversionFile);
                            }
                        }
                    }));
                }
                await Task.WhenAll(taskList);
                List<PhotoConversionFileModel> sortedPhotoConversionFileList = SortData(photoConversionFileList);
                SelectedConversionType.PhotoConversionFileCollection.Clear();
                foreach (PhotoConversionFileModel sortedPhotoConversionFile in sortedPhotoConversionFileList)
                {
                    sortedPhotoConversionFile.FileThumbnailSource = GetThumbnail(sortedPhotoConversionFile.FilePath);
                    SelectedConversionType.PhotoConversionFileCollection.Add(sortedPhotoConversionFile);
                }
                IsGettingFileInformation = false;
            }
            openFolderDialog.Dispose();
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
                    case "Photo":
                        {
                            string musicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
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
            ConversionToolsWindow.Current.CloseWindow(ContentDialogResult.Primary);
        }

        #endregion 第二部分：图片转换页面——挂载的事件

        /// <summary>
        /// 对数据进行排序
        /// </summary>
        private List<PhotoConversionFileModel> SortData(List<PhotoConversionFileModel> photoConversionFileList)
        {
            // 按照文件名称排序
            if (string.Equals(SelectedSortRule, SortRuleList[1]))
            {
                return SortWay ? [.. photoConversionFileList.OrderBy(item => item.FileName)] : [.. photoConversionFileList.OrderByDescending(item => item.FileName)];
            }
            // 按照文件大小排序
            else if (string.Equals(SelectedSortRule, SortRuleList[2]))
            {
                return SortWay ? [.. photoConversionFileList.OrderBy(item => item.FileSize)] : [.. photoConversionFileList.OrderByDescending(item => item.FileSize)];
            }
            // 按照图片宽度排序
            else if (string.Equals(SelectedSortRule, SortRuleList[3]))
            {
                return SortWay ? [.. photoConversionFileList.OrderBy(item => item.ImageWidth)] : [.. photoConversionFileList.OrderByDescending(item => item.ImageWidth)];
            }
            // 按照图片高度排序
            else if (string.Equals(SelectedSortRule, SortRuleList[4]))
            {
                return SortWay ? [.. photoConversionFileList.OrderBy(item => item.ImageHeight)] : [.. photoConversionFileList.OrderByDescending(item => item.ImageHeight)];
            }
            else
            {
                return photoConversionFileList;
            }
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        private PhotoConversionFileModel GetFileInformation(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    // 图片格式转换
                    if (Equals(SelectedConversionType, PhotoConversionTypeCollection[0]))
                    {
                        PhotoConversionFileModel photoConversionFile = new()
                        {
                            FileName = Path.GetFileName(filePath),
                            FilePath = filePath,
                        };
                        FileInfo fileInfo = new(filePath);
                        photoConversionFile.FileSize = fileInfo.Length;
                        photoConversionFile.FileSizeString = VolumeSizeHelper.ConvertVolumeSizeToString(fileInfo.Length);

                        if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                        {
                            string width = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Image, 0, "Width", InfoKind.Text, InfoKind.Name));
                            photoConversionFile.ImageWidth = double.TryParse(width, out double widthValue) ? Convert.ToInt32(widthValue) : 0;
                            string height = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Image, 0, "Height", InfoKind.Text, InfoKind.Name));
                            photoConversionFile.ImageHeight = double.TryParse(height, out double heightValue) ? Convert.ToInt32(heightValue) : 0;

                            MediaInfoLibrary.MediaInfo_Close(handle);
                            MediaInfoLibrary.MediaInfo_Delete(handle);
                        }

                        photoConversionFile.PhotoConversionOutputConfiguration = new()
                        {
                            FormatConversionType = ".jpg",
                            IsImageCropped = false,
                            ImageWidth = photoConversionFile.ImageWidth,
                            ImageHeight = photoConversionFile.ImageHeight,
                            XCoordinate = 0,
                            YCoordinate = 0,
                            ClipWidth = photoConversionFile.ImageWidth,
                            ClipHeight = photoConversionFile.ImageHeight,
                            ConstrastRatio = 0,
                            Exposure = 0,
                            Saturation = 1,
                            ColorTemperature = 0,
                            Tone = 0,
                            Blur = 0,
                            GrayScale = false,
                            Reversal = false
                        };

                        return photoConversionFile;
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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionPage), nameof(GetFileInformation), 1, e);
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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionPage), nameof(GetThumbnail), 1, e);
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
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionPage), nameof(GetThumbnail), 2, e);
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
