using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ShellUtility.Screens;

namespace Common.Utility.xaml.NoNamespace;

/// <summary>Specifies what to clamp a window to.</summary>
public enum ClampToScreenOption
{
    /// <summary>Specifies that the window should not be clamped.</summary>
    None,
    /// <summary>Specifies that the window should be clamped to the screen.</summary>
    Screen,
    /// <summary>Specifies that the window should be clamped to the work area of the screen.</summary>
    WorkArea
}

/// <summary>Contains attached properties for 'Common.'.</summary>
public static class Common
{

    #region ButtonBase

    #region ContextMenuOnLeftClick

    /// <summary>Gets whatever context menu should open on left click.</summary>
    public static bool GetContextMenuOnLeftClick(ButtonBase obj) =>
        (bool)obj.GetValue(ContextMenuOnLeftClickProperty);

    /// <summary>Sets whatever context menu should open on left click.</summary>
    public static void SetContextMenuOnLeftClick(ButtonBase obj, bool value) =>
        obj.SetValue(ContextMenuOnLeftClickProperty, value);

    /// <summary>The dependency property that determines whatever context menu should open on left click.</summary>
    public static readonly DependencyProperty ContextMenuOnLeftClickProperty =
        DependencyProperty.RegisterAttached("ContextMenuOnLeftClick", typeof(bool), typeof(Common), new PropertyMetadata(false, OnContextMenuOnLeftClickChanged));

    static void OnContextMenuOnLeftClickChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

        if (sender is not ButtonBase toggle)
            return;

        toggle.PreviewMouseDown -= Toggle_PreviewMouseDown;
        toggle.PreviewMouseUp -= Toggle_PreviewMouseUp;
        toggle.MouseLeave -= Toggle_MouseLeave;

