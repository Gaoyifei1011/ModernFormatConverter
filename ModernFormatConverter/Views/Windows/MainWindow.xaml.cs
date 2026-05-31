using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Content;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using ModernFormatConverter.Extensions.Backdrop;
using ModernFormatConverter.Extensions.DataType.Class;
using ModernFormatConverter.Extensions.DataType.Enums;
using ModernFormatConverter.Helpers.Root;
using ModernFormatConverter.Models;
using ModernFormatConverter.Services.Root;
using ModernFormatConverter.Services.Settings;
using ModernFormatConverter.Views.Pages;
using ModernFormatConverter.WindowsAPI.PInvoke.Comctl32;
using ModernFormatConverter.WindowsAPI.PInvoke.User32;
using ModernFormatConverter.WindowsAPI.PInvoke.Uxtheme;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI;

// 抑制 CA1806，CA1822，IDE0060 警告
#pragma warning disable CA1806,CA1822,IDE0060

namespace ModernFormatConverter.Views.Windows
{
    /// <summary>
    /// 应用主窗口
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly string RunningAdministratorString = ResourceService.WindowResource.GetString("RunningAdministrator");
        private readonly string TitleString = ResourceService.WindowResource.GetString("Title");
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        private readonly OverlappedPresenter overlappedPresenter;
        private readonly SUBCLASSPROC mainWindowSubClassProc;
        private readonly ContentIsland contentIsland;
        private readonly InputKeyboardSource inputKeyboardSource;
        private readonly InputPointerSource inputPointerSource;
        private bool isProgrammaticExpand;

        public new static MainWindow Current { get; private set; }

        private string _windowTitle;

