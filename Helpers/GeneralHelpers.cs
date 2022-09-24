using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RipShout.Helpers;

public static class GeneralHelpers
{
    public static string GetSHA256HashOfString(string value)
    {
        using var hash = SHA256.Create();
        var byteArray = hash.ComputeHash(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(byteArray);
    }

    public static string JustNumbersAndLettersAndLower(this string input)
    {
        return new string(input.Where(c => char.IsLetterOrDigit(c)).ToArray()).ToLower().Trim();
    }

    static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

    public static string GetHumanReadableFileSize(Int64 value)
    {
        if(value < 0)
        {
            return "-" + GetHumanReadableFileSize(-value);
        }
        if(value == 0)
        {
            return "0.0 bytes";
        }

        int mag = (int)Math.Log(value, 1024);
        decimal adjustedSize = (decimal)value / (1L << (mag * 10));

        return string.Format("{0:n2} {1}", adjustedSize, SizeSuffixes[mag]);
    }

    public static bool IsStringYear(string year)
    {
        return (int.TryParse(year, out int myYear));
    }
}