        if ((bool)e.NewValue)
        {
            toggle.PreviewMouseDown += Toggle_PreviewMouseDown;
            toggle.PreviewMouseUp += Toggle_PreviewMouseUp;
            toggle.MouseLeave += Toggle_MouseLeave;
        }

    }

    static ButtonBase? button;
    static void Toggle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {

        if (e.ChangedButton == MouseButton.Right)
        {
            e.Handled = true;
            return;
        }

        if (sender is not ButtonBase toggle || e.ChangedButton != MouseButton.Left)
            return;

        button = toggle;
        e.Handled = true;

    }

    static async void Toggle_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {

        if (e.ChangedButton == MouseButton.Right)
        {
            e.Handled = true;
            return;
        }

        if (e.ChangedButton != MouseButton.Left)
            return;

        if (sender is not ButtonBase button || button != Common.button)
            return;

        if (button.ContextMenu is null)
            return;

        if (button is ToggleButton toggle)
            toggle.IsChecked = true;

        button.ContextMenu.Closed += ContextMenu_Closed;
        button.IsHitTestVisible = false;
        button.ContextMenu.DataContext = button.DataContext;
        button.ContextMenu.PlacementTarget = button;
        button.ContextMenu.IsOpen = true;

        if (GetCenterContextMenu(button))
        {
            if (button.ContextMenu.Placement is PlacementMode.Top or PlacementMode.Bottom)
            {
                await Task.Delay(10);
                button.ContextMenu.HorizontalOffset = button.ActualWidth / 2 - button.ContextMenu.ActualWidth / 2;
            }
            else if (button.ContextMenu.Placement is PlacementMode.Left or PlacementMode.Right)
            {
                await Task.Delay(10);
                button.ContextMenu.VerticalOffset = button.ActualHeight / 2 - button.ContextMenu.ActualHeight / 2;
            }
        }

        e.Handled = true;

        void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            button.ContextMenu.Closed -= ContextMenu_Closed;
            button.IsHitTestVisible = true;
            if (button is ToggleButton toggle)
                toggle.IsChecked = false;
        }

    }

    static void Toggle_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is ButtonBase toggle && toggle == button)
            button = null;
    }

    #endregion
    #region CenterContextMenu

    /// <summary>Gets whatever context menu should be centered.</summary>
    public static bool GetCenterContextMenu(ButtonBase obj) =>
        (bool)obj.GetValue(CenterContextMenuProperty);

    /// <summary>Sets whatever context menu should be centered.</summary>
    public static void SetCenterContextMenu(ButtonBase obj, bool value) =>
        obj.SetValue(CenterContextMenuProperty, value);

    /// <summary>The dependency property that determines whatever context menu should be centered.</summary>
    public static readonly DependencyProperty CenterContextMenuProperty =
        DependencyProperty.RegisterAttached("CenterContextMenu", typeof(bool), typeof(Common), new PropertyMetadata(false));

    #endregion

    #endregion
    #region UIElement

    #region IsMouseDown property

    /// <summary>Gets whatever left mouse is down.</summary>
    public static bool GetIsMouseLeftDown(UIElement obj) =>
        (bool)obj.GetValue(IsMouseLeftDownProperty);

    /// <summary>Sets whatever left mouse is down.</summary>
    public static void SetIsMouseLeftDown(UIElement obj, bool value) =>
        obj.SetValue(IsMouseLeftDownProperty, value);

    /// <summary>The dependency property that determines whatever the left mouse is down.</summary>
    public static readonly DependencyProperty IsMouseLeftDownProperty =
        DependencyProperty.RegisterAttached("IsMouseLeftDown", typeof(bool), typeof(Common), new PropertyMetadata(false));

    /// <summary>Gets whatever <see cref="IsMouseLeftDownProperty"/> should be active.</summary>
    public static bool GetEnableIsMouseDown(DependencyObject obj) =>
        (bool)obj.GetValue(EnableIsMouseDownProperty);

    /// <summary>Sets whatever <see cref="IsMouseLeftDownProperty"/> should be active.</summary>
    public static void SetEnableIsMouseDown(DependencyObject obj, bool value) =>
        obj.SetValue(EnableIsMouseDownProperty, value);

    /// <summary>The dependency property that determines whatever we should track if left mouse is down.</summary>
    public static readonly DependencyProperty EnableIsMouseDownProperty =
        DependencyProperty.RegisterAttached("EnableIsMouseDown", typeof(bool), typeof(Common), new PropertyMetadata(false, OnEnableIsMouseLeftDownPropertyChanged));

    static void OnEnableIsMouseLeftDownPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

        if (sender is not UIElement button)
            return;

        button.PreviewMouseLeftButtonDown -= Button_MouseLeftButtonDown;
        button.PreviewMouseLeftButtonUp -= Button_MouseLeftButtonUp;
        button.MouseLeave -= Button_MouseLeave;

        if ((bool)e.NewValue)
        {
            button.PreviewMouseLeftButtonDown += Button_MouseLeftButtonDown;
            button.PreviewMouseLeftButtonUp += Button_MouseLeftButtonUp;
            button.MouseLeave += Button_MouseLeave;
        }

    }

    static void Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
        SetIsMouseLeftDown((UIElement)sender, true);

    static void Button_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) =>
        SetIsMouseLeftDown((UIElement)sender, false);

    static void Button_MouseLeave(object sender, MouseEventArgs e) =>
        SetIsMouseLeftDown((UIElement)sender, false);

    #endregion
    #region IsVisible

    /// <summary>Gets whatever element is <see cref="Visibility.Visibility"/>.</summary>
    public static bool? GetIsVisible(FrameworkElement obj) => (bool?)obj.GetValue(IsVisibleProperty);
    /// <summary>Sets whatever element is <see cref="Visibility.Visibility"/>.</summary>
    public static void SetIsVisible(FrameworkElement obj, bool? value) => obj.SetValue(IsVisibleProperty, value);

    /// <summary>The dependency property that determines whatever element is <see cref="Visibility.Visibility"/>.</summary>
    public static readonly DependencyProperty IsVisibleProperty =
        DependencyProperty.RegisterAttached("IsVisible", typeof(bool?), typeof(Common), new PropertyMetadata(null, OnIsVisibleChanged));

    static void OnIsVisibleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
        DoVisibilityChange((FrameworkElement)sender, e.NewValue is true, Visibility.Collapsed);

    #endregion
    #region IsCollapsed

    /// <summary>Gets whatever element is <see cref="Visibility.Collapsed"/>.</summary>
    public static bool? GetIsCollapsed(FrameworkElement obj) => (bool?)obj.GetValue(IsCollapsedProperty);
    /// <summary>Gets whatever element is <see cref="Visibility.Collapsed"/>.</summary>
    public static void SetIsCollapsed(FrameworkElement obj, bool? value) => obj.SetValue(IsCollapsedProperty, value);

    /// <summary>The dependency property that determines whatever element is <see cref="Visibility.Collapsed"/>.</summary>
    public static readonly DependencyProperty IsCollapsedProperty =
        DependencyProperty.RegisterAttached("IsCollapsed", typeof(bool?), typeof(Common), new PropertyMetadata(null, OnIsCollapsedChanged));

    static void OnIsCollapsedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
        DoVisibilityChange((FrameworkElement)sender, e.NewValue is false, Visibility.Collapsed);

    #endregion
    #region IsHidden

    /// <summary>Gets whatever element is <see cref="Visibility.Hidden"/>.</summary>
    public static bool? GetIsHidden(FrameworkElement obj) => (bool?)obj.GetValue(IsHiddenProperty);
    /// <summary>Sets whatever element is <see cref="Visibility.Hidden"/>.</summary>
    public static void SetIsHidden(FrameworkElement obj, bool? value) => obj.SetValue(IsHiddenProperty, value);

    /// <summary>The dependency property that determines whatever element is <see cref="Visibility.Hidden"/>.</summary>
    public static readonly DependencyProperty IsHiddenProperty =
        DependencyProperty.RegisterAttached("IsHidden", typeof(bool?), typeof(Common), new PropertyMetadata(null, OnIsHiddenChanged));

    static void OnIsHiddenChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
        DoVisibilityChange((FrameworkElement)sender, e.NewValue is false, Visibility.Hidden);

    #endregion
    #region Scale

    /// <summary>Gets Scale.</summary>
    public static double GetScale(FrameworkElement obj) => (double)obj.GetValue(ScaleProperty);

    /// <summary>Sets Scale.</summary>
    public static void SetScale(FrameworkElement obj, double value) => obj.SetValue(ScaleProperty, value);

    /// <summary>The dependency property that determines the <see cref="FrameworkElement.LayoutTransform"/>+<see cref="ScaleTransform"/>.</summary>
    public static readonly DependencyProperty ScaleProperty =
        DependencyProperty.RegisterAttached("Scale", typeof(double), typeof(Common), new PropertyMetadata(default(double), static (s, e) => OnScaleChanged((FrameworkElement)s, e)));

    static void OnScaleChanged(FrameworkElement sender, DependencyPropertyChangedEventArgs e) =>
        sender.LayoutTransform = new ScaleTransform() { ScaleX = (double)e.NewValue, ScaleY = (double)e.NewValue };

    #endregion

    static readonly Dictionary<FrameworkElement, Storyboard?> activeStoryboard = new();
    static void CancelAnimation(FrameworkElement element)
    {
        if (activeStoryboard.Remove(element, out var storyboard))
        {
            storyboard?.Stop();
            storyboard?.Remove();
        }
    }

    static async void DoVisibilityChange(FrameworkElement element, bool visible, Visibility hiddenValue)
    {

        if (element is null)
            return;

        CancelAnimation(element);

        if (visible)
        {

            if (!element.IsLoaded)
                element.Opacity = 0;

            element.Visibility = Visibility.Visible;
            if (Transition.GetShowAnimation(element, out var storyboard))
            {
                activeStoryboard.Set(element, storyboard);
                await storyboard.BeginAsync(element);
                if (activeStoryboard.TryGetValue(element, out var s) && s == storyboard)
                    OnDone();
            }
            else
                OnDone();

        }
        else
        {

            if (!element.IsLoaded)
            {
                OnDone();
                return;
            }

            if (Transition.GetHideAnimation(element, out var storyboard))
            {
                activeStoryboard.Set(element, storyboard);
                await storyboard.BeginAsync(element);
                if (activeStoryboard.TryGetValue(element, out var s) && s == storyboard)
                    OnDone();
            }
            else
                OnDone();

        }

        void OnDone()
        {
            CancelAnimation(element);
            element.Visibility = visible ? Visibility.Visible : hiddenValue;
            element.Opacity = visible ? 1 : 0;
        }

    }

    #endregion
    #region Window

    #region IsResizing

    /// <summary>Gets whatever the window is resizing.</summary>
    public static bool GetIsResizing(Window obj) => (bool)obj.GetValue(IsResizingProperty);
    /// <summary>Sets whatever the window is resizing.</summary>
    public static void SetIsResizing(Window obj, bool value) => obj.SetValue(IsResizingProperty, value);

    /// <summary>Gets whatever the window is moving.</summary>
    public static bool GetIsMoving(Window obj) => (bool)obj.GetValue(IsMovingProperty);
    /// <summary>Sets whatever the window is moving.</summary>
    public static void SetIsMoving(Window obj, bool value) => obj.SetValue(IsMovingProperty, value);

    /// <summary>The dependency property that determines the whatever the window is resizing.</summary>
    public static readonly DependencyProperty IsResizingProperty =
        DependencyProperty.RegisterAttached("IsResizing", typeof(bool), typeof(Common), new PropertyMetadata(null));

    /// <summary>The dependency property that determines the whatever the window is moving.</summary>
    public static readonly DependencyProperty IsMovingProperty =
        DependencyProperty.RegisterAttached("IsMoving", typeof(bool), typeof(Common), new PropertyMetadata(false));

    #endregion
    #region ClampToMonitors

    /// <summary>Gets whatever the window should be clamped.</summary>
    public static ClampToScreenOption GetClampToMonitors(Window obj) => (ClampToScreenOption)obj.GetValue(ClampToMonitorsProperty);
    /// <summary>Sets whatever the window should be clamped.</summary>
    public static void SetClampToMonitors(Window obj, ClampToScreenOption value) => obj.SetValue(ClampToMonitorsProperty, value);

    /// <summary>The dependency property that determines the whatever the window should be clamped.</summary>
    public static readonly DependencyProperty ClampToMonitorsProperty =
        DependencyProperty.RegisterAttached("ClampToMonitors", typeof(ClampToScreenOption), typeof(Common), new PropertyMetadata(ClampToScreenOption.None, OnClampToMonitorsChanged));

    static readonly Dictionary<IntPtr, Window> clampedWindows = new();
    static void OnClampToMonitorsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

        if (sender is not Window window)
            return;

        window.Unloaded -= Window_Unloaded;
        window.Loaded -= UpdateClamp;
        window.LocationChanged -= UpdateClamp;
        window.SizeChanged -= UpdateClamp;

        if (!window.IsLoaded && clampedWindows.ContainsValue(window))
        {
            _ = clampedWindows.Remove(clampedWindows.FirstOrDefault(kvp => kvp.Value == window).Key);
            return;
        }

        var handle = new WindowInteropHelper(window).EnsureHandle();
        var source = HwndSource.FromHwnd(handle);
        source.RemoveHook(WndProc_ClampToMonitors);
        _ = clampedWindows.Remove(handle);

        if ((ClampToScreenOption)e.NewValue != ClampToScreenOption.None)
        {
            window.Unloaded += Window_Unloaded;
            window.LocationChanged += UpdateClamp;
            window.SizeChanged += UpdateClamp;
            window.Loaded += UpdateClamp;
            clampedWindows.Add(handle, window);
            source.AddHook(WndProc_ClampToMonitors);
            EnsureClamped(window);
        }

        void Window_Unloaded(object sender, RoutedEventArgs e) =>
            SetClampToMonitors(window, ClampToScreenOption.None);

        void UpdateClamp(object? sender, EventArgs e)
        {
            if (window.IsLoaded)
                EnsureClamped(window);
        }

    }

    /// <summary>Clamps the window to screen if needed.</summary>
    /// <param name="clampTo">
    /// <para>Specifies what to clamp to.</para>
    /// <para><see langword="null"/> will use <see cref="ClampToMonitorsProperty"/> for <paramref name="window"/>.</para>
    /// <para>If <see cref="ClampToScreenOption.None"/> is specified then this method will have no effect.</para>
    /// </param>
    /// <param name="window">The window to clamp.</param>
    public static void EnsureClamped(this Window window, ClampToScreenOption? clampTo = null)
    {

        if (GetIsMoving(window) || GetIsResizing(window))
            return;

        clampTo ??= GetClampToMonitors(window);

        if (clampTo == ClampToScreenOption.None)
            return;

        var screen = Screen.FromWindowHandle(new WindowInteropHelper(window).Handle);
        var rect = clampTo == ClampToScreenOption.WorkArea ? screen.WorkArea : screen.Bounds;

        if (window.Left < rect.Left) window.Left = rect.Left;
        if (window.Top < rect.Top) window.Top = rect.Top;
        if (window.Left + window.ActualWidth > rect.Right) window.Width = rect.Right - window.Left;
        if (window.Top + window.ActualHeight > rect.Bottom) window.Height = rect.Bottom - window.Top;

    }

    static POINT? relativeMousePos;
    static IntPtr WndProc_ClampToMonitors(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {

        if (msg == (int)WindowsMessage.WM_EXITSIZEMOVE)
        {
            relativeMousePos = null;
            SetIsMoving(clampedWindows[hwnd], false);
            SetIsResizing(clampedWindows[hwnd], false);
        }
        else if (msg == (int)WindowsMessage.WM_MOVING)
        {

            SetIsMoving(clampedWindows[hwnd], true);

            _ = GetCursorPos(out var absoluteMousePos);
            var rect = Marshal.PtrToStructure<WIN32Rectangle>(lParam);
            //if (!relativeMousePos.HasValue)
            //    relativeMousePos = new POINT(absoluteMousePos.X - rect.Left, absoluteMousePos.Y - rect.Top);

            var screen = Screen.FromWindowHandle(hwnd);
            ClampToScreen(screen, ref handled);

            if (handled)
                Marshal.StructureToPtr(rect, lParam, true);

            void ClampToScreen(Screen screen, ref bool handled)
            {

                relativeMousePos ??= new(absoluteMousePos.X - rect.Left, absoluteMousePos.Y - rect.Top);

                var bounds = GetClampToMonitors(clampedWindows[hwnd]) == ClampToScreenOption.WorkArea
                    ? screen.WorkArea
                    : screen.Bounds;

                var width = rect.Right - rect.Left;
                var height = rect.Bottom - rect.Top;

                var preferredRect = new WIN32Rectangle() { Left = absoluteMousePos.X - relativeMousePos.Value.X, Top = absoluteMousePos.Y - relativeMousePos.Value.Y };
                preferredRect.Right = preferredRect.Left + width;
                preferredRect.Bottom = preferredRect.Top + height;

                var isLeft = rect.Left < bounds.Left + relativeMousePos.Value.X;
                var isRight = rect.Right > bounds.Right - relativeMousePos.Value.X;
                var isTop = rect.Top < bounds.Top + relativeMousePos.Value.Y;
                var isBottom = rect.Bottom > bounds.Bottom - relativeMousePos.Value.X;

                if (isLeft || isRight || isTop || isBottom)
                {

                    if (width > bounds.Width)
                        width = bounds.Width;

                    if (height > bounds.Height)
                        height = bounds.Height;

                    rect.Left = Math.Clamp(preferredRect.Left, bounds.Left, bounds.Right - width);
                    rect.Top = Math.Clamp(preferredRect.Top, bounds.Top, bounds.Bottom - height);

                    rect.Right = rect.Left + width;
                    rect.Bottom = rect.Top + height;

                    if (Screen.All().FirstOrDefault(s => s.Bounds.Contains(absoluteMousePos)) is Screen s && s.Handle != screen.Handle)
                        ClampToScreen(s, ref handled);

                    handled = true;

                }

            }

        }
        else if (msg == (int)WindowsMessage.WM_SIZING)
        {

            SetIsResizing(clampedWindows[hwnd], true);

            _ = GetCursorPos(out var absoluteMousePos);
            var rect = Marshal.PtrToStructure<WIN32Rectangle>(lParam);
            //if (!relativeMousePos.HasValue)
            //    relativeMousePos = new POINT(absoluteMousePos.X - rect.Left, absoluteMousePos.Y - rect.Top);

            var screen = Screen.FromWindowHandle(hwnd);
            ClampToScreen(screen, ref handled);

            if (handled)
                Marshal.StructureToPtr(rect, lParam, true);

            void ClampToScreen(Screen screen, ref bool handled)
            {

                relativeMousePos ??= new(absoluteMousePos.X - rect.Left, absoluteMousePos.Y - rect.Top);

                var bounds = GetClampToMonitors(clampedWindows[hwnd]) == ClampToScreenOption.WorkArea
                    ? screen.WorkArea
                    : screen.Bounds;

                var width = rect.Right - rect.Left;
                var height = rect.Bottom - rect.Top;

                var preferredRect = new WIN32Rectangle() { Left = absoluteMousePos.X - relativeMousePos.Value.X, Top = absoluteMousePos.Y - relativeMousePos.Value.Y };
                preferredRect.Right = preferredRect.Left + width;
                preferredRect.Bottom = preferredRect.Top + height;

                var isLeft = ((WindowEdge)wParam).IsLeft() && rect.Left < bounds.Left + relativeMousePos.Value.X;
                var isRight = ((WindowEdge)wParam).IsRight() && rect.Right > bounds.Right - relativeMousePos.Value.X;
                var isTop = ((WindowEdge)wParam).IsTop() && rect.Top < bounds.Top + relativeMousePos.Value.Y;
                var isBottom = ((WindowEdge)wParam).IsBottom() && rect.Bottom > bounds.Bottom - relativeMousePos.Value.X;

                if (isLeft || isRight || isTop || isBottom)
                {

                    if (isLeft && rect.Left < bounds.Left)
                        rect.Left = bounds.Left;

                    if (isTop && rect.Top < bounds.Top)
                        rect.Top = bounds.Top;

                    if (isRight && rect.Right > bounds.Right)
                        rect.Right = bounds.Right;

                    if (isBottom && rect.Bottom > bounds.Bottom)
                        rect.Bottom = bounds.Bottom;

                    handled = true;

                }

            }

        }

        return IntPtr.Zero;

    }

    static bool IsLeft(this WindowEdge edge) =>
        edge is WindowEdge.WMSZ_LEFT or WindowEdge.WMSZ_TOPLEFT or WindowEdge.WMSZ_BOTTOMLEFT;

    static bool IsTop(this WindowEdge edge) =>
        edge is WindowEdge.WMSZ_TOP or WindowEdge.WMSZ_TOPLEFT or WindowEdge.WMSZ_TOPRIGHT;

    static bool IsRight(this WindowEdge edge) =>
        edge is WindowEdge.WMSZ_RIGHT or WindowEdge.WMSZ_TOPRIGHT or WindowEdge.WMSZ_BOTTOMRIGHT;

    static bool IsBottom(this WindowEdge edge) =>
        edge is WindowEdge.WMSZ_BOTTOM or WindowEdge.WMSZ_BOTTOMLEFT or WindowEdge.WMSZ_BOTTOMRIGHT;

    [StructLayout(LayoutKind.Sequential)]
    struct WIN32Rectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct POINT
    {

        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator System.Drawing.Point(POINT p) => new(p.X, p.Y);
        public static implicit operator POINT(System.Drawing.Point p) => new(p.X, p.Y);

    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetCursorPos(out POINT lpPoint);

    enum WindowsMessage
    {
        /// <summary>Sent after a window has been moved.</summary>
        WM_MOVE = 0x0003,
        /// <summary>
        /// Sent to a window when the size or position of the window is about to change.
        /// An application can use this message to override the window's default maximized size and position,
        /// or its default minimum or maximum tracking size.
        /// </summary>
        WM_GETMINMAXINFO = 0x0024,
        /// <summary>
        /// Sent to a window whose size, position, or place in the Z order is about to change as a result
        /// of a call to the SetWindowPos function or another window-management function.
        /// </summary>
        WM_WINDOWPOSCHANGING = 0x0046,
        /// <summary>
        /// Sent to a window whose size, position, or place in the Z order has changed as a result of a
        /// call to the SetWindowPos function or another window-management function.
        /// </summary>
        WM_WINDOWPOSCHANGED = 0x0047,
        /// <summary>
        /// Sent to a window that the user is moving. By processing this message, an application can monitor
        /// the position of the drag rectangle and, if needed, change its position.
        /// </summary>
        WM_MOVING = 0x0216,
        /// <summary>
        /// Sent once to a window after it enters the moving or sizing modal loop. The window enters the
        /// moving or sizing modal loop when the user clicks the window's title bar or sizing border, or
        /// when the window passes the WM_SYSCOMMAND message to the DefWindowProc function and the wParam
        /// parameter of the message specifies the SC_MOVE or SC_SIZE value. The operation is complete
        /// when DefWindowProc returns.
        /// <para />
        /// The system sends the WM_ENTERSIZEMOVE message regardless of whether the dragging of full windows
        /// is enabled.
        /// </summary>
        WM_ENTERSIZEMOVE = 0x0231,
        /// <summary>
        /// Sent once to a window once it has exited moving or sizing modal loop. The window enters the
        /// moving or sizing modal loop when the user clicks the window's title bar or sizing border, or
        /// when the window passes the WM_SYSCOMMAND message to the DefWindowProc function and the
        /// wParam parameter of the message specifies the SC_MOVE or SC_SIZE value. The operation is
        /// complete when DefWindowProc returns.
        /// </summary>
        WM_EXITSIZEMOVE = 0x0232,
        WM_SIZING = 0x0214
    }

    enum WindowEdge
    {
        WMSZ_BOTTOM = 6,
        WMSZ_BOTTOMLEFT = 7,
        WMSZ_BOTTOMRIGHT = 8,
        WMSZ_LEFT = 1,
        WMSZ_RIGHT = 2,
        WMSZ_TOP = 3,
        WMSZ_TOPLEFT = 4,
        WMSZ_TOPRIGHT = 5,
    }

    #endregion
    #region IsVisibleInAltTab

    /// <summary>Gets whatever this window is visible in alt-tab.</summary>
    public static bool GetIsVisibleInAltTab(Window window) => (bool)window.GetValue(IsVisibleInAltTabProperty);
    /// <summary>Sets whatever this window is visible in alt-tab.</summary>
    public static void SetIsVisibleInAltTab(Window window, bool value) => window.SetValue(IsVisibleInAltTabProperty, value);

    /// <summary>The dependency property that determines whatever the window should be visible in alt-tab.</summary>
    public static readonly DependencyProperty IsVisibleInAltTabProperty =
        DependencyProperty.RegisterAttached("IsVisibleInAltTab", typeof(bool), typeof(Common), new PropertyMetadata(true, IsVisibleInAltTabChanged));

    static void IsVisibleInAltTabChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

        if (sender is not Window window)
            return;

        if (!window.IsLoaded)
            window.Loaded += Window_Loaded;
        else
            Window_Loaded();

        void Window_Loaded(object? _ = null, RoutedEventArgs? __ = null)
        {

            window.Loaded -= Window_Loaded;

            var handle = new WindowInteropHelper(window).Handle;
            var exStyle = (ExtendedWindowStyles)GetWindowLong(handle, GetWindowLongFields.GWL_EXSTYLE);

            if ((bool)e.NewValue)
                exStyle &= ~ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            else
                exStyle |= ExtendedWindowStyles.WS_EX_TOOLWINDOW;

            _ = SetWindowLong(handle, GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

        }

    }

    [Flags]
    enum ExtendedWindowStyles
    {
        WS_EX_TOOLWINDOW = 0x00000080,
    }

    enum GetWindowLongFields
    {
        GWL_EXSTYLE = (-20),
    }

    [DllImport("user32.dll")]
    static extern IntPtr GetWindowLong(IntPtr hWnd, GetWindowLongFields nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr SetWindowLong(IntPtr hWnd, GetWindowLongFields nIndex, IntPtr dwNewLong);

    [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
    static extern void SetLastError(int dwErrorCode);

    #endregion
    #region Rect

    /// <summary>Gets the rect of this window.</summary>
    public static Rect GetRect(Window obj) => (Rect)obj.GetValue(RectProperty);
    /// <summary>Sets the rect of this window.</summary>
    public static void SetRect(Window obj, Rect value) => obj.SetValue(RectProperty, value);

    /// <summary>The dependency property that determines what the rect of the window is.</summary>
    public static readonly DependencyProperty RectProperty =
        DependencyProperty.RegisterAttached("Rect", typeof(Rect), typeof(Common), new PropertyMetadata(default(Rect), OnRectChanged));

    static void OnRectChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

        if (sender is not Window window || e.NewValue is not Rect rect)
            return;

        window.Left = rect.Left;
        window.Top = rect.Top;
        window.Width = rect.Width;
        window.Height = rect.Height;

    }

    #endregion
    #region Location

    /// <summary>Gets the location of this window.</summary>
    public static Point GetLocation(Window obj) => (Point)obj.GetValue(LocationProperty);
    /// <summary>Sets the location of this window.</summary>
    public static void SetLocation(Window obj, Point value) => obj.SetValue(LocationProperty, value);

    /// <summary>The dependency property that determines what the location of the window is.</summary>
    public static readonly DependencyProperty LocationProperty =
        DependencyProperty.RegisterAttached("Location", typeof(Point), typeof(Common), new PropertyMetadata(default(Point), OnLocationChanged));

    static void OnLocationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

        if (sender is not Window window || e.NewValue is not Point point)
            return;

        window.Left = point.X;
        window.Top = point.Y;

    }

    #endregion
    #region Size

    /// <summary>Gets the size of this window.</summary>
    public static Size GetSize(Window obj) => (Size)obj.GetValue(SizeProperty);
    /// <summary>Sets the size of this window.</summary>
    public static void SetSize(Window obj, Size value) => obj.SetValue(SizeProperty, value);

    /// <summary>The dependency property that determines what the size of the window is.</summary>
    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.RegisterAttached("Size", typeof(Size), typeof(Common), new PropertyMetadata(default(Size), OnSizeChanged));

    static void OnSizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

        if (sender is not Window window || e.NewValue is not Size Size)
            return;

        window.Width = Size.Width;
        window.Height = Size.Height;

    }

    #endregion
    #region Pin to desktop

    #region Pinvoke

    const uint SWP_NOSIZE = 0x0001;
    const uint SWP_NOMOVE = 0x0002;
    const uint SWP_NOACTIVATE = 0x0010;

    const int WM_SETFOCUS = 0x0007;

    static readonly IntPtr HWND_BOTTOM = new(1);

    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    #endregion

    /// <summary>The dependency property that determines whatever the window should be pinned to the desktop.</summary>
    public static readonly DependencyProperty PinToDesktopProperty = DependencyProperty.RegisterAttached("PinToDesktop", typeof(bool), typeof(Common), new UIPropertyMetadata(false, OnPinToDesktopChanged));

    /// <summary>Gets whatever the window should be pinned to the desktop.</summary>
    public static bool GetPinToDesktop(Window window) => (bool)window.GetValue(PinToDesktopProperty);
    /// <summary>Sets whatever the window should be pinned to the desktop.</summary>
    public static void SetPinToDesktop(Window window, bool value) => window.SetValue(PinToDesktopProperty, value);

    static void OnPinToDesktopChanged(object sender, DependencyPropertyChangedEventArgs e)
    {

        if (sender is not Window window)
            return;

        IntPtr? handle = null;
        HwndSource? source = null;

        window.Loaded -= OnLoaded;
        window.Closing -= OnClosing;

        if ((bool)e.NewValue)
        {
            window.Loaded += OnLoaded;
            window.Closing += OnClosing;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            handle = window.Handle();
            source = HwndSource.FromHwnd(handle.Value);
            SetZPos();
            source.AddHook(WndProc);
        }

        void OnClosing(object? sender, CancelEventArgs e)
        {
            source?.RemoveHook(WndProc);
            source = null;
            window.Loaded -= OnLoaded;
            window.Closing -= OnClosing;
        }

        IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            if (msg == WM_SETFOCUS)
            {
                SetZPos();
                handled = true;
            }

            return IntPtr.Zero;

        }

        void SetZPos()
        {
            if (handle.HasValue)
                _ = SetWindowPos(handle.Value, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
        }
    }

    #endregion

    #endregion

}
