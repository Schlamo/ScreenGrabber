using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;

struct size
{
    public int x;
    public int y;
};

public class ScreenGrabber : MonoBehaviour
{

    #region Private Members 
    private Texture2D tex;
    private IntPtr hBmp;
    private IntPtr hWnd;
    private int rect;
    private bool frame = false;
    #endregion

    #region Public Members
    public GameObject screen;
    public string     window;
    public int        width = 1280;
    public int        height = 720;
    #endregion

    #region DllImport Methods
    /*** Gdi32.dll functions ***/
    //Deletes an object, freeing all system resources associated with it
    [DllImport("Gdi32.dll")] private static extern bool DeleteObject(IntPtr handle);

    /*** user32.dll functions ***/
    // Delegate to filter which windows to include 
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);


    /*** ScreenGrabDll.dll functions ***/
    //clips a screenshot of the passed window to the clipboard
    [DllImport("ScreenGrabDll.dll", EntryPoint = "Clip")]
    private static extern bool Clip(IntPtr hWnd);

    //Returns a Bitmap Handle of the passed window
    [DllImport("ScreenGrabDll.dll", EntryPoint = "GetHBitmapOfWindow")]
    private static extern IntPtr GetHBitmapOfWindow(IntPtr hWnd);

    //Returns a Win32 Bitmap of the passed window
    [DllImport("ScreenGrabDll.dll", EntryPoint = "GetBitmapOfWindow")]
    private static extern IntPtr GetBitmapOfWindow(IntPtr hWnd);

    [DllImport("ScreenGrabDll.dll", EntryPoint = "GetWindowSize")]
    private static extern size GetWindowSize(IntPtr hWnd);
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

    private IntPtr GetWindow(string target)
    {
        //If multiple windows with the same name exist, only the first one will be returned
        //The current solution is kinda stupid. Very stupid...
        IEnumerable<IntPtr> hWndList = FindWindowsWithText(target);
        foreach (IntPtr hWnd in hWndList)
        {
            if(GetWindowText(hWnd).Contains(target))
                return hWnd;
        }
        UnityEngine.Debug.LogWarning("Window not found!");
        return IntPtr.Zero;
    }

    private void InitializeTex()
    {
        if(this.hWnd != IntPtr.Zero)
        {
            IntPtr hbitmap = GetHBitmapOfWindow(this.hWnd);
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
        Bitmap bmp = Image.FromHbitmap(hBmp);
        
        //Deletes temporary handles 
        //DeleteObject(hWnd);
        DeleteObject(hBmp);

        //Locks the system storage and stores the BitmapData
        System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(
            new Rectangle(new Point(), bmp.Size), 
            System.Drawing.Imaging.ImageLockMode.ReadOnly, 
            System.Drawing.Imaging.PixelFormat.Format32bppArgb
        );

        //Calculates total size of the Bitmap and allocates 
        int size        = bmpData.Stride * bmp.Height;
        int tmpRect     = bmp.Height * bmp.Width;
        byte[] bytes    = new byte[size];

        //Copies the data from the bitmap to the byte array and unlocks the Bitmap from system memory
        Marshal.Copy(bmpData.Scan0, bytes, 0, size);
        bmp.UnlockBits(bmpData);


        //Decides whether the screen size changed or not
        if (rect != tmpRect)
        {
            rect = tmpRect;
            InitializeTex();
        }

        //Draws the new texture on the screen
        try
        {
            tex.LoadRawTextureData(bytes);
            screen.GetComponent<MeshRenderer>().material.mainTexture = tex;
            tex.Apply();
        }
        catch(Exception e)
        {
            InitializeTex();
        }

        //Releases all resources used by this Bitmap
        bmp.Dispose();
    }
    #endregion

    #region MonoBehaviour Functions 
    private void Update()
    {
        //Dumb method to reduce CPU Workload
        frame = !frame;
        if (frame)
            GetTextureOfWindow(hWnd);

        //Handles inputs and passes them to the target application
        /*if (Input.anyKeyDown)
        {
            const uint WM_KEYDOWN = 0x100;

            //Works for most windows -_-
            UnityEngine.Debug.Log(PostMessage(GetWindow(window), WM_KEYDOWN, 65, 0));
        }*/
    }

    private void Start()
    {
        if(!screen.GetComponent<MeshRenderer>())
            Destroy(this.gameObject);

        this.hWnd = GetWindow(window);

        const uint SWP_NOMOVE = 0x0002;
        UnityEngine.Debug.Log(SetWindowPos(this.hWnd, (IntPtr)0, 0, 0, this.width, this.height, SWP_NOMOVE));

        InitializeTex();
    }
    #endregion
}