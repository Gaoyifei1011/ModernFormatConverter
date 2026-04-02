using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Helpers.Root;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Views.NotificationTips;
using ModernFormatConverter.Views.Windows;
using ModernFormatConverter.WindowsAPI.PInvoke.Kernel32;
using ModernFormatConverter.WindowsAPI.PInvoke.MediaInfo;
using ModernFormatConverter.WindowsAPI.PInvoke.Shell32;
using ModernFormatConverter.WindowsAPI.PInvoke.Shlwapi;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
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
        private readonly string AlbumString = ResourceService.FileInformationResource.GetString("Album");
        private readonly string AlternateGroupString = ResourceService.FileInformationResource.GetString("AlternateGroup");
        private readonly string ApertureString = ResourceService.FileInformationResource.GetString("Aperture");
        private readonly string AperturePriorityString = ResourceService.FileInformationResource.GetString("AperturePriority");
        private readonly string AverageString = ResourceService.FileInformationResource.GetString("Average");
        private readonly string BimodalAverageString = ResourceService.FileInformationResource.GetString("BimodalAverage");
        private readonly string BitDepthString = ResourceService.FileInformationResource.GetString("BitDepth");
        private readonly string BitRateString = ResourceService.FileInformationResource.GetString("BitRate");
        private readonly string BitRateMaximumString = ResourceService.FileInformationResource.GetString("BitRateMaximum");
        private readonly string BitRateModeString = ResourceService.FileInformationResource.GetString("BitRateMode");
        private readonly string BitsPixelFrameString = ResourceService.FileInformationResource.GetString("BitsPixelFrame");
        private readonly string BrightnessString = ResourceService.FileInformationResource.GetString("Brightness");
        private readonly string CameraManufacturerString = ResourceService.FileInformationResource.GetString("CameraManufacturer");
        private readonly string CameraModelString = ResourceService.FileInformationResource.GetString("CameraModel");
        private readonly string CentimeterString = ResourceService.FileInformationResource.GetString("Centimeter");
        private readonly string CfaPatternString = ResourceService.FileInformationResource.GetString("CfaPattern");
        private readonly string ChannelString = ResourceService.FileInformationResource.GetString("Channel");
        private readonly string ChannelLayoutString = ResourceService.FileInformationResource.GetString("ChannelLayout");
        private readonly string ChromaSubsamplingString = ResourceService.FileInformationResource.GetString("ChromaSubsampling");
        private readonly string CodecConfigurationBoxString = ResourceService.FileInformationResource.GetString("CodecConfigurationBox");
        private readonly string CodecIDString = ResourceService.FileInformationResource.GetString("CodecID");
        private readonly string CodecIDInfoString = ResourceService.FileInformationResource.GetString("CodecIDInfo");
        private readonly string ColorIndicatesString = ResourceService.FileInformationResource.GetString("ColorIndicates");
        private readonly string ColorParimariesString = ResourceService.FileInformationResource.GetString("ColorParimaries");
        private readonly string ColorRangeString = ResourceService.FileInformationResource.GetString("ColorRange");
        private readonly string ColorSpaceString = ResourceService.FileInformationResource.GetString("ColorSpace");
        private readonly string CompleteNameString = ResourceService.FileInformationResource.GetString("CompleteName");
        private readonly string CompressionBitsPerPixelString = ResourceService.FileInformationResource.GetString("CompressionBitsPerPixel");
        private readonly string CompressionConfigString = ResourceService.FileInformationResource.GetString("CompressionConfig");
        private readonly string CompressionModeString = ResourceService.FileInformationResource.GetString("CompressionMode");
        private readonly string CountOfEventsString = ResourceService.FileInformationResource.GetString("CountOfEvents");
        private readonly string CountOfLinesString = ResourceService.FileInformationResource.GetString("CountOfLines");
        private readonly string CreateProgramString = ResourceService.FileInformationResource.GetString("CreateProgram");
        private readonly string ShootingDateString = ResourceService.FileInformationResource.GetString("ShootingDate");
        private readonly string DefaultString = ResourceService.FileInformationResource.GetString("Default");
        private readonly string DisplayAspectRatioString = ResourceService.FileInformationResource.GetString("DisplayAspectRatio");
        private readonly string DragOverContentString = ResourceService.FileInformationResource.GetString("DragOverContent");
        private readonly string DurationString = ResourceService.FileInformationResource.GetString("Duration");
        private readonly string EncodedApplicationString = ResourceService.FileInformationResource.GetString("EncodedApplication");
        private readonly string EncodedDateString = ResourceService.FileInformationResource.GetString("EncodedDate");
        private readonly string EncodedLibraryString = ResourceService.FileInformationResource.GetString("EncodedLibrary");
        private readonly string EndTimeString = ResourceService.FileInformationResource.GetString("EndTime");
        private readonly string ExposureCompensationString = ResourceService.FileInformationResource.GetString("ExposureCompensation");
        private readonly string ExposureIndexString = ResourceService.FileInformationResource.GetString("ExposureIndex");
        private readonly string ExposureProgramString = ResourceService.FileInformationResource.GetString("ExposureProgram");
        private readonly string ExposureTimeString = ResourceService.FileInformationResource.GetString("ExposureTime");
        private readonly string FileAccessTimeString = ResourceService.FileInformationResource.GetString("FileAccessTime");
        private readonly string FileCreateTimeString = ResourceService.FileInformationResource.GetString("FileCreateTime");
        private readonly string FileModifyTimeString = ResourceService.FileInformationResource.GetString("FileModifyTime");
        private readonly string FileNameString = ResourceService.FileInformationResource.GetString("FileName");
        private readonly string FileSizeString = ResourceService.FileInformationResource.GetString("FileSize");
        private readonly string FileSizeDescriptionString = ResourceService.FileInformationResource.GetString("FileSizeDescription");
        private readonly string FileSpaceUsageString = ResourceService.FileInformationResource.GetString("FileSpaceUsage");
        private readonly string FileTypeString = ResourceService.FileInformationResource.GetString("FileType");
        private readonly string FlashString = ResourceService.FileInformationResource.GetString("Flash");
        private readonly string FlashAutoString = ResourceService.FileInformationResource.GetString("FlashAuto");
        private readonly string FlashAutoNoStrobeReturnLightString = ResourceService.FileInformationResource.GetString("FlashAutoNoStrobeReturnLight");
        private readonly string FlashAutoRedEyeString = ResourceService.FileInformationResource.GetString("FlashAutoRedEye");
        private readonly string FlashAutoRedEyeNoStrobeReturnLightString = ResourceService.FileInformationResource.GetString("FlashAutoRedEyeNoStrobeReturnLight");
        private readonly string FlashAutoRedEyeStrobeReturnLightString = ResourceService.FileInformationResource.GetString("FlashAutoRedEyeStrobeReturnLight");
        private readonly string FlashAutoStrobeReturnLightString = ResourceService.FileInformationResource.GetString("FlashAutoStrobeReturnLight");
        private readonly string FlashEnergyString = ResourceService.FileInformationResource.GetString("FlashEnergy");
        private readonly string FlashForceString = ResourceService.FileInformationResource.GetString("FlashForce");
        private readonly string FlashForceNoStrobeReturnLightString = ResourceService.FileInformationResource.GetString("FlashForceNoStrobeReturnLight");
        private readonly string FlashForceRedEyeString = ResourceService.FileInformationResource.GetString("FlashForceRedEye");
        private readonly string FlashForceRedEyeNoStrobeReturnLightString = ResourceService.FileInformationResource.GetString("FlashForceRedEyeNoStrobeReturnLight");
        private readonly string FlashForceRedEyeStrobeReturnLightString = ResourceService.FileInformationResource.GetString("FlashForceRedEyeStrobeReturnLight");
        private readonly string FlashForceStrobeReturnLightString = ResourceService.FileInformationResource.GetString("FlashForceStrobeReturnLight");
        private readonly string FlashLampString = ResourceService.FileInformationResource.GetString("FlashLamp");
        private readonly string FlashModeString = ResourceService.FileInformationResource.GetString("FlashMode");
        private readonly string FlashNoStrobeReturnLightString = ResourceService.FileInformationResource.GetString("FlashNoStrobeReturnLight");
        private readonly string FlashRedEyeString = ResourceService.FileInformationResource.GetString("FlashRedEye");
        private readonly string FlashRedEyeNoStrobeReturnLightString = ResourceService.FileInformationResource.GetString("FlashRedEyeNoStrobeReturnLight");
        private readonly string FlashRedEyeStrobeReturnLightString = ResourceService.FileInformationResource.GetString("FlashRedEyeStrobeReturnLight");
        private readonly string FlashStrobeReturnLightString = ResourceService.FileInformationResource.GetString("FlashStrobeReturnLight");
        private readonly string FlashPixVersionString = ResourceService.FileInformationResource.GetString("FlashPixVersion");
        private readonly string FluorescentLampString = ResourceService.FileInformationResource.GetString("FluorescentLamp");
        private readonly string FNumberString = ResourceService.FileInformationResource.GetString("FNumber");
        private readonly string FocalLengthString = ResourceService.FileInformationResource.GetString("FocalLength");
        private readonly string FocalResUnitString = ResourceService.FileInformationResource.GetString("FocalResUnit");
        private readonly string FocalXResString = ResourceService.FileInformationResource.GetString("FocalXRes");
        private readonly string FocalYResString = ResourceService.FileInformationResource.GetString("FocalYRes");
        private readonly string FootString = ResourceService.FileInformationResource.GetString("Foot");
        private readonly string FormatCompressionString = ResourceService.FileInformationResource.GetString("FormatCompression");
        private readonly string FormatInfoString = ResourceService.FileInformationResource.GetString("FormatInfo");
        private readonly string FormatProfileString = ResourceService.FileInformationResource.GetString("FormatProfile");
        private readonly string FormatVersionString = ResourceService.FileInformationResource.GetString("FormatVersion");
        private readonly string FormatString = ResourceService.FileInformationResource.GetString("Format");
        private readonly string FrameRateString = ResourceService.FileInformationResource.GetString("FrameRate");
        private readonly string FrameRateModeString = ResourceService.FileInformationResource.GetString("FrameRateMode");
        private readonly string FstopString = ResourceService.FileInformationResource.GetString("Fstop");
        private readonly string HeightString = ResourceService.FileInformationResource.GetString("Height");
        private readonly string HorizontalModeString = ResourceService.FileInformationResource.GetString("HorizontalMode");
        private readonly string IDString = ResourceService.FileInformationResource.GetString("ID");
        private readonly string IncandescentLampString = ResourceService.FileInformationResource.GetString("IncandescentLamp");
        private readonly string InchString = ResourceService.FileInformationResource.GetString("Inch");
        private readonly string InteropString = ResourceService.FileInformationResource.GetString("Interop");
        private readonly string ISOSpeedString = ResourceService.FileInformationResource.GetString("ISOSpeed");
        private readonly string LightSourceString = ResourceService.FileInformationResource.GetString("LightSource");
        private readonly string ManualString = ResourceService.FileInformationResource.GetString("Manual");
        private readonly string MatrixCoefficientsString = ResourceService.FileInformationResource.GetString("MatrixCoefficients");
        private readonly string MaxApertureString = ResourceService.FileInformationResource.GetString("MaxAperture");
        private readonly string MaximumCountOfLinesPerEventString = ResourceService.FileInformationResource.GetString("MaximumCountOfLinesPerEvent");
        private readonly string MaximumFrameRateString = ResourceService.FileInformationResource.GetString("MaximumFrameRate");
        private readonly string MeteringModeString = ResourceService.FileInformationResource.GetString("MeteringMode");
        private readonly string MinimumDurationPerEventString = ResourceService.FileInformationResource.GetString("MinimumDurationPerEvent");
        private readonly string MinimumFrameRateString = ResourceService.FileInformationResource.GetString("MinimumFrameRate");
        private readonly string MultiSpotString = ResourceService.FileInformationResource.GetString("MultiSpot");
        private readonly string NoFlashString = ResourceService.FileInformationResource.GetString("NoFlash");
        private readonly string NoFlashAutoString = ResourceService.FileInformationResource.GetString("NoFlashAuto");
        private readonly string NoFlashFeatureString = ResourceService.FileInformationResource.GetString("NoFlashFeature");
        private readonly string NoFlashForceString = ResourceService.FileInformationResource.GetString("NoFlashForce");
        private readonly string NoMultiFileString = ResourceService.FileInformationResource.GetString("NoMultiFile");
        private readonly string NormalString = ResourceService.FileInformationResource.GetString("Normal");
        private readonly string NoString = ResourceService.FileInformationResource.GetString("No");
        private readonly string NoUnitString = ResourceService.FileInformationResource.GetString("NoUnit");
        private readonly string NotAvailableString = ResourceService.FileInformationResource.GetString("NotAvailable");
        private readonly string OECFString = ResourceService.FileInformationResource.GetString("OECF");
        private readonly string OneChipColorAreaSensorString = ResourceService.FileInformationResource.GetString("OneChipColorAreaSensor");
        private readonly string OperationProgramString = ResourceService.FileInformationResource.GetString("OperationProgram");
        private readonly string OtherString = ResourceService.FileInformationResource.GetString("Other");
        private readonly string OverallBitRateString = ResourceService.FileInformationResource.GetString("OverallBitRate");
        private readonly string ParsingFileInformationString = ResourceService.FileInformationResource.GetString("ParsingFileInformation");
        private readonly string PartialString = ResourceService.FileInformationResource.GetString("Partial");
        private readonly string PerformerString = ResourceService.FileInformationResource.GetString("Performer");
        private readonly string PatternString = ResourceService.FileInformationResource.GetString("Pattern");
        private readonly string RecordedDateString = ResourceService.FileInformationResource.GetString("RecordedDate");
        private readonly string ReservedString = ResourceService.FileInformationResource.GetString("Reserved");
        private readonly string SamplingRateString = ResourceService.FileInformationResource.GetString("SamplingRate");
        private readonly string SecondString = ResourceService.FileInformationResource.GetString("Second");
        private readonly string SelectFileString = ResourceService.FileInformationResource.GetString("SelectFile");
        private readonly string SenseMethodString = ResourceService.FileInformationResource.GetString("SenseMethod");
        private readonly string SceneTypeString = ResourceService.FileInformationResource.GetString("SceneType");
        private readonly string ShutterPriorityString = ResourceService.FileInformationResource.GetString("ShutterPriority");
        private readonly string ShutterSpeedString = ResourceService.FileInformationResource.GetString("ShutterSpeed");
        private readonly string SourceDurationString = ResourceService.FileInformationResource.GetString("SourceDuration");
        private readonly string SourceStreamSizeString = ResourceService.FileInformationResource.GetString("SourceStreamSize");
        private readonly string SpaceUsageDescriptionString = ResourceService.FileInformationResource.GetString("SpaceUsageDescription");
        private readonly string SpectralSenseString = ResourceService.FileInformationResource.GetString("SpectralSense");
        private readonly string SpatialFrequencyResponseString = ResourceService.FileInformationResource.GetString("SpatialFrequencyResponse");
        private readonly string SpotString = ResourceService.FileInformationResource.GetString("Spot");
        private readonly string StandardLightAString = ResourceService.FileInformationResource.GetString("StandardLightA");
        private readonly string StandardLightBString = ResourceService.FileInformationResource.GetString("StandardLightB");
        private readonly string StandardLightCString = ResourceService.FileInformationResource.GetString("StandardLightC");
        private readonly string StandardLightD55String = ResourceService.FileInformationResource.GetString("StandardLightD55");
        private readonly string StandardLightD65String = ResourceService.FileInformationResource.GetString("StandardLightD65");
        private readonly string StandardLightD75String = ResourceService.FileInformationResource.GetString("StandardLightD75");
        private readonly string StartTimeString = ResourceService.FileInformationResource.GetString("StartTime");
        private readonly string StreamSizeString = ResourceService.FileInformationResource.GetString("StreamSize");
        private readonly string SubjectDistString = ResourceService.FileInformationResource.GetString("SubjectDist");
        private readonly string SunlightString = ResourceService.FileInformationResource.GetString("Sunlight");
        private readonly string TrackNameString = ResourceService.FileInformationResource.GetString("TrackName");
        private readonly string TransferCharacteristicsString = ResourceService.FileInformationResource.GetString("TransferCharacteristics");
        private readonly string UniqueIDString = ResourceService.FileInformationResource.GetString("UniqueID");
        private readonly string UnknownString = ResourceService.FileInformationResource.GetString("Unknown");
        private readonly string VerticalModeString = ResourceService.FileInformationResource.GetString("VerticalMode");
        private readonly string VersionString = ResourceService.FileInformationResource.GetString("Version");
        private readonly string WidthString = ResourceService.FileInformationResource.GetString("Width");
        private readonly string XResolutionString = ResourceService.FileInformationResource.GetString("XResolution");
        private readonly string YesString = ResourceService.FileInformationResource.GetString("Yes");
        private readonly string YResolutionString = ResourceService.FileInformationResource.GetString("YResolution");
        private string filePath;
        private VideoInformation videoInformation;
        private AudioInformation audioInformation;
        private TextInformation textInformation;
        private ImageInformation imageInformation;

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

        private string _fileName;

        public string FileName
        {
            get { return _fileName; }

            set
            {
                if (!string.Equals(_fileName, value))
                {
                    _fileName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileName)));
                }
            }
        }

        private string _fileType;

        public string FileType
        {
            get { return _fileType; }

            set
            {
                if (!string.Equals(_fileType, value))
                {
                    _fileType = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileType)));
                }
            }
        }

        private string _fileSize;

        public string FileSize
        {
            get { return _fileSize; }

            set
            {
                if (!string.Equals(_fileSize, value))
                {
                    _fileSize = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileSize)));
                }
            }
        }

        private string _fileSpaceUsage;

        public string FileSpaceUsage
        {
            get { return _fileSpaceUsage; }

            set
            {
                if (!string.Equals(_fileSpaceUsage, value))
                {
                    _fileSpaceUsage = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileSpaceUsage)));
                }
            }
        }

        private string _fileCreateTime;

        public string FileCreateTime
        {
            get { return _fileCreateTime; }

            set
            {
                if (!string.Equals(_fileCreateTime, value))
                {
                    _fileCreateTime = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileCreateTime)));
                }
            }
        }

        private string _fileModifyTime;

        public string FileModifyTime
        {
            get { return _fileModifyTime; }

            set
            {
                if (!string.Equals(_fileModifyTime, value))
                {
                    _fileModifyTime = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileModifyTime)));
                }
            }
        }

        private string _fileAccessTime;

        public string FileAccessTime
        {
            get { return _fileAccessTime; }

            set
            {
                if (!string.Equals(_fileAccessTime, value))
                {
                    _fileAccessTime = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileAccessTime)));
                }
            }
        }

        private SelectorBarItem _videoInformationSelectedItem;

        public SelectorBarItem VideoInformationSelectedItem
        {
            get { return _videoInformationSelectedItem; }

            set
            {
                if (!Equals(_videoInformationSelectedItem, value))
                {
                    _videoInformationSelectedItem = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VideoInformationSelectedItem)));
                }
            }
        }

        private GeneralInfo _videoGeneralInfo;

        public GeneralInfo VideoGeneralInfo
        {
            get { return _videoGeneralInfo; }

            set
            {
                if (!Equals(_videoGeneralInfo, value))
                {
                    _videoGeneralInfo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VideoGeneralInfo)));
                }
            }
        }

        private VideoDetailInfo _videoDetailVideoInfo;

        public VideoDetailInfo VideoDetailVideoInfo
        {
            get { return _videoDetailVideoInfo; }

            set
            {
                if (!Equals(_videoDetailVideoInfo, value))
                {
                    _videoDetailVideoInfo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VideoDetailVideoInfo)));
                }
            }
        }

        private int _videoDetailVideoInfoSelectedIndex;

        public int VideoDetailVideoInfoSelectedIndex
        {
            get { return _videoDetailVideoInfoSelectedIndex; }

            set
            {
                if (!Equals(_videoDetailVideoInfoSelectedIndex, value))
                {
                    _videoDetailVideoInfoSelectedIndex = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VideoDetailVideoInfoSelectedIndex)));
                }
            }
        }

        private int _videoDetailVideoInfoCount;

        public int VideoDetailVideoInfoCount
        {
            get { return _videoDetailVideoInfoCount; }

            set
            {
                if (!Equals(_videoDetailVideoInfoCount, value))
                {
                    _videoDetailVideoInfoCount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VideoDetailVideoInfoCount)));
                }
            }
        }

        private AudioDetailInfo _videoDetailAudioInfo;

        public AudioDetailInfo VideoDetailAudioInfo
        {
            get { return _videoDetailAudioInfo; }

            set
            {
                if (!Equals(_videoDetailVideoInfo, value))
                {
                    _videoDetailAudioInfo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VideoDetailAudioInfo)));
                }
            }
        }

        private int _videoDetailAudioInfoSelectedIndex;

        public int VideoDetailAudioInfoSelectedIndex
        {
            get { return _videoDetailAudioInfoSelectedIndex; }

            set
            {
                if (!Equals(_videoDetailAudioInfoSelectedIndex, value))
                {
                    _videoDetailAudioInfoSelectedIndex = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VideoDetailAudioInfoSelectedIndex)));
                }
            }
        }

        private int _videoDetailAudioInfoCount;

        public int VideoDetailAudioInfoCount
        {
            get { return _videoDetailAudioInfoCount; }

            set
            {
                if (!Equals(_videoDetailAudioInfoCount, value))
                {
                    _videoDetailAudioInfoCount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VideoDetailAudioInfoCount)));
                }
            }
        }

        private TextDetailInfo _videoDetailTextInfo;

        public TextDetailInfo VideoDetailTextInfo
        {
            get { return _videoDetailTextInfo; }

            set
            {
                if (!Equals(_videoDetailVideoInfo, value))
                {
                    _videoDetailTextInfo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VideoDetailTextInfo)));
                }
            }
        }

        private int _videoDetailTextInfoSelectedIndex;

        public int VideoDetailTextInfoSelectedIndex
        {
            get { return _videoDetailTextInfoSelectedIndex; }

            set
            {
                if (!Equals(_videoDetailTextInfoSelectedIndex, value))
                {
                    _videoDetailTextInfoSelectedIndex = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VideoDetailTextInfoSelectedIndex)));
                }
            }
        }

        private int _videoDetailTextInfoCount;

        public int VideoDetailTextInfoCount
        {
            get { return _videoDetailTextInfoCount; }

            set
            {
                if (!Equals(_videoDetailTextInfoCount, value))
                {
                    _videoDetailTextInfoCount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VideoDetailTextInfoCount)));
                }
            }
        }

        private string _videoOverviewInfo;

        public string VideoOverviewInfo
        {
            get { return _videoOverviewInfo; }

            set
            {
                if (!Equals(_videoOverviewInfo, value))
                {
                    _videoOverviewInfo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VideoOverviewInfo)));
                }
            }
        }

        private bool _isVideoOverviewInfoExisted;

        public bool IsVideoOverviewInfoExisted
        {
            get { return _isVideoOverviewInfoExisted; }

            set
            {
                if (!Equals(_isVideoOverviewInfoExisted, value))
                {
                    _isVideoOverviewInfoExisted = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVideoOverviewInfoExisted)));
                }
            }
        }

        private SelectorBarItem _audioInformationSelectedItem;

        public SelectorBarItem AudioInformationSelectedItem
        {
            get { return _audioInformationSelectedItem; }

            set
            {
                if (!Equals(_audioInformationSelectedItem, value))
                {
                    _audioInformationSelectedItem = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioInformationSelectedItem)));
                }
            }
        }

        private GeneralInfo _audioGeneralInfo;

        public GeneralInfo AudioGeneralInfo
        {
            get { return _audioGeneralInfo; }

            set
            {
                if (!Equals(_audioGeneralInfo, value))
                {
                    _audioGeneralInfo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioGeneralInfo)));
                }
            }
        }

        private AudioDetailInfo _audioDetailAudioInfo;

        public AudioDetailInfo AudioDetailAudioInfo
        {
            get { return _audioDetailAudioInfo; }

            set
            {
                if (!Equals(_videoDetailVideoInfo, value))
                {
                    _audioDetailAudioInfo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioDetailAudioInfo)));
                }
            }
        }

        private int _audioDetailAudioInfoSelectedIndex;

        public int AudioDetailAudioInfoSelectedIndex
        {
            get { return _audioDetailAudioInfoSelectedIndex; }

            set
            {
                if (!Equals(_audioDetailAudioInfoSelectedIndex, value))
                {
                    _audioDetailAudioInfoSelectedIndex = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioDetailAudioInfoSelectedIndex)));
                }
            }
        }

        private int _audioDetailAudioInfoCount;

        public int AudioDetailAudioInfoCount
        {
            get { return _audioDetailAudioInfoCount; }

            set
            {
                if (!Equals(_audioDetailAudioInfoCount, value))
                {
                    _audioDetailAudioInfoCount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioDetailAudioInfoCount)));
                }
            }
        }

        private string _audioOverviewInfo;

        public string AudioOverviewInfo
        {
            get { return _audioOverviewInfo; }

            set
            {
                if (!Equals(_audioOverviewInfo, value))
                {
                    _audioOverviewInfo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioOverviewInfo)));
                }
            }
        }

        private bool _isAudioOverviewInfoExisted;

        public bool IsAudioOverviewInfoExisted
        {
            get { return _isAudioOverviewInfoExisted; }

            set
            {
                if (!Equals(_isAudioOverviewInfoExisted, value))
                {
                    _isAudioOverviewInfoExisted = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAudioOverviewInfoExisted)));
                }
            }
        }

        private SelectorBarItem _textInformationSelectedItem;

        public SelectorBarItem TextInformationSelectedItem
        {
            get { return _textInformationSelectedItem; }

            set
            {
                if (!Equals(_textInformationSelectedItem, value))
                {
                    _textInformationSelectedItem = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextInformationSelectedItem)));
                }
            }
        }

        private TextDetailInfo _textDetailInfo;

        public TextDetailInfo TextDetailInfo
        {
            get { return _textDetailInfo; }

            set
            {
                if (!Equals(_textDetailInfo, value))
                {
                    _textDetailInfo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextDetailInfo)));
                }
            }
        }

        private bool _isTextDetailInfoExisted;

        public bool IsTextDetailInfoExisted
        {
            get { return _isTextDetailInfoExisted; }

            set
            {
                if (!Equals(_isTextDetailInfoExisted, value))
                {
                    _isTextDetailInfoExisted = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTextDetailInfoExisted)));
                }
            }
        }

        private string _textOverviewInfo;

        public string TextOverviewInfo
        {
            get { return _textOverviewInfo; }

            set
            {
                if (!string.Equals(_textOverviewInfo, value))
                {
                    _textOverviewInfo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextOverviewInfo)));
                }
            }
        }

        private bool _isTextOverviewInfoExisted;

        public bool IsTextOverviewInfoExisted
        {
            get { return _isTextOverviewInfoExisted; }

            set
            {
                if (!Equals(_isTextOverviewInfoExisted, value))
                {
                    _isTextOverviewInfoExisted = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTextOverviewInfoExisted)));
                }
            }
        }

        private SelectorBarItem _imageInformationSelectedItem;

        public SelectorBarItem ImageInformationSelectedItem
        {
            get { return _imageInformationSelectedItem; }

            set
            {
                if (!Equals(_imageInformationSelectedItem, value))
                {
                    _imageInformationSelectedItem = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageInformationSelectedItem)));
                }
            }
        }

        private GeneralInfo _imageGeneralInfo;

        public GeneralInfo ImageGeneralInfo
        {
            get { return _imageGeneralInfo; }

            set
            {
                if (!Equals(_imageGeneralInfo, value))
                {
                    _imageGeneralInfo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageGeneralInfo)));
                }
            }
        }

        private ImageDetailInfo _imageDetailInfo;

        public ImageDetailInfo ImageDetailInfo
        {
            get { return _imageDetailInfo; }

            set
            {
                if (!Equals(_imageDetailInfo, value))
                {
                    _imageDetailInfo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageDetailInfo)));
                }
            }
        }

        private int _imageDetailInfoSelectedIndex;

        public int ImageDetailInfoSelectedIndex
        {
            get { return _imageDetailInfoSelectedIndex; }

            set
            {
                if (!Equals(_imageDetailInfoSelectedIndex, value))
                {
                    _imageDetailInfoSelectedIndex = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageDetailInfoSelectedIndex)));
                }
            }
        }

        private int _imageDetailInfoCount;

        public int ImageDetailInfoCount
        {
            get { return _imageDetailInfoCount; }

            set
            {
                if (!Equals(_imageDetailInfoCount, value))
                {
                    _imageDetailInfoCount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageDetailInfoCount)));
                }
            }
        }

        private ImageExifInfo _imageExifInfo;

        public ImageExifInfo ImageExifInfo
        {
            get { return _imageExifInfo; }

            set
            {
                if (!Equals(_imageExifInfo, value))
                {
                    _imageExifInfo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageExifInfo)));
                }
            }
        }

        private bool _isImageExifInfoExisted;

        public bool IsImageExifInfoExisted
        {
            get { return _isImageExifInfoExisted; }

            set
            {
                if (!Equals(_isImageExifInfoExisted, value))
                {
                    _isImageExifInfoExisted = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsImageExifInfoExisted)));
                }
            }
        }

        private string _imageOverviewInfo;

        public string ImageOverviewInfo
        {
            get { return _imageOverviewInfo; }

            set
            {
                if (!string.Equals(_imageOverviewInfo, value))
                {
                    _imageOverviewInfo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageOverviewInfo)));
                }
            }
        }

        private bool _IsImageOverviewInfoExisted;

        public bool IsImageOverviewInfoExisted
        {
            get { return _IsImageOverviewInfoExisted; }

            set
            {
                if (!Equals(_IsImageOverviewInfoExisted, value))
                {
                    _IsImageOverviewInfoExisted = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsImageOverviewInfoExisted)));
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
                FileName = Path.GetFileName(filePath);
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
            if (openFileDialog.ShowDialog() is DialogResult.OK && !string.IsNullOrEmpty(openFileDialog.FileName) && File.Exists(openFileDialog.FileName))
            {
                filePath = openFileDialog.FileName;
                FileName = Path.GetFileName(filePath);
                await GetFileInformationAsync(filePath);
            }
            openFileDialog.Dispose();
        }

        /// <summary>
        /// 查看使用说明
        /// </summary>
        private void OnFunctionInstructionClicked(object sender, RoutedEventArgs args)
        {
            FlyoutBase.ShowAttachedFlyout(ViewMoreButton);
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
        /// 复制基本信息到剪贴板
        /// </summary>
        private async void OnGeneralInfoCopyClicked(object sender, RoutedEventArgs args)
        {
            string generalInfo = await Task.Run(() =>
            {
                StringBuilder generalInfoBuilder = new();
                generalInfoBuilder.AppendLine(string.Format("{0}\t{1}", FileNameString, FileName));
                generalInfoBuilder.AppendLine(string.Format("{0}\t{1}", FileTypeString, FileType));
                generalInfoBuilder.AppendLine(string.Format("{0}\t{1}", FileSizeString, FileSize));
                generalInfoBuilder.AppendLine(string.Format("{0}\t{1}", FileSpaceUsageString, FileSpaceUsage));
                generalInfoBuilder.AppendLine(string.Format("{0}\t{1}", FileCreateTimeString, FileCreateTime));
                generalInfoBuilder.AppendLine(string.Format("{0}\t{1}", FileModifyTimeString, FileModifyTime));
                generalInfoBuilder.AppendLine(string.Format("{0}\t{1}", FileAccessTimeString, FileAccessTime));
                return generalInfoBuilder.ToString();
            });

            await MainWindow.Current.ShowNotificationAsync(new CopyPasteNotificationTip(CopyPasteHelper.CopyToClipboard(generalInfo)));
        }

        /// <summary>
        /// 复制视频基本信息到剪贴板
        /// </summary>
        private async void OnVideoGeneralInfoCopyClicked(object sender, RoutedEventArgs args)
        {
            string videoGeneralInfo = await Task.Run(() =>
            {
                if (videoInformation is not null && videoInformation.GeneralInfo is not null)
                {
                    StringBuilder videoGeneralInfoBuilder = new();
                    videoGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", CompleteNameString, videoInformation.GeneralInfo.CompleteName));
                    videoGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", FormatString, videoInformation.GeneralInfo.Format));
                    videoGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", FormatVersionString, videoInformation.GeneralInfo.FormatVersion));
                    videoGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", FormatProfileString, videoInformation.GeneralInfo.FormatProfile));
                    videoGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", CodecIDString, videoInformation.GeneralInfo.CodecID));
                    videoGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", UniqueIDString, videoInformation.GeneralInfo.UniqueID));
                    videoGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", EncodedDateString, videoInformation.GeneralInfo.EncodedDate));
                    videoGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", DurationString, videoInformation.GeneralInfo.Duration));
                    videoGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", OverallBitRateString, videoInformation.GeneralInfo.OverallBitRate));
                    videoGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", FrameRateString, videoInformation.GeneralInfo.FrameRate));
                    videoGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", StreamSizeString, videoInformation.GeneralInfo.StreamSize));
                    videoGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", RecordedDateString, videoInformation.GeneralInfo.RecordedDate));
                    videoGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", EncodedApplicationString, videoInformation.GeneralInfo.EncodedApplication));
                    videoGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", EncodedLibraryString, videoInformation.GeneralInfo.EncodedLibrary));
                    return videoGeneralInfoBuilder.ToString();
                }
                else
                {
                    return string.Empty;
                }
            });

            await MainWindow.Current.ShowNotificationAsync(new CopyPasteNotificationTip(CopyPasteHelper.CopyToClipboard(videoGeneralInfo)));
        }

        /// <summary>
        /// 复制视频视频信息到剪贴板
        /// </summary>
        private async void OnVideoDetailVideoInfoCopyClicked(object sender, RoutedEventArgs args)
        {
            string videoDetailVideoInfo = await Task.Run(() =>
            {
                if (videoInformation is not null && videoInformation.VideoDetailInfoList.Count > 0)
                {
                    StringBuilder videoDetailVideoInfoBuilder = new();

                    foreach (VideoDetailInfo videoDetailInfo in videoInformation.VideoDetailInfoList)
                    {
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", IDString, videoDetailInfo.ID));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", FormatString, videoDetailInfo.Format));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", FormatInfoString, videoDetailInfo.FormatInfo));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", FormatProfileString, videoDetailInfo.FormatProfile));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", CodecIDString, videoDetailInfo.CodecID));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", CodecIDInfoString, videoDetailInfo.CodecIDInfo));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", DurationString, videoDetailInfo.Duration));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", SourceDurationString, videoDetailInfo.SourceDuration));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", BitRateString, videoDetailInfo.BitRate));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", WidthString, videoDetailInfo.Width));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", HeightString, videoDetailInfo.Height));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", DisplayAspectRatioString, videoDetailInfo.DisplayAspectRatio));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", FrameRateString, videoDetailInfo.FrameRate));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", FrameRateModeString, videoDetailInfo.FrameRateMode));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", MinimumFrameRateString, videoDetailInfo.MinimumFrameRate));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", MaximumFrameRateString, videoDetailInfo.MaximumFrameRate));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", ColorSpaceString, videoDetailInfo.ColorSpace));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", ChromaSubsamplingString, videoDetailInfo.ChromaSubsampling));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", BitDepthString, videoDetailInfo.BitDepth));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", BitsPixelFrameString, videoDetailInfo.BitsPixelFrame));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", StreamSizeString, videoDetailInfo.StreamSize));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", SourceStreamSizeString, videoDetailInfo.SourceStreamSize));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", EncodedLibraryString, videoDetailInfo.EncodedLibrary));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", ColorRangeString, videoDetailInfo.ColorRange));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", ColorParimariesString, videoDetailInfo.ColorParimaries));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", TransferCharacteristicsString, videoDetailInfo.TransferCharacteristics));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", MatrixCoefficientsString, videoDetailInfo.MatrixCoefficients));
                        videoDetailVideoInfoBuilder.AppendLine(string.Format("{0}\t{1}", CodecConfigurationBoxString, videoDetailInfo.CodecConfigurationBox));
                        videoDetailVideoInfoBuilder.AppendLine();
                    }

                    return videoDetailVideoInfoBuilder.ToString();
                }
                else
                {
                    return string.Empty;
                }
            });

            await MainWindow.Current.ShowNotificationAsync(new CopyPasteNotificationTip(CopyPasteHelper.CopyToClipboard(videoDetailVideoInfo)));
        }

        /// <summary>
        /// 复制视频音频信息到剪贴板
        /// </summary>
        private async void OnVideoDetailAudioInfoCopyClicked(object sender, RoutedEventArgs args)
        {
            string videoDetailAudioInfo = await Task.Run(() =>
            {
                if (videoInformation is not null && videoInformation.AudioDetailInfoList.Count > 0)
                {
                    StringBuilder videoDetailAudioInfoBuilder = new();

                    foreach (AudioDetailInfo audioDetailInfo in videoInformation.AudioDetailInfoList)
                    {
                        videoDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", IDString, audioDetailInfo.ID));
                        videoDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", FormatString, audioDetailInfo.Format));
                        videoDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", FormatInfoString, audioDetailInfo.FormatInfo));
                        videoDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", CodecIDString, audioDetailInfo.CodecID));
                        videoDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", DurationString, audioDetailInfo.Duration));
                        videoDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", BitRateModeString, audioDetailInfo.BitRateMode));
                        videoDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", BitRateString, audioDetailInfo.BitRate));
                        videoDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", BitRateMaximumString, audioDetailInfo.BitRateMaximum));
                        videoDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", ChannelString, audioDetailInfo.Channel));
                        videoDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", ChannelLayoutString, audioDetailInfo.ChannelLayout));
                        videoDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", SamplingRateString, audioDetailInfo.SamplingRate));
                        videoDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", FrameRateString, audioDetailInfo.FrameRate));
                        videoDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", CompressionModeString, audioDetailInfo.CompressionMode));
                        videoDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", StreamSizeString, audioDetailInfo.StreamSize));
                        videoDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", DefaultString, audioDetailInfo.Default));
                        videoDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", AlternateGroupString, audioDetailInfo.AlternateGroup));
                        videoDetailAudioInfoBuilder.AppendLine();
                    }

                    return videoDetailAudioInfoBuilder.ToString();
                }
                else
                {
                    return string.Empty;
                }
            });

            await MainWindow.Current.ShowNotificationAsync(new CopyPasteNotificationTip(CopyPasteHelper.CopyToClipboard(videoDetailAudioInfo)));
        }

        /// <summary>
        /// 复制视频文本信息到剪贴板
        /// </summary>
        private async void OnVideoDetailTextInfoCopyClicked(object sender, RoutedEventArgs args)
        {
            string videoDetailTextInfo = await Task.Run(() =>
            {
                if (videoInformation is not null && videoInformation.TextDetailInfoList.Count > 0)
                {
                    StringBuilder videoDetailTextInfoBuilder = new();

                    foreach (TextDetailInfo textDetailInfo in videoInformation.TextDetailInfoList)
                    {
                        videoDetailTextInfoBuilder.AppendLine(string.Format("{0}\t{1}", FormatString, textDetailInfo.Format));
                        videoDetailTextInfoBuilder.AppendLine(string.Format("{0}\t{1}", DurationString, textDetailInfo.Duration));
                        videoDetailTextInfoBuilder.AppendLine(string.Format("{0}\t{1}", StartTimeString, textDetailInfo.StartTime));
                        videoDetailTextInfoBuilder.AppendLine(string.Format("{0}\t{1}", EndTimeString, textDetailInfo.EndTime));
                        videoDetailTextInfoBuilder.AppendLine(string.Format("{0}\t{1}", CompressionModeString, textDetailInfo.CompressionMode));
                        videoDetailTextInfoBuilder.AppendLine(string.Format("{0}\t{1}", CountOfEventsString, textDetailInfo.CountOfEvents));
                        videoDetailTextInfoBuilder.AppendLine(string.Format("{0}\t{1}", MinimumDurationPerEventString, textDetailInfo.MinimumDurationPerEvent));
                        videoDetailTextInfoBuilder.AppendLine(string.Format("{0}\t{1}", CountOfLinesString, textDetailInfo.CountOfLines));
                        videoDetailTextInfoBuilder.AppendLine(string.Format("{0}\t{1}", MaximumCountOfLinesPerEventString, textDetailInfo.MaximumCountOfLinesPerEvent));
                        videoDetailTextInfoBuilder.AppendLine();
                    }

                    return videoDetailTextInfoBuilder.ToString();
                }
                else
                {
                    return string.Empty;
                }
            });

            await MainWindow.Current.ShowNotificationAsync(new CopyPasteNotificationTip(CopyPasteHelper.CopyToClipboard(videoDetailTextInfo)));
        }

        /// <summary>
        /// 复制视频总览信息到剪贴板
        /// </summary>
        private async void OnVideoOverviewInfoCopyClicked(object sender, RoutedEventArgs args)
        {
            if (videoInformation is not null && !string.IsNullOrEmpty(videoInformation.VideoOverviewInformation))
            {
                await MainWindow.Current.ShowNotificationAsync(new CopyPasteNotificationTip(CopyPasteHelper.CopyToClipboard(videoInformation.VideoOverviewInformation)));
            }
        }

        /// <summary>
        /// 视频信息选中项发生变化时触发的事件
        /// </summary>
        private void OnVideoInformationSelectorBarSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            VideoInformationSelectedItem = sender.SelectedItem;
        }

        /// <summary>
        /// 视频信息的前一个视频流信息
        /// </summary>
        private void OnForwardVideoDetailVideoInfoClicked(object sender, RoutedEventArgs args)
        {
            if (videoInformation is not null && videoInformation.VideoDetailInfoList.Count > 0 && Equals(videoInformation.VideoDetailInfoList.Count, VideoDetailVideoInfoCount) && VideoDetailVideoInfoSelectedIndex > 1 && VideoDetailVideoInfoSelectedIndex <= VideoDetailVideoInfoCount)
            {
                VideoDetailVideoInfoSelectedIndex--;
                VideoDetailVideoInfo = videoInformation.VideoDetailInfoList[VideoDetailVideoInfoSelectedIndex - 1];
            }
        }

        /// <summary>
        /// 视频信息的视频流选中值发生变化时触发的事件
        /// </summary>
        private void OnVideoDetailVideoInfoSelectedIndexValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                VideoDetailVideoInfoSelectedIndex = newValue;
                VideoDetailVideoInfoSelectedIndex = Convert.ToInt32(args.OldValue);

                if (videoInformation is not null && videoInformation.VideoDetailInfoList.Count > 0 && Equals(videoInformation.VideoDetailInfoList.Count, VideoDetailVideoInfoCount))
                {
                    if (newValue > VideoDetailVideoInfoCount)
                    {
                        VideoDetailVideoInfoSelectedIndex = VideoDetailVideoInfoCount;
                    }
                    else if (newValue < 1)
                    {
                        VideoDetailVideoInfoSelectedIndex = 1;
                    }
                    else
                    {
                        VideoDetailVideoInfoSelectedIndex = newValue;
                    }

                    VideoDetailVideoInfo = videoInformation.VideoDetailInfoList[VideoDetailVideoInfoSelectedIndex - 1];
                }
            }
        }

        /// <summary>
        /// 视频信息的后一个视频流信息
        /// </summary>
        private void OnNextVideoDetailVideoInfoClicked(object sender, RoutedEventArgs args)
        {
            if (videoInformation is not null && Equals(videoInformation.VideoDetailInfoList.Count, VideoDetailVideoInfoCount) && VideoDetailVideoInfoSelectedIndex >= 1 && VideoDetailVideoInfoSelectedIndex < VideoDetailVideoInfoCount)
            {
                VideoDetailVideoInfoSelectedIndex++;
                VideoDetailVideoInfo = videoInformation.VideoDetailInfoList[VideoDetailVideoInfoSelectedIndex - 1];
            }
        }

        /// <summary>
        /// 视频信息的前一个音频流信息
        /// </summary>
        private void OnForwardVideoDetailAudioInfoClicked(object sender, RoutedEventArgs args)
        {
            if (videoInformation is not null && videoInformation.VideoDetailInfoList.Count > 0 && Equals(videoInformation.AudioDetailInfoList.Count, VideoDetailAudioInfoCount) && VideoDetailAudioInfoSelectedIndex > 1 && VideoDetailAudioInfoSelectedIndex <= VideoDetailAudioInfoCount)
            {
                VideoDetailAudioInfoSelectedIndex--;
                VideoDetailAudioInfo = videoInformation.AudioDetailInfoList[VideoDetailAudioInfoSelectedIndex - 1];
            }
        }

        /// <summary>
        /// 视频信息的音频流信息选中值发生变化时触发的事件
        /// </summary>
        private void OnVideoDetailAudioInfoSelectedIndexValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                VideoDetailAudioInfoSelectedIndex = newValue;
                VideoDetailAudioInfoSelectedIndex = Convert.ToInt32(args.OldValue);

                if (videoInformation is not null && videoInformation.AudioDetailInfoList.Count > 0 && Equals(videoInformation.AudioDetailInfoList.Count, VideoDetailAudioInfoCount))
                {
                    if (newValue > VideoDetailAudioInfoCount)
                    {
                        VideoDetailAudioInfoSelectedIndex = VideoDetailAudioInfoCount;
                    }
                    else if (newValue < 1)
                    {
                        VideoDetailAudioInfoSelectedIndex = 1;
                    }
                    else
                    {
                        VideoDetailAudioInfoSelectedIndex = newValue;
                    }

                    VideoDetailAudioInfo = videoInformation.AudioDetailInfoList[VideoDetailAudioInfoSelectedIndex - 1];
                }
            }
        }

        /// <summary>
        /// 视频信息的后一个音频流信息
        /// </summary>
        private void OnNextVideoDetailAudioInfoClicked(object sender, RoutedEventArgs args)
        {
            if (videoInformation is not null && videoInformation.AudioDetailInfoList.Count > 0 && Equals(videoInformation.AudioDetailInfoList.Count, VideoDetailAudioInfoCount) && VideoDetailAudioInfoSelectedIndex >= 1 && VideoDetailAudioInfoSelectedIndex < VideoDetailAudioInfoCount)
            {
                VideoDetailAudioInfoSelectedIndex++;
                VideoDetailAudioInfo = videoInformation.AudioDetailInfoList[VideoDetailAudioInfoSelectedIndex - 1];
            }
        }

        /// <summary>
        /// 视频信息的前一个文本流信息
        /// </summary>
        private void OnForwardVideoDetailTextInfoClicked(object sender, RoutedEventArgs args)
        {
            if (videoInformation is not null && videoInformation.TextDetailInfoList.Count > 0 && Equals(videoInformation.TextDetailInfoList.Count, VideoDetailTextInfoCount) && VideoDetailTextInfoSelectedIndex > 1 && VideoDetailTextInfoSelectedIndex <= VideoDetailTextInfoCount)
            {
                VideoDetailTextInfoSelectedIndex--;
                VideoDetailTextInfo = videoInformation.TextDetailInfoList[VideoDetailTextInfoSelectedIndex - 1];
            }
        }

        /// <summary>
        /// 视频信息的文本流信息选中值发生变化时触发的事件
        /// </summary>
        private void OnVideoDetailTextInfoSelectedIndexValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                VideoDetailTextInfoSelectedIndex = newValue;
                VideoDetailTextInfoSelectedIndex = Convert.ToInt32(args.OldValue);

                if (videoInformation is not null && videoInformation.TextDetailInfoList.Count > 0 && Equals(videoInformation.TextDetailInfoList.Count, VideoDetailTextInfoCount))
                {
                    if (newValue > VideoDetailTextInfoCount)
                    {
                        VideoDetailTextInfoSelectedIndex = VideoDetailTextInfoCount;
                    }
                    else if (newValue < 1)
                    {
                        VideoDetailTextInfoSelectedIndex = 1;
                    }
                    else
                    {
                        VideoDetailTextInfoSelectedIndex = newValue;
                    }

                    VideoDetailTextInfo = videoInformation.TextDetailInfoList[VideoDetailTextInfoSelectedIndex - 1];
                }
            }
        }

        /// <summary>
        /// 视频信息的后一个文本流信息
        /// </summary>
        private void OnNextVideoDetailTextInfoClicked(object sender, RoutedEventArgs args)
        {
            if (videoInformation is not null && videoInformation.TextDetailInfoList.Count > 0 && Equals(videoInformation.TextDetailInfoList.Count, VideoDetailTextInfoCount) && VideoDetailTextInfoSelectedIndex >= 1 && VideoDetailTextInfoSelectedIndex < VideoDetailTextInfoCount)
            {
                VideoDetailTextInfoSelectedIndex++;
                VideoDetailTextInfo = videoInformation.TextDetailInfoList[VideoDetailTextInfoSelectedIndex - 1];
            }
        }

        /// <summary>
        /// 复制音频基本信息到剪贴板
        /// </summary>
        private async void OnAudioGeneralInfoCopyClicked(object sender, RoutedEventArgs args)
        {
            string audioGeneralInfo = await Task.Run(() =>
            {
                if (audioInformation is not null && audioInformation.GeneralInfo is not null)
                {
                    StringBuilder audioGeneralInfoBuilder = new();
                    audioGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", CompleteNameString, audioInformation.GeneralInfo.CompleteName));
                    audioGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", FormatString, audioInformation.GeneralInfo.Format));
                    audioGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", FormatVersionString, audioInformation.GeneralInfo.FormatVersion));
                    audioGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", FormatProfileString, audioInformation.GeneralInfo.FormatProfile));
                    audioGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", CodecIDString, audioInformation.GeneralInfo.CodecID));
                    audioGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", UniqueIDString, audioInformation.GeneralInfo.UniqueID));
                    audioGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", EncodedDateString, audioInformation.GeneralInfo.EncodedDate));
                    audioGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", DurationString, audioInformation.GeneralInfo.Duration));
                    audioGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", OverallBitRateString, audioInformation.GeneralInfo.OverallBitRate));
                    audioGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", FrameRateString, audioInformation.GeneralInfo.FrameRate));
                    audioGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", StreamSizeString, audioInformation.GeneralInfo.StreamSize));
                    audioGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", RecordedDateString, audioInformation.GeneralInfo.RecordedDate));
                    audioGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", EncodedApplicationString, audioInformation.GeneralInfo.EncodedApplication));
                    audioGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", EncodedLibraryString, audioInformation.GeneralInfo.EncodedLibrary));
                    audioGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", AlbumString, audioInformation.GeneralInfo.Album));
                    audioGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", TrackNameString, audioInformation.GeneralInfo.TrackName));
                    audioGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", PerformerString, audioInformation.GeneralInfo.Performer));
                    return audioGeneralInfoBuilder.ToString();
                }
                else
                {
                    return string.Empty;
                }
            });

            await MainWindow.Current.ShowNotificationAsync(new CopyPasteNotificationTip(CopyPasteHelper.CopyToClipboard(audioGeneralInfo)));
        }

        /// <summary>
        /// 复制音频音频信息到剪贴板
        /// </summary>
        private async void OnAudioDetailAudioInfoCopyClicked(object sender, RoutedEventArgs args)
        {
            string audioDetailAudioInfo = await Task.Run(() =>
            {
                if (audioInformation is not null && audioInformation.AudioDetailInfoList.Count > 0)
                {
                    StringBuilder audioDetailAudioInfoBuilder = new();

                    foreach (AudioDetailInfo audioDetailInfo in audioInformation.AudioDetailInfoList)
                    {
                        audioDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", IDString, audioDetailInfo.ID));
                        audioDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", FormatString, audioDetailInfo.Format));
                        audioDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", FormatInfoString, audioDetailInfo.FormatInfo));
                        audioDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", CodecIDString, audioDetailInfo.CodecID));
                        audioDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", DurationString, audioDetailInfo.Duration));
                        audioDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", BitRateModeString, audioDetailInfo.BitRateMode));
                        audioDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", BitRateString, audioDetailInfo.BitRate));
                        audioDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", BitRateMaximumString, audioDetailInfo.BitRateMaximum));
                        audioDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", ChannelString, audioDetailInfo.Channel));
                        audioDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", ChannelLayoutString, audioDetailInfo.ChannelLayout));
                        audioDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", SamplingRateString, audioDetailInfo.SamplingRate));
                        audioDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", FrameRateString, audioDetailInfo.FrameRate));
                        audioDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", CompressionModeString, audioDetailInfo.CompressionMode));
                        audioDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", StreamSizeString, audioDetailInfo.StreamSize));
                        audioDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", DefaultString, audioDetailInfo.Default));
                        audioDetailAudioInfoBuilder.AppendLine(string.Format("{0}\t{1}", AlternateGroupString, audioDetailInfo.AlternateGroup));
                        audioDetailAudioInfoBuilder.AppendLine();
                    }

                    return audioDetailAudioInfoBuilder.ToString();
                }
                else
                {
                    return string.Empty;
                }
            });

            await MainWindow.Current.ShowNotificationAsync(new CopyPasteNotificationTip(CopyPasteHelper.CopyToClipboard(audioDetailAudioInfo)));
        }

        /// <summary>
        /// 复制音频总览信息到剪贴板
        /// </summary>
        private async void OnAudioOverviewInfoCopyClicked(object sender, RoutedEventArgs args)
        {
            if (audioInformation is not null && !string.IsNullOrEmpty(audioInformation.AudioOverviewInformation))
            {
                await MainWindow.Current.ShowNotificationAsync(new CopyPasteNotificationTip(CopyPasteHelper.CopyToClipboard(audioInformation.AudioOverviewInformation)));
            }
        }

        /// <summary>
        /// 音频信息选中项发生变化时触发的事件
        /// </summary>
        private void OnAudioInformationSelectorBarSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            AudioInformationSelectedItem = sender.SelectedItem;
        }

        /// <summary>
        /// 音频信息的前一个音频流信息
        /// </summary>
        private void OnForwardAudioDetailAudioInfoClicked(object sender, RoutedEventArgs args)
        {
            if (audioInformation is not null && audioInformation.AudioDetailInfoList.Count > 0 && Equals(audioInformation.AudioDetailInfoList.Count, AudioDetailAudioInfoCount) && AudioDetailAudioInfoSelectedIndex > 1 && AudioDetailAudioInfoSelectedIndex <= AudioDetailAudioInfoCount)
            {
                AudioDetailAudioInfoSelectedIndex--;
                AudioDetailAudioInfo = audioInformation.AudioDetailInfoList[AudioDetailAudioInfoSelectedIndex - 1];
            }
        }

        /// <summary>
        /// 音频信息的音频流信息选中值发生变化时触发的事件
        /// </summary>
        private void OnAudioDetailAudioInfoSelectedIndexValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                AudioDetailAudioInfoSelectedIndex = newValue;
                AudioDetailAudioInfoSelectedIndex = Convert.ToInt32(args.OldValue);

                if (audioInformation is not null && audioInformation.AudioDetailInfoList.Count > 0 && Equals(audioInformation.AudioDetailInfoList.Count, AudioDetailAudioInfoCount))
                {
                    if (newValue > AudioDetailAudioInfoCount)
                    {
                        AudioDetailAudioInfoSelectedIndex = AudioDetailAudioInfoCount;
                    }
                    else if (newValue < 1)
                    {
                        AudioDetailAudioInfoSelectedIndex = 1;
                    }
                    else
                    {
                        AudioDetailAudioInfoSelectedIndex = newValue;
                    }

                    AudioDetailAudioInfo = audioInformation.AudioDetailInfoList[AudioDetailAudioInfoSelectedIndex - 1];
                }
            }
        }

        /// <summary>
        /// 音频信息的后一个音频流信息
        /// </summary>
        private void OnNextAudioDetailAudioInfoClicked(object sender, RoutedEventArgs args)
        {
            if (audioInformation is not null && audioInformation.AudioDetailInfoList.Count > 0 && Equals(audioInformation.AudioDetailInfoList.Count, AudioDetailAudioInfoCount) && AudioDetailAudioInfoSelectedIndex >= 1 && AudioDetailAudioInfoSelectedIndex < AudioDetailAudioInfoCount)
            {
                AudioDetailAudioInfoSelectedIndex++;
                AudioDetailAudioInfo = audioInformation.AudioDetailInfoList[AudioDetailAudioInfoSelectedIndex - 1];
            }
        }

        /// <summary>
        /// 复制文本文本信息到剪贴板
        /// </summary>
        private async void OnTextDetailTextInfoCopyClicked(object sender, RoutedEventArgs args)
        {
            string textDetailTextInfo = await Task.Run(() =>
            {
                if (textInformation is not null)
                {
                    StringBuilder textDetailTextInfoBuilder = new();
                    textDetailTextInfoBuilder.AppendLine(string.Format("{0}\t{1}", FormatString, textInformation.TextDetailInfo.Format));
                    textDetailTextInfoBuilder.AppendLine(string.Format("{0}\t{1}", DurationString, textInformation.TextDetailInfo.Duration));
                    textDetailTextInfoBuilder.AppendLine(string.Format("{0}\t{1}", StartTimeString, textInformation.TextDetailInfo.StartTime));
                    textDetailTextInfoBuilder.AppendLine(string.Format("{0}\t{1}", EndTimeString, textInformation.TextDetailInfo.EndTime));
                    textDetailTextInfoBuilder.AppendLine(string.Format("{0}\t{1}", CompressionModeString, textInformation.TextDetailInfo.CompressionMode));
                    textDetailTextInfoBuilder.AppendLine(string.Format("{0}\t{1}", CountOfEventsString, textInformation.TextDetailInfo.CountOfEvents));
                    textDetailTextInfoBuilder.AppendLine(string.Format("{0}\t{1}", MinimumDurationPerEventString, textInformation.TextDetailInfo.MinimumDurationPerEvent));
                    textDetailTextInfoBuilder.AppendLine(string.Format("{0}\t{1}", CountOfLinesString, textInformation.TextDetailInfo.CountOfLines));
                    textDetailTextInfoBuilder.AppendLine(string.Format("{0}\t{1}", MaximumCountOfLinesPerEventString, textInformation.TextDetailInfo.MaximumCountOfLinesPerEvent));
                    return textDetailTextInfoBuilder.ToString();
                }
                else
                {
                    return string.Empty;
                }
            });

            await MainWindow.Current.ShowNotificationAsync(new CopyPasteNotificationTip(CopyPasteHelper.CopyToClipboard(textDetailTextInfo)));
        }

        /// <summary>
        /// 复制文本总览信息到剪贴板
        /// </summary>
        private async void OnTextOverviewInfoCopyClicked(object sender, RoutedEventArgs args)
        {
            if (textInformation is not null && !string.IsNullOrEmpty(textInformation.TextOverviewInformation))
            {
                await MainWindow.Current.ShowNotificationAsync(new CopyPasteNotificationTip(CopyPasteHelper.CopyToClipboard(textInformation.TextOverviewInformation)));
            }
        }

        /// <summary>
        /// 文本信息选中项发生变化时触发的事件
        /// </summary>
        private void OnTextInformationSelectorBarSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            TextInformationSelectedItem = sender.SelectedItem;
        }

        /// <summary>
        /// 复制图像基本信息到剪贴板
        /// </summary>
        private async void OnImageGeneralInfoCopyClicked(object sender, RoutedEventArgs args)
        {
            string imageGeneralInfo = await Task.Run(() =>
            {
                if (imageInformation is not null && imageInformation.GeneralInfo is not null)
                {
                    StringBuilder imageGeneralInfoBuilder = new();
                    imageGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", CompleteNameString, imageInformation.GeneralInfo.CompleteName));
                    imageGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", FormatString, imageInformation.GeneralInfo.Format));
                    imageGeneralInfoBuilder.AppendLine(string.Format("{0}\t{1}", FormatVersionString, imageInformation.GeneralInfo.FileSize));
                    return imageGeneralInfoBuilder.ToString();
                }
                else
                {
                    return string.Empty;
                }
            });

            await MainWindow.Current.ShowNotificationAsync(new CopyPasteNotificationTip(CopyPasteHelper.CopyToClipboard(imageGeneralInfo)));
        }

        /// <summary>
        /// 复制图像图像信息到剪贴板
        /// </summary>
        private async void OnImageDetailInfoCopyClicked(object sender, RoutedEventArgs args)
        {
            string imageDetailInfo = await Task.Run(() =>
            {
                if (imageInformation is not null && imageInformation.ImageDetailInfoList.Count > 0)
                {
                    StringBuilder imageDetailInfoBuilder = new();

                    foreach (ImageDetailInfo imageDetailInfo in imageInformation.ImageDetailInfoList)
                    {
                        imageDetailInfoBuilder.AppendLine(string.Format("{0}\t{1}", FormatString, imageDetailInfo.Format));
                        imageDetailInfoBuilder.AppendLine(string.Format("{0}\t{1}", FormatInfoString, imageDetailInfo.FormatInfo));
                        imageDetailInfoBuilder.AppendLine(string.Format("{0}\t{1}", FormatCompressionString, imageDetailInfo.FormatCompression));
                        imageDetailInfoBuilder.AppendLine(string.Format("{0}\t{1}", WidthString, imageDetailInfo.Width));
                        imageDetailInfoBuilder.AppendLine(string.Format("{0}\t{1}", HeightString, imageDetailInfo.Height));
                        imageDetailInfoBuilder.AppendLine(string.Format("{0}\t{1}", ColorSpaceString, imageDetailInfo.ColorSpace));
                        imageDetailInfoBuilder.AppendLine(string.Format("{0}\t{1}", ChromaSubsamplingString, imageDetailInfo.ChromaSubsampling));
                        imageDetailInfoBuilder.AppendLine(string.Format("{0}\t{1}", BitDepthString, imageDetailInfo.BitDepth));
                        imageDetailInfoBuilder.AppendLine(string.Format("{0}\t{1}", CompressionModeString, imageDetailInfo.CompressionMode));
                        imageDetailInfoBuilder.AppendLine(string.Format("{0}\t{1}", StreamSizeString, imageDetailInfo.StreamSize));
                        imageDetailInfoBuilder.AppendLine();
                    }

                    return imageDetailInfoBuilder.ToString();
                }
                else
                {
                    return string.Empty;
                }
            });

            await MainWindow.Current.ShowNotificationAsync(new CopyPasteNotificationTip(CopyPasteHelper.CopyToClipboard(imageDetailInfo)));
        }

        /// <summary>
        /// 复制图像 EXIF 信息到剪贴板
        /// </summary>
        private async void OnImageExifInfoCopyClicked(object sender, RoutedEventArgs args)
        {
            string imageExifInfo = await Task.Run(() =>
            {
                if (imageInformation is not null && imageInformation.ImageExifInfo is not null)
                {
                    StringBuilder imageExifInfoBuilder = new();
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", CameraManufacturerString, imageInformation.ImageExifInfo.CameraManufacturer));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", CameraModelString, imageInformation.ImageExifInfo.CameraModel));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", FstopString, imageInformation.ImageExifInfo.Fstop));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", ExposureTimeString, imageInformation.ImageExifInfo.ExposureTime));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", ShootingDateString, imageInformation.ImageExifInfo.ShootingDate));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", ShutterSpeedString, imageInformation.ImageExifInfo.ShutterSpeed));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", MeteringModeString, imageInformation.ImageExifInfo.MeteringMode));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", FlashModeString, imageInformation.ImageExifInfo.FlashMode));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", XResolutionString, imageInformation.ImageExifInfo.XResolution));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", YResolutionString, imageInformation.ImageExifInfo.YResolution));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", WidthString, imageInformation.ImageExifInfo.Width));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", HeightString, imageInformation.ImageExifInfo.Height));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", FNumberString, imageInformation.ImageExifInfo.FNumber));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", ExposureProgramString, imageInformation.ImageExifInfo.ExposureProgram));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", SpectralSenseString, imageInformation.ImageExifInfo.SpectralSense));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", ISOSpeedString, imageInformation.ImageExifInfo.ISOSpeed));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", OECFString, imageInformation.ImageExifInfo.OECF));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", VersionString, imageInformation.ImageExifInfo.Version));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", CompressionConfigString, imageInformation.ImageExifInfo.CompressionConfig));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", CompressionBitsPerPixelString, imageInformation.ImageExifInfo.CompressionBitsPerPixel));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", ApertureString, imageInformation.ImageExifInfo.Aperture));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", BrightnessString, imageInformation.ImageExifInfo.Brightness));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", ExposureCompensationString, imageInformation.ImageExifInfo.ExposureCompensation));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", MaxApertureString, imageInformation.ImageExifInfo.MaxAperture));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", SubjectDistString, imageInformation.ImageExifInfo.SubjectDist));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", LightSourceString, imageInformation.ImageExifInfo.LightSource));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", FocalLengthString, imageInformation.ImageExifInfo.FocalLength));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", FlashPixVersionString, imageInformation.ImageExifInfo.FlashPixVersion));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", ColorIndicatesString, imageInformation.ImageExifInfo.ColorIndicates));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", InteropString, imageInformation.ImageExifInfo.Interop));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", FlashEnergyString, imageInformation.ImageExifInfo.FlashEnergy));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", SpatialFrequencyResponseString, imageInformation.ImageExifInfo.SpatialFrequencyResponse));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", FocalXResString, imageInformation.ImageExifInfo.FocalXRes));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", FocalYResString, imageInformation.ImageExifInfo.FocalYRes));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", FocalResUnitString, imageInformation.ImageExifInfo.FocalResUnit));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", ExposureIndexString, imageInformation.ImageExifInfo.ExposureIndex));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", SenseMethodString, imageInformation.ImageExifInfo.SenseMethod));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", SceneTypeString, imageInformation.ImageExifInfo.SceneType));
                    imageExifInfoBuilder.AppendLine(string.Format("{0}\t{1}", CfaPatternString, imageInformation.ImageExifInfo.CfaPattern));
                    return imageExifInfoBuilder.ToString();
                }
                else
                {
                    return string.Empty;
                }
            });

            await MainWindow.Current.ShowNotificationAsync(new CopyPasteNotificationTip(CopyPasteHelper.CopyToClipboard(imageExifInfo)));
        }

        /// <summary>
        /// 复制图像总览信息到剪贴板
        /// </summary>
        private async void OnImageOverviewInfoCopyClicked(object sender, RoutedEventArgs args)
        {
            if (imageInformation is not null && !string.IsNullOrEmpty(imageInformation.ImageOverviewInformation))
            {
                await MainWindow.Current.ShowNotificationAsync(new CopyPasteNotificationTip(CopyPasteHelper.CopyToClipboard(imageInformation.ImageOverviewInformation)));
            }
        }

        /// <summary>
        /// 图像信息选中项发生变化时触发的事件
        /// </summary>
        private void OnImageInformationSelectorBarSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            ImageInformationSelectedItem = sender.SelectedItem;
        }

        /// <summary>
        /// 图片信息的前一个图片流信息
        /// </summary>
        private void OnForwardImageDetailInfoClicked(object sender, RoutedEventArgs args)
        {
            if (imageInformation is not null && imageInformation.ImageDetailInfoList.Count > 0 && Equals(imageInformation.ImageDetailInfoList.Count, ImageDetailInfoCount) && ImageDetailInfoSelectedIndex > 1 && ImageDetailInfoSelectedIndex <= ImageDetailInfoCount)
            {
                ImageDetailInfoSelectedIndex--;
                ImageDetailInfo = imageInformation.ImageDetailInfoList[ImageDetailInfoSelectedIndex - 1];
            }
        }

        /// <summary>
        /// 图片信息的图片流选中值发生变化时触发的事件
        /// </summary>
        private void OnImageDetailInfoSelectedIndexValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (args.NewValue is not double.NaN && args.OldValue is not double.NaN)
            {
                int newValue = Convert.ToInt32(args.NewValue);
                ImageDetailInfoSelectedIndex = newValue;
                ImageDetailInfoSelectedIndex = Convert.ToInt32(args.OldValue);

                if (imageInformation is not null && imageInformation.ImageDetailInfoList.Count > 0 && Equals(imageInformation.ImageDetailInfoList.Count, ImageDetailInfoCount))
                {
                    if (newValue > ImageDetailInfoCount)
                    {
                        ImageDetailInfoSelectedIndex = ImageDetailInfoCount;
                    }
                    else if (newValue < 1)
                    {
                        ImageDetailInfoSelectedIndex = 1;
                    }
                    else
                    {
                        ImageDetailInfoSelectedIndex = newValue;
                    }

                    ImageDetailInfo = imageInformation.ImageDetailInfoList[ImageDetailInfoSelectedIndex - 1];
                }
            }
        }

        /// <summary>
        /// 图片信息的后一个图片流信息
        /// </summary>
        private void OnNextImageDetailInfoClicked(object sender, RoutedEventArgs args)
        {
            if (imageInformation is not null && Equals(imageInformation.ImageDetailInfoList.Count, ImageDetailInfoCount) && ImageDetailInfoSelectedIndex >= 1 && ImageDetailInfoSelectedIndex < ImageDetailInfoCount)
            {
                ImageDetailInfoSelectedIndex++;
                ImageDetailInfo = imageInformation.ImageDetailInfoList[ImageDetailInfoSelectedIndex - 1];
            }
        }

        #endregion 第二部分：文件信息页面——挂载的事件

        /// <summary>
        /// 获取文件信息
        /// </summary>
        private async Task GetFileInformationAsync(string filePath)
        {
            FileInformationResultKind = FileInformationResultKind.Parsing;
            FileThumbnailImage = null;
            videoInformation = null;
            VideoInformationSelectedItem = VideoInformationSelectorBar.Items[0];
            VideoGeneralInfo = new();
            VideoDetailVideoInfo = new();
            VideoDetailVideoInfoSelectedIndex = 0;
            VideoDetailVideoInfoCount = 0;
            VideoDetailAudioInfo = new();
            VideoDetailAudioInfoSelectedIndex = 0;
            VideoDetailAudioInfoCount = 0;
            VideoDetailTextInfo = new();
            VideoDetailTextInfoSelectedIndex = 0;
            VideoDetailTextInfoCount = 0;
            VideoOverviewInfo = string.Empty;
            IsVideoOverviewInfoExisted = false;
            audioInformation = null;
            AudioInformationSelectedItem = AudioInformationSelectorBar.Items[0];
            AudioGeneralInfo = new();
            AudioDetailAudioInfo = new();
            AudioDetailAudioInfoSelectedIndex = 0;
            AudioDetailAudioInfoCount = 0;
            AudioOverviewInfo = string.Empty;
            IsAudioOverviewInfoExisted = false;
            textInformation = null;
            TextDetailInfo = new();
            IsTextOverviewInfoExisted = false;
            TextOverviewInfo = string.Empty;
            IsTextOverviewInfoExisted = false;
            imageInformation = null;
            ImageInformationSelectedItem = ImageInformationSelectorBar.Items[0];
            ImageGeneralInfo = new();
            ImageDetailInfo = new();
            ImageDetailInfoSelectedIndex = 0;
            ImageDetailInfoCount = 0;
            ImageExifInfo = new();
            IsImageExifInfoExisted = false;
            ImageOverviewInfo = string.Empty;
            IsImageOverviewInfoExisted = false;
            await GetThumbnailAsync(filePath);
            FileInformationModel fileInformation = await GetGeneralInformationAsync(filePath);
            FileType = string.IsNullOrEmpty(fileInformation.FileType) ? NotAvailableString : fileInformation.FileType;
            FileSize = string.IsNullOrEmpty(fileInformation.FileSize) ? NotAvailableString : fileInformation.FileSize;
            FileSpaceUsage = string.IsNullOrEmpty(fileInformation.SpaceUsage) ? NotAvailableString : fileInformation.SpaceUsage;
            FileCreateTime = string.IsNullOrEmpty(fileInformation.CreateTime) ? NotAvailableString : fileInformation.CreateTime;
            FileModifyTime = string.IsNullOrEmpty(fileInformation.ModifyTime) ? NotAvailableString : fileInformation.ModifyTime;
            FileAccessTime = string.IsNullOrEmpty(fileInformation.AccessTime) ? NotAvailableString : fileInformation.AccessTime;
            FileInformationResultKind fileInformationResultKind = await GetFileTypeAsync(filePath);
            if (fileInformationResultKind is FileInformationResultKind.VideoFile)
            {
                videoInformation = await GetVideoInformationAsync(filePath);
                VideoGeneralInfo = videoInformation.GeneralInfo;
                VideoDetailVideoInfoCount = videoInformation.VideoDetailInfoList.Count;
                VideoDetailVideoInfo = videoInformation.VideoDetailInfoList.Count is 0 ? VideoDetailVideoInfo : videoInformation.VideoDetailInfoList[0];
                VideoDetailVideoInfoSelectedIndex = 1;
                VideoDetailAudioInfoCount = videoInformation.AudioDetailInfoList.Count;
                VideoDetailAudioInfo = videoInformation.AudioDetailInfoList.Count is 0 ? VideoDetailAudioInfo : videoInformation.AudioDetailInfoList[0];
                VideoDetailAudioInfoSelectedIndex = 1;
                VideoDetailTextInfoCount = videoInformation.TextDetailInfoList.Count;
                VideoDetailTextInfo = videoInformation.TextDetailInfoList.Count is 0 ? VideoDetailTextInfo : videoInformation.TextDetailInfoList[0];
                VideoDetailTextInfoSelectedIndex = 1;

                if (!string.IsNullOrEmpty(videoInformation.VideoOverviewInformation))
                {
                    VideoOverviewInfo = videoInformation.VideoOverviewInformation;
                    IsVideoOverviewInfoExisted = true;
                }
            }
            else if (fileInformationResultKind is FileInformationResultKind.AudioFile)
            {
                audioInformation = await GetAudioInformationAsync(filePath);
                AudioGeneralInfo = audioInformation.GeneralInfo;
                AudioDetailAudioInfoCount = audioInformation.AudioDetailInfoList.Count;
                AudioDetailAudioInfo = audioInformation.AudioDetailInfoList.Count is 0 ? AudioDetailAudioInfo : audioInformation.AudioDetailInfoList[0];
                AudioDetailAudioInfoSelectedIndex = 1;

                if (!string.IsNullOrEmpty(audioInformation.AudioOverviewInformation))
                {
                    AudioOverviewInfo = audioInformation.AudioOverviewInformation;
                    IsAudioOverviewInfoExisted = true;
                }
            }
            else if (fileInformationResultKind is FileInformationResultKind.TextFile)
            {
                textInformation = await GetTextInformationAsync(filePath);
                TextInformationSelectedItem = TextInformationSelectorBar.Items[0];
                if (textInformation.TextDetailInfo is not null)
                {
                    TextDetailInfo = textInformation.TextDetailInfo;
                    IsTextDetailInfoExisted = true;
                }

                if (!string.IsNullOrEmpty(textInformation.TextOverviewInformation))
                {
                    TextOverviewInfo = textInformation.TextOverviewInformation;
                    IsTextOverviewInfoExisted = true;
                }
            }
            else if (fileInformationResultKind is FileInformationResultKind.ImageFile)
            {
                imageInformation = await GetImageInformationAsync(filePath);
                ImageInformationSelectedItem = ImageInformationSelectorBar.Items[0];
                ImageGeneralInfo = imageInformation.GeneralInfo;
                ImageDetailInfoCount = imageInformation.ImageDetailInfoList.Count;
                ImageDetailInfo = imageInformation.ImageDetailInfoList.Count is 0 ? ImageDetailInfo : imageInformation.ImageDetailInfoList[0];
                ImageDetailInfoSelectedIndex = 1;
                ImageExifInfo = imageInformation.ImageExifInfo;
                if (imageInformation.ImageExifInfo is not null)
                {
                    ImageExifInfo = imageInformation.ImageExifInfo;
                    IsImageExifInfoExisted = true;
                }

                if (!string.IsNullOrEmpty(imageInformation.ImageOverviewInformation))
                {
                    ImageOverviewInfo = imageInformation.ImageOverviewInformation;
                    IsImageOverviewInfoExisted = true;
                }
            }

            FileInformationResultKind = fileInformationResultKind;
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

        /// <summary>
        /// 获取文件基本信息
        /// </summary>
        private async Task<FileInformationModel> GetGeneralInformationAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                FileInformationModel fileInformation = new();

                try
                {
                    if (File.Exists(filePath))
                    {
                        // 获取文件类型
                        Shell32Library.SHGetFileInfo(filePath, 0, out SHFILEINFO shFileInfo, (uint)Marshal.SizeOf<SHFILEINFO>(), SHGFI.SHGFI_TYPENAME);
                        fileInformation.FileType = string.Format("{0} ({1})", shFileInfo.szTypeName, Path.GetExtension(filePath).ToLowerInvariant());

                        // 获取文件大小
                        FileInfo fileInfo = new(filePath);
                        fileInformation.FileSize = string.Format(FileSizeDescriptionString, VolumeSizeHelper.ConvertVolumeSizeToString(fileInfo.Length), fileInfo.Length);

                        // 获取占用空间
                        string drivePath = Path.GetPathRoot(filePath);
                        if (!string.IsNullOrEmpty(drivePath))
                        {
                            Kernel32Library.GetDiskFreeSpace(drivePath.TrimEnd('\\', '/'), out uint sectorsPerCluster, out uint bytesPerSector, out uint freeClusters, out uint totalClusters);
                            uint clusterSize = sectorsPerCluster * bytesPerSector;
                            long clusters = (fileInfo.Length + clusterSize - 1) / clusterSize;
                            long spaceUsage = clusters * clusterSize;
                            fileInformation.SpaceUsage = string.Format(SpaceUsageDescriptionString, VolumeSizeHelper.ConvertVolumeSizeToString(spaceUsage), spaceUsage);
                        }

                        // 获取创建时间
                        fileInformation.CreateTime = fileInfo.CreationTime.ToString("yyyy/MM/dd HH:mm:ss");

                        // 获取修改时间
                        fileInformation.ModifyTime = fileInfo.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss");

                        // 获取访问时间
                        fileInformation.AccessTime = fileInfo.LastAccessTime.ToString("yyyy/MM/dd HH:mm:ss");
                    }
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(FileInformationPage), nameof(GetGeneralInformationAsync), 1, e);
                }

                return fileInformation;
            });
        }

        /// <summary>
        /// 获取文件类型
        /// </summary>
        private async Task<FileInformationResultKind> GetFileTypeAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                FileInformationResultKind fileInformationResultKind = FileInformationResultKind.File;

                try
                {
                    string extension = Path.GetExtension(filePath);
                    if (!string.IsNullOrEmpty(extension))
                    {
                        ShlwapiLibrary.AssocGetPerceivedType(extension, out PERCEIVED type, out _, out _);
                        if (type is PERCEIVED.PERCEIVED_TYPE_VIDEO)
                        {
                            fileInformationResultKind = FileInformationResultKind.VideoFile;
                        }
                        else if (type is PERCEIVED.PERCEIVED_TYPE_AUDIO)
                        {
                            fileInformationResultKind = FileInformationResultKind.AudioFile;
                        }
                        else if (type is PERCEIVED.PERCEIVED_TYPE_TEXT || type is PERCEIVED.PERCEIVED_TYPE_DOCUMENT)
                        {
                            fileInformationResultKind = FileInformationResultKind.TextFile;
                        }
                        else if (type is PERCEIVED.PERCEIVED_TYPE_IMAGE)
                        {
                            fileInformationResultKind = FileInformationResultKind.ImageFile;
                        }
                        else
                        {
                            if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                            {
                                if (MediaInfoLibrary.MediaInfo_Count_Get(handle, StreamKind.Video, -1) > 0)
                                {
                                    fileInformationResultKind = FileInformationResultKind.VideoFile;
                                }
                                else
                                {
                                    if (MediaInfoLibrary.MediaInfo_Count_Get(handle, StreamKind.Audio, -1) > 0)
                                    {
                                        fileInformationResultKind = FileInformationResultKind.AudioFile;
                                    }
                                    else
                                    {
                                        if (MediaInfoLibrary.MediaInfo_Count_Get(handle, StreamKind.Image, -1) > 0)
                                        {
                                            fileInformationResultKind = FileInformationResultKind.ImageFile;
                                        }
                                        else
                                        {
                                            if (MediaInfoLibrary.MediaInfo_Count_Get(handle, StreamKind.Text, -1) > 0)
                                            {
                                                fileInformationResultKind = FileInformationResultKind.TextFile;
                                            }
                                        }
                                    }
                                }

                                MediaInfoLibrary.MediaInfo_Close(handle);
                                MediaInfoLibrary.MediaInfo_Delete(handle);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(FileInformationPage), nameof(GetFileTypeAsync), 1, e);
                }

                return fileInformationResultKind;
            });
        }

        /// <summary>
        /// 获取视频文件基本信息
        /// </summary>
        private async Task<VideoInformation> GetVideoInformationAsync(string filePath)
        {
            return await Task.Run(async () =>
            {
                VideoInformation videoInformation = new();

                try
                {
                    if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                    {
                        GeneralInfo generalInfo = new();
                        string completeName = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "CompleteName", InfoKind.Text, InfoKind.Name));
                        generalInfo.CompleteName = string.IsNullOrEmpty(completeName) ? NotAvailableString : completeName;
                        string generalFormat = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "Format", InfoKind.Text, InfoKind.Name));
                        generalInfo.Format = string.IsNullOrEmpty(generalFormat) ? NotAvailableString : generalFormat;
                        string generalFormatVersion = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "Format_Version", InfoKind.Text, InfoKind.Name));
                        generalInfo.FormatVersion = string.IsNullOrEmpty(generalFormatVersion) ? NotAvailableString : generalFormatVersion;
                        string generalFormatProfile = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "Format_Profile", InfoKind.Text, InfoKind.Name));
                        generalInfo.FormatProfile = string.IsNullOrEmpty(generalFormatProfile) ? NotAvailableString : generalFormatProfile;
                        string generalCodecID = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "CodecID/String", InfoKind.Text, InfoKind.Name));
                        generalInfo.CodecID = string.IsNullOrEmpty(generalCodecID) ? NotAvailableString : generalCodecID;
                        string fileSize = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "FileSize", InfoKind.Text, InfoKind.Name));
                        generalInfo.FileSize = int.TryParse(fileSize, out int fileSizeValue) ? VolumeSizeHelper.ConvertVolumeSizeToString(fileSizeValue) : string.IsNullOrEmpty(fileSize) ? NotAvailableString : fileSize;
                        string uniqueID = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "UniqueID", InfoKind.Text, InfoKind.Name));
                        generalInfo.UniqueID = string.IsNullOrEmpty(uniqueID) ? NotAvailableString : uniqueID;
                        string encodedDate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "Encoded_Date", InfoKind.Text, InfoKind.Name));
                        generalInfo.EncodedDate = string.IsNullOrEmpty(encodedDate) ? NotAvailableString : encodedDate;
                        string generalDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "Duration", InfoKind.Text, InfoKind.Name));
                        if (int.TryParse(generalDuration, out int generalDurationValue))
                        {
                            TimeSpan generalDurationTimeSpan = TimeSpan.FromMilliseconds(generalDurationValue);
                            generalInfo.Duration = string.Format(@"{0:00}:{1:00}:{2:00}:{3:00}", generalDurationTimeSpan.TotalHours, generalDurationTimeSpan.Minutes, generalDurationTimeSpan.Minutes, generalDurationTimeSpan.Milliseconds);
                        }
                        else
                        {
                            generalInfo.Duration = string.IsNullOrEmpty(generalDuration) ? NotAvailableString : generalDuration;
                        }
                        string overallBitRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "OverallBitRate/String", InfoKind.Text, InfoKind.Name));
                        generalInfo.OverallBitRate = string.IsNullOrEmpty(overallBitRate) ? NotAvailableString : overallBitRate;
                        string generalFrameRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "FrameRate/String", InfoKind.Text, InfoKind.Name));
                        generalInfo.FrameRate = string.IsNullOrEmpty(generalFrameRate) ? NotAvailableString : generalFrameRate;
                        string generalStreamSize = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "StreamSize", InfoKind.Text, InfoKind.Name));
                        generalInfo.StreamSize = int.TryParse(generalStreamSize, out int generalStreamSizeValue) ? VolumeSizeHelper.ConvertVolumeSizeToString(generalStreamSizeValue) : string.IsNullOrEmpty(generalStreamSize) ? NotAvailableString : generalStreamSize;
                        string recordedDate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "Recorded_Date", InfoKind.Text, InfoKind.Name));
                        generalInfo.RecordedDate = string.IsNullOrEmpty(recordedDate) ? NotAvailableString : recordedDate;
                        string encodedApplication = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "Encoded_Application", InfoKind.Text, InfoKind.Name));
                        generalInfo.EncodedApplication = string.IsNullOrEmpty(encodedApplication) ? NotAvailableString : encodedApplication;
                        string generalEncodedLibrary = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "Encoded_Library", InfoKind.Text, InfoKind.Name));
                        generalInfo.EncodedLibrary = string.IsNullOrEmpty(generalEncodedLibrary) ? NotAvailableString : generalEncodedLibrary;
                        videoInformation.GeneralInfo = generalInfo;

                        int videoCount = MediaInfoLibrary.MediaInfo_Count_Get(handle, StreamKind.Video, -1);
                        for (int index = 0; index < videoCount; index++)
                        {
                            VideoDetailInfo videoDetailInfo = new();
                            string id = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "ID", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.ID = string.IsNullOrEmpty(id) ? NotAvailableString : id;
                            string videoFormat = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "Format", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.Format = string.IsNullOrEmpty(videoFormat) ? NotAvailableString : videoFormat;
                            string formatInfo = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "Format/Info", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.FormatInfo = string.IsNullOrEmpty(formatInfo) ? NotAvailableString : formatInfo;
                            string videoFormatProfile = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "Format_Profile", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.FormatProfile = string.IsNullOrEmpty(videoFormatProfile) ? NotAvailableString : videoFormatProfile;
                            string videoCodecID = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "CodecID/String", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.CodecID = string.IsNullOrEmpty(videoCodecID) ? NotAvailableString : videoCodecID;
                            string codecIDInfo = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "CodecID/Info", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.CodecIDInfo = string.IsNullOrEmpty(codecIDInfo) ? NotAvailableString : codecIDInfo;
                            string videoDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "Duration", InfoKind.Text, InfoKind.Name));
                            if (int.TryParse(videoDuration, out int videoDurationValue))
                            {
                                TimeSpan videoDurationTimeSpan = TimeSpan.FromMilliseconds(videoDurationValue);
                                videoDetailInfo.Duration = string.Format(@"{0:00}:{1:00}:{2:00}:{3:00}", videoDurationTimeSpan.TotalHours, videoDurationTimeSpan.Minutes, videoDurationTimeSpan.Minutes, videoDurationTimeSpan.Milliseconds);
                            }
                            else
                            {
                                videoDetailInfo.Duration = string.IsNullOrEmpty(videoDuration) ? NotAvailableString : videoDuration;
                            }
                            string sourceDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "Source_Duration", InfoKind.Text, InfoKind.Name));
                            if (int.TryParse(sourceDuration, out int sourceDurationValue))
                            {
                                TimeSpan sourceDurationTimeSpan = TimeSpan.FromMilliseconds(sourceDurationValue);
                                videoDetailInfo.SourceDuration = string.Format(@"{0:00}:{1:00}:{2:00}:{3:00}", sourceDurationTimeSpan.TotalHours, sourceDurationTimeSpan.Minutes, sourceDurationTimeSpan.Minutes, sourceDurationTimeSpan.Milliseconds);
                            }
                            else
                            {
                                videoDetailInfo.SourceDuration = string.IsNullOrEmpty(sourceDuration) ? NotAvailableString : sourceDuration;
                            }
                            string bitRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "BitRate/String", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.BitRate = string.IsNullOrEmpty(bitRate) ? NotAvailableString : bitRate;
                            string width = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "Width", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.Width = string.IsNullOrEmpty(width) ? NotAvailableString : width;
                            string height = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "Height", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.Height = string.IsNullOrEmpty(height) ? NotAvailableString : height;
                            string displayAspectRatio = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "DisplayAspectRatio/String", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.DisplayAspectRatio = string.IsNullOrEmpty(displayAspectRatio) ? NotAvailableString : displayAspectRatio;
                            string frameRateMode = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "FrameRate_Mode/String", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.FrameRateMode = string.IsNullOrEmpty(frameRateMode) ? NotAvailableString : frameRateMode;
                            string videoFrameRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "FrameRate/String", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.FrameRate = string.IsNullOrEmpty(videoFrameRate) ? NotAvailableString : videoFrameRate;
                            string minimumFrameRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "FrameRate_Minimum/String", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.MinimumFrameRate = string.IsNullOrEmpty(minimumFrameRate) ? NotAvailableString : minimumFrameRate;
                            string maximumFrameRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "FrameRate_Maximum/String", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.MaximumFrameRate = string.IsNullOrEmpty(maximumFrameRate) ? NotAvailableString : maximumFrameRate;
                            string colorSpace = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "ColorSpace", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.ColorSpace = string.IsNullOrEmpty(colorSpace) ? NotAvailableString : colorSpace;
                            string chromaSubsampling = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "ChromaSubsampling", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.ChromaSubsampling = string.IsNullOrEmpty(chromaSubsampling) ? NotAvailableString : chromaSubsampling;
                            string bitDepth = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "BitDepth/String", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.BitDepth = string.IsNullOrEmpty(bitDepth) ? NotAvailableString : bitDepth;
                            string bitsPxixelFrame = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "Bits-(Pixel*Frame)", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.BitsPixelFrame = string.IsNullOrEmpty(bitsPxixelFrame) ? NotAvailableString : bitsPxixelFrame;
                            string videoStreamSize = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "StreamSize", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.StreamSize = int.TryParse(videoStreamSize, out int videoStreamSizeValue) ? VolumeSizeHelper.ConvertVolumeSizeToString(videoStreamSizeValue) : string.IsNullOrEmpty(videoStreamSize) ? NotAvailableString : videoStreamSize;
                            string sourceStreamSize = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "Source_StreamSize", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.SourceStreamSize = int.TryParse(sourceStreamSize, out int sourceStreamSizeValue) ? VolumeSizeHelper.ConvertVolumeSizeToString(sourceStreamSizeValue) : string.IsNullOrEmpty(sourceStreamSize) ? NotAvailableString : sourceStreamSize;
                            string colorRange = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "colour_range", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.ColorRange = string.IsNullOrEmpty(colorRange) ? NotAvailableString : colorRange;
                            string videoEncodedLibrary = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "Encoded_Library", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.EncodedLibrary = string.IsNullOrEmpty(videoEncodedLibrary) ? NotAvailableString : videoEncodedLibrary;
                            string colorParimaries = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "colour_primaries", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.ColorParimaries = string.IsNullOrEmpty(colorParimaries) ? NotAvailableString : colorParimaries;
                            string transferCharacteristics = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "transfer_characteristics", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.TransferCharacteristics = string.IsNullOrEmpty(transferCharacteristics) ? NotAvailableString : transferCharacteristics;
                            string matrixCoefficients = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "matrix_coefficients", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.MatrixCoefficients = string.IsNullOrEmpty(matrixCoefficients) ? NotAvailableString : matrixCoefficients;
                            string codecConfigurationBox = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Video, index, "CodecConfigurationBox", InfoKind.Text, InfoKind.Name));
                            videoDetailInfo.CodecConfigurationBox = string.IsNullOrEmpty(codecConfigurationBox) ? NotAvailableString : codecConfigurationBox;
                            videoInformation.VideoDetailInfoList.Add(videoDetailInfo);
                        }

                        int audioCount = MediaInfoLibrary.MediaInfo_Count_Get(handle, StreamKind.Audio, -1);
                        for (int index = 0; index < audioCount; index++)
                        {
                            AudioDetailInfo audioDetailInfo = new();
                            string id = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "ID", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.ID = string.IsNullOrEmpty(id) ? NotAvailableString : id;
                            string audioFormat = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "Format", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.Format = string.IsNullOrEmpty(audioFormat) ? NotAvailableString : audioFormat;
                            string formatInfo = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "Format/Info", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.FormatInfo = string.IsNullOrEmpty(formatInfo) ? NotAvailableString : formatInfo;
                            string audioCodecID = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "CodecID/String", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.CodecID = string.IsNullOrEmpty(audioCodecID) ? NotAvailableString : audioCodecID;
                            string audioDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "Duration", InfoKind.Text, InfoKind.Name));
                            if (int.TryParse(audioDuration, out int audioDurationValue))
                            {
                                TimeSpan audioDurationTimeSpan = TimeSpan.FromMilliseconds(audioDurationValue);
                                audioDetailInfo.Duration = string.Format(@"{0:00}:{1:00}:{2:00}:{3:00}", audioDurationTimeSpan.TotalHours, audioDurationTimeSpan.Minutes, audioDurationTimeSpan.Minutes, audioDurationTimeSpan.Milliseconds);
                            }
                            else
                            {
                                audioDetailInfo.Duration = string.IsNullOrEmpty(audioDuration) ? NotAvailableString : audioDuration;
                            }
                            string bitRateMode = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "BitRate_Mode/String", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.BitRateMode = string.IsNullOrEmpty(bitRateMode) ? NotAvailableString : bitRateMode;
                            string bitRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "BitRate/String", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.BitRate = string.IsNullOrEmpty(bitRate) ? NotAvailableString : bitRate;
                            string bitRateMaximum = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "BitRate_Maximum/String", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.BitRateMaximum = string.IsNullOrEmpty(bitRateMaximum) ? NotAvailableString : bitRateMaximum;
                            string channel = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "Channel(s)/String", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.Channel = string.IsNullOrEmpty(channel) ? NotAvailableString : channel;
                            string channelLayout = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "ChannelLayout", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.ChannelLayout = string.IsNullOrEmpty(channelLayout) ? NotAvailableString : channelLayout;
                            string samplingRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "SamplingRate/String", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.SamplingRate = string.IsNullOrEmpty(samplingRate) ? NotAvailableString : samplingRate;
                            string audioFrameRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "FrameRate/String", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.FrameRate = string.IsNullOrEmpty(audioFrameRate) ? NotAvailableString : audioFrameRate;
                            string compressionMode = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "Compression_Mode", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.CompressionMode = string.IsNullOrEmpty(compressionMode) ? NotAvailableString : compressionMode;
                            string audioStreamSize = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "StreamSize", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.StreamSize = int.TryParse(fileSize, out int audioStreamSizeValue) ? VolumeSizeHelper.ConvertVolumeSizeToString(audioStreamSizeValue) : string.IsNullOrEmpty(audioStreamSize) ? NotAvailableString : audioStreamSize;
                            string @default = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "Default", InfoKind.Text, InfoKind.Name));
                            if (!string.IsNullOrEmpty(@default))
                            {
                                if (string.Equals(@default, "Yes", StringComparison.OrdinalIgnoreCase))
                                {
                                    audioDetailInfo.Default = YesString;
                                }
                                else if (string.Equals(@default, "No", StringComparison.OrdinalIgnoreCase))
                                {
                                    audioDetailInfo.Default = NoString;
                                }
                                else
                                {
                                    audioDetailInfo.Default = NotAvailableString;
                                }
                            }
                            else
                            {
                                audioDetailInfo.Default = NotAvailableString;
                            }
                            string alternateGroup = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "AlternateGroup", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.AlternateGroup = string.IsNullOrEmpty(alternateGroup) ? NotAvailableString : alternateGroup;
                            videoInformation.AudioDetailInfoList.Add(audioDetailInfo);
                        }

                        int textCount = MediaInfoLibrary.MediaInfo_Count_Get(handle, StreamKind.Text, -1);
                        for (int index = 0; index < textCount; index++)
                        {
                            TextDetailInfo textDetailInfo = new();
                            string textFormat = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Text, index, "Format", InfoKind.Text, InfoKind.Name));
                            textDetailInfo.Format = string.IsNullOrEmpty(textFormat) ? NotAvailableString : textFormat;
                            string textDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Text, index, "Duration", InfoKind.Text, InfoKind.Name));
                            if (int.TryParse(textDuration, out int textDurationValue))
                            {
                                TimeSpan textDurationTimeSpan = TimeSpan.FromMilliseconds(textDurationValue);
                                textDetailInfo.Duration = string.Format(@"{0:00}:{1:00}:{2:00}:{3:00}", textDurationTimeSpan.TotalHours, textDurationTimeSpan.Minutes, textDurationTimeSpan.Minutes, textDurationTimeSpan.Milliseconds);
                            }
                            else
                            {
                                textDetailInfo.Duration = string.IsNullOrEmpty(textDuration) ? NotAvailableString : textDuration;
                            }
                            string startTime = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Text, index, "Duration_Start", InfoKind.Text, InfoKind.Name));
                            if (int.TryParse(startTime, out int startTimeValue))
                            {
                                TimeSpan startTimeSpan = TimeSpan.FromMilliseconds(startTimeValue);
                                textDetailInfo.StartTime = string.Format(@"{0:00}:{1:00}:{2:00}:{3:00}", startTimeSpan.TotalHours, startTimeSpan.Minutes, startTimeSpan.Minutes, startTimeSpan.Milliseconds);
                            }
                            else
                            {
                                textDetailInfo.StartTime = string.IsNullOrEmpty(startTime) ? NotAvailableString : startTime;
                            }
                            string endTime = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Text, index, "Duration_End", InfoKind.Text, InfoKind.Name));
                            if (int.TryParse(endTime, out int endTimeValue))
                            {
                                TimeSpan endTimeSpan = TimeSpan.FromMilliseconds(endTimeValue);
                                textDetailInfo.EndTime = string.Format(@"{0:00}:{1:00}:{2:00}:{3:00}", endTimeSpan.TotalHours, endTimeSpan.Minutes, endTimeSpan.Minutes, endTimeSpan.Milliseconds);
                            }
                            else
                            {
                                textDetailInfo.EndTime = string.IsNullOrEmpty(endTime) ? NotAvailableString : endTime;
                            }
                            string compressionMode = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Text, index, "Compression_Mode", InfoKind.Text, InfoKind.Name));
                            textDetailInfo.CompressionMode = string.IsNullOrEmpty(compressionMode) ? NotAvailableString : compressionMode;
                            string countOfEvents = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Text, index, "Events_Total", InfoKind.Text, InfoKind.Name));
                            textDetailInfo.CountOfEvents = string.IsNullOrEmpty(countOfEvents) ? NotAvailableString : countOfEvents;
                            string minimumDurationPerEvent = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Text, index, "Events_MinDuration", InfoKind.Text, InfoKind.Name));
                            if (int.TryParse(minimumDurationPerEvent, out int minimumDurationPerEventValue))
                            {
                                TimeSpan minimumDurationPerEventTimeSpan = TimeSpan.FromMilliseconds(minimumDurationPerEventValue);
                                textDetailInfo.MinimumDurationPerEvent = string.Format(@"{0:00}:{1:00}:{2:00}:{3:00}", minimumDurationPerEventTimeSpan.TotalHours, minimumDurationPerEventTimeSpan.Minutes, minimumDurationPerEventTimeSpan.Minutes, minimumDurationPerEventTimeSpan.Milliseconds);
                            }
                            else
                            {
                                textDetailInfo.MinimumDurationPerEvent = string.IsNullOrEmpty(minimumDurationPerEvent) ? NotAvailableString : minimumDurationPerEvent;
                            }
                            string countOfLines = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Text, index, "Lines_Count", InfoKind.Text, InfoKind.Name));
                            textDetailInfo.CountOfLines = string.IsNullOrEmpty(countOfLines) ? NotAvailableString : countOfLines;
                            string maximumCountOfLinesPerEvent = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Text, index, "Lines_MaxCountPerEvent", InfoKind.Text, InfoKind.Name));
                            textDetailInfo.MaximumCountOfLinesPerEvent = string.IsNullOrEmpty(maximumCountOfLinesPerEvent) ? NotAvailableString : maximumCountOfLinesPerEvent;
                            videoInformation.TextDetailInfoList.Add(textDetailInfo);
                        }

                        string videoOverviewInformation = await GetOverviewInformationAsync(handle);
                        videoInformation.VideoOverviewInformation = string.IsNullOrEmpty(videoOverviewInformation) ? NotAvailableString : videoOverviewInformation;

                        MediaInfoLibrary.MediaInfo_Close(handle);
                        MediaInfoLibrary.MediaInfo_Delete(handle);
                    }
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(FileInformationPage), nameof(GetVideoInformationAsync), 1, e);
                }

                return videoInformation;
            });
        }

        /// <summary>
        /// 获取音频文件基本信息
        /// </summary>
        private async Task<AudioInformation> GetAudioInformationAsync(string filePath)
        {
            return await Task.Run(async () =>
            {
                AudioInformation audioInformation = new();

                try
                {
                    if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                    {
                        GeneralInfo generalInfo = new();
                        string completeName = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "CompleteName", InfoKind.Text, InfoKind.Name));
                        generalInfo.CompleteName = string.IsNullOrEmpty(completeName) ? NotAvailableString : completeName;
                        string generalFormat = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "Format", InfoKind.Text, InfoKind.Name));
                        generalInfo.Format = string.IsNullOrEmpty(generalFormat) ? NotAvailableString : generalFormat;
                        string generalFormatVersion = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "Format_Version", InfoKind.Text, InfoKind.Name));
                        generalInfo.FormatVersion = string.IsNullOrEmpty(generalFormatVersion) ? NotAvailableString : generalFormatVersion;
                        string generalFormatProfile = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "Format_Profile", InfoKind.Text, InfoKind.Name));
                        generalInfo.FormatProfile = string.IsNullOrEmpty(generalFormatProfile) ? NotAvailableString : generalFormatProfile;
                        string generalCodecID = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "CodecID/String", InfoKind.Text, InfoKind.Name));
                        generalInfo.CodecID = string.IsNullOrEmpty(generalCodecID) ? NotAvailableString : generalCodecID;
                        string fileSize = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "FileSize", InfoKind.Text, InfoKind.Name));
                        generalInfo.FileSize = int.TryParse(fileSize, out int fileSizeValue) ? VolumeSizeHelper.ConvertVolumeSizeToString(fileSizeValue) : string.IsNullOrEmpty(fileSize) ? NotAvailableString : fileSize;
                        string uniqueID = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "UniqueID", InfoKind.Text, InfoKind.Name));
                        generalInfo.UniqueID = string.IsNullOrEmpty(uniqueID) ? NotAvailableString : uniqueID;
                        string encodedDate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "Encoded_Date", InfoKind.Text, InfoKind.Name));
                        generalInfo.EncodedDate = string.IsNullOrEmpty(encodedDate) ? NotAvailableString : encodedDate;
                        string generalDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "Duration", InfoKind.Text, InfoKind.Name));
                        if (int.TryParse(generalDuration, out int generalDurationValue))
                        {
                            TimeSpan generalDurationTimeSpan = TimeSpan.FromMilliseconds(generalDurationValue);
                            generalInfo.Duration = string.Format(@"{0:00}:{1:00}:{2:00}:{3:00}", generalDurationTimeSpan.TotalHours, generalDurationTimeSpan.Minutes, generalDurationTimeSpan.Minutes, generalDurationTimeSpan.Milliseconds);
                        }
                        else
                        {
                            generalInfo.Duration = string.IsNullOrEmpty(generalDuration) ? NotAvailableString : generalDuration;
                        }
                        string overallBitRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "OverallBitRate/String", InfoKind.Text, InfoKind.Name));
                        generalInfo.OverallBitRate = string.IsNullOrEmpty(overallBitRate) ? NotAvailableString : overallBitRate;
                        string generalFrameRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "FrameRate/String", InfoKind.Text, InfoKind.Name));
                        generalInfo.FrameRate = string.IsNullOrEmpty(generalFrameRate) ? NotAvailableString : generalFrameRate;
                        string generalStreamSize = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "StreamSize", InfoKind.Text, InfoKind.Name));
                        generalInfo.StreamSize = int.TryParse(fileSize, out int generalStreamSizeValue) ? VolumeSizeHelper.ConvertVolumeSizeToString(generalStreamSizeValue) : string.IsNullOrEmpty(generalStreamSize) ? NotAvailableString : generalStreamSize;
                        string recordedDate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "Recorded_Date", InfoKind.Text, InfoKind.Name));
                        generalInfo.RecordedDate = string.IsNullOrEmpty(recordedDate) ? NotAvailableString : recordedDate;
                        string encodedApplication = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "Encoded_Application", InfoKind.Text, InfoKind.Name));
                        generalInfo.EncodedApplication = string.IsNullOrEmpty(encodedApplication) ? NotAvailableString : encodedApplication;
                        string generalEncodedLibrary = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "Encoded_Library", InfoKind.Text, InfoKind.Name));
                        generalInfo.EncodedLibrary = string.IsNullOrEmpty(generalEncodedLibrary) ? NotAvailableString : generalEncodedLibrary;
                        string album = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "Album", InfoKind.Text, InfoKind.Name));
                        generalInfo.Album = string.IsNullOrEmpty(album) ? NotAvailableString : album;
                        string trackName = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "TrackName", InfoKind.Text, InfoKind.Name));
                        generalInfo.TrackName = string.IsNullOrEmpty(trackName) ? NotAvailableString : trackName;
                        string performer = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "Performer", InfoKind.Text, InfoKind.Name));
                        generalInfo.Performer = string.IsNullOrEmpty(performer) ? NotAvailableString : performer;
                        audioInformation.GeneralInfo = generalInfo;

                        int audioCount = MediaInfoLibrary.MediaInfo_Count_Get(handle, StreamKind.Audio, -1);
                        for (int index = 0; index < audioCount; index++)
                        {
                            AudioDetailInfo audioDetailInfo = new();
                            string id = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "ID", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.ID = string.IsNullOrEmpty(id) ? NotAvailableString : id;
                            string audioFormat = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "Format", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.Format = string.IsNullOrEmpty(audioFormat) ? NotAvailableString : audioFormat;
                            string formatInfo = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "Format/Info", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.FormatInfo = string.IsNullOrEmpty(formatInfo) ? NotAvailableString : formatInfo;
                            string audioCodecID = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "CodecID/String", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.CodecID = string.IsNullOrEmpty(audioCodecID) ? NotAvailableString : audioCodecID;
                            string audioDuration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "Duration", InfoKind.Text, InfoKind.Name));
                            if (int.TryParse(audioDuration, out int audioDurationValue))
                            {
                                TimeSpan audioDurationTimeSpan = TimeSpan.FromMilliseconds(audioDurationValue);
                                audioDetailInfo.Duration = string.Format(@"{0:00}:{1:00}:{2:00}:{3:00}", audioDurationTimeSpan.TotalHours, audioDurationTimeSpan.Minutes, audioDurationTimeSpan.Minutes, audioDurationTimeSpan.Milliseconds);
                            }
                            else
                            {
                                audioDetailInfo.Duration = string.IsNullOrEmpty(audioDuration) ? NotAvailableString : audioDuration;
                            }
                            string bitRateMode = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "BitRate_Mode/String", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.BitRateMode = string.IsNullOrEmpty(bitRateMode) ? NotAvailableString : bitRateMode;
                            string bitRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "BitRate/String", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.BitRate = string.IsNullOrEmpty(bitRate) ? NotAvailableString : bitRate;
                            string bitRateMaximum = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "BitRate_Maximum/String", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.BitRateMaximum = string.IsNullOrEmpty(bitRateMaximum) ? NotAvailableString : bitRateMaximum;
                            string channel = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "Channel(s)/String", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.Channel = string.IsNullOrEmpty(channel) ? NotAvailableString : channel;
                            string channelLayout = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "ChannelLayout", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.ChannelLayout = string.IsNullOrEmpty(channelLayout) ? NotAvailableString : channelLayout;
                            string samplingRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "SamplingRate/String", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.SamplingRate = string.IsNullOrEmpty(samplingRate) ? NotAvailableString : samplingRate;
                            string audioFrameRate = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "FrameRate/String", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.FrameRate = string.IsNullOrEmpty(audioFrameRate) ? NotAvailableString : audioFrameRate;
                            string compressionMode = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "Compression_Mode", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.CompressionMode = string.IsNullOrEmpty(compressionMode) ? NotAvailableString : compressionMode;
                            string audioStreamSize = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "StreamSize", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.StreamSize = int.TryParse(fileSize, out int audioStreamSizeValue) ? VolumeSizeHelper.ConvertVolumeSizeToString(audioStreamSizeValue) : string.IsNullOrEmpty(audioStreamSize) ? NotAvailableString : audioStreamSize;
                            string @default = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "Default", InfoKind.Text, InfoKind.Name));
                            if (!string.IsNullOrEmpty(@default))
                            {
                                if (string.Equals(@default, "Yes", StringComparison.OrdinalIgnoreCase))
                                {
                                    audioDetailInfo.Default = YesString;
                                }
                                else if (string.Equals(@default, "No", StringComparison.OrdinalIgnoreCase))
                                {
                                    audioDetailInfo.Default = NoString;
                                }
                                else
                                {
                                    audioDetailInfo.Default = NotAvailableString;
                                }
                            }
                            else
                            {
                                audioDetailInfo.Default = NotAvailableString;
                            }
                            string alternateGroup = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Audio, index, "AlternateGroup", InfoKind.Text, InfoKind.Name));
                            audioDetailInfo.AlternateGroup = string.IsNullOrEmpty(alternateGroup) ? NotAvailableString : alternateGroup;
                            audioInformation.AudioDetailInfoList.Add(audioDetailInfo);
                        }

                        string audioOverviewInformation = await GetOverviewInformationAsync(handle);
                        audioInformation.AudioOverviewInformation = string.IsNullOrEmpty(audioOverviewInformation) ? NotAvailableString : audioOverviewInformation;

                        MediaInfoLibrary.MediaInfo_Close(handle);
                        MediaInfoLibrary.MediaInfo_Delete(handle);
                    }
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(FileInformationPage), nameof(GetAudioInformationAsync), 1, e);
                }

                return audioInformation;
            });
        }

        /// <summary>
        /// 获取文本文件基本信息
        /// </summary>
        private async Task<TextInformation> GetTextInformationAsync(string filePath)
        {
            return await Task.Run(async () =>
            {
                TextInformation textInformation = new();

                try
                {
                    if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                    {
                        TextDetailInfo textDetailInfo = new();
                        string format = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Text, 0, "Format", InfoKind.Text, InfoKind.Name));
                        textDetailInfo.Format = string.IsNullOrEmpty(format) ? NotAvailableString : format;
                        string duration = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Text, 0, "Duration", InfoKind.Text, InfoKind.Name));
                        if (int.TryParse(duration, out int durationValue))
                        {
                            TimeSpan durationTimeSpan = TimeSpan.FromMilliseconds(durationValue);
                            textDetailInfo.Duration = string.Format(@"{0:00}:{1:00}:{2:00}:{3:00}", durationTimeSpan.TotalHours, durationTimeSpan.Minutes, durationTimeSpan.Minutes, durationTimeSpan.Milliseconds);
                        }
                        else
                        {
                            textDetailInfo.Duration = string.IsNullOrEmpty(duration) ? NotAvailableString : duration;
                        }
                        string startTime = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Text, 0, "Duration_Start", InfoKind.Text, InfoKind.Name));
                        if (int.TryParse(startTime, out int startTimeValue))
                        {
                            TimeSpan startTimeSpan = TimeSpan.FromMilliseconds(startTimeValue);
                            textDetailInfo.StartTime = string.Format(@"{0:00}:{1:00}:{2:00}:{3:00}", startTimeSpan.TotalHours, startTimeSpan.Minutes, startTimeSpan.Minutes, startTimeSpan.Milliseconds);
                        }
                        else
                        {
                            textDetailInfo.StartTime = string.IsNullOrEmpty(startTime) ? NotAvailableString : startTime;
                        }
                        string endTime = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Text, 0, "Duration_End", InfoKind.Text, InfoKind.Name));
                        if (int.TryParse(endTime, out int endTimeValue))
                        {
                            TimeSpan endTimeSpan = TimeSpan.FromMilliseconds(endTimeValue);
                            textDetailInfo.EndTime = string.Format(@"{0:00}:{1:00}:{2:00}:{3:00}", endTimeSpan.TotalHours, endTimeSpan.Minutes, endTimeSpan.Minutes, endTimeSpan.Milliseconds);
                        }
                        else
                        {
                            textDetailInfo.EndTime = string.IsNullOrEmpty(endTime) ? NotAvailableString : endTime;
                        }
                        string compressionMode = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Text, 0, "Compression_Mode", InfoKind.Text, InfoKind.Name));
                        textDetailInfo.CompressionMode = string.IsNullOrEmpty(compressionMode) ? NotAvailableString : compressionMode;
                        string countOfEvents = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Text, 0, "Events_Total", InfoKind.Text, InfoKind.Name));
                        textDetailInfo.CountOfEvents = string.IsNullOrEmpty(countOfEvents) ? NotAvailableString : countOfEvents;
                        string minimumDurationPerEvent = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Text, 0, "Events_MinDuration", InfoKind.Text, InfoKind.Name));
                        if (int.TryParse(minimumDurationPerEvent, out int minimumDurationPerEventValue))
                        {
                            TimeSpan minimumDurationPerEventTimeSpan = TimeSpan.FromMilliseconds(minimumDurationPerEventValue);
                            textDetailInfo.MinimumDurationPerEvent = string.Format(@"{0:00}:{1:00}:{2:00}:{3:00}", minimumDurationPerEventTimeSpan.TotalHours, minimumDurationPerEventTimeSpan.Minutes, minimumDurationPerEventTimeSpan.Minutes, minimumDurationPerEventTimeSpan.Milliseconds);
                        }
                        else
                        {
                            textDetailInfo.MinimumDurationPerEvent = string.IsNullOrEmpty(minimumDurationPerEvent) ? NotAvailableString : minimumDurationPerEvent;
                        }
                        string countOfLines = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Text, 0, "Lines_Count", InfoKind.Text, InfoKind.Name));
                        textDetailInfo.CountOfLines = string.IsNullOrEmpty(countOfLines) ? NotAvailableString : countOfLines;
                        string maximumCountOfLinesPerEvent = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Text, 0, "Lines_MaxCountPerEvent", InfoKind.Text, InfoKind.Name));
                        textDetailInfo.MaximumCountOfLinesPerEvent = string.IsNullOrEmpty(maximumCountOfLinesPerEvent) ? NotAvailableString : maximumCountOfLinesPerEvent;
                        textInformation.TextDetailInfo = textDetailInfo;

                        string textOverviewInformation = await GetOverviewInformationAsync(handle);
                        textInformation.TextOverviewInformation = string.IsNullOrEmpty(textOverviewInformation) ? NotAvailableString : textOverviewInformation;

                        MediaInfoLibrary.MediaInfo_Close(handle);
                        MediaInfoLibrary.MediaInfo_Delete(handle);
                    }
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(FileInformationPage), nameof(GetTextInformationAsync), 1, e);
                }

                return textInformation;
            });
        }

        /// <summary>
        /// 获取图片文件基本信息
        /// </summary>
        private async Task<ImageInformation> GetImageInformationAsync(string filePath)
        {
            return await Task.Run(async () =>
            {
                ImageInformation imageInformation = new();

                try
                {
                    if (MediaInfoLibrary.MediaInfo_New() is nint handle && handle is not 0 && MediaInfoLibrary.MediaInfo_Open(handle, filePath) is not 0)
                    {
                        GeneralInfo generalInfo = new();
                        string completeName = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "CompleteName", InfoKind.Text, InfoKind.Name));
                        generalInfo.CompleteName = string.IsNullOrEmpty(completeName) ? NotAvailableString : completeName;
                        string generalFormat = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "Format", InfoKind.Text, InfoKind.Name));
                        generalInfo.Format = string.IsNullOrEmpty(generalFormat) ? NotAvailableString : generalFormat;
                        string fileSize = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.General, 0, "FileSize", InfoKind.Text, InfoKind.Name));
                        generalInfo.FileSize = int.TryParse(fileSize, out int fileSizeValue) ? VolumeSizeHelper.ConvertVolumeSizeToString(fileSizeValue) : string.IsNullOrEmpty(fileSize) ? NotAvailableString : fileSize;
                        imageInformation.GeneralInfo = generalInfo;

                        int imageCount = MediaInfoLibrary.MediaInfo_Count_Get(handle, StreamKind.Image, -1);
                        for (int index = 0; index < imageCount; index++)
                        {
                            ImageDetailInfo imageDetailInfo = new();
                            string imageFormat = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Image, index, "Format", InfoKind.Text, InfoKind.Name));
                            imageDetailInfo.Format = string.IsNullOrEmpty(imageFormat) ? NotAvailableString : imageFormat;
                            string formatInfo = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Image, index, "Format/Info", InfoKind.Text, InfoKind.Name));
                            imageDetailInfo.FormatInfo = string.IsNullOrEmpty(formatInfo) ? NotAvailableString : formatInfo;
                            string formatCompression = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Image, index, "Format_Compression", InfoKind.Text, InfoKind.Name));
                            imageDetailInfo.FormatCompression = string.IsNullOrEmpty(formatCompression) ? NotAvailableString : formatCompression;
                            string width = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Image, index, "Width", InfoKind.Text, InfoKind.Name));
                            imageDetailInfo.Width = string.IsNullOrEmpty(width) ? NotAvailableString : width;
                            string height = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Image, index, "Height", InfoKind.Text, InfoKind.Name));
                            imageDetailInfo.Height = string.IsNullOrEmpty(height) ? NotAvailableString : height;
                            string colorSpace = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Image, index, "ColorSpace", InfoKind.Text, InfoKind.Name));
                            imageDetailInfo.ColorSpace = string.IsNullOrEmpty(colorSpace) ? NotAvailableString : colorSpace;
                            string chromaSubsampling = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Image, index, "ChromaSubsampling", InfoKind.Text, InfoKind.Name));
                            imageDetailInfo.ChromaSubsampling = string.IsNullOrEmpty(chromaSubsampling) ? NotAvailableString : chromaSubsampling;
                            string bitDepth = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Image, index, "BitDepth/String", InfoKind.Text, InfoKind.Name));
                            imageDetailInfo.BitDepth = string.IsNullOrEmpty(bitDepth) ? NotAvailableString : bitDepth;
                            string compressionMode = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Image, 0, "Compression_Mode", InfoKind.Text, InfoKind.Name));
                            imageDetailInfo.CompressionMode = string.IsNullOrEmpty(compressionMode) ? NotAvailableString : compressionMode;
                            string imageStreamSize = Marshal.PtrToStringUni(MediaInfoLibrary.MediaInfo_Get(handle, StreamKind.Image, index, "StreamSize", InfoKind.Text, InfoKind.Name));
                            imageDetailInfo.StreamSize = int.TryParse(imageStreamSize, out int imageStreamSizeValue) ? VolumeSizeHelper.ConvertVolumeSizeToString(imageStreamSizeValue) : string.IsNullOrEmpty(imageStreamSize) ? NotAvailableString : imageStreamSize;
                            imageInformation.ImageDetailInfoList.Add(imageDetailInfo);
                        }

                        Metadata metadata = GetExifMetadata(filePath);
                        if (metadata is not null)
                        {
                            imageInformation.ImageExifInfo = new()
                            {
                                CameraManufacturer = string.IsNullOrEmpty(metadata.EquipmentMake.DisplayValue) ? NotAvailableString : metadata.EquipmentMake.DisplayValue,
                                CameraModel = string.IsNullOrEmpty(metadata.CameraModel.DisplayValue) ? NotAvailableString : metadata.CameraModel.DisplayValue,
                                Fstop = string.IsNullOrEmpty(metadata.Fstop.DisplayValue) ? NotAvailableString : metadata.Fstop.DisplayValue,
                                ExposureTime = string.IsNullOrEmpty(metadata.ExposureTime.DisplayValue) ? NotAvailableString : metadata.ExposureTime.DisplayValue,
                                ShootingDate = string.IsNullOrEmpty(metadata.DatePictureTaken.DisplayValue) ? NotAvailableString : metadata.DatePictureTaken.DisplayValue,
                                ShutterSpeed = string.IsNullOrEmpty(metadata.ShutterSpeed.DisplayValue) ? NotAvailableString : metadata.ShutterSpeed.DisplayValue,
                                MeteringMode = string.IsNullOrEmpty(metadata.MeteringMode.DisplayValue) ? NotAvailableString : metadata.MeteringMode.DisplayValue,
                                FlashMode = string.IsNullOrEmpty(metadata.Flash.DisplayValue) ? NotAvailableString : metadata.Flash.DisplayValue,
                                XResolution = string.IsNullOrEmpty(metadata.XResolution.DisplayValue) ? NotAvailableString : metadata.XResolution.DisplayValue,
                                YResolution = string.IsNullOrEmpty(metadata.YResolution.DisplayValue) ? NotAvailableString : metadata.YResolution.DisplayValue,
                                Width = string.IsNullOrEmpty(metadata.ImageWidth.DisplayValue) ? NotAvailableString : metadata.ImageWidth.DisplayValue,
                                Height = string.IsNullOrEmpty(metadata.ImageHeight.DisplayValue) ? NotAvailableString : metadata.ImageHeight.DisplayValue,
                                FNumber = string.IsNullOrEmpty(metadata.FNumber.DisplayValue) ? NotAvailableString : metadata.FNumber.DisplayValue,
                                ExposureProgram = string.IsNullOrEmpty(metadata.ExposureProg.DisplayValue) ? NotAvailableString : metadata.ExposureProg.DisplayValue,
                                SpectralSense = string.IsNullOrEmpty(metadata.SpectralSense.DisplayValue) ? NotAvailableString : metadata.SpectralSense.DisplayValue,
                                ISOSpeed = string.IsNullOrEmpty(metadata.ISOSpeed.DisplayValue) ? NotAvailableString : metadata.ISOSpeed.DisplayValue,
                                OECF = string.IsNullOrEmpty(metadata.OECF.DisplayValue) ? NotAvailableString : metadata.OECF.DisplayValue,
                                Version = string.IsNullOrEmpty(metadata.Ver.DisplayValue) ? NotAvailableString : metadata.Ver.DisplayValue,
                                CompressionConfig = string.IsNullOrEmpty(metadata.CompConfig.DisplayValue) ? NotAvailableString : metadata.CompConfig.DisplayValue,
                                CompressionBitsPerPixel = string.IsNullOrEmpty(metadata.CompBPP.DisplayValue) ? NotAvailableString : metadata.CompBPP.DisplayValue,
                                Aperture = string.IsNullOrEmpty(metadata.Aperture.DisplayValue) ? NotAvailableString : metadata.Aperture.DisplayValue,
                                Brightness = string.IsNullOrEmpty(metadata.Brightness.DisplayValue) ? NotAvailableString : metadata.Brightness.DisplayValue,
                                ExposureCompensation = string.IsNullOrEmpty(metadata.ExposureBias.DisplayValue) ? NotAvailableString : metadata.ExposureBias.DisplayValue,
                                MaxAperture = string.IsNullOrEmpty(metadata.MaxAperture.DisplayValue) ? NotAvailableString : metadata.MaxAperture.DisplayValue,
                                SubjectDist = string.IsNullOrEmpty(metadata.SubjectDist.DisplayValue) ? NotAvailableString : metadata.SubjectDist.DisplayValue,
                                LightSource = string.IsNullOrEmpty(metadata.LightSource.DisplayValue) ? NotAvailableString : metadata.LightSource.DisplayValue,
                                FocalLength = string.IsNullOrEmpty(metadata.FocalLength.DisplayValue) ? NotAvailableString : metadata.FocalLength.DisplayValue,
                                FlashPixVersion = string.IsNullOrEmpty(metadata.FPXVer.DisplayValue) ? NotAvailableString : metadata.FPXVer.DisplayValue,
                                ColorIndicates = string.IsNullOrEmpty(metadata.ColorSpace.DisplayValue) ? NotAvailableString : metadata.ColorSpace.DisplayValue,
                                Interop = string.IsNullOrEmpty(metadata.Interop.DisplayValue) ? NotAvailableString : metadata.Interop.DisplayValue,
                                FlashEnergy = string.IsNullOrEmpty(metadata.FlashEnergy.DisplayValue) ? NotAvailableString : metadata.FlashEnergy.DisplayValue,
                                SpatialFrequencyResponse = string.IsNullOrEmpty(metadata.SpatialFR.DisplayValue) ? NotAvailableString : metadata.SpatialFR.DisplayValue,
                                FocalXRes = string.IsNullOrEmpty(metadata.FocalXRes.DisplayValue) ? NotAvailableString : metadata.FocalXRes.DisplayValue,
                                FocalYRes = string.IsNullOrEmpty(metadata.FocalYRes.DisplayValue) ? NotAvailableString : metadata.FocalYRes.DisplayValue,
                                FocalResUnit = string.IsNullOrEmpty(metadata.FocalResUnit.DisplayValue) ? NotAvailableString : metadata.FocalResUnit.DisplayValue,
                                ExposureIndex = string.IsNullOrEmpty(metadata.ExposureIndex.DisplayValue) ? NotAvailableString : metadata.ExposureIndex.DisplayValue,
                                SenseMethod = string.IsNullOrEmpty(metadata.SensingMethod.DisplayValue) ? NotAvailableString : metadata.SensingMethod.DisplayValue,
                                SceneType = string.IsNullOrEmpty(metadata.SceneType.DisplayValue) ? NotAvailableString : metadata.SceneType.DisplayValue,
                                CfaPattern = string.IsNullOrEmpty(metadata.CfaPattern.DisplayValue) ? NotAvailableString : metadata.CfaPattern.DisplayValue,
                            };
                        }

                        string imageOverviewInformation = await GetOverviewInformationAsync(handle);
                        imageInformation.ImageOverviewInformation = string.IsNullOrEmpty(imageOverviewInformation) ? NotAvailableString : imageOverviewInformation;
                    }
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(FileInformationPage), nameof(GetImageInformationAsync), 1, e);
                }

                return imageInformation;
            });
        }

        /// <summary>
        /// 获取总览信息
        /// </summary>
        private async Task<string> GetOverviewInformationAsync(nint handle)
        {
            return await Task.Run(() =>
            {
                string overviewInformation = string.Empty;
                MediaInfoLibrary.MediaInfo_Option(handle, "Inform", "XML");
                MediaInfoLibrary.MediaInfo_Option(handle, "Complete", "1");
                nint informationPtr = MediaInfoLibrary.MediaInfo_Inform(handle, 0);
                string rawOverviewInformation = informationPtr is not 0 ? Marshal.PtrToStringUni(informationPtr).Trim() : string.Empty;

                if (!string.IsNullOrEmpty(rawOverviewInformation))
                {
                    StringBuilder overviewBuilder = new();

                    try
                    {
                        XmlDocument xmlDocument = new();
                        xmlDocument.LoadXml(rawOverviewInformation);
                        XmlNamespaceManager xmlNamespaceManager = new(xmlDocument.NameTable);
                        xmlNamespaceManager.AddNamespace("mi", "https://mediaarea.net/mediainfo");
                        XmlNodeList mediaNodeList = xmlDocument.SelectNodes("//mi:media", xmlNamespaceManager);

                        foreach (XmlNode mediaNode in mediaNodeList)
                        {
                            XmlNodeList trackNodeList = mediaNode.SelectNodes("mi:track", xmlNamespaceManager);
                            int maxTrackChildNodeLength = 0;
                            foreach (XmlNode trackNode in trackNodeList)
                            {
                                foreach (XmlNode trackChildNode in trackNode.ChildNodes)
                                {
                                    if (trackChildNode.NodeType is XmlNodeType.Element)
                                    {
                                        if (trackChildNode.Name.Length > maxTrackChildNodeLength)
                                        {
                                            maxTrackChildNodeLength = trackChildNode.Name.Length;
                                        }
                                    }
                                }
                            }

                            foreach (XmlNode trackNode in trackNodeList)
                            {
                                string trackType = trackNode.Attributes["type"]?.Value;
                                overviewBuilder.AppendLine(trackType);

                                foreach (XmlNode trackChildNode in trackNode.ChildNodes)
                                {
                                    if (trackChildNode.NodeType is XmlNodeType.Element)
                                    {
                                        overviewBuilder.AppendLine(string.Format($"{{0,-{maxTrackChildNodeLength + 10}}}{{1}}", trackChildNode.Name, trackChildNode.InnerText));
                                    }
                                }

                                overviewBuilder.AppendLine();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(FileInformationPage), nameof(GetOverviewInformationAsync), 1, e);
                    }

                    overviewInformation = overviewBuilder.ToString();
                }
                else
                {
                    overviewInformation = rawOverviewInformation;
                }

                return overviewInformation;
            });
        }

        /// <summary>
        /// 获取图片的 EXIF 信息
        /// </summary>
        private Metadata GetExifMetadata(string imagePath)
        {
            try
            {
                if (File.Exists(imagePath))
                {
                    // TODO：不支持 .HEIC 和 WEBP（暂未想到好的解决方案，使用 COM API IWICImagingFactory?）
                    System.Drawing.Image image = System.Drawing.Image.FromFile(imagePath);
                    int[] imagePropertyIdList = image.PropertyIdList;
                    PropertyItem[] imagePropertyItemList = new PropertyItem[imagePropertyIdList.Length];

                    Metadata metadata = new();
                    metadata.EquipmentMake.Hex = "10f";
                    metadata.CameraModel.Hex = "110";
                    metadata.DatePictureTaken.Hex = "9003";
                    metadata.ExposureTime.Hex = "829a";
                    metadata.Fstop.Hex = "829d";
                    metadata.ShutterSpeed.Hex = "9201";
                    metadata.MeteringMode.Hex = "9207";
                    metadata.Flash.Hex = "9209";
                    metadata.FNumber.Hex = "829d";
                    metadata.ExposureProg.Hex = string.Empty;
                    metadata.SpectralSense.Hex = "8824";
                    metadata.ISOSpeed.Hex = "8827";
                    metadata.OECF.Hex = "8828";
                    metadata.Ver.Hex = "9000";
                    metadata.CompConfig.Hex = "9101";
                    metadata.CompBPP.Hex = "9102";
                    metadata.Aperture.Hex = "9202";
                    metadata.Brightness.Hex = "9203";
                    metadata.ExposureBias.Hex = "9204";
                    metadata.MaxAperture.Hex = "9205";
                    metadata.SubjectDist.Hex = "9206";
                    metadata.LightSource.Hex = "9208";
                    metadata.FocalLength.Hex = "920a";
                    metadata.FPXVer.Hex = "a000";
                    metadata.ColorSpace.Hex = "a001";
                    metadata.FocalXRes.Hex = "a20e";
                    metadata.FocalYRes.Hex = "a20f";
                    metadata.FocalResUnit.Hex = "a210";
                    metadata.ExposureIndex.Hex = "a215";
                    metadata.SensingMethod.Hex = "a217";
                    metadata.SceneType.Hex = "a301";
                    metadata.CfaPattern.Hex = "a302";

                    int index = 0;
                    int imagePropertyIdListCount = imagePropertyIdList.Length;
                    if (imagePropertyIdListCount is not 0)
                    {
                        foreach (int imagePropertyId in imagePropertyIdList)
                        {
                            string hexVal = string.Empty;
                            imagePropertyItemList[index] = image.GetPropertyItem(imagePropertyId);
                            string imagePropertyIdString = image.GetPropertyItem(imagePropertyId).Id.ToString("x");
                            switch (imagePropertyIdString)
                            {
                                case "10f":
                                    {
                                        metadata.EquipmentMake.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        metadata.EquipmentMake.DisplayValue = Encoding.ASCII.GetString(imagePropertyItemList[index].Value);
                                        break;
                                    }
                                case "110":
                                    {
                                        metadata.CameraModel.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        metadata.CameraModel.DisplayValue = Encoding.ASCII.GetString(imagePropertyItemList[index].Value);
                                        break;
                                    }
                                case "9003":
                                    {
                                        metadata.DatePictureTaken.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        string displayValue = Encoding.ASCII.GetString(imagePropertyItemList[index].Value).Trim('\0');
                                        metadata.DatePictureTaken.DisplayValue = DateTime.TryParseExact(displayValue, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime) ? dateTime.ToString("yyyy/MM/dd HH:mm:ss") : displayValue;
                                        break;
                                    }
                                case "9207":
                                    {
                                        metadata.MeteringMode.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        metadata.MeteringMode.DisplayValue = LookupExifValue("MeteringMode", BitConverter.ToInt16(image.GetPropertyItem(imagePropertyId).Value, 0).ToString());
                                        break;
                                    }
                                case "9209":
                                    {
                                        metadata.Flash.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        metadata.Flash.DisplayValue = LookupExifValue("Flash", BitConverter.ToInt16(image.GetPropertyItem(imagePropertyId).Value, 0).ToString());
                                        break;
                                    }
                                case "829a":
                                    {
                                        byte[] value = image.GetPropertyItem(imagePropertyId).Value;
                                        metadata.ExposureTime.RawValueAsString = BitConverter.ToString(value);
                                        uint numerator = BitConverter.ToUInt32(value, 0);
                                        uint denominator = BitConverter.ToUInt32(value, 4);
                                        string displayValue;
                                        if (denominator is 0)
                                        {
                                            displayValue = NotAvailableString;
                                        }
                                        else
                                        {
                                            double exposure = (double)numerator / denominator;
                                            if (exposure >= 1)
                                            {
                                                displayValue = string.Format("{0:0.###} {1}", exposure, SecondString);
                                            }
                                            else
                                            {
                                                int reciprocal = (int)Math.Round(1.0 / exposure);
                                                displayValue = string.Format("1/{0} {1}", reciprocal, SecondString);
                                            }
                                        }
                                        metadata.ExposureTime.DisplayValue = displayValue;
                                        break;
                                    }
                                case "829d":
                                    {
                                        metadata.Fstop.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        int int1;
                                        int int2;
                                        int1 = BitConverter.ToInt32(image.GetPropertyItem(imagePropertyId).Value, 0);
                                        int2 = BitConverter.ToInt32(image.GetPropertyItem(imagePropertyId).Value, 4);
                                        metadata.Fstop.DisplayValue = string.Format("F/{0:0.0}", (double)int1 / int2);
                                        metadata.FNumber.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        metadata.FNumber.DisplayValue = BitConverter.ToInt16(image.GetPropertyItem(imagePropertyId).Value, 0).ToString();
                                        break;
                                    }
                                case "9201":
                                    {
                                        metadata.ShutterSpeed.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        string stringValue = BitConverter.ToInt32(image.GetPropertyItem(imagePropertyId).Value, 0).ToString();
                                        metadata.ShutterSpeed.DisplayValue = "1/" + stringValue;
                                        break;
                                    }
                                case "8822":
                                    {
                                        metadata.ExposureProg.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        metadata.ExposureProg.DisplayValue = LookupExifValue("ExposureProg", BitConverter.ToInt16(image.GetPropertyItem(imagePropertyId).Value, 0).ToString());
                                        break;
                                    }
                                case "8824":
                                    {
                                        metadata.SpectralSense.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        metadata.SpectralSense.DisplayValue = Encoding.ASCII.GetString(imagePropertyItemList[index].Value);
                                        break;
                                    }
                                case "8827":
                                    {
                                        hexVal = string.Empty;
                                        metadata.ISOSpeed.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        hexVal = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value).Substring(0, 2);
                                        metadata.ISOSpeed.DisplayValue = Convert.ToInt32(hexVal, 16).ToString();
                                        break;
                                    }
                                case "8828":
                                    {
                                        metadata.OECF.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        metadata.OECF.DisplayValue = Encoding.ASCII.GetString(imagePropertyItemList[index].Value);
                                        break;
                                    }
                                case "9000":
                                    {
                                        metadata.Ver.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        metadata.Ver.DisplayValue = Encoding.ASCII.GetString(imagePropertyItemList[index].Value).Substring(1, 1) + "." + Encoding.ASCII.GetString(imagePropertyItemList[index].Value).Substring(2, 2);
                                        break;
                                    }
                                case "9101":
                                    {
                                        metadata.CompConfig.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        metadata.CompConfig.DisplayValue = LookupExifValue("CompConfig", BitConverter.ToInt16(image.GetPropertyItem(imagePropertyId).Value, 0).ToString());
                                        break;
                                    }
                                case "9102":
                                    {
                                        byte[] value = image.GetPropertyItem(imagePropertyId).Value;
                                        uint numerator = BitConverter.ToUInt32(value, 0);
                                        uint denominator = BitConverter.ToUInt32(value, 4);
                                        metadata.CompBPP.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        metadata.CompBPP.DisplayValue = denominator is 0 ? NotAvailableString : string.Format("{0:0.##}", (double)numerator / denominator);
                                        break;
                                    }
                                case "9202":
                                    {
                                        hexVal = string.Empty;
                                        metadata.Aperture.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        hexVal = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value).Substring(0, 2);
                                        hexVal = Convert.ToInt32(hexVal, 16).ToString();
                                        hexVal += "00";
                                        metadata.Aperture.DisplayValue = hexVal.Substring(0, 1) + "." + hexVal.Substring(1, 2);
                                        break;
                                    }
                                case "9203":
                                    {
                                        hexVal = string.Empty;
                                        metadata.Brightness.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        hexVal = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value).Substring(0, 2);
                                        hexVal = Convert.ToInt32(hexVal, 16).ToString();
                                        hexVal += "00";
                                        metadata.Brightness.DisplayValue = hexVal.Substring(0, 1) + "." + hexVal.Substring(1, 2);
                                        break;
                                    }
                                case "9204":
                                    {
                                        metadata.ExposureBias.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        metadata.ExposureBias.DisplayValue = BitConverter.ToInt16(image.GetPropertyItem(imagePropertyId).Value, 0).ToString();
                                        break;
                                    }
                                case "9205":
                                    {
                                        hexVal = string.Empty;
                                        metadata.MaxAperture.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        hexVal = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value).Substring(0, 2);
                                        hexVal = Convert.ToInt32(hexVal, 16).ToString();
                                        hexVal += "00";
                                        metadata.MaxAperture.DisplayValue = hexVal.Substring(0, 1) + "." + hexVal.Substring(1, 2);
                                        break;
                                    }
                                case "9206":
                                    {
                                        metadata.SubjectDist.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        metadata.SubjectDist.DisplayValue = Encoding.ASCII.GetString(imagePropertyItemList[index].Value);
                                        break;
                                    }
                                case "9208":
                                    {
                                        metadata.LightSource.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        metadata.LightSource.DisplayValue = LookupExifValue("LightSource", BitConverter.ToInt16(image.GetPropertyItem(imagePropertyId).Value, 0).ToString());
                                        break;
                                    }
                                case "920a":
                                    {
                                        hexVal = string.Empty;
                                        metadata.FocalLength.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        hexVal = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value).Substring(0, 2);
                                        hexVal = Convert.ToInt32(hexVal, 16).ToString();
                                        hexVal += "00";
                                        metadata.FocalLength.DisplayValue = hexVal.Substring(0, 1) + "." + hexVal.Substring(1, 2);
                                        break;
                                    }
                                case "a000":
                                    {
                                        metadata.FPXVer.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        metadata.FPXVer.DisplayValue = Encoding.ASCII.GetString(imagePropertyItemList[index].Value).Substring(1, 1) + "." + Encoding.ASCII.GetString(imagePropertyItemList[index].Value).Substring(2, 2);
                                        break;
                                    }
                                case "a001":
                                    {
                                        metadata.ColorSpace.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        if (BitConverter.ToInt16(image.GetPropertyItem(imagePropertyId).Value, 0).ToString() is "1")
                                        {
                                            metadata.ColorSpace.DisplayValue = "RGB";
                                        }
                                        if (BitConverter.ToInt16(image.GetPropertyItem(imagePropertyId).Value, 0).ToString() is "65535")
                                        {
                                            metadata.ColorSpace.DisplayValue = "Uncalibrated";
                                        }
                                        break;
                                    }
                                case "a20e":
                                    {
                                        metadata.FocalXRes.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        metadata.FocalXRes.DisplayValue = BitConverter.ToInt16(image.GetPropertyItem(imagePropertyId).Value, 0).ToString();
                                        break;
                                    }
                                case "a20f":
                                    {
                                        metadata.FocalYRes.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        metadata.FocalYRes.DisplayValue = BitConverter.ToInt16(image.GetPropertyItem(imagePropertyId).Value, 0).ToString();
                                        break;
                                    }
                                case "a210":
                                    {
                                        string aa;
                                        metadata.FocalResUnit.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        aa = BitConverter.ToInt16(image.GetPropertyItem(imagePropertyId).Value, 0).ToString(); ;
                                        if (aa is "1")
                                        {
                                            metadata.FocalResUnit.DisplayValue = NoUnitString;
                                        }
                                        else if (aa is "2")
                                        {
                                            metadata.FocalResUnit.DisplayValue = FootString;
                                        }
                                        else if (aa is "3")
                                        {
                                            metadata.FocalResUnit.DisplayValue = CentimeterString;
                                        }
                                        break;
                                    }
                                case "a215":
                                    {
                                        metadata.ExposureIndex.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        metadata.ExposureIndex.DisplayValue = Encoding.ASCII.GetString(imagePropertyItemList[index].Value);
                                        break;
                                    }
                                case "a217":
                                    {
                                        string aa;
                                        aa = BitConverter.ToInt16(image.GetPropertyItem(imagePropertyId).Value, 0).ToString();
                                        metadata.SensingMethod.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        if (aa is "2")
                                        {
                                            metadata.SensingMethod.DisplayValue = OneChipColorAreaSensorString;
                                        }
                                        break;
                                    }
                                case "a301":
                                    {
                                        metadata.SceneType.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        metadata.SceneType.DisplayValue = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        break;
                                    }
                                case "a302":
                                    {
                                        metadata.CfaPattern.RawValueAsString = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        metadata.CfaPattern.DisplayValue = BitConverter.ToString(image.GetPropertyItem(imagePropertyId).Value);
                                        break;
                                    }
                            }
                            index++;
                        }
                    }
                    metadata.XResolution.DisplayValue = Convert.ToString(image.HorizontalResolution);
                    metadata.YResolution.DisplayValue = Convert.ToString(image.VerticalResolution);
                    metadata.ImageHeight.DisplayValue = Convert.ToString(image.Height);
                    metadata.ImageWidth.DisplayValue = Convert.ToString(image.Width);
                    image.Dispose();
                    return metadata;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(FileInformationPage), nameof(GetExifMetadata), 1, e);
                return null;
            }
        }

        /// <summary>
        /// 查找 EXIF 元素值
        /// </summary>
        private string LookupExifValue(string description, string value)
        {
            string descriptionValue = null;

            switch (description)
            {
                case "MeteringMode":
                    {
                        switch (value)
                        {
                            case "0": descriptionValue = UnknownString; break;
                            case "1": descriptionValue = AverageString; break;
                            case "2": descriptionValue = BimodalAverageString; break;
                            case "3": descriptionValue = SpotString; break;
                            case "4": descriptionValue = MultiSpotString; break;
                            case "5": descriptionValue = PatternString; break;
                            case "6": descriptionValue = PartialString; break;
                            case "255": descriptionValue = OtherString; break;
                        }
                        break;
                    }
                case "ResolutionUnit":
                    {
                        switch (value)
                        {
                            case "1": descriptionValue = NoUnitString; break;
                            case "2": descriptionValue = InchString; break;
                            case "3": descriptionValue = CentimeterString; break;
                        }
                        break;
                    }
                case "Flash":
                    {
                        switch (value)
                        {
                            case "0": descriptionValue = NoFlashString; break;
                            case "1": descriptionValue = FlashString; break;
                            case "5": descriptionValue = FlashNoStrobeReturnLightString; break;
                            case "7": descriptionValue = FlashStrobeReturnLightString; break;
                            case "9": descriptionValue = FlashForceString; break;
                            case "13": descriptionValue = FlashForceNoStrobeReturnLightString; break;
                            case "15": descriptionValue = FlashForceStrobeReturnLightString; break;
                            case "16": descriptionValue = FlashForceString; break;
                            case "24": descriptionValue = NoFlashAutoString; break;
                            case "25": descriptionValue = FlashAutoString; break;
                            case "29": descriptionValue = FlashAutoNoStrobeReturnLightString; break;
                            case "31": descriptionValue = FlashAutoStrobeReturnLightString; break;
                            case "32": descriptionValue = NoFlashFeatureString; break;
                            case "65": descriptionValue = FlashRedEyeString; break;
                            case "69": descriptionValue = FlashRedEyeNoStrobeReturnLightString; break;
                            case "71": descriptionValue = FlashRedEyeStrobeReturnLightString; break;
                            case "73": descriptionValue = FlashForceRedEyeString; break;
                            case "77": descriptionValue = FlashForceRedEyeNoStrobeReturnLightString; break;
                            case "79": descriptionValue = FlashForceRedEyeStrobeReturnLightString; break;
                            case "89": descriptionValue = FlashAutoRedEyeString; break;
                            case "93": descriptionValue = FlashAutoRedEyeNoStrobeReturnLightString; break;
                            case "95": descriptionValue = FlashAutoRedEyeStrobeReturnLightString; break;
                        }
                        break;
                    }
                case "ExposureProg":
                    {
                        switch (value)
                        {
                            case "0": descriptionValue = UnknownString; break;
                            case "1": descriptionValue = ManualString; break;
                            case "2": descriptionValue = NormalString; break;
                            case "3": descriptionValue = AperturePriorityString; break;
                            case "4": descriptionValue = ShutterPriorityString; break;
                            case "5": descriptionValue = CreateProgramString; break;
                            case "6": descriptionValue = OperationProgramString; break;
                            case "7": descriptionValue = VerticalModeString; break;
                            case "8": descriptionValue = HorizontalModeString; break;
                            case "9": descriptionValue = ReservedString; break;
                        }
                        break;
                    }
                case "CompConfig":
                    {
                        switch (value)
                        {
                            case "513": descriptionValue = "YCbCr"; break;
                        }
                        break;
                    }
                case "Aperture":
                    {
                        descriptionValue = value;
                        break;
                    }
                case "LightSource":
                    {
                        switch (value)
                        {
                            case "0": descriptionValue = UnknownString; break;
                            case "1": descriptionValue = SunlightString; break;
                            case "2": descriptionValue = FluorescentLampString; break;
                            case "3": descriptionValue = IncandescentLampString; break;
                            case "10": descriptionValue = FlashLampString; break;
                            case "17": descriptionValue = StandardLightAString; break;
                            case "18": descriptionValue = StandardLightBString; break;
                            case "19": descriptionValue = StandardLightCString; break;
                            case "20": descriptionValue = StandardLightD55String; break;
                            case "21": descriptionValue = StandardLightD65String; break;
                            case "22": descriptionValue = StandardLightD75String; break;
                            case "255": descriptionValue = OtherString; break;
                        }
                        break;
                    }
            }
            return descriptionValue;
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

        /// <summary>
        /// 获取文件信息解析是否成功
        /// </summary>
        private Visibility GetFileInformationSuccessfullyState(FileInformationResultKind fileInformationResultKind, bool isSuccessfully)
        {
            return isSuccessfully ? (fileInformationResultKind is FileInformationResultKind.File || fileInformationResultKind is FileInformationResultKind.VideoFile || fileInformationResultKind is FileInformationResultKind.AudioFile || fileInformationResultKind is FileInformationResultKind.TextFile || fileInformationResultKind is FileInformationResultKind.ImageFile) ? Visibility.Visible : Visibility.Collapsed : (fileInformationResultKind is FileInformationResultKind.File || fileInformationResultKind is FileInformationResultKind.VideoFile || fileInformationResultKind is FileInformationResultKind.AudioFile || fileInformationResultKind is FileInformationResultKind.TextFile || fileInformationResultKind is FileInformationResultKind.ImageFile) ? Visibility.Collapsed : Visibility.Visible;
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

        /// <summary>
        /// 获取选中项对应内容显示状态
        /// </summary>
        private Visibility GetSelectedSelectorBarItem(SelectorBarItem selectedItem, SelectorBarItem selectorBarItem)
        {
            return Equals(selectedItem, selectorBarItem) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
