using System;
using System.Runtime.InteropServices;

namespace EnhancedYoutubeDownloader.Utils;

/// <summary>
/// Contains native methods imported from system libraries.
/// </summary>
public static class NativeMethods
{
    /// <summary>
    /// Contains native methods specific to the Windows operating system.
    /// </summary>
    public static class Windows
    {
        /// <summary>
        /// Displays a modal dialog box that contains a system icon, a set of buttons, and a brief, application-specific message, such as status or error information.
        /// </summary>
        /// <param name="hWnd">A handle to the owner window of the message box to be created. If this parameter is 0, the message box has no owner window.</param>
        /// <param name="text">The message to be displayed.</param>
        /// <param name="caption">The dialog box title.</param>
        /// <param name="type">The contents and behavior of the dialog box.</param>
        /// <returns>An integer value that indicates which button the user clicked.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(int hWnd, string text, string caption, uint type);
    }
}