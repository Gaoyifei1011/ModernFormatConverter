using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Navigation;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Views.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

// 抑制 CA1806，CA1822，IDE0060 警告
#pragma warning disable CA1806,CA1822,IDE0060

namespace ModernFormatConverter.Views.Pages
{
    /// <summary>
    /// 图片转换输出配置页面
    /// </summary>
    public sealed partial class PhotoConversionOutputConfigurationPage : Page, INotifyPropertyChanged
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
        private readonly bool isInitialized;
        private PhotoConversionNavigationParameter photoConversionNavigationParameter;
        private string filePath;
        private int rawImageWidth;
        private int rawImageHeight;
        private double aspectRatio;

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

        private bool _adjustPhoto;

        public bool AdjustPhoto
        {
            get { return _adjustPhoto; }

            set
            {
                if (!Equals(_adjustPhoto, value))
                {
                    _adjustPhoto = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AdjustPhoto)));
                }
            }
        }

        private double _contrastRatio;

        public double ContrastRatio
        {
            get { return _contrastRatio; }

            set
            {
                if (!Equals(_contrastRatio, value))
                {
                    _contrastRatio = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContrastRatio)));
                }
            }
        }

        private double _brightness;

        public double Brightness
        {
            get { return _brightness; }

            set
            {
                if (!Equals(_brightness, value))
                {
                    _brightness = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Brightness)));
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

        private double _hue;

        public double Hue
        {
            get { return _hue; }

            set
            {
                if (!Equals(_hue, value))
                {
                    _hue = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Hue)));
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

        public PhotoConversionOutputConfigurationPage()
        {
            InitializeComponent();
        }

        #region 第一部分：重载父类事件

        /// <summary>
        /// 导航到该页面触发的事件
        /// </summary>
        protected override async void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);

            SelectedItem = PhotoConversionOutputConfigurationSelectorBar.Items[0];
            if (args.Parameter is PhotoConversionNavigationParameter photoConversionNavigationParameterData)
            {
                photoConversionNavigationParameter = photoConversionNavigationParameterData;

                // 图片格式转换
                if (photoConversionNavigationParameter.IsGlobalSettings)
                {
                    UpdateData(photoConversionNavigationParameter.IsGlobalSettings, null);
                }
                else
                {
                    if (photoConversionNavigationParameter.PhotoConversionData is PhotoFormatConversionFileModel photoFormatConversionFile && photoFormatConversionFile.PhotoConversionOutputConfiguration is not null)
                    {
                        UpdateData(photoConversionNavigationParameter.IsGlobalSettings, photoFormatConversionFile);
                    }
                }
            }
        }

        /// <summary>
        /// 离开该页面触发的事件
        /// </summary>
        protected override void OnNavigatedFrom(NavigationEventArgs args)
        {
            base.OnNavigatedFrom(args);
            photoConversionNavigationParameter = null;
        }

        #endregion 第一部分：重载父类事件

        #region 第二部分：音频转换输出配置页面——挂载的事件

        /// <summary>
        /// 预览图片
        /// </summary>
        private void OnPreviewPhotoClicked(object sender, RoutedEventArgs args)
        {
            Task.Run(async () =>
            {
                try
                {
                    string tempFilePath = Path.Combine(Path.GetTempPath(), string.Format("{0}.png", Path.GetRandomFileName()));
                    StringBuilder convertParametersBuilder = new();
                    convertParametersBuilder.Append("eq=contrast={0}:brightness={1}:saturation={2}");
                    convertParametersBuilder.Append(',');
                    convertParametersBuilder.Append("colortemperature=temperature={3}");
                    convertParametersBuilder.Append(',');
                    convertParametersBuilder.Append("gblur=sigma={4}");
                    convertParametersBuilder.Append(',');
                    convertParametersBuilder.Append(GrayScale ? "hue=s=0:h={5}" : "hue=h={5}");
                    if (Reversal)
                    {
                        convertParametersBuilder.Append(',');
                        convertParametersBuilder.Append("negate");
                    }

                    string convertParameters = string.Format(convertParametersBuilder.ToString(), ContrastRatio, Brightness, Saturation, ColorTemperature, Blur, Hue);
                    string arguments = string.Format("-i \"{0}\" -vf \"{1}\" \"{2}\"", filePath, convertParameters, tempFilePath);

                    Process process = new()
                    {
                        StartInfo = new()
                        {
                            FileName = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory), "FFmpeg.exe"),
                            Arguments = arguments,
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden,
                            UseShellExecute = false
                        }
                    };
                    process.Start();
                    process.WaitForExit();

                    if (process.ExitCode is 0 && File.Exists(tempFilePath))
                    {
                        Process.Start(tempFilePath);
                    }
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionOutputConfigurationPage), nameof(OnPreviewPhotoClicked), 1, e);
                }
            });
        }

        /// <summary>
        /// 确定
        /// </summary>
        private void OnOkClicked(object sender, RoutedEventArgs args)
        {
            // 图片格式转换
            if (photoConversionNavigationParameter.IsGlobalSettings)
            {
                if (photoConversionNavigationParameter.PhotoConversionData is List<PhotoFormatConversionFileModel> photoFormatConversionFileList)
                {
                    foreach (PhotoFormatConversionFileModel photoFormatConversionFile in photoFormatConversionFileList)
                    {
                        if (photoFormatConversionFile.PhotoConversionOutputConfiguration is not null)
                        {
                            photoFormatConversionFile.PhotoConversionOutputConfiguration.FormatConversionType = Convert.ToString(SelectedFormatConversionType.SelectedValue);
                            photoFormatConversionFile.PhotoConversionOutputConfiguration.AdjustPhoto = AdjustPhoto;
                            photoFormatConversionFile.PhotoConversionOutputConfiguration.ContrastRatio = ContrastRatio;
                            photoFormatConversionFile.PhotoConversionOutputConfiguration.Brightness = Brightness;
                            photoFormatConversionFile.PhotoConversionOutputConfiguration.Saturation = Saturation;
                            photoFormatConversionFile.PhotoConversionOutputConfiguration.ColorTemperature = ColorTemperature;
                            photoFormatConversionFile.PhotoConversionOutputConfiguration.Hue = Hue;
                            photoFormatConversionFile.PhotoConversionOutputConfiguration.Blur = Blur;
                            photoFormatConversionFile.PhotoConversionOutputConfiguration.GrayScale = GrayScale;
                            photoFormatConversionFile.PhotoConversionOutputConfiguration.Reversal = Reversal;
                        }
                    }
                }
            }
            else
            {
                if (photoConversionNavigationParameter.PhotoConversionData is PhotoFormatConversionFileModel photoFormatConversionFile && photoFormatConversionFile.PhotoConversionOutputConfiguration is not null)
                {
                    photoFormatConversionFile.PhotoConversionOutputConfiguration.FormatConversionType = Convert.ToString(SelectedFormatConversionType.SelectedValue);
                    photoFormatConversionFile.PhotoConversionOutputConfiguration.IsImageCropped = IsImageCropped;
                    photoFormatConversionFile.PhotoConversionOutputConfiguration.ImageWidth = ImageWidth;
                    photoFormatConversionFile.PhotoConversionOutputConfiguration.ImageHeight = ImageHeight;
                    photoFormatConversionFile.PhotoConversionOutputConfiguration.XCoordinate = XCoordinate;
                    photoFormatConversionFile.PhotoConversionOutputConfiguration.YCoordinate = YCoordinate;
                    photoFormatConversionFile.PhotoConversionOutputConfiguration.ClipWidth = ClipWidth;
                    photoFormatConversionFile.PhotoConversionOutputConfiguration.ClipHeight = ClipHeight;
                    photoFormatConversionFile.PhotoConversionOutputConfiguration.AdjustPhoto = AdjustPhoto;
                    photoFormatConversionFile.PhotoConversionOutputConfiguration.ContrastRatio = ContrastRatio;
                    photoFormatConversionFile.PhotoConversionOutputConfiguration.Brightness = Brightness;
                    photoFormatConversionFile.PhotoConversionOutputConfiguration.Saturation = Saturation;
                    photoFormatConversionFile.PhotoConversionOutputConfiguration.ColorTemperature = ColorTemperature;
                    photoFormatConversionFile.PhotoConversionOutputConfiguration.Hue = Hue;
                    photoFormatConversionFile.PhotoConversionOutputConfiguration.Blur = Blur;
                    photoFormatConversionFile.PhotoConversionOutputConfiguration.GrayScale = GrayScale;
                    photoFormatConversionFile.PhotoConversionOutputConfiguration.Reversal = Reversal;
                }
            }

            // 返回到上一页面
            if (MainWindow.Current.GetFrameContent() is PhotoConversionPage photoConversionPage)
            {
                photoConversionPage.NavigateTo(photoConversionPage.PageList[0], null, false);
            }
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
                    LogService.WriteLog(TraceEventType.Error, nameof(PhotoConversionOutputConfigurationPage), nameof(PhotoConversionOutputConfigurationPage), nameof(OnOpenWithSystemPhotoClicked), 1, e);
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
        private void OnImageCroppedToggled(object sender, RoutedEventArgs args)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                IsImageCropped = toggleSwitch.IsOn;
            }
        }

        /// <summary>
        /// 裁剪图片
        /// </summary>
        private async void OnCutImageClicked(object sender, RoutedEventArgs args)
        {
            IsCroppingImage = true;
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
                    if (LockRatio)
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
        private void OnXCoordinateValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                XCoordinate = int.MaxValue;
                XCoordinate = Convert.ToInt32(args.OldValue);
                XCoordinate = newValue < 0 ? 0 : XCoordinate + ClipWidth > rawImageWidth ? rawImageWidth - ClipWidth : newValue;
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
                YCoordinate = newValue < 0 ? 0 : YCoordinate + ClipHeight > rawImageHeight ? rawImageHeight - ClipHeight : newValue;
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
                ClipWidth = newValue < 0 ? 0 : ClipWidth + XCoordinate > rawImageWidth ? rawImageWidth - XCoordinate : newValue;
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
                ClipHeight = newValue < 0 ? 0 : ClipHeight + YCoordinate > rawImageHeight ? rawImageHeight - YCoordinate : newValue;
            }
        }

        /// <summary>
        /// 调整图片
        /// </summary>
        private void OnAdjustPhotoToggled(object sender, RoutedEventArgs args)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                AdjustPhoto = toggleSwitch.IsOn;
            }
        }

        /// <summary>
        /// 对比度发生变化时触发的事件
        /// </summary>
        private void OnContrastRatioValueChanged(object sender, RangeBaseValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && isInitialized)
            {
                try
                {
                    ContrastRatio = args.NewValue;
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionOutputConfigurationPage), nameof(OnContrastRatioValueChanged), 1, e);
                }
            }
        }

        /// <summary>
        /// 重置对比度
        /// </summary>
        private void OnResetContrastRatioClicked(object sender, RoutedEventArgs args)
        {
            ContrastRatio = 1;
        }

        /// <summary>
        /// 亮度发生变化时触发的事件
        /// </summary>
        private void OnBrightnessValueChanged(object sender, RangeBaseValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && isInitialized)
            {
                try
                {
                    Brightness = args.NewValue;
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionOutputConfigurationPage), nameof(OnBrightnessValueChanged), 1, e);
                }
            }
        }

        /// <summary>
        /// 重置曝光
        /// </summary>
        private void OnResetBrightnessClicked(object sender, RoutedEventArgs args)
        {
            Brightness = 0;
        }

        /// <summary>
        /// 饱和度发生变化时触发的事件
        /// </summary>
        private void OnSaturationValueChanged(object sender, RangeBaseValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && isInitialized)
            {
                try
                {
                    Saturation = args.NewValue;
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionOutputConfigurationPage), nameof(OnSaturationValueChanged), 1, e);
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
            if (args.NewValue is not double.NaN && isInitialized)
            {
                try
                {
                    ColorTemperature = args.NewValue;
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionOutputConfigurationPage), nameof(OnColorTemperatureValueChanged), 1, e);
                }
            }
        }

        /// <summary>
        /// 重置色温
        /// </summary>
        private void OnResetColorTemperatureClicked(object sender, RoutedEventArgs args)
        {
            ColorTemperature = 6500;
        }

        /// <summary>
        /// 色调发生变化时触发的事件
        /// </summary>
        private void OnHueValueChanged(object sender, RangeBaseValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && isInitialized)
            {
                try
                {
                    Hue = args.NewValue;
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionOutputConfigurationPage), nameof(OnHueValueChanged), 1, e);
                }
            }
        }

        /// <summary>
        /// 重置色调
        /// </summary>
        private void OnResetHueClicked(object sender, RoutedEventArgs args)
        {
            Hue = 0;
        }

        /// <summary>
        /// 模糊发生变化时触发的事件
        /// </summary>
        private void OnBlurValueChanged(object sender, RangeBaseValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && isInitialized)
            {
                try
                {
                    Blur = args.NewValue;
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(PhotoConversionOutputConfigurationPage), nameof(OnBlurValueChanged), 1, e);
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
        }

        #endregion 第二部分：音频转换输出配置页面——挂载的事件

        /// <summary>
        /// 更新数据
        /// </summary>
        private void UpdateData(bool isGlobalSettings,PhotoFormatConversionFileModel photoFormatConversionFile)
        {
            IsGlobalSettings = isGlobalSettings;
            if (IsGlobalSettings)
            {
                filePath = string.Empty;
                aspectRatio = 1;
                SelectedFormatConversionType = FormatConversionTypeList[0];
                IsCroppingImage = false;
                IsImageCropped = false;
                rawImageWidth = 0;
                rawImageHeight = 0;
                ImageWidth = 0;
                ImageHeight = 0;
                XCoordinate = 0;
                YCoordinate = 0;
                ClipWidth = 0;
                ClipHeight = 0;
                AdjustPhoto = false;
                ContrastRatio = 1;
                Brightness = 0;
                Saturation = 1;
                ColorTemperature = 6500;
                Hue = 0;
                Blur = 0;
                GrayScale = false;
                Reversal = false;
            }
            else
            {
                if(photoFormatConversionFile is not null)
                {
                    filePath = photoFormatConversionFile.FilePath;
                    aspectRatio = ImageHeight is not 0 ? (double)photoFormatConversionFile.ImageWidth / photoFormatConversionFile.ImageHeight : 1;
                    SelectedFormatConversionType = photoFormatConversionFile.PhotoConversionOutputConfiguration is not null && FormatConversionTypeList.Find(item => string.Equals(Convert.ToString(item.SelectedValue), photoFormatConversionFile.PhotoConversionOutputConfiguration.FormatConversionType)) is ComboBoxItemModel selectedFormatConversionType ? selectedFormatConversionType : FormatConversionTypeList[0];
                    IsCroppingImage = false;
                    IsImageCropped = photoFormatConversionFile.PhotoConversionOutputConfiguration is not null && photoFormatConversionFile.PhotoConversionOutputConfiguration.IsImageCropped;
                    rawImageWidth = photoFormatConversionFile.ImageWidth;
                    rawImageHeight = photoFormatConversionFile.ImageHeight;
                    ImageWidth = photoFormatConversionFile.PhotoConversionOutputConfiguration is not null ? photoFormatConversionFile.PhotoConversionOutputConfiguration.ImageWidth : photoFormatConversionFile.ImageWidth;
                    ImageHeight = photoFormatConversionFile.PhotoConversionOutputConfiguration is not null ? photoFormatConversionFile.PhotoConversionOutputConfiguration.ImageHeight : photoFormatConversionFile.ImageHeight;
                    XCoordinate = photoFormatConversionFile.PhotoConversionOutputConfiguration is not null ? photoFormatConversionFile.PhotoConversionOutputConfiguration.XCoordinate : 0;
                    YCoordinate = photoFormatConversionFile.PhotoConversionOutputConfiguration is not null ? photoFormatConversionFile.PhotoConversionOutputConfiguration.YCoordinate : 0;
                    ClipWidth = photoFormatConversionFile.PhotoConversionOutputConfiguration is not null ? photoFormatConversionFile.PhotoConversionOutputConfiguration.ClipWidth : photoFormatConversionFile.ImageWidth;
                    ClipHeight = photoFormatConversionFile.PhotoConversionOutputConfiguration is not null ? photoFormatConversionFile.PhotoConversionOutputConfiguration.ClipHeight : photoFormatConversionFile.ImageHeight;
                    AdjustPhoto = photoFormatConversionFile.PhotoConversionOutputConfiguration is not null && photoFormatConversionFile.PhotoConversionOutputConfiguration.AdjustPhoto;
                    ContrastRatio = photoFormatConversionFile.PhotoConversionOutputConfiguration is not null ? photoFormatConversionFile.PhotoConversionOutputConfiguration.ContrastRatio : 1;
                    Brightness = photoFormatConversionFile.PhotoConversionOutputConfiguration is not null ? photoFormatConversionFile.PhotoConversionOutputConfiguration.Brightness : 0;
                    Saturation = photoFormatConversionFile.PhotoConversionOutputConfiguration is not null ? photoFormatConversionFile.PhotoConversionOutputConfiguration.Saturation : 1;
                    ColorTemperature = photoFormatConversionFile.PhotoConversionOutputConfiguration is not null ? photoFormatConversionFile.PhotoConversionOutputConfiguration.ColorTemperature : 6500;
                    Hue = photoFormatConversionFile.PhotoConversionOutputConfiguration is not null ? photoFormatConversionFile.PhotoConversionOutputConfiguration.Hue : 0;
                    Blur = photoFormatConversionFile.PhotoConversionOutputConfiguration is not null ? photoFormatConversionFile.PhotoConversionOutputConfiguration.Blur : 0;
                    GrayScale = photoFormatConversionFile.PhotoConversionOutputConfiguration is not null && photoFormatConversionFile.PhotoConversionOutputConfiguration.GrayScale;
                    Reversal = photoFormatConversionFile.PhotoConversionOutputConfiguration is not null && photoFormatConversionFile.PhotoConversionOutputConfiguration.Reversal;

                    AspectRatioList.Clear();
                    AspectRatioList.Add(new ComboBoxItemModel() { SelectedValue = -1, DisplayMember = CustomString });
                    AspectRatioList.Add(new ComboBoxItemModel() { SelectedValue = 1, DisplayMember = SquareString });
                    AspectRatioList.Add(new ComboBoxItemModel() { SelectedValue = 16d / 9d, DisplayMember = LandscapeString });
                    AspectRatioList.Add(new ComboBoxItemModel() { SelectedValue = 9d / 16d, DisplayMember = PortraitString });
                    AspectRatioList.Add(new ComboBoxItemModel() { SelectedValue = 4d / 3d, DisplayMember = FourToThreeString });
                    AspectRatioList.Add(new ComboBoxItemModel() { SelectedValue = 3d / 4d, DisplayMember = ThreeToFourString });
                    AspectRatioList.Add(new ComboBoxItemModel() { SelectedValue = 3d / 2d, DisplayMember = ThreeToTwoString });
                    AspectRatioList.Add(new ComboBoxItemModel() { SelectedValue = 2d / 3d, DisplayMember = TwoToThreeString });
                    SelectedAspectRatio = AspectRatioList[0];
                    PhotoConversionOutputConfigurationImageCropper.AspectRatio = Convert.ToDouble(SelectedAspectRatio.SelectedValue);
                }

            }
        }

        private Visibility GetIsPreviewPhotoEnabled(bool isGlobalSettings, bool adjustPhoto)
        {
            return isGlobalSettings ? Visibility.Collapsed : adjustPhoto ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
