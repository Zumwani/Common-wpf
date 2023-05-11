using System;

namespace Common.Utility;

public static partial class AppUtility
{

    const string PackScheme = "pack";

    /// <summary>Registers 'pack' uri scheme. Useful when overriding void main() in wpf.</summary>
    public static void RegisterPackUriScheme()
    {
        if (!UriParser.IsKnownScheme(PackScheme))
            UriParser.Register(new GenericUriParser(GenericUriParserOptions.GenericAuthority), PackScheme, -1);
    }

}