using System.Security.Principal;

namespace Common.Utility;

public static partial class AppUtility
{

    /// <summary>Gets if this instance is elevated.</summary>
    public static bool IsElevated { get; } = GetIsElevated();

    static bool GetIsElevated()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

}
