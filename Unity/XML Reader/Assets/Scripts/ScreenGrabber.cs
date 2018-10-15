using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Drawing;
using System.Linq;
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
    private int frame = 0;
    private int framesPerFrame = 3;

    private int opt_framesPerFrame = 1;
    private int opt_oldHash = 0;
    private int opt_hash = 0;
    private float opt_trySlow = 2.0f;
    public  float opt_timer = 0.0f;


    private float[] dTimes;
    private int dCounter = 0;
    #endregion

    #region Public Members
    public GameObject screen;
    public string window;
    public int width = 1280;
    public int height = 720;
    public int minFramerate = 60;
    #endregion

    #region DllImport Methods
    /*** Gdi32.dll functions ***/
    //Deletes an object, freeing all system resources associated with it
    [DllImport("Gdi32.dll")] private static extern bool DeleteObject(IntPtr handle);

    /*** user32.dll functions ***/
    // Delegate to filter which windows to include 
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

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

    /*
    //Returns a Win32 Bitmap of the passed window
    [DllImport("ScreenGrabDll.dll", EntryPoint = "GetBitmapOfWindow")]
    private static extern IntPtr GetBitmapOfWindow(IntPtr hWnd);

    [DllImport("ScreenGrabDll.dll", EntryPoint = "GetWindowSize")]
    private static extern size GetWindowSize(IntPtr hWnd);*/
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
            if (GetWindowText(hWnd).Contains(target))
                return hWnd;
        }
        UnityEngine.Debug.LogWarning("Window not found!");
        return IntPtr.Zero;
    }

    private void InitializeTex()
    {
        if (this.hWnd != IntPtr.Zero)
        {
            IntPtr hbitmap = GetHBitmapOfWindow(this.hWnd);
            Bitmap bmp = Image.FromHbitmap(hbitmap);

            rect = bmp.Height * bmp.Width;
            tex = new Texture2D(bmp.Width, bmp.Height, TextureFormat.BGRA32, false);
            DeleteObject(hbitmap);
            bmp.Dispose();
        }
    }

    private void GetTextureOfWindow(IntPtr hWnd)
    {
        //Get the handle of the windows bitmap via ScreenGrab.dll
        hBmp = GetHBitmapOfWindow(hWnd);

        //Get the actual bitmap from the handle via System.Drawing.dll
        Bitmap bmp = Image.FromHbitmap(hBmp);

        opt_oldHash = opt_hash;
        opt_hash = bmp.GetHashCode();

        //Decides whether the screen size changed or not
        int tmpRect = bmp.Height * bmp.Width;
        if (rect != tmpRect)
        {
            rect = tmpRect;
            InitializeTex();
        }

        //Locks the system storage and stores the BitmapData
        System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(
            new Rectangle(new Point(), bmp.Size),
            System.Drawing.Imaging.ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb
        );

        //Calculates total size of the Bitmap and allocates memory
        int size = bmpData.Stride * bmp.Height;
        byte[] bytes = new byte[size];

        //Copies the data from the bitmap to the byte array and unlocks the Bitmap from system memory
        Marshal.Copy(bmpData.Scan0, bytes, 0, size);
        bmp.UnlockBits(bmpData);


        //Destroy(tex);
        //Draws the new texture on the screen
        try
        {
            tex.LoadRawTextureData(bytes);
            screen.GetComponent<MeshRenderer>().material.mainTexture = tex;
            tex.Apply();
        }
        catch (Exception)
        {
            InitializeTex();
        }

        //Releases all resources used by this Bitmap
        DeleteObject(hBmp);
        bmp.Dispose();
    }
    #endregion
     
    #region MonoBehaviour Functions 
    private void Update()
    {
        frame++;
        float upperLimit = 1.5f;
        float lowerLimit = 1.3f;

        dTimes[dCounter] = Time.deltaTime;
        dCounter = dCounter == minFramerate - 1 ? 0 : ++dCounter;

        if (dCounter == 0 && dTimes.Sum() > upperLimit)
        {
            framesPerFrame++;
            Debug.Log("FramesPerFrame increased to " + framesPerFrame + " (" + dTimes.Sum() + ")");
        }
        else if (dCounter == 0 && dTimes.Sum() < lowerLimit)
        {
            framesPerFrame = framesPerFrame < 2 ? 1 : --framesPerFrame;
            Debug.Log("FramesPerFrame reduced to " + framesPerFrame + " (" + dTimes.Sum() + ")");
        }

        if (frame >= framesPerFrame)
        {
            frame = 0;
            GetTextureOfWindow(hWnd);
        }

        if (Input.anyKeyDown || opt_hash != opt_oldHash)
        {
            opt_framesPerFrame = 1;
        }

        opt_timer += Time.deltaTime;

        if(opt_timer >= opt_trySlow)
        {
            opt_timer -= opt_trySlow;
        }
    }

    private void Start()
    {
        dTimes = new float[minFramerate];
        this.hWnd = GetWindow(window);

        if (!screen.GetComponent<MeshRenderer>())
            Destroy(this.gameObject);


        const uint SWP_NOMOVE = 0x0002;
        UnityEngine.Debug.Log(SetWindowPos(this.hWnd, (IntPtr)0, 0, 0, this.width, this.height, SWP_NOMOVE));

        InitializeTex();
    }
    #endregion
}