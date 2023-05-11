using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media.Animation;

namespace Common.Utility.xaml.NoNamespace;

/// <summary>Contains attached properties for 'Transition.'.</summary>
public static class Transition
{

    internal static readonly CubicEase ease = new();

    /// <summary>Gets a fade animation.</summary>
    public static Storyboard FadeAnimation(double to, double durationInSeconds = 0.25, double? from = null)
    {

        var storyboard = new Storyboard();

        var animation = new DoubleAnimation { Duration = new(TimeSpan.FromSeconds(durationInSeconds)), From = from, To = to, EasingFunction = ease, FillBehavior = FillBehavior.Stop };
        Storyboard.SetTargetProperty(animation, new(nameof(UIElement.Opacity)));
        storyboard.Children.Add(animation);

        return storyboard;

    }

    /// <summary>Gets the show animation for <paramref name="element"/>.</summary>
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

    /// <summary>Gets the hide animation for <paramref name="element"/>.</summary>
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

    /// <summary>Gets the default show animation.</summary>
    public static Storyboard GetShowDefault(double durationInSeconds = 0.25) =>
        FadeAnimation(to: 1, durationInSeconds);

    /// <summary>Gets whatever the default show animation should be used.</summary>
    public static bool GetUseDefaultShow(FrameworkElement obj) => (bool)obj.GetValue(UseDefaultShowProperty);
    /// <summary>Sets whatever the default show animation should be used.</summary>
    public static void SetUseDefaultShow(FrameworkElement obj, bool value) => obj.SetValue(UseDefaultShowProperty, value);

    /// <summary>The dependency property that determines whatever the default show animation should be used.</summary>
    public static readonly DependencyProperty UseDefaultShowProperty = DependencyProperty.RegisterAttached("UseDefaultShow", typeof(bool), typeof(Common), new PropertyMetadata(true));

    /// <summary>Gets the show animation.</summary>
    public static Storyboard? GetShow(FrameworkElement obj) => (Storyboard?)obj.GetValue(ShowProperty);
    /// <summary>Sets the show animation.</summary>
    public static void SetShow(FrameworkElement obj, Storyboard? value) => obj.SetValue(ShowProperty, value);

    /// <summary>The dependency property that determines the show animation.</summary>
    public static readonly DependencyProperty ShowProperty = DependencyProperty.RegisterAttached("Show", typeof(Storyboard), typeof(Common), new PropertyMetadata(null));

    #endregion
    #region Hide

    /// <summary>Gets the default hide animation.</summary>
    public static Storyboard GetHideDefault(double durationInSeconds = 0.25) =>
        FadeAnimation(to: 0, durationInSeconds);

    /// <summary>Gets whatever the default hide animation should be used.</summary>
    public static bool GetUseDefaultHide(FrameworkElement obj) => (bool)obj.GetValue(UseDefaultHideProperty);
    /// <summary>Sets whatever the default hide animation should be used.</summary>
    public static void SetUseDefaultHide(FrameworkElement obj, bool value) => obj.SetValue(UseDefaultHideProperty, value);

    /// <summary>The dependency property that determines whatever the default hide animation should be used.</summary>
    public static readonly DependencyProperty UseDefaultHideProperty =
        DependencyProperty.RegisterAttached("UseDefaultHide", typeof(bool), typeof(Common), new PropertyMetadata(true));

    /// <summary>Gets the hide animation.</summary>
    public static Storyboard? GetHide(FrameworkElement obj) => (Storyboard?)obj.GetValue(HideProperty);
    /// <summary>Sets the hide animation.</summary>
    public static void SetHide(FrameworkElement obj, Storyboard? value) => obj.SetValue(HideProperty, value);

    /// <summary>The dependency property that determines the hide animation.</summary>
    public static readonly DependencyProperty HideProperty =
        DependencyProperty.RegisterAttached("Hide", typeof(Storyboard), typeof(Common), new PropertyMetadata(null));

    #endregion
    #region IsEnabled

    /// <summary>Gets whatever the element should use transitions.</summary>
    public static bool GetIsEnabled(FrameworkElement obj) => (bool)obj.GetValue(IsEnabledProperty);
    /// <summary>Sets whatever the element should use transitions.</summary>
    public static void SetIsEnabled(FrameworkElement obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    /// <summary>The dependency property that determines whatever transitions should be used for this element.</summary>
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(Common), new PropertyMetadata(true));

    #endregion

}