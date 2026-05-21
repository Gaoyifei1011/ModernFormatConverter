using Microsoft.UI.Composition;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using ModernFormatConverter.Extensions.Others;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;

namespace ModernFormatConverter.Views.Controls
{
    /// <summary>
    /// The ImageCropper control allows user to crop image freely
    /// </summary>
    [TemplatePart(Name = LayoutGridName, Type = typeof(Grid))]
    [TemplatePart(Name = ImageCanvasPartName, Type = typeof(Canvas))]
    [TemplatePart(Name = SourceImagePartName, Type = typeof(Image))]
    [TemplatePart(Name = MaskAreaPathPartName, Type = typeof(Path))]
    [TemplatePart(Name = TopButtonPartName, Type = typeof(Button))]
    [TemplatePart(Name = BottomButtonPartName, Type = typeof(Button))]
    [TemplatePart(Name = LeftButtonPartName, Type = typeof(Button))]
    [TemplatePart(Name = RightButtonPartName, Type = typeof(Button))]
    [TemplatePart(Name = UpperLeftButtonPartName, Type = typeof(Button))]
    [TemplatePart(Name = UpperRightButtonPartName, Type = typeof(Button))]
    [TemplatePart(Name = LowerLeftButtonPartName, Type = typeof(Button))]
    [TemplatePart(Name = LowerRightButtonPartName, Type = typeof(Button))]
    public class ImageCropper : Control
    {
        private Grid _layoutGrid;
        private Canvas _imageCanvas;
        private Image _sourceImage;
        private Path _maskAreaPath;
        private Button _topButton;
        private Button _bottomButton;
        private Button _leftButton;
        private Button _rigthButton;
        private Button _upperLeftButton;
        private Button _upperRightButton;
        private Button _lowerLeftButton;
        private Button _lowerRigthButton;
        private double _startX;
        private double _startY;
        private double _endX;
        private double _endY;
        private readonly CompositeTransform _imageTransform = new();
        private readonly CompositeTransform _inverseImageTransform = new();
        private readonly GeometryGroup _maskAreaGeometryGroup = new() { FillRule = FillRule.EvenOdd };
        private RectangleGeometry _outerGeometry;
        private Geometry _innerGeometry;
        private Rect _currentCroppedRect = Rect.Empty;
        private Rect _restrictedCropRect = Rect.Empty;
        private Rect _restrictedSelectRect = Rect.Empty;
        private readonly TimeSpan _animationDuration = TimeSpan.FromSeconds(0.3);

        /// <summary>
        /// Key of the root layout container
        /// </summary>
        private const string LayoutGridName = "PART_LayoutGrid";

        /// <summary>
        /// Key of the Canvas that contains the image and control buttons
        /// </summary>
        private const string ImageCanvasPartName = "PART_ImageCanvas";

        /// <summary>
        /// Key of the Image Control inside the ImageCropper Control
        /// </summary>
        private const string SourceImagePartName = "PART_SourceImage";

        /// <summary>
        /// Key of the mask layer
        /// </summary>
        private const string MaskAreaPathPartName = "PART_MaskAreaPath";

        /// <summary>
        /// Key of the button that on the top
        /// </summary>
        private const string TopButtonPartName = "PART_TopButton";

        /// <summary>
        /// Key of the button on the bottom
        /// </summary>
        private const string BottomButtonPartName = "PART_BottomButton";

        /// <summary>
        /// Key of the button on the left
        /// </summary>
        private const string LeftButtonPartName = "PART_LeftButton";

        /// <summary>
        /// Key of the button on the right
        /// </summary>
        private const string RightButtonPartName = "PART_RightButton";

        /// <summary>
        /// Key of the button that on the upper left
        /// </summary>
        private const string UpperLeftButtonPartName = "PART_UpperLeftButton";

        /// <summary>
        /// Key of the button that on the upper right
        /// </summary>
        private const string UpperRightButtonPartName = "PART_UpperRightButton";

        /// <summary>
        /// Key of the button that on the lower left
        /// </summary>
        private const string LowerLeftButtonPartName = "PART_LowerLeftButton";

        /// <summary>
        /// Key of the button that on the lower right
        /// </summary>
        private const string LowerRightButtonPartName = "PART_LowerRightButton";

        /// <summary>
        /// Gets or sets the minimum cropped length(in pixel)
        /// </summary>
        public double MinCroppedPixelLength { get; set; } = 40;

        /// <summary>
        /// Gets or sets the minimum selectable length
        /// </summary>
        public double MinSelectedLength { get; set; } = 40;

        /// <summary>
        ///  Gets or sets the source of the cropped image
        /// </summary>
        public WriteableBitmap SourceImage
        {
            get => (WriteableBitmap)GetValue(SourceImageProperty);
            set => SetValue(SourceImageProperty, value);
        }

        /// <summary>
        /// Gets or sets the aspect ratio of the cropped image，the default value is -1
        /// </summary>
        public double AspectRatio
        {
            get => (double)GetValue(AspectRatioProperty);
            set => SetValue(AspectRatioProperty, value);
        }

        /// <summary>
        /// Gets or sets whether to use a circular ImageCropper
        /// </summary>
        public bool CircularCrop
        {
            get => (bool)GetValue(CircularCropProperty);
            set => SetValue(CircularCropProperty, value);
        }

        /// <summary>
        /// Gets or sets the mask on the cropped image
        /// </summary>
        public Brush Mask
        {
            get => (Brush)GetValue(MaskProperty);
            set => SetValue(MaskProperty, value);
        }