        public string WindowTitle
        {
            get { return _windowTitle; }

            set
            {
                if (!string.Equals(_windowTitle, value))
                {
                    _windowTitle = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowTitle)));
                }
            }
        }

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

        private bool _isWindowMaximized;

        public bool IsWindowMaximized
        {
            get { return _isWindowMaximized; }

            set
            {
                if (!Equals(_isWindowMaximized, value))
                {
                    _isWindowMaximized = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsWindowMaximized)));
                }
            }
        }

        private bool _isBackEnabled;

        public bool IsBackEnabled
        {
            get { return _isBackEnabled; }

            set
            {
                if (!Equals(_isBackEnabled, value))
                {
                    _isBackEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBackEnabled)));
                }
            }
        }

        private NavigationViewItemModel _selectedItem;

        public NavigationViewItemModel SelectedItem
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

        public WinRTObservableCollection<NavigationViewItemModel> NavigationViewItemMenuItemsCollection { get; } = [];

        public WinRTObservableCollection<NavigationViewItemModel> NavigationViewItemFooterMenuItemsCollection { get; } = [];

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            Current = this;
            InitializeComponent();

            // 窗口部分初始化
            WindowTitle = RuntimeHelper.IsElevated ? TitleString + RunningAdministratorString : TitleString;
            overlappedPresenter = AppWindow.Presenter as OverlappedPresenter;
            ExtendsContentIntoTitleBar = true;
            AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            AppWindow.TitleBar.InactiveBackgroundColor = Colors.Transparent;
            AppWindow.TitleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;
            IsWindowMaximized = overlappedPresenter.State is OverlappedPresenterState.Maximized;
            contentIsland = ContentIsland.FindAllForCompositor(Compositor)[0];
            inputKeyboardSource = InputKeyboardSource.GetForIsland(contentIsland);
            inputPointerSource = InputPointerSource.GetForIsland(contentIsland);

            // 挂载相应的事件
            ThemeService.PropertyChanged += OnServicePropertyChanged;
            BackdropService.PropertyChanged += OnServicePropertyChanged;
            inputKeyboardSource.SystemKeyDown += OnSystemKeyDown;
            inputPointerSource.PointerReleased += OnPointerReleased;

            // 标题栏和右键菜单设置
            SetClassicMenuTheme((Content as FrameworkElement).ActualTheme);

            // 为应用主窗口添加窗口过程
            mainWindowSubClassProc = new SUBCLASSPROC(MainWindowSubClassProc);
            Comctl32Library.SetWindowSubclass((nint)AppWindow.Id.Value, mainWindowSubClassProc, 0, 0);

            SetWindowTheme();
            SetSystemBackdrop();

            // 默认直接显示到窗口中间
            User32Library.GetWindowRect((nint)AppWindow.Id.Value, out RECT rect);
            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;
            User32Library.SetWindowPos((nint)AppWindow.Id.Value, 0, (System.Windows.Forms.SystemInformation.WorkingArea.Width - width) / 2, (System.Windows.Forms.SystemInformation.WorkingArea.Height - height) / 2, 0, 0, SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOZORDER);

            int dpi = User32Library.GetDpiForWindow((nint)AppWindow.Id.Value);
            overlappedPresenter.PreferredMinimumWidth = Convert.ToInt32(1000 * Convert.ToDouble(dpi) / 96);
            overlappedPresenter.PreferredMinimumHeight = Convert.ToInt32(600 * Convert.ToDouble(dpi) / 96);

            NavigationViewItemMenuItemsCollection.Add(new NavigationViewItemModel()
            {
                NavigationViewItemKind = NavigationViewItemKind.Item,
                NavigationIcon = new ImageIcon() { Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/ControlIcon/Home.png") } },
                NavigationTitle = ResourceService.WindowResource.GetString("Home"),
                NavigationTag = "Home",
                ParentTag = null,
                NavigationPage = typeof(HomePage),
                VisibleState = Visibility.Visible
            });
            NavigationViewItemMenuItemsCollection.Add(new NavigationViewItemModel()
            {
                NavigationViewItemKind = NavigationViewItemKind.Seperator,
                NavigationIcon = null,
                NavigationTitle = null,
                NavigationTag = null,
                ParentTag = null,
                NavigationPage = null,
                VisibleState = Visibility.Visible
            });
            NavigationViewItemModel conversionToolsItem = new()
            {
                NavigationViewItemKind = NavigationViewItemKind.Item,
                NavigationIcon = new ImageIcon() { Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/ControlIcon/ConversionTools.png") } },
                NavigationTitle = ResourceService.WindowResource.GetString("ConversionTools"),
                NavigationTag = "ConversionTools",
                ParentTag = null,
                NavigationPage = null,
                VisibleState = Visibility.Visible
            };
            conversionToolsItem.NavigationViewItemMenuItemsCollection.Add(new NavigationViewItemModel()
            {
                NavigationViewItemKind = NavigationViewItemKind.Item,
                NavigationIcon = new ImageIcon() { Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/ControlIcon/VideoConversion.png") } },
                NavigationTitle = ResourceService.WindowResource.GetString("VideoConversion"),
                NavigationTag = "VideoConversion",
                ParentTag = "ConversionTools",
                NavigationPage = typeof(VideoConversionPage),
                VisibleState = Visibility.Visible
            });
            conversionToolsItem.NavigationViewItemMenuItemsCollection.Add(new NavigationViewItemModel()
            {
                NavigationViewItemKind = NavigationViewItemKind.Item,
                NavigationIcon = new ImageIcon() { Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/ControlIcon/AudioConversion.png") } },
                NavigationTitle = ResourceService.WindowResource.GetString("AudioConversion"),
                NavigationTag = "AudioConversion",
                ParentTag = "ConversionTools",
                NavigationPage = typeof(AudioConversionPage),
                VisibleState = Visibility.Visible
            });
            conversionToolsItem.NavigationViewItemMenuItemsCollection.Add(new NavigationViewItemModel()
            {
                NavigationViewItemKind = NavigationViewItemKind.Item,
                NavigationIcon = new ImageIcon() { Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/ControlIcon/PhotoConversion.png") } },
                NavigationTitle = ResourceService.WindowResource.GetString("PhotoConversion"),
                NavigationTag = "PhotoConversion",
                ParentTag = "ConversionTools",
                NavigationPage = typeof(PhotoConversionPage),
                VisibleState = Visibility.Visible
            });
            NavigationViewItemMenuItemsCollection.Add(conversionToolsItem);
            NavigationViewItemMenuItemsCollection.Add(new NavigationViewItemModel()
            {
                NavigationViewItemKind = NavigationViewItemKind.Seperator,
                NavigationIcon = null,
                NavigationTitle = null,
                NavigationTag = null,
                ParentTag = null,
                NavigationPage = null,
                VisibleState = Visibility.Visible
            });
            NavigationViewItemMenuItemsCollection.Add(new NavigationViewItemModel()
            {
                NavigationViewItemKind = NavigationViewItemKind.Item,
                NavigationIcon = new ImageIcon() { Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/ControlIcon/TaskManager.png") } },
                NavigationTitle = ResourceService.WindowResource.GetString("TaskManager"),
                NavigationTag = "TaskManager",
                ParentTag = null,
                NavigationPage = typeof(TaskManagerPage),
                VisibleState = Visibility.Visible
            });
            NavigationViewItemMenuItemsCollection.Add(new NavigationViewItemModel()
            {
                NavigationViewItemKind = NavigationViewItemKind.Seperator,
                NavigationIcon = null,
                NavigationTitle = null,
                NavigationTag = null,
                ParentTag = null,
                NavigationPage = null,
                VisibleState = Visibility.Visible
            });
            NavigationViewItemModel otherToolsItem = new()
            {
                NavigationViewItemKind = NavigationViewItemKind.Item,
                NavigationIcon = new ImageIcon() { Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/ControlIcon/OtherTools.png") } },
                NavigationTitle = ResourceService.WindowResource.GetString("OtherTools"),
                NavigationTag = "OtherTools",
                ParentTag = null,
                NavigationPage = null,
                VisibleState = Visibility.Visible
            };
            otherToolsItem.NavigationViewItemMenuItemsCollection.Add(new NavigationViewItemModel()
            {
                NavigationViewItemKind = NavigationViewItemKind.Item,
                NavigationIcon = new ImageIcon() { Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/ControlIcon/FileInformation.png") } },
                NavigationTitle = ResourceService.WindowResource.GetString("FileInformation"),
                NavigationTag = "FileInformation",
                ParentTag = "OtherTools",
                NavigationPage = typeof(FileInformationPage),
                VisibleState = Visibility.Visible
            });
            otherToolsItem.NavigationViewItemMenuItemsCollection.Add(new NavigationViewItemModel()
            {
                NavigationViewItemKind = NavigationViewItemKind.Item,
                NavigationIcon = new ImageIcon() { Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/ControlIcon/HATest.png") } },
                NavigationTitle = ResourceService.WindowResource.GetString("HATest"),
                NavigationTag = "HATest",
                ParentTag = "ConversionTools",
                NavigationPage = typeof(HATestPage),
                VisibleState = Visibility.Visible
            });
            otherToolsItem.NavigationViewItemMenuItemsCollection.Add(new NavigationViewItemModel()
            {
                NavigationViewItemKind = NavigationViewItemKind.Item,
                NavigationIcon = new ImageIcon() { Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/ControlIcon/CustomCommand.png") } },
                NavigationTitle = ResourceService.WindowResource.GetString("CustomCommand"),
                NavigationTag = "CustomCommand",
                ParentTag = "ConversionTools",
                NavigationPage = typeof(CustomCommandPage),
                VisibleState = Visibility.Visible
            });
            NavigationViewItemMenuItemsCollection.Add(otherToolsItem);
            NavigationViewItemFooterMenuItemsCollection.Add(new NavigationViewItemModel()
            {
                NavigationViewItemKind = NavigationViewItemKind.Item,
                NavigationIcon = new ImageIcon() { Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/ControlIcon/Settings.png") } },
                NavigationTitle = ResourceService.WindowResource.GetString("Settings"),
                NavigationTag = "Settings",
                ParentTag = null,
                NavigationPage = typeof(SettingsPage),
                VisibleState = Visibility.Visible
            });
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
        /// 窗口还原
        /// </summary>
        private void OnRestoreClicked(object sender, RoutedEventArgs args)
        {
            User32Library.SendMessage((nint)AppWindow.Id.Value, WindowMessage.WM_SYSCOMMAND, (nuint)SYSTEMCOMMAND.SC_RESTORE, 0);
        }

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
        /// 窗口最小化
        /// </summary>
        private void OnMinimizeClicked(object sender, RoutedEventArgs args)
        {
            User32Library.SendMessage((nint)AppWindow.Id.Value, WindowMessage.WM_SYSCOMMAND, (nuint)SYSTEMCOMMAND.SC_MINIMIZE, 0);
        }

        /// <summary>
        /// 窗口最大化
        /// </summary>
        private void OnMaximizeClicked(object sender, RoutedEventArgs args)
        {
            User32Library.SendMessage((nint)AppWindow.Id.Value, WindowMessage.WM_SYSCOMMAND, (nuint)SYSTEMCOMMAND.SC_MAXIMIZE, 0);
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

        /// <summary>
        /// 按下 Alt + BackSpace 键时，导航控件返回到上一页
        /// </summary>
        private void OnKeyDown(object sender, KeyRoutedEventArgs args)
        {
            if (args.Key is VirtualKey.Back && args.KeyStatus.IsMenuKeyDown)
            {
                NavigationFrom();
            }
        }

        #endregion 第三部分：窗口内容挂载的事件

        #region 第四部分：导航控件及其内容挂载的事件

        /// <summary>
        /// 当后退按钮收到交互（如单击或点击）时发生
        /// </summary>
        private void OnBackClicked(object sender, RoutedEventArgs args)
        {
            NavigationFrom();
        }

        /// <summary>
        /// 导航控件加载完成后初始化内容，初始化导航控件属性、屏幕缩放比例值和应用的背景色
        /// </summary>
        private async void OnLoaded(object sender, RoutedEventArgs args)
        {
            // 设置标题栏主题
            SetTitleBarTheme((Content as FrameworkElement).ActualTheme);

            SelectedItem = NavigationViewItemMenuItemsCollection[0];
            NavigateTo(typeof(HomePage));
            IsBackEnabled = CanGoBack();
            SetPopupControlTheme(WindowTheme);
        }

        /// <summary>
        /// 当导航栏菜单中的选中项发生改变时触发的事件
        /// </summary>
        private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is not null && !Equals(SelectedItem, args.SelectedItem))
            {
                SelectedItem = args.SelectedItem as NavigationViewItemModel;

                // 对应的页面为空，选中项修改为已经选择的页面
                if (SelectedItem.NavigationPage is null)
                {
                    Type currentPageType = GetCurrentPageType();
                    NavigationViewItemModel selectedNavigationViewItem = GetSelectedItem(currentPageType, NavigationViewItemMenuItemsCollection);
                    if (selectedNavigationViewItem is not null)
                    {
                        SelectedItem = selectedNavigationViewItem;
                    }
                    else
                    {
                        selectedNavigationViewItem = GetSelectedItem(currentPageType, NavigationViewItemFooterMenuItemsCollection);
                        SelectedItem = selectedNavigationViewItem is not null ? selectedNavigationViewItem : null;
                    }
                }
                // 切换到选中项对应的页面
                else
                {
                    NavigateTo(SelectedItem.NavigationPage);
                }
            }
        }

        /// <summary>
        /// 当树中的节点开始展开时发生时的事件
        /// </summary>
        private async void OnExpanding(NavigationView sender, NavigationViewItemExpandingEventArgs args)
        {
            Type currentPageType = GetCurrentPageType();
            if (isProgrammaticExpand)
            {
                isProgrammaticExpand = false;
                await Task.Delay(5);
            }

            // 切换到选中页面对应的项
            SelectedItem = null;
            NavigationViewItemModel selectedNavigationViewItem = GetSelectedItem(currentPageType, NavigationViewItemMenuItemsCollection);
            if (selectedNavigationViewItem is not null)
            {
                SelectedItem = selectedNavigationViewItem;
            }
            else
            {
                selectedNavigationViewItem = GetSelectedItem(currentPageType, NavigationViewItemFooterMenuItemsCollection);
                SelectedItem = selectedNavigationViewItem is not null ? selectedNavigationViewItem : null;
            }
        }

        /// <summary>
        /// 当树中的节点开始折叠时发生时的事件
        /// </summary>
        private void OnCollapsed(NavigationView sender, NavigationViewItemCollapsedEventArgs args)
        {
            Type currentPageType = GetCurrentPageType();

            // 切换到选中页面对应的项
            SelectedItem = null;
            NavigationViewItemModel selectedNavigationViewItem = GetSelectedItem(currentPageType, NavigationViewItemMenuItemsCollection);
            if (selectedNavigationViewItem is not null)
            {
                SelectedItem = selectedNavigationViewItem;
            }
            else
            {
                selectedNavigationViewItem = GetSelectedItem(currentPageType, NavigationViewItemFooterMenuItemsCollection);
                SelectedItem = selectedNavigationViewItem is not null ? selectedNavigationViewItem : null;
            }
        }

        /// <summary>
        /// 导航完成后发生
        /// </summary>
        private async void OnNavigated(object sender, NavigationEventArgs args)
        {
            try
            {
                Type currentPageType = GetCurrentPageType();

                // 切换到选中页面对应的项
                NavigationViewItemModel selectedNavigationViewItem = GetSelectedItem(currentPageType, NavigationViewItemMenuItemsCollection);
                // 显示未打开的父项
                if (ShowParentNavigationViewItem(selectedNavigationViewItem))
                {
                    await Task.Delay(5);
                }

                SelectedItem = null;
                if (selectedNavigationViewItem is not null)
                {
                    SelectedItem = selectedNavigationViewItem;
                }
                else
                {
                    selectedNavigationViewItem = GetSelectedItem(currentPageType, NavigationViewItemFooterMenuItemsCollection);
                    SelectedItem = selectedNavigationViewItem is not null ? selectedNavigationViewItem : null;
                }

                IsBackEnabled = CanGoBack();
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(MainWindow), nameof(OnNavigated), 1, e);
            }
        }

        /// <summary>
        /// 导航失败时发生
        /// </summary>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs args)
        {
            args.Handled = true;
            LogService.WriteLog(TraceEventType.Warning, nameof(ModernFormatConverter), nameof(MainWindow), nameof(OnNavigationFailed), 1, args.Exception);
            (Application.Current as MainApp).Dispose();
        }

        #endregion 第四部分：导航控件及其内容挂载的事件

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
                VisualStateManager.GoToState(MainPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[2]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(MicaKind.BaseAlt);
                VisualStateManager.GoToState(MainPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[3]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Default);
                VisualStateManager.GoToState(MainPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[4]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Base);
                VisualStateManager.GoToState(MainPage, "BackgroundTransparent", false);
            }
            else if (string.Equals(BackdropService.AppBackdrop, BackdropService.BackdropList[5]))
            {
                WindowSystemBackdrop = new MaterialBackdrop(DesktopAcrylicKind.Thin);
                VisualStateManager.GoToState(MainPage, "BackgroundTransparent", false);
            }
            else
            {
                WindowSystemBackdrop = null;
                VisualStateManager.GoToState(MainPage, "BackgroundDefault", false);
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
        /// 应用主窗口消息处理
        /// </summary>
        private nint MainWindowSubClassProc(nint hWnd, WindowMessage Msg, nuint wParam, nint lParam, uint uIdSubclass, nint dwRefData)
        {
            switch (Msg)
            {
                // 窗口位置发生变化时触发的消息
                case WindowMessage.WM_MOVE:
                    {
                        if (TitlebarMenuFlyout.IsOpen)
                        {
                            TitlebarMenuFlyout.Hide();
                        }

                        if (overlappedPresenter is not null)
                        {
                            IsWindowMaximized = overlappedPresenter.State is OverlappedPresenterState.Maximized;
                        }
                        break;
                    }
                // 窗口大小发生变化时触发的消息
                case WindowMessage.WM_SIZE:
                    {
                        if (TitlebarMenuFlyout.IsOpen)
                        {
                            TitlebarMenuFlyout.Hide();
                        }

                        if (overlappedPresenter is not null)
                        {
                            IsWindowMaximized = overlappedPresenter.State is OverlappedPresenterState.Maximized;
                        }
                        break;
                    }
                // 窗口激活状态发生变化时触发的消息
                case WindowMessage.WM_ACTIVATE:
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
                            LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(MainWindow), nameof(MainWindowSubClassProc), 1, e);
                        }
                        break;
                    }
                // 窗口关闭时触发的消息
                case WindowMessage.WM_CLOSE:
                    {
                        ThemeService.PropertyChanged -= OnServicePropertyChanged;
                        BackdropService.PropertyChanged -= OnServicePropertyChanged;
                        inputKeyboardSource.SystemKeyDown -= OnSystemKeyDown;
                        inputPointerSource.PointerReleased -= OnPointerReleased;
                        Comctl32Library.RemoveWindowSubclass((nint)AppWindow.Id.Value, mainWindowSubClassProc, 0);
                        (Application.Current as MainApp).Dispose();
                        return 0;
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
                        overlappedPresenter.PreferredMinimumWidth = Convert.ToInt32(1000 * Convert.ToDouble(wParam) / 96);
                        overlappedPresenter.PreferredMinimumHeight = Convert.ToInt32(600 * Convert.ToDouble(wParam) / 96);
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
                                    Position = new Point(0, 45),
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

        #region 第八部分：窗口导航方法

        /// <summary>
        /// 页面向前导航
        /// </summary>
        public void NavigateTo(Type navigationPageType, object parameter = null)
        {
            try
            {
                // 导航到该项目对应的页面
                if (!Equals(GetCurrentPageType(), navigationPageType))
                {
                    (MainNavigationView.Content as Frame).Navigate(navigationPageType, parameter);
                }
            }
            catch (Exception e)
            {
                LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(MainWindow), nameof(NavigateTo), 1, e);
            }
        }

        /// <summary>
        /// 页面向后导航
        /// </summary>
        public void NavigationFrom()
        {
            if (GetFrameContent() is VideoConversionPage videoConversionPage && videoConversionPage.BreadCollection.Count is 2)
            {
                videoConversionPage.NavigateTo(videoConversionPage.PageList[0], null, false);
                return;
            }

            if ((MainNavigationView.Content as Frame).CanGoBack)
            {
                (MainNavigationView.Content as Frame).GoBack();
            }
        }

        /// <summary>
        /// 获取当前导航到的页
        /// </summary>
        public Type GetCurrentPageType()
        {
            return (MainNavigationView.Content as Frame).CurrentSourcePageType;
        }

        /// <summary>
        /// 获取当前导航控件内容对应的页面
        /// </summary>
        public object GetFrameContent()
        {
            return (MainNavigationView.Content as Frame).Content;
        }

        /// <summary>
        /// 检查当前页面是否能向后导航
        /// </summary>
        public bool CanGoBack()
        {
            return (MainNavigationView.Content as Frame).CanGoBack;
        }

        /// <summary>
        /// 获取选中项
        /// </summary>
        public NavigationViewItemModel GetSelectedItem(Type currentPageType, WinRTObservableCollection<NavigationViewItemModel> navigationViewItemMenuItemCollection)
        {
            foreach (NavigationViewItemModel navigationViewItem in navigationViewItemMenuItemCollection)
            {
                if (Equals(navigationViewItem.NavigationPage, currentPageType))
                {
                    return navigationViewItem;
                }

                // 递归遍历
                if (GetSelectedItem(currentPageType, navigationViewItem.NavigationViewItemMenuItemsCollection) is NavigationViewItemModel searchedNavigationViewItem)
                {
                    return searchedNavigationViewItem;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取选中的父项
        /// </summary>
        private NavigationViewItemModel GetParentNavigationViewItem(NavigationViewItemModel searchNavigationViewItem)
        {
            foreach (NavigationViewItemModel naviationViewItem in NavigationViewItemMenuItemsCollection)
            {
                if (string.Equals(naviationViewItem.NavigationTag, searchNavigationViewItem.ParentTag))
                {
                    return naviationViewItem;
                }
            }

            foreach (NavigationViewItemModel naviationViewItem in NavigationViewItemFooterMenuItemsCollection)
            {
                if (string.Equals(naviationViewItem.NavigationTag, searchNavigationViewItem.ParentTag))
                {
                    return naviationViewItem;
                }
            }

            return null;
        }

        /// <summary>
        /// 显示未打开的父项
        /// </summary>
        private bool ShowParentNavigationViewItem(NavigationViewItemModel selectedNavigationViewItem)
        {
            // 如果选中的是子项，而父项没有展开，则自动展开父项中所有的子项
            if (selectedNavigationViewItem is not null && !string.IsNullOrEmpty(selectedNavigationViewItem.ParentTag))
            {
                NavigationViewItemModel parentNavigationViewModelItem = GetParentNavigationViewItem(selectedNavigationViewItem);
                if (MainNavigationView.ContainerFromMenuItem(parentNavigationViewModelItem) is NavigationViewItem parentNavigationViewItem)
                {
                    MainNavigationView.Expand(parentNavigationViewItem);
                    isProgrammaticExpand = true;
                    return true;
                }
            }

            return false;
        }

        #endregion 第八部分：窗口导航方法

        #region 第九部分：显示对话框和应用通知

        /// <summary>
        /// 显示内容对话框
        /// </summary>
        public async Task<ContentDialogResult> ShowDialogAsync(ContentDialog contentDialog)
        {
            ContentDialogResult dialogResult = ContentDialogResult.None;
            bool isDialogOpening = false;
            if (contentDialog is not null && Content is not null)
            {
                foreach (Popup popup in VisualTreeHelper.GetOpenPopupsForXamlRoot(Content.XamlRoot))
                {
                    if (popup.Child is ContentDialog)
                    {
                        isDialogOpening = true;
                        break;
                    }
                }

                if (!isDialogOpening)
                {
                    try
                    {
                        contentDialog.XamlRoot = Content.XamlRoot;
                        dialogResult = await contentDialog.ShowAsync();
                    }
                    catch (Exception e)
                    {
                        LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(MainWindow), nameof(ShowDialogAsync), 1, e);
                    }
                }
            }

            return dialogResult;
        }

        /// <summary>
        /// 使用教学提示显示应用内通知
        /// </summary>
        public async Task ShowNotificationAsync(TeachingTip teachingTip, int duration = 2000)
        {
            if (teachingTip is not null && Content is Page page && page.Content is Grid grid)
            {
                try
                {
                    grid.Children.Add(teachingTip);

                    teachingTip.IsOpen = true;
                    await Task.Delay(duration);
                    teachingTip.IsOpen = false;

                    // 应用内通知关闭动画显示耗费 300 ms
                    await Task.Delay(300);
                    grid.Children.Remove(teachingTip);
                }
                catch (Exception e)
                {
                    LogService.WriteLog(TraceEventType.Error, nameof(ModernFormatConverter), nameof(MainWindow), nameof(ShowNotificationAsync), 1, e);
                }
            }
        }

        #endregion 第九部分：显示对话框和应用通知

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
