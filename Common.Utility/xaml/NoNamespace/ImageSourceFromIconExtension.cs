using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace Common.Utility.xaml.NoNamespace;

/// <summary>Converts a <see cref="Icon"/> to a <see cref="BitmapImage"/>.</summary>
[ContentProperty(nameof(Icon))]
public class ImageSourceFromIconExtension : System.Windows.Markup.MarkupExtension
{

    /// <summary>Specifies the icon to convert.</summary>
    public Icon? Icon { get; set; }

    /// <summary>Creates a new <see cref="ImageSourceFromIconExtension"/>.</summary>
    public ImageSourceFromIconExtension()
    { }

    /// <summary>Creates a new <see cref="ImageSourceFromIconExtension"/>.</summary>
    public ImageSourceFromIconExtension(Icon icon) =>
        Icon = icon;

    /// <inheritdoc/>
    public override object? ProvideValue(IServiceProvider serviceProvider) =>
        Icon?.Handle is not null && Icon.Handle != IntPtr.Zero
        ? Imaging.CreateBitmapSourceFromHIcon(Icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
        : null;

}