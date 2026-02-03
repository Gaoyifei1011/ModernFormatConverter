using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Helpers.Root;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.WindowsAPI.PInvoke.Shell32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
    /// 文件信息界面
    /// </summary>
    public sealed partial class FileInformationPage : Page, INotifyPropertyChanged
    {
        private readonly string DragOverContentString = ResourceService.FileInformationResource.GetString("DragOverContent");
        private readonly string NoMultiFileString = ResourceService.FileInformationResource.GetString("NoMultiFile");
        private readonly string ParsingFileInformationString = ResourceService.FileInformationResource.GetString("ParsingFileInformation");
        private readonly string SelectFileString = ResourceService.FileInformationResource.GetString("SelectFile");
        private string filePath;

        private FileInformationResultKind _fileInformationResultKind;

        public FileInformationResultKind FileInformationResultKind
        {
            get { return _fileInformationResultKind; }

            set
            {
                if (!Equals(_fileInformationResultKind, value))
                {
                    _fileInformationResultKind = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileInformationResultKind)));
                }
            }
        }

        private string _fileInformationFailedContent;

        public string FileInformationFailedContent
        {
            get { return _fileInformationFailedContent; }

            set
            {
                if (!string.Equals(_fileInformationFailedContent, value))
                {
                    _fileInformationFailedContent = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileInformationFailedContent)));
                }
            }
        }

        private ImageSource _fileThumbnailImage;

        public ImageSource FileThumbnailImage
        {
            get { return _fileThumbnailImage; }

            set
            {
                if (!Equals(_fileThumbnailImage, value))
                {
                    _fileThumbnailImage = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileThumbnailImage)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public FileInformationPage()
        {
            InitializeComponent();
        }

        #region 第一部分：重写父类事件

        /// <summary>
        /// 设置拖动的数据的可视表示形式
        /// </summary>
        protected override async void OnDragEnter(Microsoft.UI.Xaml.DragEventArgs args)
        {
            base.OnDragEnter(args);
            DragOperationDeferral dragOperationDeferral = args.GetDeferral();

            try
            {
                if (FileInformationResultKind is FileInformationResultKind.Parsing)
                {
                    args.AcceptedOperation = DataPackageOperation.None;
                    args.DragUIOverride.IsCaptionVisible = true;
                    args.DragUIOverride.IsContentVisible = false;
                    args.DragUIOverride.IsGlyphVisible = true;
                    args.DragUIOverride.Caption = ParsingFileInformationString;
                }
                else
                {
                    IReadOnlyList<IStorageItem> dragItemsList = await args.DataView.GetStorageItemsAsync();

                    if (dragItemsList.Count is 1)
                    {
                        args.AcceptedOperation = DataPackageOperation.Copy;
                        args.DragUIOverride.IsCaptionVisible = true;
                        args.DragUIOverride.IsContentVisible = false;
                        args.DragUIOverride.IsGlyphVisible = true;
                        args.DragUIOverride.Caption = DragOverContentString;
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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(FileInformationPage), nameof(OnDragOver), 1, e);
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
        protected override async void OnDrop(Microsoft.UI.Xaml.DragEventArgs args)
        {
            base.OnDrop(args);
            DragOperationDeferral dragOperationDeferral = args.GetDeferral();
            filePath = string.Empty;

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
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(FileInformationPage), nameof(OnDrop), 1, e);
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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(FileInformationPage), nameof(OnDrop), 2, e);
            }
            finally
            {
                dragOperationDeferral.Complete();
            }

            if (File.Exists(filePath))
            {
                await GetFileInformationAsync(filePath);
            }
        }

        #endregion 第一部分：重写父类事件

        #region 第二部分：文件信息页面——挂载的事件

        /// <summary>
        /// 打开本地文件
        /// </summary>
        private async void OnOpenFileClicked(object sender, RoutedEventArgs args)
        {
            OpenFileDialog openFileDialog = new()
            {
                Multiselect = false,
                Title = SelectFileString
            };
            if (openFileDialog.ShowDialog() is DialogResult.OK && !string.IsNullOrEmpty(openFileDialog.FileName))
            {
                await GetFileInformationAsync(filePath);
            }
            openFileDialog.Dispose();
        }

        /// <summary>
        /// 打开文件属性页面
        /// </summary>
        private void OnFilePropertiesClicked(object sender, RoutedEventArgs args)
        {
            if (File.Exists(filePath))
            {
                Task.Run(() =>
                {
                    try
                    {
                        StringCollection stringCollection = [filePath];
                        DataObject data = new();
                        data.SetData("Preferred DropEffect", true, new MemoryStream([5, 0, 0, 0]));
                        data.SetData("Shell IDList Array", true, CreateShellIDList(stringCollection));
                        data.SetFileDropList(stringCollection);
                        Shell32Library.SHMultiFileProperties(data, 0);
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(FileInformationPage), nameof(OnFilePropertiesClicked), 1, e);
                    }
                });
            }
        }

        /// <summary>
        /// 文件定位
        /// </summary>
        private void OnOpenFileLocationClicked(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        if (File.Exists(filePath))
                        {
                            nint pidlList = Shell32Library.ILCreateFromPath(filePath);
                            if (pidlList is not 0)
                            {
                                Shell32Library.SHOpenFolderAndSelectItems(pidlList, 0, 0, 0);
                                Shell32Library.ILFree(pidlList);
                            }
                        }
                        else
                        {
                            string directoryPath = Path.GetDirectoryName(filePath);

                            if (Directory.Exists(directoryPath))
                            {
                                Process.Start(directoryPath);
                            }
                            else
                            {
                                Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(FileInformationPage), nameof(OnOpenFileLocationClicked), 1, e);
                }
            });
        }

        /// <summary>
        /// 复制到剪贴板
        /// </summary>
        private void OnCopyClipboardClicked(object sender, RoutedEventArgs args)
        {
            // TODO：未完成
        }

        /// <summary>
        /// 获取文件信息解析是否成功
        /// </summary>
        private Visibility GetFileInformationSuccessfullyState(FileInformationResultKind fileInformationResultKind, bool isSuccessfully)
        {
            return isSuccessfully ? (fileInformationResultKind is FileInformationResultKind.File || fileInformationResultKind is FileInformationResultKind.VideoFile || fileInformationResultKind is FileInformationResultKind.MusicFile || fileInformationResultKind is FileInformationResultKind.DocumentFile) ? Visibility.Visible : Visibility.Collapsed : (fileInformationResultKind is FileInformationResultKind.File || fileInformationResultKind is FileInformationResultKind.VideoFile || fileInformationResultKind is FileInformationResultKind.MusicFile || fileInformationResultKind is FileInformationResultKind.DocumentFile) ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// 检查文件信息解析是否成功
        /// </summary>
        private Visibility CheckFileInformationState(FileInformationResultKind fileInformationResultKind, FileInformationResultKind comparedFileInformationResultKind)
        {
            return Equals(fileInformationResultKind, comparedFileInformationResultKind) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 获取是否正在解析中
        /// </summary>
        private bool GetIsParsing(FileInformationResultKind fileInformationReusltKind)
        {
            return fileInformationReusltKind is not FileInformationResultKind.Parsing;
        }

        #endregion 第二部分：文件信息页面——挂载的事件

        /// <summary>
        /// 获取文件信息
        /// </summary>
        private async Task GetFileInformationAsync(string filePath)
        {
            FileInformationResultKind = FileInformationResultKind.Parsing;
            await GetThumbnailAsync(filePath);
            FileInformationResultKind = FileInformationResultKind.File;
        }

        /// <summary>
        /// 获取文件缩略图
        /// </summary>
        private async Task GetThumbnailAsync(string filePath)
        {
            MemoryStream memoryStream = null;
            try
            {
                Bitmap thumbnailBitmap = ThumbnailHelper.GetThumbnailBitmap(filePath, 300);

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
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(FileInformationPage), nameof(GetThumbnailAsync), 1, e);
            }

            if (memoryStream is not null)
            {
                try
                {
                    BitmapImage bitmapImage = new();
                    bitmapImage.SetSource(memoryStream.AsRandomAccessStream());
                    FileThumbnailImage = bitmapImage;
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(FileInformationPage), nameof(GetThumbnailAsync), 2, e);
                }
                finally
                {
                    memoryStream?.Dispose();
                }
            }
        }

        private static MemoryStream CreateShellIDList(StringCollection fileNameCollection)
        {
            int pos = 0;
            byte[][] pidls = new byte[fileNameCollection.Count][];
            foreach (object filename in fileNameCollection)
            {
                nint pidl = Shell32Library.ILCreateFromPath(filename.ToString());
                int pidlSize = Shell32Library.ILGetSize(pidl);
                pidls[pos] = new byte[pidlSize];
                Marshal.Copy(pidl, pidls[pos++], 0, pidlSize);
                Shell32Library.ILFree(pidl);
            }

            int pidlOffset = 4 * (fileNameCollection.Count + 2);
            MemoryStream memoryStream = new();
            BinaryWriter binaryWriter = new(memoryStream);
            binaryWriter.Write(fileNameCollection.Count);
            binaryWriter.Write(pidlOffset);
            pidlOffset += 4;
            foreach (byte[] pidl in pidls)
            {
                binaryWriter.Write(pidlOffset);
                pidlOffset += pidl.Length;
            }

            binaryWriter.Write(0);
            foreach (byte[] pidl in pidls)
            {
                binaryWriter.Write(pidl);
            }

            return memoryStream;
        }
    }
}
