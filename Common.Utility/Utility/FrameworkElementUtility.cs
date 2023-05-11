using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace Common.Utility;

/// <summary>Provides utility methods for <see cref="FrameworkElement"/>.</summary>
public static class FrameworkElementUtility
{

    /// <summary>Finds a parent of this element.</summary>
    /// <param name="element">The element to find a parent of.</param>
    /// <param name="parent">The found parent.</param>
    /// <param name="name">The name of the parent, optional.</param>
    /// <returns>Whatever a parent was found.</returns>
    public static bool FindParent<T>(this FrameworkElement? element, [NotNullWhen(true)] out T? parent, string? name = null) where T : FrameworkElement
    {

        parent = null;
        if (element == null)
            return false;

        T? foundParent = null;
        var currentParent = VisualTreeHelper.GetParent(element);


        while (currentParent is not null)
        {

            if (currentParent is T el && (string.IsNullOrEmpty(name) || el.Name == name))
            {
                foundParent = el;
                break;
            }

            currentParent = VisualTreeHelper.GetParent(currentParent);

        }

        parent = foundParent;
        return parent is not null;

    }

}
