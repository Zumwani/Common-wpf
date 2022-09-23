using System;

namespace Common.Utility;

/// <inheritdoc/>
/// <remarks>Overriden to return <see langword="this"/>.</remarks>
public class MarkupExtension : System.Windows.Markup.MarkupExtension
{

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        this;

}

