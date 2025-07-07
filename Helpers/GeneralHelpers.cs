using RipShout.Models;
using System;
using System.Collections.Generic;
using System.IO;
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

    public static string SetChannelID(ChannelModel station)
    {
        return GetHashString(station.Name + ";" + station.PrimaryURL + ";" + station.Family);
    }

    public static byte[] GetHash(string inputString)
    {
        using(HashAlgorithm algorithm = MD5.Create())// Doesn't need to be super secure, just fast && unique
        {
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }
    }

    public static string GetHashString(string inputString)
    {
        StringBuilder sb = new StringBuilder();
        foreach(byte b in GetHash(inputString))
        {
            sb.Append(b.ToString("X2"));
        }
        return sb.ToString();
    }


    public enum LogFileType
    {
        Exception,
        ArtistLookupFailure,
        AlbumLookupFailure,
    }

    public static void WriteLogEntry(string entry, LogFileType logType)
    {
        if(!App.MySettings.LoggingOn)
        {
            return;
        }
        string logFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ripshout\\Logs\\";
        if(!Directory.Exists(logFolder))
        {
            Directory.CreateDirectory(logFolder);
        }
        var fileName = "log-" + logType.ToString() + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ff") + ".txt";
        using(TextWriter tw = new StreamWriter(logFolder + fileName))
        {
            tw.Write(entry);
        }
    }
}
