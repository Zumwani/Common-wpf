using System;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml", "Common.Utility.Extensions")]
namespace Common.Utility.Extensions;

public class TrueExtension : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider) => true;
}

public class FalseExtension : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider) => false;
}
