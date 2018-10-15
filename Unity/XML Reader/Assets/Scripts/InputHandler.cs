using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class InputHandler : MonoBehaviour {

    [DllImport("user32.dll")]
    private static extern bool PostMessageA(IntPtr hWnd, uint Msg, uint wParam, int lParam);

    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //Handles inputs and passes them to the target application
        if (Input.anyKeyDown)
        {
            const uint WM_KEYDOWN = 0x100;

            //Works for most windows -_-
            UnityEngine.Debug.Log(PostMessageA(WindowManager.GetWindow(), WM_KEYDOWN, 0x30, 0));
        }
    }

    private static void ForwardPage()
    {
        //UnityEngine.Debug.Log(PostMessage(WindowManager.instance.GetWindow(), WM_KEYDOWN, 65, 0));
    }
}
