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

}
