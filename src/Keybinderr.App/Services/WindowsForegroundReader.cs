using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Keybinderr.Core.Models;

namespace Keybinderr.App.Services;

public static class WindowsForegroundReader
{
    public static ForegroundWindowSnapshot? ReadForegroundWindow()
    {
        var windowHandle = GetForegroundWindow();
        if (windowHandle == IntPtr.Zero)
        {
            return null;
        }

        _ = GetWindowThreadProcessId(windowHandle, out var processId);
        if (processId == 0)
        {
            return null;
        }

        string? processName = null;
        string? processPath = null;

        try
        {
            using var process = Process.GetProcessById((int)processId);
            processName = process.ProcessName;

            try
            {
                processPath = process.MainModule?.FileName;
            }
            catch (InvalidOperationException)
            {
                processPath = null;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                processPath = null;
            }
        }
        catch (ArgumentException)
        {
            return null;
        }

        return new ForegroundWindowSnapshot
        {
            ProcessId = (int)processId,
            ProcessName = processName,
            ProcessPath = processPath,
            WindowTitle = GetWindowTitle(windowHandle)
        };
    }

    private static string? GetWindowTitle(IntPtr windowHandle)
    {
        var length = GetWindowTextLength(windowHandle);
        if (length <= 0)
        {
            return null;
        }

        var builder = new StringBuilder(length + 1);
        return GetWindowText(windowHandle, builder, builder.Capacity) > 0
            ? builder.ToString()
            : null;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(IntPtr hWnd);
}