        /// <summary>
        /// Gets or sets a value for the style to use for the primary control buttons of the ImageCropper
        /// </summary>
        public Style PrimaryControlButtonStyle
        {
            get => (Style)GetValue(PrimaryControlButtonStyleProperty);
            set => SetValue(PrimaryControlButtonStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets a value for the style to use for the secondary control buttons of the ImageCropper
        /// </summary>
        public Style SecondaryControlButtonStyle
        {
            get => (Style)GetValue(SecondaryControlButtonStyleProperty);
            set => SetValue(SecondaryControlButtonStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the visibility of secondary control buttons
        /// </summary>
        public bool IsSecondaryControlButtonVisible
        {
            get => (bool)GetValue(IsSecondaryControlButtonVisibleProperty);
            set => SetValue(IsSecondaryControlButtonVisibleProperty, value);
        }

        /// <summary>
        /// Identifies the AspectRatio dependency property
        /// </summary>
        public static readonly DependencyProperty AspectRatioProperty = DependencyProperty.Register(nameof(AspectRatio), typeof(double), typeof(ImageCropper), new PropertyMetadata(-1d, OnAspectRatioChanged));

        /// <summary>
        /// Identifies the SourceImage dependency property
        /// </summary>
        public static readonly DependencyProperty SourceImageProperty = DependencyProperty.Register(nameof(SourceImage), typeof(WriteableBitmap), typeof(ImageCropper), new PropertyMetadata(null, OnSourceImageChanged));

        /// <summary>
        /// Identifies the CircularCrop dependency property
        /// </summary>
        public static readonly DependencyProperty CircularCropProperty = DependencyProperty.Register(nameof(CircularCrop), typeof(bool), typeof(ImageCropper), new PropertyMetadata(false, OnCircularCropChanged));

        /// <summary>
        /// Identifies the Mask dependency property
        /// </summary>
        public static readonly DependencyProperty MaskProperty = DependencyProperty.Register(nameof(Mask), typeof(Brush), typeof(ImageCropper), new PropertyMetadata(default(Brush)));

        /// <summary>
        /// Identifies the PrimaryControlButtonStyle dependency property
        /// </summary>
        public static readonly DependencyProperty PrimaryControlButtonStyleProperty = DependencyProperty.Register(nameof(PrimaryControlButtonStyle), typeof(Style), typeof(ImageCropper), new PropertyMetadata(default(Style)));

        /// <summary>
        /// Identifies the SecondaryControlButtonStyle dependency property
        /// </summary>
        public static readonly DependencyProperty SecondaryControlButtonStyleProperty = DependencyProperty.Register(nameof(SecondaryControlButtonStyle), typeof(Style), typeof(ImageCropper), new PropertyMetadata(default(Style)));

        /// <summary>
        /// Identifies the IsSecondaryControlButtonVisible dependency property
        /// </summary>
        public static readonly DependencyProperty IsSecondaryControlButtonVisibleProperty = DependencyProperty.Register(nameof(IsSecondaryControlButtonVisible), typeof(bool), typeof(ImageCropper), new PropertyMetadata(true, OnIsSecondaryControlButtonVisibleChanged));

        private Rect CanvasRect => new(0, 0, _imageCanvas?.ActualWidth ?? 0, _imageCanvas?.ActualHeight ?? 0);

        private bool KeepAspectRatio => UsedAspectRatio > 0;

        private double UsedAspectRatio => CircularCrop ? 1 : AspectRatio;

        /// <summary>
        /// Gets the minimum cropped size
        /// </summary>
        private Size MinCropSize
        {
            get
            {
                double aspectRatio = KeepAspectRatio ? UsedAspectRatio : 1;
                Size size = new(MinCroppedPixelLength, MinCroppedPixelLength);
                if (aspectRatio >= 1)
                {
                    size.Width = size.Height * aspectRatio;
                }
                else
                {
                    size.Height = size.Width / aspectRatio;
                }

                return size;
            }
        }

        /// <summary>
        /// Gets the minimum selectable size.
        /// </summary>
        private Size MinSelectSize
        {
            get
            {
                Rect realMinSelectSize = _imageTransform.TransformBounds(new Rect(new Point(), MinCropSize));
                double minLength = Math.Min(realMinSelectSize.Width, realMinSelectSize.Height);
                if (minLength < MinSelectedLength)
                {
                    double aspectRatio = KeepAspectRatio ? UsedAspectRatio : 1;
                    Size minSelectSize = new(MinSelectedLength, MinSelectedLength);
                    if (aspectRatio >= 1)
                    {
                        minSelectSize.Width = minSelectSize.Height * aspectRatio;
                    }
                    else
                    {
                        minSelectSize.Height = minSelectSize.Width / aspectRatio;
                    }
                    return minSelectSize;
                }

                return new Size(realMinSelectSize.Width, realMinSelectSize.Height);
            }
        }

        /// <summary>
        /// Initializes a new instance of the ImageCropper class
        /// </summary>
        public ImageCropper()
        {
            DefaultStyleKey = typeof(ImageCropper);
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate()
        {
            UnhookEvents();
            _layoutGrid = GetTemplateChild(LayoutGridName) as Grid;
            _imageCanvas = GetTemplateChild(ImageCanvasPartName) as Canvas;
            _sourceImage = GetTemplateChild(SourceImagePartName) as Image;
            _maskAreaPath = GetTemplateChild(MaskAreaPathPartName) as Path;
            _topButton = GetTemplateChild(TopButtonPartName) as Button;
            _bottomButton = GetTemplateChild(BottomButtonPartName) as Button;
            _leftButton = GetTemplateChild(LeftButtonPartName) as Button;
            _rigthButton = GetTemplateChild(RightButtonPartName) as Button;
            _upperLeftButton = GetTemplateChild(UpperLeftButtonPartName) as Button;
            _upperRightButton = GetTemplateChild(UpperRightButtonPartName) as Button;
            _lowerLeftButton = GetTemplateChild(LowerLeftButtonPartName) as Button;
            _lowerRigthButton = GetTemplateChild(LowerRightButtonPartName) as Button;
            HookUpEvents();
            UpdateControlButtonVisibility();
        }

        private void HookUpEvents()
        {
            _imageCanvas?.SizeChanged += ImageCanvas_SizeChanged;
            if (_sourceImage is not null)
            {
                _sourceImage.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
                _sourceImage.ManipulationDelta += SourceImage_ManipulationDelta;
            }

            _maskAreaPath?.Data = _maskAreaGeometryGroup;

            if (_topButton is not null)
            {
                _topButton.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
                _topButton.Tag = DragPosition.Top;
                _topButton.ManipulationDelta += ControlButton_ManipulationDelta;
                _topButton.ManipulationCompleted += ControlButton_ManipulationCompleted;
                _topButton.KeyDown += ControlButton_KeyDown;
                _topButton.KeyUp += ControlButton_KeyUp;
            }

            if (_bottomButton is not null)
            {
                _bottomButton.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
                _bottomButton.Tag = DragPosition.Bottom;
                _bottomButton.ManipulationDelta += ControlButton_ManipulationDelta;
                _bottomButton.ManipulationCompleted += ControlButton_ManipulationCompleted;
                _bottomButton.KeyDown += ControlButton_KeyDown;
                _bottomButton.KeyUp += ControlButton_KeyUp;
            }

            if (_leftButton is not null)
            {
                _leftButton.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
                _leftButton.Tag = DragPosition.Left;
                _leftButton.ManipulationDelta += ControlButton_ManipulationDelta;
                _leftButton.ManipulationCompleted += ControlButton_ManipulationCompleted;
                _leftButton.KeyDown += ControlButton_KeyDown;
                _leftButton.KeyUp += ControlButton_KeyUp;
            }

            if (_rigthButton is not null)
            {
                _rigthButton.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
                _rigthButton.Tag = DragPosition.Right;
                _rigthButton.ManipulationDelta += ControlButton_ManipulationDelta;
                _rigthButton.ManipulationCompleted += ControlButton_ManipulationCompleted;
                _rigthButton.KeyDown += ControlButton_KeyDown;
                _rigthButton.KeyUp += ControlButton_KeyUp;
            }

            if (_upperLeftButton is not null)
            {
                _upperLeftButton.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
                _upperLeftButton.Tag = DragPosition.UpperLeft;
                _upperLeftButton.ManipulationDelta += ControlButton_ManipulationDelta;
                _upperLeftButton.ManipulationCompleted += ControlButton_ManipulationCompleted;
                _upperLeftButton.KeyDown += ControlButton_KeyDown;
                _upperLeftButton.KeyUp += ControlButton_KeyUp;
            }

            if (_upperRightButton is not null)
            {
                _upperRightButton.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
                _upperRightButton.Tag = DragPosition.UpperRight;
                _upperRightButton.ManipulationDelta += ControlButton_ManipulationDelta;
                _upperRightButton.ManipulationCompleted += ControlButton_ManipulationCompleted;
                _upperRightButton.KeyDown += ControlButton_KeyDown;
                _upperRightButton.KeyUp += ControlButton_KeyUp;
            }

            if (_lowerLeftButton is not null)
            {
                _lowerLeftButton.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
                _lowerLeftButton.Tag = DragPosition.LowerLeft;
                _lowerLeftButton.ManipulationDelta += ControlButton_ManipulationDelta;
                _lowerLeftButton.ManipulationCompleted += ControlButton_ManipulationCompleted;
                _lowerLeftButton.KeyDown += ControlButton_KeyDown;
                _lowerLeftButton.KeyUp += ControlButton_KeyUp;
            }

            if (_lowerRigthButton is not null)
            {
                _lowerRigthButton.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
                _lowerRigthButton.Tag = DragPosition.LowerRight;
                _lowerRigthButton.ManipulationDelta += ControlButton_ManipulationDelta;
                _lowerRigthButton.ManipulationCompleted += ControlButton_ManipulationCompleted;
                _lowerRigthButton.KeyDown += ControlButton_KeyDown;
                _lowerRigthButton.KeyUp += ControlButton_KeyUp;
            }
        }

        private void UnhookEvents()
        {
            _imageCanvas?.SizeChanged -= ImageCanvas_SizeChanged;
            _sourceImage?.ManipulationDelta -= SourceImage_ManipulationDelta;
            _maskAreaPath?.Data = null;

            if (_topButton is not null)
            {
                _topButton.ManipulationDelta -= ControlButton_ManipulationDelta;
                _topButton.ManipulationCompleted -= ControlButton_ManipulationCompleted;
                _topButton.KeyDown -= ControlButton_KeyDown;
                _topButton.KeyUp -= ControlButton_KeyUp;
            }

            if (_bottomButton is not null)
            {
                _bottomButton.ManipulationDelta -= ControlButton_ManipulationDelta;
                _bottomButton.ManipulationCompleted -= ControlButton_ManipulationCompleted;
                _bottomButton.KeyDown -= ControlButton_KeyDown;
                _bottomButton.KeyUp -= ControlButton_KeyUp;
            }

            if (_leftButton is not null)
            {
                _leftButton.ManipulationDelta -= ControlButton_ManipulationDelta;
                _leftButton.ManipulationCompleted += ControlButton_ManipulationCompleted;
                _leftButton.KeyDown -= ControlButton_KeyDown;
                _leftButton.KeyUp -= ControlButton_KeyUp;
            }

            if (_rigthButton is not null)
            {
                _rigthButton.ManipulationDelta -= ControlButton_ManipulationDelta;
                _rigthButton.ManipulationCompleted -= ControlButton_ManipulationCompleted;
                _rigthButton.KeyDown -= ControlButton_KeyDown;
                _rigthButton.KeyUp -= ControlButton_KeyUp;
            }

            if (_upperLeftButton is not null)
            {
                _upperLeftButton.ManipulationDelta -= ControlButton_ManipulationDelta;
                _upperLeftButton.ManipulationCompleted -= ControlButton_ManipulationCompleted;
                _upperLeftButton.KeyDown -= ControlButton_KeyDown;
                _upperLeftButton.KeyUp -= ControlButton_KeyUp;
            }

            if (_upperRightButton is not null)
            {
                _upperRightButton.ManipulationDelta -= ControlButton_ManipulationDelta;
                _upperRightButton.ManipulationCompleted -= ControlButton_ManipulationCompleted;
                _upperRightButton.KeyDown -= ControlButton_KeyDown;
                _upperRightButton.KeyUp -= ControlButton_KeyUp;
            }

            if (_lowerLeftButton is not null)
            {
                _lowerLeftButton.ManipulationDelta -= ControlButton_ManipulationDelta;
                _lowerLeftButton.ManipulationCompleted -= ControlButton_ManipulationCompleted;
                _lowerLeftButton.KeyDown -= ControlButton_KeyDown;
                _lowerLeftButton.KeyUp -= ControlButton_KeyUp;
            }

            if (_lowerRigthButton is not null)
            {
                _lowerRigthButton.ManipulationDelta -= ControlButton_ManipulationDelta;
                _lowerRigthButton.ManipulationCompleted -= ControlButton_ManipulationCompleted;
                _lowerRigthButton.KeyDown -= ControlButton_KeyDown;
                _lowerRigthButton.KeyUp -= ControlButton_KeyUp;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (SourceImage is null || SourceImage.PixelWidth is 0 || SourceImage.PixelHeight is 0)
            {
                return base.MeasureOverride(availableSize);
            }
            if (double.IsInfinity(availableSize.Width) || double.IsInfinity(availableSize.Height))
            {
                if (!double.IsInfinity(availableSize.Width))
                {
                    availableSize.Height = availableSize.Width / SourceImage.PixelWidth * SourceImage.PixelHeight;
                }
                else if (!double.IsInfinity(availableSize.Height))
                {
                    availableSize.Width = availableSize.Height / SourceImage.PixelHeight * SourceImage.PixelWidth;
                }
                else
                {
                    availableSize.Width = SourceImage.PixelWidth;
                    availableSize.Height = SourceImage.PixelHeight;
                }
                base.MeasureOverride(availableSize);
                return availableSize;
            }
            return base.MeasureOverride(availableSize);
        }

        private static void OnSourceImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ImageCropper target = (ImageCropper)d;
            if (args.NewValue is WriteableBitmap bitmap)
            {
                if (bitmap.PixelWidth < target.MinCropSize.Width || bitmap.PixelHeight < target.MinCropSize.Height)
                {
                    target.SourceImage = null;
                    throw new ArgumentException("The resolution of the image is too small!");
                }
            }

            target.InvalidateMeasure();
            target.InitImageLayout();
        }

        private static void OnAspectRatioChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ImageCropper target = (ImageCropper)d;
            target.UpdateAspectRatio(true);
        }

        private static void OnCircularCropChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ImageCropper target = (ImageCropper)d;
            target.UpdateAspectRatio();
            target.UpdateControlButtonVisibility();
            target.UpdateCropShape();
        }

        private static void OnIsSecondaryControlButtonVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ImageCropper target = (ImageCropper)d;
            target.UpdateControlButtonVisibility();
        }

        private void ControlButton_KeyDown(object sender, KeyRoutedEventArgs args)
        {
            bool changed = false;
            Point diffPos = new();
            if (args.Key is VirtualKey.Left)
            {
                diffPos.X--;
                CoreVirtualKeyStates upKeyState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Up);
                CoreVirtualKeyStates downKeyState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Down);
                if (upKeyState is CoreVirtualKeyStates.Down)
                {
                    diffPos.Y--;
                }
                if (downKeyState is CoreVirtualKeyStates.Down)
                {
                    diffPos.Y++;
                }
                changed = true;
            }
            else if (args.Key is VirtualKey.Right)
            {
                diffPos.X++;
                CoreVirtualKeyStates upKeyState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Up);
                CoreVirtualKeyStates downKeyState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Down);
                if (upKeyState is CoreVirtualKeyStates.Down)
                {
                    diffPos.Y--;
                }
                if (downKeyState is CoreVirtualKeyStates.Down)
                {
                    diffPos.Y++;
                }
                changed = true;
            }
            else if (args.Key is VirtualKey.Up)
            {
                diffPos.Y--;
                CoreVirtualKeyStates leftKeyState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Left);
                CoreVirtualKeyStates rightKeyState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Right);
                if (leftKeyState is CoreVirtualKeyStates.Down)
                {
                    diffPos.X--;
                }
                if (rightKeyState is CoreVirtualKeyStates.Down)
                {
                    diffPos.X++;
                }
                changed = true;
            }
            else if (args.Key is VirtualKey.Down)
            {
                diffPos.Y++;
                CoreVirtualKeyStates leftKeyState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Left);
                CoreVirtualKeyStates rightKeyState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Right);
                if (leftKeyState is CoreVirtualKeyStates.Down)
                {
                    diffPos.X--;
                }
                if (rightKeyState is CoreVirtualKeyStates.Down)
                {
                    diffPos.X++;
                }
                changed = true;
            }

            if (changed)
            {
                FrameworkElement controlButton = (FrameworkElement)sender;
                object tag = controlButton.Tag;
                if (tag is not null && Enum.TryParse(tag.ToString(), false, out DragPosition dragPosition))
                {
                    UpdateCroppedRectWithAspectRatio(dragPosition, diffPos);
                }
            }
        }

        private void ControlButton_KeyUp(object sender, KeyRoutedEventArgs args)
        {
            Rect selectedRect = new(new Point(_startX, _startY), new Point(_endX, _endY));
            Rect croppedRect = _inverseImageTransform.TransformBounds(selectedRect);
            if (croppedRect.Width > MinCropSize.Width && croppedRect.Height > MinCropSize.Height)
            {
                croppedRect.Intersect(_restrictedCropRect);
                _currentCroppedRect = croppedRect;
            }

            UpdateImageLayout(true);
        }

        private void ControlButton_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs args)
        {
            Rect selectedRect = new(new Point(_startX, _startY), new Point(_endX, _endY));
            Rect croppedRect = _inverseImageTransform.TransformBounds(selectedRect);
            if (croppedRect.Width > MinCropSize.Width && croppedRect.Height > MinCropSize.Height)
            {
                croppedRect.Intersect(_restrictedCropRect);
                _currentCroppedRect = croppedRect;
            }

            UpdateImageLayout(true);
        }

        private void ControlButton_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            FrameworkElement controlButton = (FrameworkElement)sender;
            Point dragButtomPosition = new(Canvas.GetLeft(controlButton), Canvas.GetTop(controlButton));
            Point currentPointerPosition = new(
                dragButtomPosition.X + e.Position.X + e.Delta.Translation.X - controlButton.ActualWidth / 2,
                dragButtomPosition.Y + e.Position.Y + e.Delta.Translation.Y - controlButton.ActualHeight / 2);
            Point safePosition = _restrictedSelectRect.GetSafePoint(currentPointerPosition);
            Point safeDiffPoint = new(safePosition.X - dragButtomPosition.X, safePosition.Y - dragButtomPosition.Y);
            object tag = controlButton.Tag;
            if (tag is not null && Enum.TryParse(tag.ToString(), false, out DragPosition dragPosition))
            {
                UpdateCroppedRectWithAspectRatio(dragPosition, safeDiffPoint);
            }
        }

        private void SourceImage_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs args)
        {
            double offsetX = -args.Delta.Translation.X;
            double offsetY = -args.Delta.Translation.Y;
            offsetX = offsetX > 0
                ? Math.Min(offsetX, _restrictedSelectRect.X + _restrictedSelectRect.Width - _endX)
                : Math.Max(offsetX, _restrictedSelectRect.X - _startX);
            offsetY = offsetY > 0
                ? Math.Min(offsetY, _restrictedSelectRect.Y + _restrictedSelectRect.Height - _endY)
                : Math.Max(offsetY, _restrictedSelectRect.Y - _startY);
            Rect selectedRect = new(new Point(_startX, _startY), new Point(_endX, _endY));
            selectedRect.X += offsetX;
            selectedRect.Y += offsetY;
            Rect croppedRect = _inverseImageTransform.TransformBounds(selectedRect);
            croppedRect.Intersect(_restrictedCropRect);
            _currentCroppedRect = croppedRect;
            UpdateImageLayout();
        }

        private void ImageCanvas_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            if (SourceImage is null)
            {
                return;
            }
            UpdateImageLayout();
            UpdateMaskArea();
        }

        /// <summary>
        /// Load an image from a file
        /// </summary>
        public async Task LoadImageFromFileAsync(StorageFile imageFile)
        {
            WriteableBitmap writeableBitmap = new(1, 1);
            using (IRandomAccessStreamWithContentType stream = await imageFile.OpenReadAsync())
            {
                await writeableBitmap.SetSourceAsync(stream);
            }

            SourceImage = writeableBitmap;
        }

        /// <summary>
        /// Gets the cropped image
        /// </summary>
        /// <returns>WriteableBitmap</returns>
        public Task<WriteableBitmap> GetCroppedBitmapAsync()
        {
            return SourceImage?.GetCroppedImageAsync(_currentCroppedRect);
        }

        /// <summary>
        /// Save the cropped image to a file
        /// </summary>
        /// <param name="imageFile">The target file</param>
        /// <returns></returns>
        public async Task SaveAsync(StorageFile imageFile, BitmapFileFormat bitmapFileFormat)
        {
            if (SourceImage is null)
            {
                return;
            }

            using IRandomAccessStream fileStream = await imageFile.OpenAsync(FileAccessMode.ReadWrite, StorageOpenOptions.None);
            await SaveAsync(fileStream, bitmapFileFormat);
        }

        /// <summary>
        /// Saves the cropped image to a stream with the specified format
        /// </summary>
        /// <param name="stream">The target stream</param>
        /// <param name="bitmapFileFormat">the specified format</param>
        /// <returns>Task</returns>
        public async Task SaveAsync(IRandomAccessStream stream, BitmapFileFormat bitmapFileFormat)
        {
            if (SourceImage is null)
            {
                return;
            }
            BitmapEncoder bitmapEncoder = await BitmapEncoder.CreateAsync(WriteableBitmapExtensions.GetEncoderId(bitmapFileFormat), stream);
            await WriteableBitmapExtensions.CropImageAsync(SourceImage, _currentCroppedRect, bitmapEncoder);
        }

        /// <summary>
        /// Reset the cropped area
        /// </summary>
        public void Reset()
        {
            InitImageLayout(true);
        }

        /// <summary>
        /// Tries to set a new value for the cropped region, returns true if it succeeded, false if the region is invalid
        /// </summary>
        /// <param name="rect">The new cropped region</param>
        /// <returns>bool</returns>
        public bool TrySetCroppedRegion(Rect rect)
        {
            // Reject regions smaller than the minimum size
            if (rect.Width < MinCropSize.Width || rect.Height < MinCropSize.Height)
            {
                return false;
            }

            // Reject regions that are not contained in the original picture
            if (rect.Left < _restrictedCropRect.Left || rect.Top < _restrictedCropRect.Top || rect.Right > _restrictedCropRect.Right || rect.Bottom > _restrictedCropRect.Bottom)
            {
                return false;
            }

            // If an aspect ratio is set, reject regions that don't respect it
            // If cropping a circle, reject regions where the aspect ratio is not 1
            if (KeepAspectRatio && UsedAspectRatio != rect.Width / rect.Height)
            {
                return false;
            }

            _currentCroppedRect = rect;
            UpdateImageLayout(true);
            return true;
        }

        /// <summary>
        /// Initializes image source transform
        /// </summary>
        private void InitImageLayout(bool animate = false)
        {
            if (SourceImage is not null)
            {
                _restrictedCropRect = new Rect(0, 0, SourceImage.PixelWidth, SourceImage.PixelHeight);
                if (_restrictedCropRect.IsValid())
                {
                    _currentCroppedRect = KeepAspectRatio ? _restrictedCropRect.GetUniformRect(UsedAspectRatio) : _restrictedCropRect;
                    UpdateCropShape();
                    UpdateImageLayout(animate);
                }
            }
            else
            {
                _currentCroppedRect = Rect.Empty;
                _restrictedCropRect = Rect.Empty;
                _restrictedSelectRect = Rect.Empty;
            }
            UpdateControlButtonVisibility();
        }

        /// <summary>
        /// Update image source transform
        /// </summary>
        private void UpdateImageLayout(bool animate = false)
        {
            if (SourceImage is not null && CanvasRect.IsValid())
            {
                Rect uniformSelectedRect = CanvasRect.GetUniformRect(_currentCroppedRect.Width / _currentCroppedRect.Height);
                UpdateImageLayoutWithViewport(uniformSelectedRect, _currentCroppedRect, animate);
            }
        }

        /// <summary>
        /// Update image source transform
        /// </summary>
        /// <param name="viewport">Viewport</param>
        /// <param name="viewportImageRect"> The real image area of viewport</param>
        private void UpdateImageLayoutWithViewport(Rect viewport, Rect viewportImageRect, bool animate = false)
        {
            if (!viewport.IsValid() || !viewportImageRect.IsValid())
            {
                return;
            }

            double imageScale = viewport.Width / viewportImageRect.Width;
            _imageTransform.ScaleX = _imageTransform.ScaleY = imageScale;
            _imageTransform.TranslateX = viewport.X - viewportImageRect.X * imageScale;
            _imageTransform.TranslateY = viewport.Y - viewportImageRect.Y * imageScale;
            _inverseImageTransform.ScaleX = _inverseImageTransform.ScaleY = 1 / imageScale;
            _inverseImageTransform.TranslateX = -_imageTransform.TranslateX / imageScale;
            _inverseImageTransform.TranslateY = -_imageTransform.TranslateY / imageScale;
            Rect selectedRect = _imageTransform.TransformBounds(_currentCroppedRect);
            _restrictedSelectRect = _imageTransform.TransformBounds(_restrictedCropRect);
            Point startPoint = _restrictedSelectRect.GetSafePoint(new Point(selectedRect.X, selectedRect.Y));
            Point endPoint = _restrictedSelectRect.GetSafePoint(new Point(selectedRect.X + selectedRect.Width,
                selectedRect.Y + selectedRect.Height));
            if (animate)
            {
                AnimateUIElementOffset(new Point(_imageTransform.TranslateX, _imageTransform.TranslateY), _animationDuration, _sourceImage);
                AnimateUIElementScale(imageScale, _animationDuration, _sourceImage);
            }
            else
            {
                Visual targetVisual = ElementCompositionPreview.GetElementVisual(_sourceImage);
                targetVisual.Offset = new Vector3((float)_imageTransform.TranslateX, (float)_imageTransform.TranslateY, 0);
                targetVisual.Scale = new Vector3((float)imageScale);
            }
            UpdateSelectedRect(startPoint, endPoint, animate);
        }

        /// <summary>
        /// Update cropped area.
        /// </summary>
        /// <param name="dragPosition">The control point</param>
        /// <param name="diffPos">Position offset</param>
        private void UpdateCroppedRectWithAspectRatio(DragPosition dragPosition, Point diffPos)
        {
            if (diffPos == default || !CanvasRect.IsValid())
            {
                return;
            }

            double radian = 0d, diffPointRadian = 0d;
            if (KeepAspectRatio)
            {
                radian = Math.Atan(UsedAspectRatio);
                diffPointRadian = Math.Atan(diffPos.X / diffPos.Y);
            }

            Point startPoint = new(_startX, _startY);
            Point endPoint = new(_endX, _endY);
            Rect currentSelectedRect = new(startPoint, endPoint);
            double effectiveLength;
            switch (dragPosition)
            {
                case DragPosition.Top:
                    {
                        if (KeepAspectRatio)
                        {
                            Point originSizeChange = new(-diffPos.Y * UsedAspectRatio, -diffPos.Y);
                            Point safeChange = _restrictedSelectRect.GetSafeSizeChangeWhenKeepAspectRatio(dragPosition, currentSelectedRect, originSizeChange, UsedAspectRatio);
                            startPoint.X += -safeChange.X / 2;
                            endPoint.X -= -safeChange.X / 2;
                            startPoint.Y += -safeChange.Y;
                        }
                        else
                        {
                            startPoint.Y += diffPos.Y;
                        }
                        break;
                    }
                case DragPosition.Bottom:
                    {
                        if (KeepAspectRatio)
                        {
                            Point originSizeChange = new(diffPos.Y * UsedAspectRatio, diffPos.Y);
                            Point safeChange = _restrictedSelectRect.GetSafeSizeChangeWhenKeepAspectRatio(dragPosition, currentSelectedRect, originSizeChange, UsedAspectRatio);
                            startPoint.X -= safeChange.X / 2;
                            endPoint.X += safeChange.X / 2;
                            endPoint.Y += safeChange.Y;
                        }
                        else
                        {
                            endPoint.Y += diffPos.Y;
                        }
                        break;
                    }
                case DragPosition.Left:
                    {
                        if (KeepAspectRatio)
                        {
                            Point originSizeChange = new(-diffPos.X, -diffPos.X / UsedAspectRatio);
                            Point safeChange = _restrictedSelectRect.GetSafeSizeChangeWhenKeepAspectRatio(dragPosition, currentSelectedRect, originSizeChange, UsedAspectRatio);
                            startPoint.Y += -safeChange.Y / 2;
                            endPoint.Y -= -safeChange.Y / 2;
                            startPoint.X += -safeChange.X;
                        }
                        else
                        {
                            startPoint.X += diffPos.X;
                        }
                        break;
                    }
                case DragPosition.Right:
                    {
                        if (KeepAspectRatio)
                        {
                            Point originSizeChange = new(diffPos.X, diffPos.X / UsedAspectRatio);
                            Point safeChange = _restrictedSelectRect.GetSafeSizeChangeWhenKeepAspectRatio(dragPosition, currentSelectedRect, originSizeChange, UsedAspectRatio);
                            startPoint.Y -= safeChange.Y / 2;
                            endPoint.Y += safeChange.Y / 2;
                            endPoint.X += safeChange.X;
                        }
                        else
                        {
                            endPoint.X += diffPos.X;
                        }
                        break;
                    }
                case DragPosition.UpperLeft:
                    {
                        if (KeepAspectRatio)
                        {
                            effectiveLength = diffPos.Y / Math.Cos(diffPointRadian) * Math.Cos(diffPointRadian - radian);
                            Point originSizeChange = new(-effectiveLength * Math.Sin(radian), -effectiveLength * Math.Cos(radian));
                            Point safeChange = _restrictedSelectRect.GetSafeSizeChangeWhenKeepAspectRatio(dragPosition, currentSelectedRect, originSizeChange, UsedAspectRatio);
                            diffPos.X = -safeChange.X;
                            diffPos.Y = -safeChange.Y;
                        }
                        startPoint.X += diffPos.X;
                        startPoint.Y += diffPos.Y;
                        break;
                    }
                case DragPosition.UpperRight:
                    if (KeepAspectRatio)
                    {
                        diffPointRadian = -diffPointRadian;
                        effectiveLength = diffPos.Y / Math.Cos(diffPointRadian) * Math.Cos(diffPointRadian - radian);
                        Point originSizeChange = new(-effectiveLength * Math.Sin(radian), -effectiveLength * Math.Cos(radian));
                        Point safeChange = _restrictedSelectRect.GetSafeSizeChangeWhenKeepAspectRatio(dragPosition, currentSelectedRect, originSizeChange, UsedAspectRatio);
                        diffPos.X = safeChange.X;
                        diffPos.Y = -safeChange.Y;
                    }
                    endPoint.X += diffPos.X;
                    startPoint.Y += diffPos.Y;
                    break;

                case DragPosition.LowerLeft:
                    if (KeepAspectRatio)
                    {
                        diffPointRadian = -diffPointRadian;
                        effectiveLength = diffPos.Y / Math.Cos(diffPointRadian) * Math.Cos(diffPointRadian - radian);
                        Point originSizeChange = new(effectiveLength * Math.Sin(radian), effectiveLength * Math.Cos(radian));
                        Point safeChange = _restrictedSelectRect.GetSafeSizeChangeWhenKeepAspectRatio(dragPosition, currentSelectedRect, originSizeChange, UsedAspectRatio);
                        diffPos.X = -safeChange.X;
                        diffPos.Y = safeChange.Y;
                    }
                    startPoint.X += diffPos.X;
                    endPoint.Y += diffPos.Y;
                    break;

                case DragPosition.LowerRight:
                    if (KeepAspectRatio)
                    {
                        effectiveLength = diffPos.Y / Math.Cos(diffPointRadian) * Math.Cos(diffPointRadian - radian);
                        Point originSizeChange = new(effectiveLength * Math.Sin(radian), effectiveLength * Math.Cos(radian));
                        Point safeChange = _restrictedSelectRect.GetSafeSizeChangeWhenKeepAspectRatio(dragPosition, currentSelectedRect, originSizeChange, UsedAspectRatio);
                        diffPos.X = safeChange.X;
                        diffPos.Y = safeChange.Y;
                    }
                    endPoint.X += diffPos.X;
                    endPoint.Y += diffPos.Y;
                    break;
            }

            if (!RectExtensions.IsSafeRect(startPoint, endPoint, MinSelectSize))
            {
                if (KeepAspectRatio)
                {
                    if ((endPoint.Y - startPoint.Y) < (_endY - _startY) ||
                        (endPoint.X - startPoint.X) < (_endX - _startX))
                    {
                        return;
                    }
                }
                else
                {
                    Rect safeRect = RectExtensions.GetSafeRect(startPoint, endPoint, MinSelectSize, dragPosition);
                    safeRect.Intersect(_restrictedSelectRect);
                    startPoint = new Point(safeRect.X, safeRect.Y);
                    endPoint = new Point(safeRect.X + safeRect.Width, safeRect.Y + safeRect.Height);
                }
            }

            bool isEffectiveRegion = _restrictedSelectRect.IsSafePoint(startPoint) && _restrictedSelectRect.IsSafePoint(endPoint);
            Rect selectedRect = new(startPoint, endPoint);
            if (!isEffectiveRegion)
            {
                if (_restrictedSelectRect.GetContainsRect(ref selectedRect))
                {
                    startPoint = new Point(selectedRect.Left, selectedRect.Top);
                    endPoint = new Point(selectedRect.Right, selectedRect.Bottom);
                }
                else
                {
                    return;
                }
            }
            selectedRect.Union(CanvasRect);
            if (selectedRect != CanvasRect)
            {
                Rect croppedRect = _inverseImageTransform.TransformBounds(new Rect(startPoint, endPoint));
                croppedRect.Intersect(_restrictedCropRect);
                _currentCroppedRect = croppedRect;
                Rect viewportRect = CanvasRect.GetUniformRect(selectedRect.Width / selectedRect.Height);
                Rect viewportImgRect = _inverseImageTransform.TransformBounds(selectedRect);
                UpdateImageLayoutWithViewport(viewportRect, viewportImgRect);
            }
            else
            {
                UpdateSelectedRect(startPoint, endPoint);
            }
        }

        /// <summary>
        /// Update selection area
        /// </summary>
        /// <param name="startPoint">The point on the upper left corner</param>
        /// <param name="endPoint">The point on the lower right corner</param>
        private void UpdateSelectedRect(Point startPoint, Point endPoint, bool animate = false)
        {
            _startX = startPoint.X;
            _startY = startPoint.Y;
            _endX = endPoint.X;
            _endY = endPoint.Y;
            double centerX = (_endX - _startX) / 2 + _startX;
            double centerY = (_endY - _startY) / 2 + _startY;
            if (_topButton is not null)
            {
                UpdateThumbPosition(_topButton, new Point(centerX, _startY), animate);
            }

            if (_bottomButton is not null)
            {
                UpdateThumbPosition(_bottomButton, new Point(centerX, _endY), animate);
            }

            if (_leftButton is not null)
            {
                UpdateThumbPosition(_leftButton, new Point(_startX, centerY), animate);
            }

            if (_rigthButton is not null)
            {
                UpdateThumbPosition(_rigthButton, new Point(_endX, centerY), animate);
            }

            if (_upperLeftButton is not null)
            {
                UpdateThumbPosition(_upperLeftButton, new Point(_startX, _startY), animate);
            }

            if (_upperRightButton is not null)
            {
                UpdateThumbPosition(_upperRightButton, new Point(_endX, _startY), animate);
            }

            if (_lowerLeftButton is not null)
            {
                UpdateThumbPosition(_lowerLeftButton, new Point(_startX, _endY), animate);
            }

            if (_lowerRigthButton is not null)
            {
                UpdateThumbPosition(_lowerRigthButton, new Point(_endX, _endY), animate);
            }

            UpdateMaskArea(animate);
        }

        private void UpdateThumbPosition(UIElement target, Point position, bool animate = false)
        {
            if (animate)
            {
                Storyboard storyboard = new();
                storyboard.Children.Add(CreateDoubleAnimation(position.X, _animationDuration, target, "(Canvas.Left)", false));
                storyboard.Children.Add(CreateDoubleAnimation(position.Y, _animationDuration, target, "(Canvas.Top)", false));
                storyboard.Begin();
            }
            else
            {
                Canvas.SetLeft(target, position.X);
                Canvas.SetTop(target, position.Y);
            }
        }

        /// <summary>
        /// Update the mask layer
        /// </summary>
        private void UpdateMaskArea(bool animate = false)
        {
            if (_layoutGrid is null || _maskAreaGeometryGroup.Children.Count < 2)
            {
                return;
            }

            _outerGeometry.Rect = new Rect(-_layoutGrid.Padding.Left, -_layoutGrid.Padding.Top, _layoutGrid.ActualWidth, _layoutGrid.ActualHeight);

            if (CircularCrop)
            {
                if (_innerGeometry is EllipseGeometry ellipseGeometry)
                {
                    Point center = new((_endX - _startX) / 2 + _startX, (_endY - _startY) / 2 + _startY);
                    double radiusX = (_endX - _startX) / 2;
                    double radiusY = (_endY - _startY) / 2;
                    if (animate)
                    {
                        Storyboard storyboard = new();
                        storyboard.Children.Add(CreatePointAnimation(center, _animationDuration, ellipseGeometry, "EllipseGeometry.Center", true));
                        storyboard.Children.Add(CreateDoubleAnimation(radiusX, _animationDuration, ellipseGeometry, "EllipseGeometry.RadiusX", true));
                        storyboard.Children.Add(CreateDoubleAnimation(radiusY, _animationDuration, ellipseGeometry, "EllipseGeometry.RadiusY", true));
                        storyboard.Begin();
                    }
                    else
                    {
                        ellipseGeometry.Center = center;
                        ellipseGeometry.RadiusX = radiusX;
                        ellipseGeometry.RadiusY = radiusY;
                    }
                }
            }
            else
            {
                if (_innerGeometry is RectangleGeometry rectangleGeometry)
                {
                    Rect to = new(new Point(_startX, _startY), new Point(_endX, _endY));
                    if (animate)
                    {
                        Storyboard storyboard = new();
                        storyboard.Children.Add(CreateRectangleAnimation(to, _animationDuration, rectangleGeometry, true));
                        storyboard.Begin();
                    }
                    else
                    {
                        rectangleGeometry.Rect = to;
                    }
                }
            }
            _layoutGrid.Clip = new RectangleGeometry
            {
                Rect = new Rect(0, 0, _layoutGrid.ActualWidth, _layoutGrid.ActualHeight)
            };
        }

        private void UpdateCropShape()
        {
            _maskAreaGeometryGroup.Children.Clear();
            _outerGeometry = new RectangleGeometry();
            if (CircularCrop)
            {
                _innerGeometry = new EllipseGeometry();
            }
            else
            {
                _innerGeometry = new RectangleGeometry();
            }
            _maskAreaGeometryGroup.Children.Add(_outerGeometry);
            _maskAreaGeometryGroup.Children.Add(_innerGeometry);
            UpdateMaskArea();
        }

        /// <summary>
        /// Update image aspect ratio
        /// </summary>
        private void UpdateAspectRatio(bool animate = false)
        {
            if (KeepAspectRatio && SourceImage is not null && _restrictedSelectRect.IsValid())
            {
                double centerX = (_endX - _startX) / 2 + _startX;
                double centerY = (_endY - _startY) / 2 + _startY;
                double restrictedMinLength = MinCroppedPixelLength * _imageTransform.ScaleX;
                double maxSelectedLength = Math.Max(_endX - _startX, _endY - _startY);
                Rect viewRect = new(centerX - maxSelectedLength / 2, centerY - maxSelectedLength / 2, maxSelectedLength, maxSelectedLength);
                Rect uniformSelectedRect = viewRect.GetUniformRect(UsedAspectRatio);
                if (uniformSelectedRect.Width > _restrictedSelectRect.Width || uniformSelectedRect.Height > _restrictedSelectRect.Height)
                {
                    uniformSelectedRect = _restrictedSelectRect.GetUniformRect(UsedAspectRatio);
                }
                if (uniformSelectedRect.Width < restrictedMinLength || uniformSelectedRect.Height < restrictedMinLength)
                {
                    double scale = restrictedMinLength / Math.Min(uniformSelectedRect.Width, uniformSelectedRect.Height);
                    uniformSelectedRect.Width *= scale;
                    uniformSelectedRect.Height *= scale;
                    if (uniformSelectedRect.Width > _restrictedSelectRect.Width || uniformSelectedRect.Height > _restrictedSelectRect.Height)
                    {
                        AspectRatio = -1;
                        return;
                    }
                }
                if (_restrictedSelectRect.X > uniformSelectedRect.X)
                {
                    uniformSelectedRect.X += _restrictedSelectRect.X - uniformSelectedRect.X;
                }
                if (_restrictedSelectRect.Y > uniformSelectedRect.Y)
                {
                    uniformSelectedRect.Y += _restrictedSelectRect.Y - uniformSelectedRect.Y;
                }
                if ((_restrictedSelectRect.X + _restrictedSelectRect.Width) < (uniformSelectedRect.X + uniformSelectedRect.Width))
                {
                    uniformSelectedRect.X += (_restrictedSelectRect.X + _restrictedSelectRect.Width) - (uniformSelectedRect.X + uniformSelectedRect.Width);
                }
                if ((_restrictedSelectRect.Y + _restrictedSelectRect.Height) < (uniformSelectedRect.Y + uniformSelectedRect.Height))
                {
                    uniformSelectedRect.Y += (_restrictedSelectRect.Y + _restrictedSelectRect.Height) - (uniformSelectedRect.Y + uniformSelectedRect.Height);
                }
                Rect croppedRect = _inverseImageTransform.TransformBounds(uniformSelectedRect);
                croppedRect.Intersect(_restrictedCropRect);
                _currentCroppedRect = croppedRect;
                UpdateImageLayout(animate);
            }
        }

        /// <summary>
        /// Update the visibility of the control button.
        /// </summary>
        private void UpdateControlButtonVisibility()
        {
            Visibility cornerBtnVisibility = CircularCrop ? Visibility.Collapsed : Visibility.Visible;
            Visibility otherBtnVisibility = (CircularCrop || IsSecondaryControlButtonVisible) ? Visibility.Visible : Visibility.Collapsed;
            if (SourceImage is null)
            {
                cornerBtnVisibility = otherBtnVisibility = Visibility.Collapsed;
            }

            _topButton?.Visibility = otherBtnVisibility;
            _bottomButton?.Visibility = otherBtnVisibility;
            _leftButton?.Visibility = otherBtnVisibility;
            _rigthButton?.Visibility = otherBtnVisibility;
            _upperLeftButton?.Visibility = cornerBtnVisibility;
            _upperRightButton?.Visibility = cornerBtnVisibility;
            _lowerLeftButton?.Visibility = cornerBtnVisibility;
            _lowerRigthButton?.Visibility = cornerBtnVisibility;
        }

        private static void AnimateUIElementOffset(Point to, TimeSpan duration, UIElement target)
        {
            Visual targetVisual = ElementCompositionPreview.GetElementVisual(target);
            Compositor compositor = targetVisual.Compositor;
            LinearEasingFunction linear = compositor.CreateLinearEasingFunction();
            Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.Duration = duration;
            offsetAnimation.Target = "Offset";
            offsetAnimation.InsertKeyFrame(1.0f, new Vector3((float)to.X, (float)to.Y, 0), linear);
            targetVisual.StartAnimation("Offset", offsetAnimation);
        }

        private static void AnimateUIElementScale(double to, TimeSpan duration, UIElement target)
        {
            Visual targetVisual = ElementCompositionPreview.GetElementVisual(target);
            Compositor compositor = targetVisual.Compositor;
            LinearEasingFunction linear = compositor.CreateLinearEasingFunction();
            Vector3KeyFrameAnimation scaleAnimation = compositor.CreateVector3KeyFrameAnimation();
            scaleAnimation.Duration = duration;
            scaleAnimation.Target = "Scale";
            scaleAnimation.InsertKeyFrame(1.0f, new Vector3((float)to), linear);
            targetVisual.StartAnimation("Scale", scaleAnimation);
        }

        private static DoubleAnimation CreateDoubleAnimation(double to, TimeSpan duration, DependencyObject target, string propertyName, bool enableDependentAnimation)
        {
            DoubleAnimation animation = new()
            {
                To = to,
                Duration = new() { TimeSpan = duration, Type = DurationType.TimeSpan },
                EnableDependentAnimation = enableDependentAnimation
            };

            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, propertyName);

            return animation;
        }

        private static PointAnimation CreatePointAnimation(Point to, TimeSpan duration, DependencyObject target, string propertyName, bool enableDependentAnimation)
        {
            PointAnimation animation = new()
            {
                To = to,
                Duration = new() { TimeSpan = duration, Type = DurationType.TimeSpan },
                EnableDependentAnimation = enableDependentAnimation
            };

            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, propertyName);
            return animation;
        }

        private static ObjectAnimationUsingKeyFrames CreateRectangleAnimation(Rect to, TimeSpan duration, RectangleGeometry rectangle, bool enableDependentAnimation)
        {
            ObjectAnimationUsingKeyFrames animation = new()
            {
                Duration = new() { TimeSpan = duration, Type = DurationType.TimeSpan },
                EnableDependentAnimation = enableDependentAnimation
            };

            List<DiscreteObjectKeyFrame> frames = GetRectKeyframes(rectangle.Rect, to, duration);
            foreach (DiscreteObjectKeyFrame item in frames)
            {
                animation.KeyFrames.Add(item);
            }

            Storyboard.SetTarget(animation, rectangle);
            Storyboard.SetTargetProperty(animation, "RectangleGeometry.Rect");

            return animation;
        }

        private static List<DiscreteObjectKeyFrame> GetRectKeyframes(Rect from, Rect to, TimeSpan duration)
        {
            List<DiscreteObjectKeyFrame> rectKeyframes = [];
            TimeSpan step = TimeSpan.FromMilliseconds(10);
            double total = duration.TotalMilliseconds;
            Point startPointFrom = new(from.X, from.Y);
            Point endPointFrom = new(from.X + from.Width, from.Y + from.Height);
            Point startPointTo = new(to.X, to.Y);
            Point endPointTo = new(to.X + to.Width, to.Y + to.Height);
            for (TimeSpan i = new(); i < duration; i += step)
            {
                double progress = i.TotalMilliseconds / total;
                Point startPoint = new()
                {
                    X = startPointFrom.X + progress * (startPointTo.X - startPointFrom.X),
                    Y = startPointFrom.Y + progress * (startPointTo.Y - startPointFrom.Y),
                };
                Point endPoint = new()
                {
                    X = endPointFrom.X + progress * (endPointTo.X - endPointFrom.X),
                    Y = endPointFrom.Y + progress * (endPointTo.Y - endPointFrom.Y),
                };
                rectKeyframes.Add(new DiscreteObjectKeyFrame
                {
                    KeyTime = KeyTimeHelper.FromTimeSpan(i),
                    Value = new Rect(startPoint, endPoint)
                });
            }
            rectKeyframes.Add(new DiscreteObjectKeyFrame
            {
                KeyTime = new() { TimeSpan = duration },
                Value = to
            });
            return rectKeyframes;
        }
    }
}
