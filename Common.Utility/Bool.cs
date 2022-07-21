using System;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml", "Common.Utility.Bool")]
namespace Common.Utility.Bool;

class TrueExtension : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider) => true;
}

class FalseExtension : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider) => false;
}
