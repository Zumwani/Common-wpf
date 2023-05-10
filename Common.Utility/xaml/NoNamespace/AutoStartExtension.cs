using System.Windows.Data;

namespace Common.Utility.App;

public class AutoStartExtension : Binding
{

    public AutoStartExtension()
    {

        Source = AppUtility.AutoStart;
        Path = new(nameof(AppUtility.AutoStartHelper.Value));
        Mode = BindingMode.TwoWay;
    }

}
