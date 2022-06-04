using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Common.Utility.Utility;

/// <summary>Provides attached properties for use in xaml.</summary>
public static partial class Common
{

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

    static ButtonBase button;
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
    #region Center

    public static bool GetCenterContextMenu(DependencyObject obj) =>
        (bool)obj.GetValue(CenterContextMenuProperty);

    public static void SetCenterContextMenu(DependencyObject obj, bool value) =>
        obj.SetValue(CenterContextMenuProperty, value);

    public static readonly DependencyProperty CenterContextMenuProperty =
        DependencyProperty.RegisterAttached("CenterContextMenu", typeof(bool), typeof(Common), new PropertyMetadata(false));

    #endregion
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

}

