using System;

namespace Common.Utility;

/// <inheritdoc/>
/// <remarks>Overriden to return <see langword="this"/>.</remarks>
public class MarkupExtension : System.Windows.Markup.MarkupExtension
{

    /// <summary>Returns <see langword="this"/>.</summary>
    public override object ProvideValue(IServiceProvider serviceProvider) =>
        this;

}

