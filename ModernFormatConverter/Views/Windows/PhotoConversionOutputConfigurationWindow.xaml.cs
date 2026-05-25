using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Content;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using ModernFormatConverter.Extensions.Backdrop;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Services.Settings;
using ModernFormatConverter.WindowsAPI.PInvoke.Comctl32;
using ModernFormatConverter.WindowsAPI.PInvoke.User32;
using ModernFormatConverter.WindowsAPI.PInvoke.Uxtheme;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI;

// 抑制 CA1806，CA1822，IDE0060 警告
#pragma warning disable CA1806,CA1822,IDE0060

namespace ModernFormatConverter.Views.Windows
{
    /// <summary>
    /// 图片转换输出配置窗口
    /// </summary>
    public sealed partial class PhotoConversionOutputConfigurationWindow : Window, INotifyPropertyChanged
    {
        private readonly string CustomString = ResourceService.PhotoConversionOutputConfigurationResource.GetString("Custom");
        private readonly string FourToThreeString = ResourceService.PhotoConversionOutputConfigurationResource.GetString("FourToThree");
        private readonly string LandscapeString = ResourceService.PhotoConversionOutputConfigurationResource.GetString("Landscape");
        private readonly string PortraitString = ResourceService.PhotoConversionOutputConfigurationResource.GetString("Portrait");
        private readonly string SquareString = ResourceService.PhotoConversionOutputConfigurationResource.GetString("Square");
        private readonly string ThreeToFourString = ResourceService.PhotoConversionOutputConfigurationResource.GetString("ThreeToFour");
        private readonly string ThreeToTwoString = ResourceService.PhotoConversionOutputConfigurationResource.GetString("ThreeToTwo");
        private readonly string TwoToThreeString = ResourceService.PhotoConversionOutputConfigurationResource.GetString("TwoToThree");
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        private string filePath;
        private int rawImageWidth;
        private int rawImageHeight;
        private double aspectRatio;
        private OverlappedPresenter overlappedPresenter;
        private SUBCLASSPROC photoConversionOutputConfigurationWindowSubClassProc;
        private ContentIsland contentIsland;
        private InputKeyboardSource inputKeyboardSource;
        private InputPointerSource inputPointerSource;
        private TaskCompletionSource<ContentDialogResult> taskCompletionSource;

        private ConversionToolsWindow ConversionToolsWindow { get; set; }

        private SystemBackdrop _windowSystemBackdrop;

