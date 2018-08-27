#define TESTFUNCDLL_API __declspec(dllexport) 

#include "windows.h"

extern "C" {
	TESTFUNCDLL_API BITMAP  GetBitmapOfWindow(HWND hwnd);
	TESTFUNCDLL_API HBITMAP GetHBitmapOfWindow(HWND hwnd);
	TESTFUNCDLL_API bool    Clip(HWND hwnd);
}