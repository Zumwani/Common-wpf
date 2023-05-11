using System.Windows;

namespace Common.Utility;

/// <summary>Provides utility functions for <see cref="Rect"/>.</summary>
public static class RectUtility
{

    /// <summary>Converts a <see cref="System.Drawing.Rectangle"/> to <see cref="Rect"/>.</summary>
    public static Rect ToRect(this System.Drawing.Rectangle rect) =>
        new(rect.Left, rect.Top, rect.Width, rect.Height);

}