        public SystemBackdrop WindowSystemBackdrop
        {
            get { return _windowSystemBackdrop; }

            set
            {
                if (!Equals(_windowSystemBackdrop, value))
                {
                    _windowSystemBackdrop = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowSystemBackdrop)));
                }
            }
        }

        private ElementTheme _windowTheme;

        public ElementTheme WindowTheme
        {
            get { return _windowTheme; }

            set
            {
                if (!Equals(_windowTheme, value))
                {
                    _windowTheme = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowTheme)));
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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem)));
                }
            }
        }

        private bool _isGlobalSettings;

        public bool IsGlobalSettings
        {
            get { return _isGlobalSettings; }

            set
            {
                if (!Equals(_isGlobalSettings, value))
                {
                    _isGlobalSettings = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsGlobalSettings)));
                }
            }
        }

        private ComboBoxItemModel _selectedFormatConversionType;

        public ComboBoxItemModel SelectedFormatConversionType
        {
            get { return _selectedFormatConversionType; }

            set
            {
                if (!Equals(_selectedFormatConversionType, value))
                {
                    _selectedFormatConversionType = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFormatConversionType)));
                }
            }
        }

        private bool _isImageCropped;

        public bool IsImageCropped
        {
            get { return _isImageCropped; }

            set
            {
                if (!Equals(_isImageCropped, value))
                {
                    _isImageCropped = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsImageCropped)));
                }
            }
        }

        private int _imageWidth;

        public int ImageWidth
        {
            get { return _imageWidth; }

            set
            {
                if (!Equals(_imageWidth, value))
                {
                    _imageWidth = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageWidth)));
                }
            }
        }

        private int _imageHeight;

        public int ImageHeight
        {
            get { return _imageHeight; }

            set
            {
                if (!Equals(_imageHeight, value))
                {
                    _imageHeight = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageHeight)));
                }
            }
        }

        private bool _lockRatio;

        public bool LockRatio
        {
            get { return _lockRatio; }

            set
            {
                if (!Equals(_lockRatio, value))
                {
                    _lockRatio = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LockRatio)));
                }
            }
        }

        private int _xCoordinate;

        public int XCoordinate
        {
            get { return _xCoordinate; }

            set
            {
                if (!Equals(_xCoordinate, value))
                {
                    _xCoordinate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(XCoordinate)));
                }
            }
        }

        private int _yCoordinate;

        public int YCoordinate
        {
            get { return _yCoordinate; }

            set
            {
                if (!Equals(_yCoordinate, value))
                {
                    _yCoordinate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(YCoordinate)));
                }
            }
        }

        private int _clipWidth;

        public int ClipWidth
        {
            get { return _clipWidth; }

            set
            {
                if (!Equals(_clipWidth, value))
                {
                    _clipWidth = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClipWidth)));
                }
            }
        }

        private int _clipHeight;

        public int ClipHeight
        {
            get { return _clipHeight; }

            set
            {
                if (!Equals(_clipHeight, value))
                {
                    _clipHeight = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClipHeight)));
                }
            }
        }


        private double _constrastRatio;

        public double ConstrastRatio
        {
            get { return _constrastRatio; }

            set
            {
                if (!Equals(_constrastRatio, value))
                {
                    _constrastRatio = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConstrastRatio)));
                }
            }
        }

        private double _exposure;

        public double Exposure
        {
            get { return _exposure; }

            set
            {
                if (!Equals(_exposure, value))
                {
                    _exposure = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Exposure)));
                }
            }
        }

        private double _saturation;

        public double Saturation
        {
            get { return _saturation; }

            set
            {
                if (!Equals(_saturation, value))
                {
                    _saturation = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Saturation)));
                }
            }
        }

        private double _colorTemperature;

        public double ColorTemperature
        {
            get { return _colorTemperature; }

            set
            {
                if (!Equals(_colorTemperature, value))
                {
                    _colorTemperature = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColorTemperature)));
                }
            }
        }

        private double _tone;

        public double Tone
        {
            get { return _tone; }

            set
            {
                if (!Equals(_tone, value))
                {
                    _tone = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tone)));
                }
            }
        }

        private double _blur;

        public double Blur
        {
            get { return _blur; }

            set
            {
                if (!Equals(_blur, value))
                {
                    _blur = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Blur)));
                }
            }
        }

        private bool _grayScale;

        public bool GrayScale
        {
            get { return _grayScale; }

            set
            {
                if (!Equals(_grayScale, value))
                {
                    _grayScale = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GrayScale)));
                }
            }
        }

        private bool _reversal;

        public bool Reversal
        {
            get { return _reversal; }

            set
            {
                if (!Equals(_reversal, value))
                {
                    _reversal = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Reversal)));
                }
            }
        }

        private bool _isCroppingImage;

        public bool IsCroppingImage
        {
            get { return _isCroppingImage; }

            set
            {
                if (!Equals(_isCroppingImage, value))
                {
                    _isCroppingImage = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCroppingImage)));
                }
            }
        }

        private ComboBoxItemModel _selectedAspectRatio;

        public ComboBoxItemModel SelectedAspectRatio
        {
            get { return _selectedAspectRatio; }

            set
            {
                if (!Equals(_selectedAspectRatio, value))
                {
                    _selectedAspectRatio = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAspectRatio)));
                }
            }
        }

        private int _clipedXCoordinate;

        public int ClipedXCoordinate
        {
            get { return _clipedXCoordinate; }

            set
            {
                if (!Equals(_clipedXCoordinate, value))
                {
                    _clipedXCoordinate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClipedXCoordinate)));
                }
            }
        }

        private int _clipedYCoordinate;

        public int ClipedYCoordinate
        {
            get { return _clipedYCoordinate; }

            set
            {
                if (!Equals(_clipedYCoordinate, value))
                {
                    _clipedYCoordinate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClipedYCoordinate)));
                }
            }
        }

        private int _clipedWidth;

        public int ClipedWidth
        {
            get { return _clipedWidth; }

            set
            {
                if (!Equals(_clipedWidth, value))
                {
                    _clipedWidth = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClipedWidth)));
                }
            }
        }

        private int _clipedHeight;

        public int ClipedHeight
        {
            get { return _clipedHeight; }

            set
            {
                if (!Equals(_clipedHeight, value))
                {
                    _clipedHeight = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClipedHeight)));
                }
            }
        }


        public List<ComboBoxItemModel> FormatConversionTypeList { get; } =
        [
            new ComboBoxItemModel(){ SelectedValue = "JPG", DisplayMember = ".jpg" },
            new ComboBoxItemModel(){ SelectedValue = "JPEG", DisplayMember = ".jpeg" },
            new ComboBoxItemModel(){ SelectedValue = "PNG", DisplayMember =  ".png" },
            new ComboBoxItemModel(){ SelectedValue = "BMP", DisplayMember =  ".bmp" },
            new ComboBoxItemModel(){ SelectedValue = "WEBP", DisplayMember =  ".webp" },
            new ComboBoxItemModel(){ SelectedValue = "GIF",DisplayMember =  ".gif" },
            new ComboBoxItemModel(){ SelectedValue = "TIFF", DisplayMember =  ".tiff" },
            new ComboBoxItemModel(){ SelectedValue = "TGA", DisplayMember =  ".tga" },
            new ComboBoxItemModel(){ SelectedValue = "RAW", DisplayMember =  ".raw" }
        ];

        public List<ComboBoxItemModel> AspectRatioList { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public PhotoConversionOutputConfigurationWindow(ConversionToolsWindow conversionToolsWindow, PhotoConversionFileModel photoConversionFile = null)
        {
            InitializeData(photoConversionFile);
            InitializeComponent();
            InitializeUI(conversionToolsWindow);
        }

        #region 第一部分：窗口辅助类挂载的事件

        /// <summary>
        /// 处理键盘系统按键事件
        /// </summary>
        private async void OnSystemKeyDown(InputKeyboardSource sender, KeyEventArgs args)
        {
            if (args.VirtualKey is VirtualKey.F10 && Content is not null && Content.XamlRoot is not null)
            {
                await Task.Delay(50);
                SetPopupControlTheme(WindowTheme);
            }
        }

        /// <summary>
        /// 处理鼠标事件
        /// </summary>
        private async void OnPointerReleased(InputPointerSource sender, PointerEventArgs args)
        {
            if (args.CurrentPoint.Properties.PointerUpdateKind is PointerUpdateKind.RightButtonReleased && Content is not null && Content.XamlRoot is not null)
            {
                await Task.Delay(50);
                SetPopupControlTheme(WindowTheme);
            }
        }

        #endregion 第一部分：窗口辅助类挂载的事件

        #region 第二部分：窗口右键菜单事件

        /// <summary>
        /// 窗口移动
        /// </summary>
        private void OnMoveClicked(object sender, RoutedEventArgs args)
        {
            if (sender is MenuFlyoutItem menuFlyoutItem && menuFlyoutItem.Tag is MenuFlyout menuFlyout)
            {
                menuFlyout.Hide();
                User32Library.SendMessage((nint)AppWindow.Id.Value, WindowMessage.WM_SYSCOMMAND, (nuint)SYSTEMCOMMAND.SC_MOVE, 0);
            }
        }

        /// <summary>
        /// 窗口大小
        /// </summary>
        private void OnSizeClicked(object sender, RoutedEventArgs args)
        {
            if (sender is MenuFlyoutItem menuFlyoutItem && menuFlyoutItem.Tag is MenuFlyout menuFlyout)
            {
                menuFlyout.Hide();
                User32Library.SendMessage((nint)AppWindow.Id.Value, WindowMessage.WM_SYSCOMMAND, (nuint)SYSTEMCOMMAND.SC_SIZE, 0);
            }
        }

        /// <summary>
        /// 窗口关闭
        /// </summary>
        private void OnCloseClicked(object sender, RoutedEventArgs args)
        {
            User32Library.SendMessage((nint)AppWindow.Id.Value, WindowMessage.WM_SYSCOMMAND, (nuint)SYSTEMCOMMAND.SC_CLOSE, 0);
        }

        #endregion 第二部分：窗口右键菜单事件

        #region 第三部分：窗口内容挂载的事件

        /// <summary>
        /// 应用主题变化时设置标题栏按钮的颜色
        /// </summary>
        private void OnActualThemeChanged(FrameworkElement sender, object args)
        {
            SetTitleBarTheme(sender.ActualTheme);
            SetClassicMenuTheme(sender.ActualTheme);
        }

        #endregion 第三部分：窗口内容挂载的事件

        #region 第四部分：内容挂载的事件

        /// <summary>
        /// 加载完成后触发的事件
        /// </summary>
        private async void OnLoaded(object sender, RoutedEventArgs args)
        {
            // 设置标题栏主题
            SetTitleBarTheme((Content as FrameworkElement).ActualTheme);
            SetPopupControlTheme(WindowTheme);

            if(!IsGlobalSettings)
            {
                PhotoConversionOutputConfigurationImageCropper.AspectRatio = Convert.ToDouble(SelectedAspectRatio.SelectedValue);
            }
        }

        /// <summary>
        /// 预览图片
        /// </summary>
        private void OnPreviewPhotoClicked(object sender, RoutedEventArgs args)
        {
            // TODO：未完成
        }

        /// <summary>
        /// 确定
        /// </summary>
        private void OnOkClicked(object sender, RoutedEventArgs args)
        {
            if (!taskCompletionSource.Task.IsCompleted)
            {
                taskCompletionSource.TrySetResult(ContentDialogResult.Primary);
            }
            Close();
        }

        /// <summary>
        /// 使用系统图片查看器打开
        /// </summary>
        private void OnOpenWithSystemPhotoClicked(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>
            {
                try
                {
                    Process.Start(filePath);
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(PhotoConversionOutputConfigurationWindow), nameof(PhotoConversionOutputConfigurationWindow), nameof(OnOpenWithSystemPhotoClicked), 1, e);
                }
            });
        }

        /// <summary>
        /// 格式转换类型菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnFormatConversionTypeSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel formatConversionType && !Equals(SelectedFormatConversionType, formatConversionType))
            {
                SelectedFormatConversionType = formatConversionType;
            }
        }

        /// <summary>
        /// 是否裁剪图片
        /// </summary>
        private void OnImageCroppedToggled(object sender,RoutedEventArgs args)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                IsImageCropped = toggleSwitch.IsOn;
            }
        }

        /// <summary>
        /// 裁剪图片
        /// </summary>
        private async void OnCutImageClicked(object sender,RoutedEventArgs args)
        {
            IsCroppingImage = true;
            double dpi = Convert.ToDouble(User32Library.GetDpiForWindow((nint)AppWindow.Id.Value)) / 96;
            int width = Convert.ToInt32(1366 * dpi);
            int height = Convert.ToInt32(768 * dpi);
            overlappedPresenter.PreferredMinimumWidth = width;
            overlappedPresenter.PreferredMinimumHeight = height;
            User32Library.SetWindowPos((nint)AppWindow.Id.Value, 0, 0, 0, width, height, SetWindowPosFlags.SWP_NOREPOSITION | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOMOVE);
            if (PhotoConversionOutputConfigurationImageCropper.SourceImage is null)
            {
                await PhotoConversionOutputConfigurationImageCropper.LoadImageFromFileAsync(await StorageFile.GetFileFromPathAsync(filePath));
            }
            SelectedAspectRatio = AspectRatioList[0];
            PhotoConversionOutputConfigurationImageCropper.AspectRatio = Convert.ToDouble(SelectedAspectRatio.SelectedValue);
            PhotoConversionOutputConfigurationImageCropper.TrySetCroppedRegion(new Rect(XCoordinate, YCoordinate, ClipWidth, ClipHeight));
        }

        /// <summary>
        /// 图片长度发生变化时触发的事件
        /// </summary>
        private void OnImageWidthValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                ImageWidth = int.MaxValue;
                ImageWidth = Convert.ToInt32(args.OldValue);

                if (newValue < 1)
                {
                    ImageWidth = 1;
                    // 按纵横比调整图片高度
                    if(LockRatio)
                    {
                        ImageHeight = 1;
                    }
                }
                else
                {
                        ImageWidth = newValue;
                        // 按纵横比调整图片高度
                        if (LockRatio && aspectRatio is not 0)
                        {
                            ImageHeight = Convert.ToInt32(ImageWidth * aspectRatio);
                        }
                }
            }
        }

        /// <summary>
        /// 图片宽度发生变化时触发的事件
        /// </summary>
        private void OnImageHeightValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                ImageHeight = int.MaxValue;
                ImageHeight = Convert.ToInt32(args.OldValue);

                if (newValue < 1)
                {
                    ImageHeight = 1;
                    // 按纵横比调整图片宽度
                    if (LockRatio)
                    {
                        ImageWidth = 1;
                    }
                }
                else
                {
                        ImageHeight = newValue;
                        // 按纵横比调整图片宽度
                        if (LockRatio && aspectRatio is not 0)
                        {
                            ImageWidth = Convert.ToInt32(ImageHeight / aspectRatio);
                        }
                }
            }
        }

        /// <summary>
        /// 锁定比例
        /// </summary>
        private void OnLockRatioToggled(object sender, RoutedEventArgs args)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                LockRatio = toggleSwitch.IsOn;
            }
        }

        /// <summary>
        /// 截取起点 X 坐标发生变化时触发的事件
        /// </summary>
        private void OnXCoordinateValueChanged(NumberBox sender,NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                XCoordinate = int.MaxValue;
                XCoordinate = Convert.ToInt32(args.OldValue);

                if (newValue < 0)
                {
                    XCoordinate = 0;
                }
                else
                {
                    if(XCoordinate + ClipWidth > rawImageWidth)
                    {
                        XCoordinate = rawImageWidth - ClipWidth;
                    }
                    else
                    {
                        XCoordinate = newValue;
                    }
                }
            }
        }

        /// <summary>
        /// 截取起点 Y 坐标发生变化时触发的事件
        /// </summary>
        private void OnYCoordinateValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                YCoordinate = int.MaxValue;
                YCoordinate = Convert.ToInt32(args.OldValue);

                if (newValue < 0)
                {
                    YCoordinate = 0;
                }
                else
                {
                    if (YCoordinate + ClipHeight > rawImageHeight)
                    {
                        YCoordinate = rawImageHeight - ClipHeight;
                    }
                    else
                    {
                        YCoordinate = newValue;
                    }
                }
            }
        }

        /// <summary>
        /// 截取宽度发生变化时触发的事件
        /// </summary>
        private void OnClipWidthValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                ClipWidth = int.MaxValue;
                ClipWidth = Convert.ToInt32(args.OldValue);

                if (newValue < 0)
                {
                    ClipWidth = 0;
                }
                else
                {
                    if (ClipWidth + XCoordinate > rawImageWidth)
                    {
                        ClipWidth = rawImageWidth - XCoordinate;
                    }
                    else
                    {
                        ClipWidth = newValue;
                    }
                }
            }
        }

        /// <summary>
        /// 截取高度发生变化时触发的事件
        /// </summary>
        private void OnClipHeightValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                ClipHeight = int.MaxValue;
                ClipHeight = Convert.ToInt32(args.OldValue);

                if (newValue < 0)
                {
                    ClipHeight = 0;
                }
                else
                {
                    if (ClipHeight + YCoordinate > rawImageHeight)
                    {
                        ClipHeight = rawImageHeight - YCoordinate;
                    }
                    else
                    {
                        ClipHeight = newValue;
                    }
                }
            }
        }

        /// <summary>
        /// 对比度发生变化时触发的事件
        /// </summary>
        private void OnConstrastRatioValueChanged(object sender, RangeBaseValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN)
            {
                try
                {
                    ConstrastRatio = args.NewValue;
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionOutputConfigurationWindow), nameof(OnConstrastRatioValueChanged), 1, e);
                }
            }
        }

        /// <summary>
        /// 重置对比度
        /// </summary>
        private void OnResetConstrastRatioClicked(object sender,RoutedEventArgs args)
        {
            ConstrastRatio = 0;
        }

        /// <summary>
        /// 曝光发生变化时触发的事件
        /// </summary>
        private void OnExposureValueChanged(object sender, RangeBaseValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN)
            {
                try
                {
                    Exposure = args.NewValue;
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionOutputConfigurationWindow), nameof(OnExposureValueChanged), 1, e);
                }
            }
        }

        /// <summary>
        /// 重置曝光
        /// </summary>
        private void OnResetExposureClicked(object sender, RoutedEventArgs args)
        {
            Exposure = 0;
        }

        /// <summary>
        /// 饱和度发生变化时触发的事件
        /// </summary>
        private void OnSaturationValueChanged(object sender, RangeBaseValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN)
            {
                try
                {
                    Saturation = args.NewValue;
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionOutputConfigurationWindow), nameof(OnSaturationValueChanged), 1, e);
                }
            }
        }

        /// <summary>
        /// 重置饱和度
        /// </summary>
        private void OnResetSaturationClicked(object sender, RoutedEventArgs args)
        {
            Saturation = 1;
        }

        /// <summary>
        /// 色温发生变化时触发的事件
        /// </summary>
        private void OnColorTemperatureValueChanged(object sender, RangeBaseValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN)
            {
                try
                {
                    ColorTemperature = args.NewValue;
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionOutputConfigurationWindow), nameof(OnColorTemperatureValueChanged), 1, e);
                }
            }
        }

        /// <summary>
        /// 重置色温
        /// </summary>
        private void OnResetColorTemperatureClicked(object sender, RoutedEventArgs args)
        {
            ColorTemperature = 0;
        }

        /// <summary>
        /// 色调发生变化时触发的事件
        /// </summary>
        private void OnToneValueChanged(object sender, RangeBaseValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN)
            {
                try
                {
                    Tone = args.NewValue;
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionOutputConfigurationWindow), nameof(OnToneValueChanged), 1, e);
                }
            }
        }

        /// <summary>
        /// 重置色调
        /// </summary>
        private void OnResetToneClicked(object sender, RoutedEventArgs args)
        {
            Tone = 0;
        }

        /// <summary>
        /// 模糊发生变化时触发的事件
        /// </summary>
        private void OnBlurValueChanged(object sender, RangeBaseValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN)
            {
                try
                {
                    Blur = args.NewValue;
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionOutputConfigurationWindow), nameof(OnBlurValueChanged), 1, e);
                }
            }
        }

        /// <summary>
        /// 重置模糊
        /// </summary>
        private void OnResetBlurClicked(object sender, RoutedEventArgs args)
        {
            Blur = 0;
        }

        /// <summary>
        /// 灰度
        /// </summary>
        private void OnGrayScaleToggled(object sender, RoutedEventArgs args)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                GrayScale = toggleSwitch.IsOn;
            }
        }

        /// <summary>
        /// 反相
        /// </summary>
        private void OnReversalToggled(object sender, RoutedEventArgs args)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                Reversal = toggleSwitch.IsOn;
            }
        }

        /// <summary>
        /// 裁剪区域变化后触发的事件
        /// </summary>
        private void OnCropSizeChanged(object sender, EventArgs args)
        {
            ClipedXCoordinate = Convert.ToInt32(PhotoConversionOutputConfigurationImageCropper.CroppedRect.X);
            ClipedYCoordinate = Convert.ToInt32(PhotoConversionOutputConfigurationImageCropper.CroppedRect.Y);
            ClipedWidth = Convert.ToInt32(PhotoConversionOutputConfigurationImageCropper.CroppedRect.Width);
            ClipedHeight = Convert.ToInt32(PhotoConversionOutputConfigurationImageCropper.CroppedRect.Height);
        }

        /// <summary>
        /// 宽高比菜单选中项发生改变时触发的事件
        /// </summary>
        private void OnAspectRatioSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ComboBoxItemModel aspectRatio && !Equals(SelectedAspectRatio, aspectRatio))
            {
                SelectedAspectRatio = aspectRatio;
                PhotoConversionOutputConfigurationImageCropper.AspectRatio = Convert.ToDouble(SelectedAspectRatio.SelectedValue);
            }
        }

        /// <summary>
        /// 确定裁剪
        /// </summary>
        private void OnImageClipOkClicked(object sender, RoutedEventArgs args)
        {
            IsCroppingImage = false;
            double dpi = Convert.ToDouble(User32Library.GetDpiForWindow((nint)AppWindow.Id.Value)) / 96;
            int width = Convert.ToInt32(768 * dpi);
            int height = Convert.ToInt32(560 * dpi);
            overlappedPresenter.PreferredMinimumWidth = width;
            overlappedPresenter.PreferredMinimumHeight = height;
            User32Library.SetWindowPos((nint)AppWindow.Id.Value, 0, 0, 0, width, height, SetWindowPosFlags.SWP_NOREPOSITION | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOMOVE);
            IsImageCropped = true;
            XCoordinate = Convert.ToInt32(PhotoConversionOutputConfigurationImageCropper.CroppedRect.X);
            YCoordinate = Convert.ToInt32(PhotoConversionOutputConfigurationImageCropper.CroppedRect.Y);
            ClipWidth = Convert.ToInt32(PhotoConversionOutputConfigurationImageCropper.CroppedRect.Width);
            ClipHeight = Convert.ToInt32(PhotoConversionOutputConfigurationImageCropper.CroppedRect.Height);
        }

        /// <summary>
        /// 取消裁剪
        /// </summary>
        private void OnImageClipCancelClicked(object sender, RoutedEventArgs args)
        {
            IsCroppingImage = false;
            double dpi = Convert.ToDouble(User32Library.GetDpiForWindow((nint)AppWindow.Id.Value)) / 96;
            int width = Convert.ToInt32(768 * dpi);
            int height = Convert.ToInt32(560 * dpi);
            overlappedPresenter.PreferredMinimumWidth = width;
            overlappedPresenter.PreferredMinimumHeight = height;
            User32Library.SetWindowPos((nint)AppWindow.Id.Value, 0, 0, 0, width, height, SetWindowPosFlags.SWP_NOREPOSITION | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOMOVE);
        }

        #endregion 第四部分：内容挂载的事件

        #region 第五部分：自定义事件

        /// <summary>
        /// 设置选项发生变化时触发的事件
        /// </summary>
        private void OnServicePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            synchronizationContext.Post((_) =>
            {
                if (string.Equals(args.PropertyName, nameof(ThemeService.AppTheme)))
                {
                    SetWindowTheme();
                }
                if (string.Equals(args.PropertyName, nameof(BackdropService.AppBackdrop)))
                {
                    SetSystemBackdrop();
                }
            }, null);
        }

        #endregion 第五部分：自定义事件

        #region 第六部分：窗口及内容属性设置

        /// <summary>
        /// 设置应用显示的主题
        /// </summary>
        public void SetWindowTheme()
        {
            WindowTheme = string.Equals(ThemeService.AppTheme, ThemeService.ThemeList[0]) ? Application.Current.RequestedTheme is ApplicationTheme.Light ? ElementTheme.Light : ElementTheme.Dark : Enum.TryParse(ThemeService.AppTheme, out ElementTheme elementTheme) ? elementTheme : ElementTheme.Default;
        }

        /// <summary>
        /// 设置应用的背景色
        /// </summary>
        private void SetSystemBackdrop()
        {
            if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[1]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(MicaKind.Base);
                VisualStateManager.GoToState(PhotoConversionOutputConfigurationPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[2]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(MicaKind.BaseAlt);
                VisualStateManager.GoToState(PhotoConversionOutputConfigurationPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[3]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Default);
                VisualStateManager.GoToState(PhotoConversionOutputConfigurationPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[4]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Base);
                VisualStateManager.GoToState(PhotoConversionOutputConfigurationPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[5]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Thin);
                VisualStateManager.GoToState(PhotoConversionOutputConfigurationPage, "BackgroundTransparent", false);
            }
            else
            {
                WindowSystemBackdrop = null;
                VisualStateManager.GoToState(PhotoConversionOutputConfigurationPage, "BackgroundDefault", false);
            }
        }

        /// <summary>
        /// 设置标题栏按钮的主题色
        /// </summary>
        private void SetTitleBarTheme(ElementTheme theme)
        {
            AppWindowTitleBar titleBar = AppWindow.TitleBar;

            titleBar.BackgroundColor = Colors.Transparent;
            titleBar.ForegroundColor = Colors.Transparent;
            titleBar.InactiveBackgroundColor = Colors.Transparent;
            titleBar.InactiveForegroundColor = Colors.Transparent;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            if (theme is ElementTheme.Light)
            {
                titleBar.ButtonForegroundColor = Color.FromArgb(255, 23, 23, 23);
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(25, 0, 0, 0);
                titleBar.ButtonHoverForegroundColor = Colors.Black;
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(51, 0, 0, 0);
                titleBar.ButtonPressedForegroundColor = Colors.Black;
                titleBar.ButtonInactiveForegroundColor = Color.FromArgb(255, 153, 153, 153);
            }
            else
            {
                titleBar.ButtonForegroundColor = Color.FromArgb(255, 242, 242, 242);
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(25, 255, 255, 255);
                titleBar.ButtonHoverForegroundColor = Colors.White;
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(51, 255, 255, 255);
                titleBar.ButtonPressedForegroundColor = Colors.White;
                titleBar.ButtonInactiveForegroundColor = Color.FromArgb(255, 102, 102, 102);
            }
        }

        /// <summary>
        /// 设置传统菜单标题栏按钮的主题色
        /// </summary>
        private void SetClassicMenuTheme(ElementTheme theme)
        {
            AppWindowTitleBar titleBar = AppWindow.TitleBar;

            if (theme is ElementTheme.Light)
            {
                titleBar.PreferredTheme = TitleBarTheme.Light;
                UxthemeLibrary.SetPreferredAppMode(PreferredAppMode.ForceLight);
            }
            else
            {
                titleBar.PreferredTheme = TitleBarTheme.Dark;
                UxthemeLibrary.SetPreferredAppMode(PreferredAppMode.ForceDark);
            }

            UxthemeLibrary.FlushMenuThemes();
        }

        /// <summary>
        /// 设置所有弹出控件主题
        /// </summary>
        private void SetPopupControlTheme(ElementTheme elementTheme)
        {
            foreach (Popup popup in VisualTreeHelper.GetOpenPopupsForXamlRoot(Content.XamlRoot))
            {
                popup.RequestedTheme = elementTheme;

                if (popup.Child is FlyoutPresenter flyoutPresenter)
                {
                    flyoutPresenter.RequestedTheme = elementTheme;
                }

                if (popup.Child is Grid grid && grid.Name is "OuterOverflowContentRootV2")
                {
                    grid.RequestedTheme = elementTheme;
                }
            }
        }

        #endregion 第六部分：窗口及内容属性设置

        #region 第七部分：窗口过程

        /// <summary>
        /// 图片转换输出配置窗口消息处理
        /// </summary>
        private nint PhotoConversionOutputConfigurationWindowSubClassProc(nint hWnd, WindowMessage Msg, nuint wParam, nint lParam, uint uIdSubclass, nint dwRefData)
        {
            switch (Msg)
            {
                // 窗口位置发生变化时触发的消息
                case WindowMessage.WM_MOVE:
                    {
                        synchronizationContext.Post((_) =>
                        {
                            if (TitlebarMenuFlyout.IsOpen)
                            {
                                TitlebarMenuFlyout.Hide();
                            }
                        }, null);
                        break;
                    }
                // 窗口大小发生变化时触发的消息
                case WindowMessage.WM_SIZE:
                    {
                        synchronizationContext.Post((_) =>
                        {
                            if (TitlebarMenuFlyout.IsOpen)
                            {
                                TitlebarMenuFlyout.Hide();
                            }
                        }, null);
                        break;
                    }
                // 窗口激活状态发生变化时触发的消息
                case WindowMessage.WM_ACTIVATEAPP:
                    {
                        synchronizationContext.Post((_) =>
                        {
                            try
                            {
                                if (WindowSystemBackdrop is MaterialBackdrop materialBackdrop && materialBackdrop.BackdropConfiguration is not null)
                                {
                                    materialBackdrop.BackdropConfiguration.IsInputActive = AlwaysShowBackdropService.AlwaysShowBackdropValue || wParam is not 0;
                                }
                            }
                            catch (Exception e)
                            {
                                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionOutputConfigurationWindow), nameof(PhotoConversionOutputConfigurationWindowSubClassProc), 1, e);
                            }
                        }, null);
                        break;
                    }
                // 窗口销毁后触发的消息
                case WindowMessage.WM_DESTROY:
                    {
                        ThemeService.PropertyChanged -= OnServicePropertyChanged;
                        BackdropService.PropertyChanged -= OnServicePropertyChanged;
                        inputKeyboardSource.SystemKeyDown -= OnSystemKeyDown;
                        inputPointerSource.PointerReleased -= OnPointerReleased;
                        Comctl32Library.RemoveWindowSubclass((nint)AppWindow.Id.Value, PhotoConversionOutputConfigurationWindowSubClassProc, 0);
                        if (!taskCompletionSource.Task.IsCompleted)
                        {
                            taskCompletionSource.TrySetResult(ContentDialogResult.None);
                        }
                        ConversionToolsWindow.Activate();
                        ConversionToolsWindow = null;
                        break;
                    }
                // 当用户按下鼠标左键时，光标位于窗口的非工作区内的消息
                case WindowMessage.WM_NCLBUTTONDOWN:
                    {
                        if (TitlebarMenuFlyout.IsOpen)
                        {
                            TitlebarMenuFlyout.Hide();
                        }
                        break;
                    }
                // 当用户按下鼠标右键并释放时，光标位于窗口的非工作区内的消息
                case WindowMessage.WM_NCRBUTTONUP:
                    {
                        if (wParam is 2 && Content is not null && Content.XamlRoot is not null)
                        {
                            System.Drawing.Point cursorPos = new((int)LOWORD((uint)lParam), (int)HIWORD((uint)lParam));
                            User32Library.MapWindowPoints(0, hWnd, ref cursorPos, 2); ;
                            double dpi = Convert.ToDouble(User32Library.GetDpiForWindow((nint)AppWindow.Id.Value)) / 96;

                            FlyoutShowOptions options = new()
                            {
                                ShowMode = FlyoutShowMode.Standard,
                                Position = Environment.OSVersion.Version.Build > 22000 ? new Point(cursorPos.X / dpi, cursorPos.Y / dpi) : new Point(cursorPos.X, cursorPos.Y)
                            };

                            TitlebarMenuFlyout.ShowAt(Content, options);
                        }
                        return 0;
                    }
                // 应用主题设置跟随系统发生变化时，当系统主题设置发生变化时修改修改应用背景色
                case WindowMessage.WM_SETTINGCHANGE:
                    {
                        SetWindowTheme();
                        SetClassicMenuTheme(WindowTheme);

                        synchronizationContext.Post((_) =>
                        {
                            SetPopupControlTheme(WindowTheme);
                        }, null);
                        break;
                    }
                // 窗口 DPI 发生变化后触发的消息
                case WindowMessage.WM_DPICHANGED:
                    {
                        if(IsCroppingImage)
                        {
                            double dpi = Convert.ToDouble(User32Library.GetDpiForWindow((nint)AppWindow.Id.Value)) / 96;
                            int width = Convert.ToInt32(1366 * dpi);
                            int height = Convert.ToInt32(768 * dpi);
                            overlappedPresenter.PreferredMinimumWidth = width;
                            overlappedPresenter.PreferredMinimumHeight = height;
                            User32Library.SetWindowPos((nint)AppWindow.Id.Value, 0, 0, 0, width, height, SetWindowPosFlags.SWP_NOREPOSITION | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOMOVE);
                        }
                        else
                        {
                            double dpi = Convert.ToDouble(User32Library.GetDpiForWindow((nint)AppWindow.Id.Value)) / 96;
                            int width = Convert.ToInt32(768 * dpi);
                            int height = Convert.ToInt32(560 * dpi);
                            overlappedPresenter.PreferredMinimumWidth = width;
                            overlappedPresenter.PreferredMinimumHeight = height;
                            User32Library.SetWindowPos((nint)AppWindow.Id.Value, 0, 0, 0, width, height, SetWindowPosFlags.SWP_NOREPOSITION | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOMOVE);
                        }
                        break;
                    }
                // 选择窗口右键菜单的条目时接收到的消息
                case WindowMessage.WM_SYSCOMMAND:
                    {
                        SYSTEMCOMMAND sysCommand = (SYSTEMCOMMAND)(wParam & 0xFFF0);

                        if (sysCommand is SYSTEMCOMMAND.SC_MOUSEMENU)
                        {
                            FlyoutShowOptions options = new()
                            {
                                Position = new Point(0, 15),
                                ShowMode = FlyoutShowMode.Standard
                            };
                            TitlebarMenuFlyout.ShowAt(null, options);
                            return 0;
                        }
                        else if (sysCommand is SYSTEMCOMMAND.SC_KEYMENU)
                        {
                            if (lParam is (int)System.Windows.Forms.Keys.Space)
                            {
                                FlyoutShowOptions options = new()
                                {
                                    Position = new Point(0, 30),
                                    ShowMode = FlyoutShowMode.Standard
                                };
                                TitlebarMenuFlyout.ShowAt(null, options);
                                return 0;
                            }
                        }
                        break;
                    }
            }
            return Comctl32Library.DefSubclassProc(hWnd, Msg, wParam, lParam);
        }

        #endregion 第七部分：窗口过程

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData(PhotoConversionFileModel photoConversionFile)
        {
            IsGlobalSettings = photoConversionFile is null;
            if(IsGlobalSettings)
            {
                filePath = string.Empty;
                aspectRatio = 1;
                SelectedFormatConversionType = FormatConversionTypeList[0];
                IsImageCropped = false;
                rawImageWidth = 0;
                rawImageHeight = 0;
                ImageWidth = 0;
                ImageHeight = 0;
                XCoordinate = 0;
                YCoordinate = 0;
                ClipWidth = 0;
                ClipHeight = 0;
                ConstrastRatio = 0;
                Exposure = 0;
                Saturation = 1;
                ColorTemperature = 0;
                Tone = 0;
                Blur = 0;
                GrayScale = false;
                Reversal = false;
            }
            else
            {
                filePath = photoConversionFile.FilePath;
                aspectRatio = ImageHeight is not 0 ? (double)photoConversionFile.ImageWidth / photoConversionFile.ImageHeight : 1;
                SelectedFormatConversionType = photoConversionFile.PhotoConversionOutputConfiguration is not null && FormatConversionTypeList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), photoConversionFile.PhotoConversionOutputConfiguration.FormatConversionType)) is ComboBoxItemModel selectedFormatConversionType ? selectedFormatConversionType : FormatConversionTypeList[0];
                IsImageCropped = photoConversionFile.PhotoConversionOutputConfiguration is not null && photoConversionFile.PhotoConversionOutputConfiguration.IsImageCropped;
                rawImageWidth = photoConversionFile.ImageWidth;
                rawImageHeight = photoConversionFile.ImageHeight;
                ImageWidth = photoConversionFile.PhotoConversionOutputConfiguration is not null ? photoConversionFile.PhotoConversionOutputConfiguration.ImageWidth : photoConversionFile.ImageWidth;
                ImageHeight = photoConversionFile.PhotoConversionOutputConfiguration is not null ? photoConversionFile.PhotoConversionOutputConfiguration.ImageHeight : photoConversionFile.ImageHeight;
                XCoordinate = photoConversionFile.PhotoConversionOutputConfiguration is not null ? photoConversionFile.PhotoConversionOutputConfiguration.XCoordinate : 0;
                YCoordinate = photoConversionFile.PhotoConversionOutputConfiguration is not null ? photoConversionFile.PhotoConversionOutputConfiguration.YCoordinate : 0;
                ClipWidth = photoConversionFile.PhotoConversionOutputConfiguration is not null ? photoConversionFile.PhotoConversionOutputConfiguration.ClipWidth : photoConversionFile.ImageWidth;
                ClipHeight = photoConversionFile.PhotoConversionOutputConfiguration is not null ? photoConversionFile.PhotoConversionOutputConfiguration.ClipHeight : photoConversionFile.ImageHeight;
                ConstrastRatio = photoConversionFile.PhotoConversionOutputConfiguration is not null ? photoConversionFile.PhotoConversionOutputConfiguration.ConstrastRatio : 0;
                Exposure = photoConversionFile.PhotoConversionOutputConfiguration is not null ? photoConversionFile.PhotoConversionOutputConfiguration.Exposure : 0;
                Saturation = photoConversionFile.PhotoConversionOutputConfiguration is not null ? photoConversionFile.PhotoConversionOutputConfiguration.Saturation : 1;
                ColorTemperature = photoConversionFile.PhotoConversionOutputConfiguration is not null ? photoConversionFile.PhotoConversionOutputConfiguration.ColorTemperature : 0;
                Tone = photoConversionFile.PhotoConversionOutputConfiguration is not null ? photoConversionFile.PhotoConversionOutputConfiguration.Tone : 0;
                Blur = photoConversionFile.PhotoConversionOutputConfiguration is not null ? photoConversionFile.PhotoConversionOutputConfiguration.Blur : 0;
                GrayScale = photoConversionFile.PhotoConversionOutputConfiguration is not null && photoConversionFile.PhotoConversionOutputConfiguration.GrayScale;
                Reversal = photoConversionFile.PhotoConversionOutputConfiguration is not null && photoConversionFile.PhotoConversionOutputConfiguration.Reversal;

                AspectRatioList.Add(new ComboBoxItemModel() { SelectedValue = -1, DisplayMember = CustomString });
                AspectRatioList.Add(new ComboBoxItemModel() { SelectedValue = 1, DisplayMember = SquareString });
                AspectRatioList.Add(new ComboBoxItemModel() { SelectedValue = 16d / 9d, DisplayMember = LandscapeString });
                AspectRatioList.Add(new ComboBoxItemModel() { SelectedValue = 9d / 16d, DisplayMember = PortraitString });
                AspectRatioList.Add(new ComboBoxItemModel() { SelectedValue = 4d / 3d, DisplayMember = FourToThreeString });
                AspectRatioList.Add(new ComboBoxItemModel() { SelectedValue = 3d / 4d, DisplayMember = ThreeToFourString });
                AspectRatioList.Add(new ComboBoxItemModel() { SelectedValue = 3d / 2d, DisplayMember = ThreeToTwoString });
                AspectRatioList.Add(new ComboBoxItemModel() { SelectedValue = 2d / 3d, DisplayMember = TwoToThreeString });
                SelectedAspectRatio = AspectRatioList[0];
            }
        }

        /// <summary>
        /// 初始化界面
        /// </summary>
        private void InitializeUI(ConversionToolsWindow conversionToolsWindow)
        {
            ConversionToolsWindow = conversionToolsWindow;
            if (IntPtr.Size is 8)
            {
                User32Library.SetWindowLongPtr((nint)AppWindow.Id.Value, WindowLongIndexFlags.GWLP_HWNDPARENT, ConversionToolsWindow.AppWindow.Id.Value);
            }
            else
            {
                User32Library.SetWindowLong((nint)AppWindow.Id.Value, WindowLongIndexFlags.GWLP_HWNDPARENT, ConversionToolsWindow.AppWindow.Id.Value);
            }
            overlappedPresenter = OverlappedPresenter.CreateForDialog();
            ExtendsContentIntoTitleBar = true;
            overlappedPresenter.IsResizable = false;
            overlappedPresenter.IsMinimizable = false;
            overlappedPresenter.IsMaximizable = false;
            overlappedPresenter.IsModal = true;
            AppWindow.SetPresenter(overlappedPresenter);
            AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            AppWindow.TitleBar.InactiveBackgroundColor = Colors.Transparent;
            AppWindow.TitleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;
            double dpi = Convert.ToDouble(User32Library.GetDpiForWindow((nint)AppWindow.Id.Value)) / 96;
            int width = Convert.ToInt32(768 * dpi);
            int height = Convert.ToInt32(560 * dpi);
            overlappedPresenter.PreferredMinimumWidth = width;
            overlappedPresenter.PreferredMinimumHeight = height;
            User32Library.GetWindowRect((nint)ConversionToolsWindow.AppWindow.Id.Value, out RECT parentRect);
            int childX = parentRect.left + (parentRect.right - parentRect.left - width) / 2;
            int childY = parentRect.top + (parentRect.bottom - parentRect.top - height) / 2;
            User32Library.SetWindowPos((nint)AppWindow.Id.Value, 0, childX, childY, width, height, SetWindowPosFlags.SWP_NOREPOSITION | SetWindowPosFlags.SWP_NOZORDER);
            contentIsland = ContentIsland.FindAllForCompositor(Compositor)[0];
            inputKeyboardSource = InputKeyboardSource.GetForIsland(contentIsland);
            inputPointerSource = InputPointerSource.GetForIsland(contentIsland);
            SelectedItem = PhotoConversionOutputConfigurationSelectorBar.Items[0];

            // 挂载相应的事件
            ThemeService.PropertyChanged += OnServicePropertyChanged;
            BackdropService.PropertyChanged += OnServicePropertyChanged;
            inputKeyboardSource.SystemKeyDown += OnSystemKeyDown;
            inputPointerSource.PointerReleased += OnPointerReleased;

            // 标题栏和右键菜单设置
            SetClassicMenuTheme((Content as FrameworkElement).ActualTheme);

            // 为窗口添加窗口过程
            photoConversionOutputConfigurationWindowSubClassProc = new SUBCLASSPROC(PhotoConversionOutputConfigurationWindowSubClassProc);
            Comctl32Library.SetWindowSubclass((nint)AppWindow.Id.Value, photoConversionOutputConfigurationWindowSubClassProc, 0, 0);

            SetWindowTheme();
            SetSystemBackdrop();
        }

        /// <summary>
        /// 显示模态窗口
        /// </summary>
        public async Task<ContentDialogResult> ShowAsync()
        {
            taskCompletionSource = new();
            AppWindow.Show();
            return await taskCompletionSource.Task;
        }

        private uint HIWORD(uint dword)
        {
            return (dword >> 16) & 0xffff;
        }

        private uint LOWORD(uint dword)
        {
            return dword & 0xffff;
        }
    }
}
