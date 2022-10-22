using Microsoft.VisualBasic.Logging;
using RipShout.Helpers;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Xml;
using System.Xml.Serialization;

namespace RipShout.Services;

// RECT structure required by WINDOWPLACEMENT structure
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;

    public RECT(int left, int top, int right, int bottom)
    {
        this.Left = left;
        this.Top = top;
        this.Right = right;
        this.Bottom = bottom;
    }
}

// POINT structure required by WINDOWPLACEMENT structure
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int X;
    public int Y;

    public POINT(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
}

// WINDOWPLACEMENT stores the position, size, and state of a window
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct WINDOWPLACEMENT
{
    public int length;
    public int flags;
    public int showCmd;
    public POINT minPosition;
    public POINT maxPosition;
    public RECT normalPosition;
}

public static class WindowPlacement
{
    private static readonly Encoding UTF8Encoder = new UTF8Encoding();
    private static readonly XmlSerializer WinPlacementXmlSerializer = new XmlSerializer(typeof(WINDOWPLACEMENT));

    [DllImport("user32.dll")]
    private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

    [DllImport("user32.dll")]
    private static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

    private const int SW_SHOWNORMAL = 1;
    private const int SW_SHOWMINIMIZED = 2;

    private static void SetPlacement(IntPtr windowHandle, string placementXml)
    {
        if(string.IsNullOrEmpty(placementXml))
        {
            return;
        }

        byte[] xmlBytes = UTF8Encoder.GetBytes(placementXml);

        try
        {
            WINDOWPLACEMENT placement;
            using(MemoryStream memoryStream = new MemoryStream(xmlBytes))
            {
                placement = (WINDOWPLACEMENT)WinPlacementXmlSerializer.Deserialize(memoryStream);
            }

            placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
            placement.flags = 0;
            placement.showCmd = (placement.showCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : placement.showCmd);
            SetWindowPlacement(windowHandle, ref placement);
        }
        catch(InvalidOperationException)
        {
            // Parsing placement XML failed. Fail silently.
        }
    }

    private static string GetPlacement(IntPtr windowHandle)
    {
        WINDOWPLACEMENT placement;
        GetWindowPlacement(windowHandle, out placement);

        using(MemoryStream memoryStream = new MemoryStream())
        {
            using(XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8))
            {
                WinPlacementXmlSerializer.Serialize(xmlTextWriter, placement);
                byte[] xmlBytes = memoryStream.ToArray();
                return UTF8Encoder.GetString(xmlBytes);
            }
        }
    }

    public static void ApplyPlacement(this Window window)
    {
        var className = window.GetType().Name;
        try
        {
            var pos = SettingsIoHelpers.GetFileContents(className + ".pos");
            SetPlacement(new WindowInteropHelper(window).Handle, pos);
        }
        catch(Exception ex)
        {
            GeneralHelpers.WriteLogEntry(ex.ToString(), GeneralHelpers.LogFileType.Exception);
        }
    }

    public static void SavePlacement(this Window window)
    {
        var className = window.GetType().Name;
        var pos = GetPlacement(new WindowInteropHelper(window).Handle);
        try
        {
            SettingsIoHelpers.WriteFile(className + ".pos", pos);
        }
        catch(Exception ex)
        {
            GeneralHelpers.WriteLogEntry(ex.ToString(), GeneralHelpers.LogFileType.Exception);
        }
    }
}