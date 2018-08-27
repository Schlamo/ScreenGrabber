using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;

public class ScreenGrabber : MonoBehaviour
{

    #region Public Members

    public GameObject screen;
    public string window;

    #endregion

    #region Private Members 

    private Texture2D tex;
    private IntPtr hBmp;
    private IntPtr hWnd;
    private int rect;
    private bool running = false;

    #endregion

    #region DllImport Methods

    /***** user32.dll functions *****/

    //
    [DllImport("Gdi32.dll")] private static extern bool DeleteObject(IntPtr handle);


    /***** user32.dll functions *****/

    // Delegate to filter which windows to include 
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);


    /***** ScreenGrabDll.dll functions *****/

    //clips a screenshot of a window handle to the clipboard
    [DllImport("ScreenGrabDll.dll", EntryPoint = "Clip")] public static extern bool Clip(IntPtr hWnd);

    //Returns a Bitmap Handle of the passed window
    [DllImport("ScreenGrabDll.dll", EntryPoint = "GetHBitmapOfWindow")] public static extern IntPtr GetHBitmapOfWindow(IntPtr hWnd);

    //Returns a Win32 Bitmap of the passed window
    [DllImport("ScreenGrabDll.dll", EntryPoint = "GetBitmapOfWindow")] public static extern IntPtr GetBitmapOfWindow(IntPtr hWnd);

    #endregion

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

    private IntPtr GetPresentationWindow()
    {
        IEnumerable<IntPtr> hWndList = FindWindowsWithText(window);
        foreach (IntPtr hWnd in hWndList)
        {
            if(GetWindowText(hWnd).Contains(window));
            return hWnd;
        }
        return IntPtr.Zero;
    }

    private void InitializeTex()
    {
        if(hWnd != IntPtr.Zero)
        {
            IntPtr hbitmap = GetHBitmapOfWindow(hWnd);
            Bitmap bmp     = Image.FromHbitmap(hbitmap);

            rect = bmp.Height * bmp.Width;
            tex  = new Texture2D(bmp.Width, bmp.Height, TextureFormat.BGRA32, false);

            bmp.Dispose();
        }
    }

    private void GetTextureOfWindow(IntPtr hWnd)
    {
        //Get the handle of the windows bitmap via ScreenGrab.dll
        hBmp        = GetHBitmapOfWindow(hWnd);

        //Get the actual bitmap from the handle via System.Drawing.dll
        Bitmap bmp  = Image.FromHbitmap(hBmp);

        //Delete temporary handles 
        DeleteObject(hWnd);
        DeleteObject(hBmp);

        //Locks the system storage and stores some BitmapData in bmpData
        System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(
            new Rectangle(new Point(), bmp.Size), 
            System.Drawing.Imaging.ImageLockMode.ReadOnly, 
            System.Drawing.Imaging.PixelFormat.Format32bppArgb
        );

        //Calculates total size of the bitmap and allocates 
        int size        = bmpData.Stride * bmp.Height;
        int tmpRect     = bmp.Height * bmp.Width;
        byte[] bytes    = new byte[size];

        if (rect != tmpRect)
        {
            rect = tmpRect;
            InitializeTex();
        }
        
        Marshal.Copy(bmpData.Scan0, bytes, 0, size);

        bmp.UnlockBits(bmpData);
        try
        {
            tex.LoadRawTextureData(bytes);
            GameObject.Find("Cube").GetComponent<Renderer>().material.mainTexture = tex;
            tex.Apply();
        }
        catch(Exception e)
        {
            InitializeTex();
        }
        bmp.Dispose();
    }

    #endregion

    #region MonoBehaviour Functions 

    private void Update()
    {
        //Displaying the PP-Windows Content on the cube's surface
        GetTextureOfWindow(hWnd);


        /*if (Input.anyKeyDown)
        {
            const uint WM_KEYDOWN = 0x100;

            //Forwards
            int key = 39;

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                //Backwards
                key = 37;
            }
        }*/
    }

    private void Start()
    {
        if(!screen.GetComponent<MeshRenderer>())
        {
            Destroy(this.gameObject);
        }

        running = true;
        hWnd = GetPresentationWindow();
        InitializeTex();
    }

    #endregion
}