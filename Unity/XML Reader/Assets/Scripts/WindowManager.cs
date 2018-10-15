using System.Collections.Generic;
using System;
using System.Text;
using System.Runtime.InteropServices;

public class WindowManager : Singleton {
    private static IntPtr window;
    public string name;

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    private static string GetWindowText(IntPtr hWnd)
    {
        int size = GetWindowTextLength(hWnd);
        if (size > 0)
        {
            var builder = new StringBuilder(size + 1);
            GetWindowText(hWnd, builder, builder.Capacity);
            return builder.ToString();
        }

        return String.Empty;
    }

    private static IEnumerable<IntPtr> FindWindows(EnumWindowsProc filter)
    {
        //IntPtr found = IntPtr.Zero;
        List<IntPtr> windows = new List<IntPtr>();

        EnumWindows(delegate (IntPtr wnd, IntPtr param)
        {
            if (filter(wnd, param))
            {
                // only add the windows that pass the filter
                windows.Add(wnd);
            }

            // but return true here so that we iterate all windows
            return true;
        }, IntPtr.Zero);

        return windows;
    }

    private static IEnumerable<IntPtr> FindWindowsWithText(string titleText)
    {
        return FindWindows(delegate (IntPtr wnd, IntPtr param)
        {
            return GetWindowText(wnd).Contains(titleText);
        });
    }

    private IntPtr GetWindow(string target)
    {
        //If multiple windows with the same name exist, only the first one will be returned
        //The current solution is kinda stupid. Very stupid...
        IEnumerable<IntPtr> hWndList = FindWindowsWithText(target);
        foreach (IntPtr hWnd in hWndList)
        {
            if (GetWindowText(hWnd).Contains(target))
                return hWnd;
        }
        UnityEngine.Debug.LogWarning("Window not found!");
        return IntPtr.Zero;
    }

    public static IntPtr GetWindow()
    {
        return window;
    }

    void Start () {
        window = GetWindow(name);
        //SetWindowPos(this.window, (IntPtr)0, 0, 0, this.width, this.height, SWP_NOMOVE);
    }
}
