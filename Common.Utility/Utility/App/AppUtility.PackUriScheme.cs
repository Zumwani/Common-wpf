using System;

namespace Common.Utility;

public static partial class AppUtility
{

    const string PackScheme = "pack";
    public static void RegisterPackUriScheme()
    {
        if (!UriParser.IsKnownScheme(PackScheme))
            UriParser.Register(new GenericUriParser(GenericUriParserOptions.GenericAuthority), PackScheme, -1);
    }

}