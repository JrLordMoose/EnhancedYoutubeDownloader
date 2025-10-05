using System;
using System.Runtime.InteropServices;

namespace EnhancedYoutubeDownloader.Utils;

public static class NativeMethods
{
    public static class Windows
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(int hWnd, string text, string caption, uint type);
    }
}
