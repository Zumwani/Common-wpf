using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media.Animation;

namespace Common.Utility.AttachedProperties;

public static class Transition
{

    internal static readonly CubicEase ease = new();

    public static Storyboard FadeAnimation(double to, double durationInSeconds = 0.25, double? from = null)
    {

        var storyboard = new Storyboard();

        var animation = new DoubleAnimation { Duration = new(TimeSpan.FromSeconds(durationInSeconds)), From = from, To = to, EasingFunction = ease, FillBehavior = FillBehavior.Stop };
        Storyboard.SetTargetProperty(animation, new(nameof(UIElement.Opacity)));
        storyboard.Children.Add(animation);

        return storyboard;

    }

    public static bool GetShowAnimation(FrameworkElement element, [NotNullWhen(true)] out Storyboard? storyboard)
    {

        storyboard = null;
        if (!GetIsEnabled(element))
            return false;
        else if (GetShow(element) is Storyboard storyboard1)
        {
            storyboard = storyboard1;
            return true;
        }
        else if (GetUseDefaultShow(element))
        {
            storyboard = GetShowDefault();
            return true;
        }

        return false;

    }

    public static bool GetHideAnimation(FrameworkElement element, [NotNullWhen(true)] out Storyboard? storyboard)
    {

        storyboard = null;
        if (!GetIsEnabled(element))
            return false;
        else if (GetHide(element) is Storyboard storyboard1)
        {
            storyboard = storyboard1;
            return true;
        }
        else if (GetUseDefaultHide(element))
        {
            storyboard = GetHideDefault();
            return true;
        }

        return false;

    }

    #region Show

    public static Storyboard GetShowDefault(double durationInSeconds = 0.25) =>
        FadeAnimation(to: 1, durationInSeconds);

    public static bool GetUseDefaultShow(FrameworkElement obj) => (bool)obj.GetValue(UseDefaultShowProperty);
    public static void SetUseDefaultShow(FrameworkElement obj, bool value) => obj.SetValue(UseDefaultShowProperty, value);

    public static readonly DependencyProperty UseDefaultShowProperty = DependencyProperty.RegisterAttached("UseDefaultShow", typeof(bool), typeof(Common), new PropertyMetadata(true));

    public static Storyboard? GetShow(FrameworkElement obj) => (Storyboard?)obj.GetValue(ShowProperty);
    public static void SetShow(FrameworkElement obj, Storyboard? value) => obj.SetValue(ShowProperty, value);

    public static readonly DependencyProperty ShowProperty = DependencyProperty.RegisterAttached("Show", typeof(Storyboard), typeof(Common), new PropertyMetadata(null));

    #endregion
    #region Hide

    public static Storyboard GetHideDefault(double durationInSeconds = 0.25) =>
        FadeAnimation(to: 0, durationInSeconds);

    public static bool GetUseDefaultHide(FrameworkElement obj) => (bool)obj.GetValue(UseDefaultHideProperty);
    public static void SetUseDefaultHide(FrameworkElement obj, bool value) => obj.SetValue(UseDefaultHideProperty, value);

    public static readonly DependencyProperty UseDefaultHideProperty =
        DependencyProperty.RegisterAttached("UseDefaultHide", typeof(bool), typeof(Common), new PropertyMetadata(true));

    public static Storyboard? GetHide(FrameworkElement obj) => (Storyboard?)obj.GetValue(HideProperty);
    public static void SetHide(FrameworkElement obj, Storyboard? value) => obj.SetValue(HideProperty, value);

    public static readonly DependencyProperty HideProperty =
        DependencyProperty.RegisterAttached("Hide", typeof(Storyboard), typeof(Common), new PropertyMetadata(null));

    #endregion
    #region IsEnabled

    public static bool GetIsEnabled(FrameworkElement obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(FrameworkElement obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(Common), new PropertyMetadata(true, sak));

    static void sak(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

    }

    #endregion

}