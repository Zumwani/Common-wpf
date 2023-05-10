using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace Common.Utility.xaml.NoNamespace;

[ContentProperty(nameof(Icon))]
public class ImageSourceFromIconExtension : System.Windows.Markup.MarkupExtension
{

    public Icon? Icon { get; set; }

    public ImageSourceFromIconExtension()
    { }

    public ImageSourceFromIconExtension(Icon icon) =>
        Icon = icon;

    public override object? ProvideValue(IServiceProvider serviceProvider) =>
        Icon?.Handle is not null && Icon.Handle != IntPtr.Zero
        ? Imaging.CreateBitmapSourceFromHIcon(Icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
        : null;

}