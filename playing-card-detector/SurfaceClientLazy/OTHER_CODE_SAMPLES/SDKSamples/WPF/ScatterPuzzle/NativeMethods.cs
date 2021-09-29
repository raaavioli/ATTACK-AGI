using System;
using System.Runtime.InteropServices;

namespace ScatterPuzzle
{   
    /// <summary>
    /// This static class encapsulates the Win32 method for pinvoke.
    /// </summary>
    internal static class NativeMethods
    {      
        [DllImport("shell32.dll")]
        public static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);
    }
}
