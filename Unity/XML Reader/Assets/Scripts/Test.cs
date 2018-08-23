using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;

public class Test : MonoBehaviour
{
    /***** user32.dll functions *****/

    // Delegate to filter which windows to include 
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);


    /***** ScreenGrabber.dll functions *****/

    //clips a screenshot of a window handle to the clipboard
    [DllImport("TEST.dll", EntryPoint = "Clip")] public static extern bool Clip(IntPtr hWnd);

    //Returns a Bitmap Handle of the passed window
    [DllImport("TEST.dll", EntryPoint = "GetHBitmapOfWindow")] public static extern IntPtr GetHBitmapOfWindow(IntPtr hWnd);

    //Returns a Windows Bitmap of the passed window
    [DllImport("TEST.dll", EntryPoint = "GetBitmapOfWindow")] public static extern IntPtr GetBitmapOfWindow(IntPtr hWnd);

    #region Private Methods

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

    private Texture2D GetTextureOfWindow(IntPtr hWnd)
    {
        IntPtr hbitmap = GetHBitmapOfWindow(hWnd);
        Bitmap bmp = System.Drawing.Image.FromHbitmap(hbitmap);
        Texture2D tex = new Texture2D(bmp.Width, bmp.Height, TextureFormat.ARGB32, false);

        MemoryStream mStream = new MemoryStream();
        UnityEngine.Debug.Log(bmp.GetPixel(500, 60));
        bmp.Save(mStream, bmp.RawFormat);
        tex.LoadRawTextureData(mStream.ToArray());
        return tex;
    }

    #endregion

    #region MonoBehaviour Functions 

    void Update()
    {
        if (Input.anyKeyDown)
        {
            IEnumerable<IntPtr> hWndList = FindWindowsWithText("PowerPoint");

            const uint WM_KEYDOWN = 0x100;

            //Forwards
            int key = 39;

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                //Backwards
                key = 37;
            }

            foreach (IntPtr hWnd in hWndList)
            {
                //Getting the targets window as Texture2D
                Texture2D tex = GetTextureOfWindow(hWnd);

                //GameObject.Find("Cube").GetComponent<Renderer>().material.mainTexture = tex;
            }
        }
    }

    #endregion
}