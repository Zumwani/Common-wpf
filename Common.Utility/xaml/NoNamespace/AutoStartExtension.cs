using System.Windows.Data;

namespace Common.Utility.App;

/// <summary>A binding extension to easily bind auto start functionality.</summary>
public class AutoStartExtension : Binding
{

    /// <summary>Creates a new <see cref="AutoStartExtension"/>.</summary>
    public AutoStartExtension()
    {

        Source = AppUtility.AutoStart;
        Path = new(nameof(AppUtility.AutoStartHelper.Value));
        Mode = BindingMode.TwoWay;
    }

}
