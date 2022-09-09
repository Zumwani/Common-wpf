using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace Common.Utility;

public static class FrameworkElementUtility
{

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
