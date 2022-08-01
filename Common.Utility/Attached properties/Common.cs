using ShellUtility.Screens;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media.Animation;

[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Common.Utility.AttachedProperties")]
namespace Common.Utility.AttachedProperties;

public enum ClampToScreenOption
{
    None, Screen, WorkArea
}

public static class Common
{

    #region ButtonBase

    #region ContextMenuOnLeftClick

    public static bool GetContextMenuOnLeftClick(ButtonBase obj) =>
        (bool)obj.GetValue(ContextMenuOnLeftClickProperty);

    public static void SetContextMenuOnLeftClick(ButtonBase obj, bool value) =>
        obj.SetValue(ContextMenuOnLeftClickProperty, value);

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

    public static bool GetCenterContextMenu(ButtonBase obj) =>
        (bool)obj.GetValue(CenterContextMenuProperty);

    public static void SetCenterContextMenu(ButtonBase obj, bool value) =>
        obj.SetValue(CenterContextMenuProperty, value);

    public static readonly DependencyProperty CenterContextMenuProperty =
        DependencyProperty.RegisterAttached("CenterContextMenu", typeof(bool), typeof(Common), new PropertyMetadata(false));

    #endregion

    #endregion
    #region UIElement

    #region IsMouseDown property

    public static bool GetIsMouseLeftDown(UIElement obj) =>
        (bool)obj.GetValue(IsMouseLeftDownProperty);

    public static void SetIsMouseLeftDown(UIElement obj, bool value) =>
        obj.SetValue(IsMouseLeftDownProperty, value);

    public static readonly DependencyProperty IsMouseLeftDownProperty =
        DependencyProperty.RegisterAttached("IsMouseLeftDown", typeof(bool), typeof(Common), new PropertyMetadata(false));

    public static bool GetEnableIsMouseDown(DependencyObject obj) =>
        (bool)obj.GetValue(EnableIsMouseDownProperty);

    public static void SetEnableIsMouseDown(DependencyObject obj, bool value) =>
        obj.SetValue(EnableIsMouseDownProperty, value);

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

    public static bool? GetIsVisible(UIElement obj) => (bool?)obj.GetValue(IsVisibleProperty);
    public static void SetIsVisible(UIElement obj, bool? value) => obj.SetValue(IsVisibleProperty, value);

    public static readonly DependencyProperty IsVisibleProperty =
        DependencyProperty.RegisterAttached("IsVisible", typeof(bool?), typeof(Common), new PropertyMetadata(null, OnIsVisibleChanged));

    static async void OnIsVisibleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

        if (sender is not UIElement element)
            return;

        if (e.NewValue is true)
        {
            await DoTransition(GetShowTransition(element), element as FrameworkElement);
            element.Visibility = Visibility.Visible;
        }
        else if (e.NewValue is false)
        {
            await DoTransition(GetHideTransition(element), element as FrameworkElement);
            element.Visibility = Visibility.Collapsed;
        }

    }

    #endregion
    #region IsCollapsed

    public static bool? GetIsCollapsed(UIElement obj) => (bool?)obj.GetValue(IsCollapsedProperty);
    public static void SetIsCollapsed(UIElement obj, bool? value) => obj.SetValue(IsCollapsedProperty, value);

    public static readonly DependencyProperty IsCollapsedProperty =
        DependencyProperty.RegisterAttached("IsCollapsed", typeof(bool?), typeof(Common), new PropertyMetadata(null, OnIsCollapsedChanged));

    static async void OnIsCollapsedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

        if (sender is not UIElement element)
            return;

        if (e.NewValue is true)
        {
            await DoTransition(GetHideTransition(element), element as FrameworkElement);
            element.Visibility = Visibility.Collapsed;
        }
        else if (e.NewValue is false)
        {
            await DoTransition(GetShowTransition(element), element as FrameworkElement);
            element.Visibility = Visibility.Visible;
        }

    }

    #endregion
    #region IsHidden

    public static bool? GetIsHidden(UIElement obj) => (bool?)obj.GetValue(IsHiddenProperty);
    public static void SetIsHidden(UIElement obj, bool? value) => obj.SetValue(IsHiddenProperty, value);

    public static readonly DependencyProperty IsHiddenProperty =
        DependencyProperty.RegisterAttached("IsHidden", typeof(bool?), typeof(Common), new PropertyMetadata(null, OnIsHiddenChanged));

    static async void OnIsHiddenChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

        if (sender is not UIElement element)
            return;

        if (e.NewValue is true)
        {
            await DoTransition(GetHideTransition(element), element as FrameworkElement);
            element.Visibility = Visibility.Hidden;
        }
        else if (e.NewValue is false)
        {
            await DoTransition(GetShowTransition(element), element as FrameworkElement);
            element.Visibility = Visibility.Visible;
        }

    }

    #endregion
    #region Transitions

    static Task DoTransition(Storyboard? storyboard, FrameworkElement? element)
    {

        if (storyboard is null || element is null)
            return Task.CompletedTask;

        var tcs = new TaskCompletionSource<bool>();
        storyboard.Completed += Storyboard_Completed;
        storyboard.RemoveRequested += Storyboard_Completed;
        storyboard.Begin(element);

        return tcs.Task;

        void Storyboard_Completed(object? sender, EventArgs e)
        {
            storyboard.Completed -= Storyboard_Completed;
            storyboard.RemoveRequested -= Storyboard_Completed;
            tcs.SetResult(true);
        }

    }

    public static Storyboard? GetShowTransition(UIElement obj) => (Storyboard?)obj.GetValue(ShowTransitionProperty);
    public static void SetShowTransition(UIElement obj, Storyboard? value) => obj.SetValue(ShowTransitionProperty, value);

    public static readonly DependencyProperty ShowTransitionProperty =
        DependencyProperty.RegisterAttached("ShowTransition", typeof(Storyboard), typeof(Common), new PropertyMetadata(null));

    public static Storyboard? GetHideTransition(UIElement obj) => (Storyboard?)obj.GetValue(HideTransitionProperty);
    public static void SetHideTransition(UIElement obj, Storyboard? value) => obj.SetValue(HideTransitionProperty, value);

    public static readonly DependencyProperty HideTransitionProperty =
        DependencyProperty.RegisterAttached("HideTransition", typeof(Storyboard), typeof(Common), new PropertyMetadata(null));

    #endregion

    #endregion
    #region Window

    #region IsResizing

    public static bool GetIsResizing(Window obj) => (bool)obj.GetValue(IsResizingProperty);
    public static void SetIsResizing(Window obj, bool value) => obj.SetValue(IsResizingProperty, value);

    public static bool GetIsMoving(Window obj) => (bool)obj.GetValue(IsMovingProperty);
    public static void SetIsMoving(Window obj, bool value) => obj.SetValue(IsMovingProperty, value);

    public static readonly DependencyProperty IsResizingProperty =
        DependencyProperty.RegisterAttached("IsResizing", typeof(bool), typeof(Common), new PropertyMetadata(null));

    public static readonly DependencyProperty IsMovingProperty =
        DependencyProperty.RegisterAttached("IsMoving", typeof(bool), typeof(Common), new PropertyMetadata(false));

    #endregion
    #region ClampToMonitors

    public static ClampToScreenOption GetClampToMonitors(Window obj) => (ClampToScreenOption)obj.GetValue(ClampToMonitorsProperty);
    public static void SetClampToMonitors(Window obj, ClampToScreenOption value) => obj.SetValue(ClampToMonitorsProperty, value);

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
    public struct WIN32Rectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
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

    public enum WindowEdge
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

    public static bool GetIsVisibleInAltTab(Window window) => (bool)window.GetValue(IsVisibleInAltTabProperty);
    public static void SetIsVisibleInAltTab(Window window, bool value) => window.SetValue(IsVisibleInAltTabProperty, value);

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

    public static Rect GetRect(Window obj) => (Rect)obj.GetValue(RectProperty);
    public static void SetRect(Window obj, Rect value) => obj.SetValue(RectProperty, value);

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

    public static Point GetLocation(Window obj) => (Point)obj.GetValue(LocationProperty);
    public static void SetLocation(Window obj, Point value) => obj.SetValue(LocationProperty, value);

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

    public static Size GetSize(Window obj) => (Size)obj.GetValue(SizeProperty);
    public static void SetSize(Window obj, Size value) => obj.SetValue(SizeProperty, value);

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

    public static readonly DependencyProperty PinToDesktopProperty = DependencyProperty.RegisterAttached("PinToDesktop", typeof(bool), typeof(Common), new UIPropertyMetadata(false, OnPinToDesktopChanged));

    public static bool GetPinToDesktop(Window window) => (bool)window.GetValue(PinToDesktopProperty);
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
    #region Save Position

    public static bool GetSavePosition(Window window) => (bool)window.GetValue(SavePositionProperty);
    public static void SetSavePosition(Window window, bool value) => window.SetValue(SavePositionProperty, value);

    public static readonly DependencyProperty SavePositionProperty =
        DependencyProperty.RegisterAttached("SavePosition", typeof(bool), typeof(Common), new PropertyMetadata(false, OnSavePositionChanged));

    static readonly List<Type> list = new();

    static void OnSavePositionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

        if (sender is not Window window)
            return;

        var name = window.GetType().FullName ?? throw new NullReferenceException("Window.GetType().Fullname is null.");

        window.LocationChanged -= Window_LocationChanged;
        window.SizeChanged -= Window_SizeChanged;
        window.Closed -= Window_Closed;

        if (list.Contains(window.GetType()) && (bool)e.NewValue)
            throw new InvalidOperationException("Cannot register multiple instances of same window type.");

        if ((bool)e.NewValue)
        {
            list.Add(window.GetType());
            Load(window, name);
            window.LocationChanged += Window_LocationChanged;
            window.SizeChanged += Window_SizeChanged;
            window.Closed += Window_Closed;
        }
        else
            _ = list.Remove(window.GetType());

        void Window_SizeChanged(object sender, SizeChangedEventArgs e) =>
            Save(window, name);

        void Window_LocationChanged(object? sender, EventArgs e) =>
            Save(window, name);

        void Window_Closed(object? sender, EventArgs e)
        {
            _ = list.Remove(window.GetType());
            window.LocationChanged -= Window_LocationChanged;
            window.SizeChanged -= Window_SizeChanged;
            window.Closed -= Window_Closed;
            Save(window, name, delay: false);
        }

    }

    static async void Load(Window window, string name)
    {

        if (SettingsUtility.Read<Rect>(name, out var rect))
        {

            window.Left = rect.Left;
            window.Top = rect.Top;
            window.Width = rect.Width;
            window.Height = rect.Height;
            window.Loaded += Window_Loaded;

            void Window_Loaded(object sender, RoutedEventArgs e)
            {
                window.Loaded -= Window_Loaded;
                window.Width = rect.Width;
                window.Height = rect.Height;
            }

        }
        else
        {

            while ((double.IsNaN(window.Width) && window.ActualWidth == 0) || (double.IsNaN(window.Height) && window.ActualHeight == 0))
                await Task.Delay(100);

            var w = window.ActualWidth != 0 ? window.ActualWidth : window.Width;
            var h = window.ActualHeight != 0 ? window.ActualHeight : window.Height;

            window.Left = (SystemParameters.PrimaryScreenWidth / 2) - (w / 2);
            window.Top = (SystemParameters.PrimaryScreenHeight / 2) - (h / 2);

        }

    }

    static void Save(Window window, string name, bool delay = true)
    {
        if (window.IsLoaded)
            SettingsUtility.Save(name, new Rect(window.Left, window.Top, window.ActualWidth, window.ActualHeight), delay);
    }

    #region SettingsUtility

    static class SettingsUtility
    {

        public static bool Read<T>(string name, [NotNullWhen(true)] out T? value)
        {

            var method = GetMethod().MakeGenericMethod(typeof(T));

            var param = new object?[] { name, null };
            var result = method.Invoke(null, param);

            value = result is true
                ? (T?)param[1]
                : default;

            return value is not null;

        }

        public static void Save(string key, object? value, bool delay = true) =>
            _ = GetMethod().Invoke(null, new[] { key, value, delay });

        static MethodInfo GetMethod([CallerMemberName] string name = "")
        {

            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Common.Settings");
            var type = assembly?.GetType("Common.Settings.Utility.SettingsUtility");
            var method = type?.GetMethod(name);

            return method is not null
                ? method
                : throw new InvalidOperationException("Common-wpf.Settings must be installed in order to use 'Common.SavePosition'.");

        }

    }

    #endregion

    #endregion

    #endregion

}

