using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
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
    /// 图片列表页面
    /// </summary>
    public sealed partial class PhotoListPage : Page, INotifyPropertyChanged
    {
        private readonly string NoFolderString = ResourceService.PhotoListResource.GetString("NoFolder");
        private readonly string PhotoFormatConversionString = ResourceService.PhotoListResource.GetString("PhotoFormatConversion");
        private readonly string PhotoFormatConversionDragOverContentString = ResourceService.PhotoListResource.GetString("PhotoFormatConversionDragOverContent");
        private readonly string SelectFileString = ResourceService.PhotoListResource.GetString("SelectFile");
        private readonly string SelectFolderString = ResourceService.PhotoListResource.GetString("SelectFolder");
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

        public List<string> SortRuleList { get; } = ["NotSort", "SortByFileName", "SortByFileSize", "SortByWidth", "SortByHeight"];

        public WinRTObservableCollection<PhotoConversionTypeModel> PhotoConversionTypeCollection { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public PhotoListPage()
        {
            InitializeComponent();
            PhotoConversionTypeCollection.Add(new PhotoConversionTypeModel
            {
                PhotoConversionType = PhotoFormatConversionString,
                PhotoConversionIcon = "\uE895",
                PhotoConversionTypeKind = PhotoConversionTypeKind.PhotoFormatConversion,
                PhotoFormatConversion = new()
            });
            SelectedItem = PhotoListSelectorBar.Items[0];
            SelectedConversionType = PhotoConversionTypeCollection[0];
            SelectedSortRule = SortRuleList[0];
            SortWay = true;
        }

        #region 第一部分：ExecuteCommand 命令调用时挂载的事件

        /// <summary>
        /// 移除选中项
        /// </summary>
        private void OnRemoveExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            // 图片格式转换
            if (SelectedConversionType.PhotoConversionTypeKind is PhotoConversionTypeKind.PhotoFormatConversion)
            {
                SelectedConversionType.PhotoFormatConversion.PhotoFormatConversionFileCollection.Remove(args.Parameter as PhotoFormatConversionFileModel);
            }
        }

        /// <summary>
        /// 配置选中项转换参数
        /// </summary>
        private async void OnOutputConfigurationExecuteRequested(object sender, ExecuteRequestedEventArgs args)
        {
            if (MainWindow.Current.GetFrameContent() is PhotoConversionPage photoConversionPage)
            {
                // 图片格式转换
                if (args.Parameter is PhotoFormatConversionFileModel photoFormatConversionFile && photoFormatConversionFile.PhotoConversionOutputConfiguration is not null)
                {
                    photoConversionPage.NavigateTo(photoConversionPage.PageList[1], new PhotoConversionNavigationParameter()
                    {
                        PhotoConversionTypeKind = PhotoConversionTypeKind.PhotoFormatConversion,
                        IsGlobalSettings = false,
                        PhotoConversionData = photoFormatConversionFile
                    }, true);
                }
            }
        }

        #endregion 第一部分：ExecuteCommand 命令调用时挂载的事件

        #region 第二部分：图片列表页面——挂载的事件

        /// <summary>
        /// 设置拖动的数据的可视表示形式
        /// </summary>
        private async void OnPhotoListDragEnter(object sender, Microsoft.UI.Xaml.DragEventArgs args)
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
                    if (SelectedConversionType.PhotoConversionTypeKind is PhotoConversionTypeKind.PhotoFormatConversion)
                    {
                        args.AcceptedOperation = DataPackageOperation.Copy;
                        args.DragUIOverride.IsCaptionVisible = true;
                        args.DragUIOverride.IsContentVisible = false;
                        args.DragUIOverride.IsGlyphVisible = true;
                        args.DragUIOverride.Caption = PhotoFormatConversionDragOverContentString;
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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionPage), nameof(OnPhotoListDragEnter), 1, e);
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
        private async void OnPhotoListDrop(object sender, Microsoft.UI.Xaml.DragEventArgs args)
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
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoListPage), nameof(OnPhotoListDrop), 1, e);
                    }

                    return null;
                });
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionPage), nameof(OnPhotoListDrop), 2, e);
            }
            finally
            {
                dragOperationDeferral.Complete();
            }

            if (fileList is not null && fileList.Count > 0)
            {
                await AddPhotoDataAsync(fileList);
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
                if (PhotoListScrollViewer.HorizontalOffset <= 0)
                {
                    IsPreviousEnabled = false;
                    IsNextEnabled = true;
                }
                else if (PhotoListScrollViewer.HorizontalOffset >= PhotoListScrollViewer.ScrollableWidth)
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
            canScrollHorizontally = PhotoListScrollViewer.ExtentWidth > PhotoListScrollViewer.ViewportWidth;
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
                if (PhotoListScrollViewer.HorizontalOffset <= 0)
                {
                    IsPreviousEnabled = false;
                    IsNextEnabled = true;
                }
                else if (PhotoListScrollViewer.HorizontalOffset >= PhotoListScrollViewer.ScrollableWidth)
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
            PhotoListScrollViewer.ChangeView(PhotoListScrollViewer.HorizontalOffset < 150 ? 0 : PhotoListScrollViewer.HorizontalOffset - 150, null, null);
        }

        /// <summary>
        /// 向后移动
        /// </summary>
        private void OnNextClick(object sender, RoutedEventArgs args)
        {
            PhotoListScrollViewer.ChangeView(PhotoListScrollViewer.HorizontalOffset >= PhotoListScrollViewer.ScrollableWidth - 150 ? PhotoListScrollViewer.ScrollableWidth : PhotoListScrollViewer.HorizontalOffset + 150, null, null);
        }

        /// <summary>
        /// 图片转换选择器栏选中项发生变化时触发的事件
        /// </summary>
        private async void OnSelectorBarSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            SelectedItem = sender.SelectedItem;
            SelectedConversionType = PhotoConversionTypeCollection[sender.Items.IndexOf(SelectedItem)];
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
            // 图片格式转换
            if (SelectedConversionType.PhotoConversionTypeKind is PhotoConversionTypeKind.PhotoFormatConversion)
            {
                SelectedConversionType.PhotoFormatConversion.PhotoFormatConversionFileCollection.Clear();
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
                await AddPhotoDataAsync([.. openFileDialog.FileNames]);
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
                await AddPhotoDataAsync(fileList);
                IsGettingFileInformation = false;
            }
            openFolderDialog.Dispose();
        }

        /// <summary>
        /// 打开输出配置
        /// </summary>
        private async void OnOutputConfigurationClicked(object sender, RoutedEventArgs args)
        {
            if (MainWindow.Current.GetFrameContent() is PhotoConversionPage photoConversionPage)
            {
                // 图片格式转换
                if (SelectedConversionType.PhotoConversionTypeKind is PhotoConversionTypeKind.PhotoFormatConversion)
                {
                    photoConversionPage.NavigateTo(photoConversionPage.PageList[1], new PhotoConversionNavigationParameter()
                    {
                        PhotoConversionTypeKind = PhotoConversionTypeKind.PhotoFormatConversion,
                        IsGlobalSettings = true,
                        PhotoConversionData = PhotoConversionTypeCollection[0].PhotoFormatConversion.PhotoFormatConversionFileCollection.ToList()
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

        #endregion 第二部分：图片列表页面——挂载的事件

        /// <summary>
        /// 添加图片数据
        /// </summary>
        private async Task AddPhotoDataAsync(List<string> fileList)
        {
            // 图片格式转换
            if (SelectedConversionType.PhotoConversionTypeKind is PhotoConversionTypeKind.PhotoFormatConversion)
            {
                List<PhotoFormatConversionFileModel> photoFormatConversionFileList = [.. SelectedConversionType.PhotoFormatConversion.PhotoFormatConversionFileCollection];
                List<Task> taskList = [];
                object fileInformationLock = new();
                foreach (string file in fileList)
                {
                    taskList.Add(Task.Run(async () =>
                    {
                        if (GetFileInformation(file) is PhotoFormatConversionFileModel photoFormatConversionFile)
                        {
                            lock (fileInformationLock)
                            {
                                photoFormatConversionFileList.Add(photoFormatConversionFile);
                            }
                        }
                    }));
                }
                await Task.WhenAll(taskList);
                List<PhotoFormatConversionFileModel> sortedPhotoFormatConversionFileList = SortPhotoFormatConversionFileData(photoFormatConversionFileList);
                SelectedConversionType.PhotoFormatConversion.PhotoFormatConversionFileCollection.Clear();
                foreach (PhotoFormatConversionFileModel sortedPhotoFormatConversionFile in sortedPhotoFormatConversionFileList)
                {
                    sortedPhotoFormatConversionFile.FileThumbnailSource ??= GetThumbnail(sortedPhotoFormatConversionFile.FilePath);
                    SelectedConversionType.PhotoFormatConversion.PhotoFormatConversionFileCollection.Add(sortedPhotoFormatConversionFile);
                }
            }
        }

        /// <summary>
        /// 对数据进行排序
        /// </summary>
        private async Task SortDataAsync()
        {
            // 图片格式转换
            if (SelectedConversionType.PhotoConversionTypeKind is PhotoConversionTypeKind.PhotoFormatConversion)
            {
                List<PhotoFormatConversionFileModel> sortedPhotoFormatConversionFileList = await Task.Run(() =>
                {
                    List<PhotoFormatConversionFileModel> sortedPhotoFormatConversionFileList = [.. SelectedConversionType.PhotoFormatConversion.PhotoFormatConversionFileCollection];
                    return SortPhotoFormatConversionFileData(sortedPhotoFormatConversionFileList);
                });
                SelectedConversionType.PhotoFormatConversion.PhotoFormatConversionFileCollection.Clear();
                foreach (PhotoFormatConversionFileModel sortedPhotoFormatConversionFile in sortedPhotoFormatConversionFileList)
                {
                    SelectedConversionType.PhotoFormatConversion.PhotoFormatConversionFileCollection.Add(sortedPhotoFormatConversionFile);
                }
            }
        }

        /// <summary>
        /// 对数据进行排序
        /// </summary>
        private List<PhotoFormatConversionFileModel> SortPhotoFormatConversionFileData(List<PhotoFormatConversionFileModel> photoConversionFileList)
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
        private PhotoFormatConversionFileModel GetFileInformation(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    // 图片格式转换
                    if (SelectedConversionType.PhotoConversionTypeKind is PhotoConversionTypeKind.PhotoFormatConversion)
                    {
                        PhotoFormatConversionFileModel photoFormatConversionFile = new()
                        {
                            FileName = Path.GetFileName(filePath),
                            FilePath = filePath,
                        };
                        FileInfo fileInfo = new(filePath);
                        photoFormatConversionFile.FileSize = fileInfo.Length;

                        if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                        {
                            string width = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Image, 0, "Width", InfoKind.Text, InfoKind.Name));
                            photoFormatConversionFile.ImageWidth = double.TryParse(width, out double widthValue) ? Convert.ToInt32(widthValue) : 0;
                            string height = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Image, 0, "Height", InfoKind.Text, InfoKind.Name));
                            photoFormatConversionFile.ImageHeight = double.TryParse(height, out double heightValue) ? Convert.ToInt32(heightValue) : 0;

                            MediaInfoLibrary.MediaInfo_Close(handle);
                            MediaInfoLibrary.MediaInfo_Delete(handle);
                        }

                        photoFormatConversionFile.PhotoConversionOutputConfiguration = new()
                        {
                            FormatConversionType = ".jpg",
                            IsImageCropped = false,
                            ImageWidth = photoFormatConversionFile.ImageWidth,
                            ImageHeight = photoFormatConversionFile.ImageHeight,
                            XCoordinate = 0,
                            YCoordinate = 0,
                            ClipWidth = photoFormatConversionFile.ImageWidth,
                            ClipHeight = photoFormatConversionFile.ImageHeight,
                            AdjustPhoto = false,
                            ContrastRatio = 1,
                            Brightness = 0,
                            Saturation = 1,
                            ColorTemperature = 6500,
                            Hue = 0,
                            Blur = 0,
                            GrayScale = false,
                            Reversal = false
                        };

                        return photoFormatConversionFile;
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
