#include "Test.h"

extern "C" {
	BITMAP GetBitmapOfWindow(HWND hwnd) {
		RECT rc;

		GetClientRect(hwnd, &rc);

		HDC hdcScreen = GetDC(NULL);
		HDC hdc = CreateCompatibleDC(hdcScreen);
		HBITMAP hbmp = CreateCompatibleBitmap(hdcScreen,
			rc.right - rc.left, rc.bottom - rc.top);

		BITMAP bmp;
		GetObject(hbmp, sizeof(bmp), (LPVOID)&bmp);

		SelectObject(hdc, hbmp);

		PrintWindow(hwnd, hdc, PW_CLIENTONLY);

		DeleteDC(hdc);
		DeleteObject(hbmp);
		ReleaseDC(NULL, hdcScreen);
		return bmp;
	}

	HBITMAP GetHBitmapOfWindow(HWND hwnd) {
		RECT rc;

		GetClientRect(hwnd, &rc);

		HDC hdcScreen = GetDC(NULL);
		HDC hdc = CreateCompatibleDC(hdcScreen);
		HBITMAP hbmp = CreateCompatibleBitmap(hdcScreen,
			rc.right - rc.left, rc.bottom - rc.top);

		BITMAP bmp;
		GetObject(hbmp, sizeof(bmp), (LPVOID)&bmp);

		SelectObject(hdc, hbmp);

		PrintWindow(hwnd, hdc, PW_CLIENTONLY);

		DeleteDC(hdc);
		ReleaseDC(NULL, hdcScreen);

		return hbmp;
	}

	float TestMultiply(float a, float b)
	{
		return a * b;
	}

	float Clop(float a, float b)
	{
		if (b == 0) {
			return 0;
		}

		return a / b;
	}

	bool Clip(HWND hwnd) {
		RECT rc;

		GetClientRect(hwnd, &rc);

		HDC hdcScreen = GetDC(NULL);
		HDC hdc = CreateCompatibleDC(hdcScreen);
		HBITMAP hbmp = CreateCompatibleBitmap(hdcScreen,
			rc.right - rc.left, rc.bottom - rc.top);

		BITMAP bmp;
		GetObject(hbmp, sizeof(bmp), (LPVOID)&bmp);

		SelectObject(hdc, hbmp);

		PrintWindow(hwnd, hdc, PW_CLIENTONLY);

		OpenClipboard(NULL);
		EmptyClipboard();
		SetClipboardData(CF_BITMAP, hbmp);
		CloseClipboard();

		DeleteObject(hbmp);
		DeleteDC(hdc);
		ReleaseDC(NULL, hdcScreen);

		return true;
	}
}