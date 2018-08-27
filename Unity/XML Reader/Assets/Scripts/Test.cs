﻿using System;
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
    #region DllImport Methods

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

    private Texture2D GetTextureOfWindow(IntPtr hWnd)
    {
        IntPtr hbitmap = GetHBitmapOfWindow(hWnd);
        System.Drawing.Bitmap bmp = (System.Drawing.Bitmap)System.Drawing.Image.FromHbitmap(hbitmap);
        
        System.Drawing.Color c = bmp.GetPixel(1,1);

        Texture2D tex = new Texture2D(bmp.Width, bmp.Height, TextureFormat.BGRA32, false);

        /*** Approch 1 ***/
        
        System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(
            new Rectangle(new Point(), bmp.Size), 
            System.Drawing.Imaging.ImageLockMode.ReadOnly, 
            System.Drawing.Imaging.PixelFormat.Format32bppArgb
        );

        int size = bmpData.Stride * bmp.Height;
        byte[] bytes = new byte[size];
        
        Marshal.Copy(bmpData.Scan0, bytes, 0, size);

        bmp.UnlockBits(bmpData);
        tex.LoadRawTextureData(bytes);

        GameObject.Find("Cube").GetComponent<Renderer>().material.mainTexture = tex;
        tex.Apply();
        //*/


        /*** Approch 2 ***
         * causes Unity Crash
        
        MemoryStream ms = new MemoryStream();
        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.*);
        Byte[] bytes = ms.GetBuffer();
        ms.Close();

        tex.LoadRawTextureData(bytes);
        */

        bmp.Dispose();
        return tex;
    }

    #endregion



    #region MonoBehaviour Functions 

    void Update()
    {
        IEnumerable<IntPtr> hWndList = FindWindowsWithText("PowerPoint");

        foreach (IntPtr hWnd in hWndList)
        {
            Texture2D tex = GetTextureOfWindow(hWnd);
            //Getting the targets window as Texture2D
        }

        if (Input.anyKeyDown)
        {
            const uint WM_KEYDOWN = 0x100;

            //Forwards
            int key = 39;

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                //Backwards
                key = 37;
            }
        }
    }

    private void Start()
    {
        IEnumerable<IntPtr> hWndList = FindWindowsWithText("PowerPoint");
        const uint WM_KEYDOWN = 0x100;

        foreach (IntPtr hWnd in hWndList)
        {
            //GameObject.Find("Cube").GetComponent<Renderer>().material.mainTexture = tex;
        }

    }

    #endregion



    #region Public Methods

    public static void /*byte[]*/
        BitmapToByteArray(Bitmap bitmap)
    {
        ImageConverter converter = new ImageConverter();
        byte[] arr =  (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
        UnityEngine.Debug.Log(arr);
        //return bytedata;

    }

    public static byte[] ImageToByte2(Image img)
    {
        using (var stream = new MemoryStream())
        {
            img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.ToArray();
        }
    }

    #endregion
}