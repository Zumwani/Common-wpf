namespace Common.Utility;

/// <summary>Contains some functions relating to strings.</summary>
public static class StringUtility
{

    /// <summary>Adds quotes to the start and end of the string. Won't add duplicates.</summary>
    public static string Quotify(this string str, char quoteChar = '"')
    {
        if (!str.StartsWith(quoteChar)) str = quoteChar + str;
        if (!str.EndsWith(quoteChar)) str += quoteChar;
        return str;
    }

    /// <summary>Adds single quotes to the start and end of the string. Won't add duplicates.</summary>
    public static string SingleQuotify(this string str) =>
        str.Quotify('\'');

}

