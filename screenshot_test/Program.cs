using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

[DllImport("user32.dll")] static extern IntPtr GetDesktopWindow();
[DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr hWnd);
[DllImport("user32.dll")] static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
[DllImport("gdi32.dll")]  static extern IntPtr CreateCompatibleDC(IntPtr hDC);
[DllImport("gdi32.dll")]  static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int w, int h);
[DllImport("gdi32.dll")]  static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObj);
[DllImport("gdi32.dll")]  static extern bool BitBlt(IntPtr dst, int x, int y, int w, int h, IntPtr src, int sx, int sy, int rop);
[DllImport("gdi32.dll")]  static extern bool DeleteDC(IntPtr hDC);
[DllImport("gdi32.dll")]  static extern bool DeleteObject(IntPtr hObj);
[DllImport("user32.dll")] static extern int GetSystemMetrics(int n);

const int SRCCOPY = 0x00CC0020;
int width  = GetSystemMetrics(0);
int height = GetSystemMetrics(1);

string savePath = Path.Combine(Directory.GetCurrentDirectory(), $"test_screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");

Console.WriteLine($"Screen size: {width}x{height}");
Console.WriteLine($"Saving to: {savePath}");

IntPtr desktopWnd = GetDesktopWindow();
IntPtr desktopDC  = GetDC(desktopWnd);
IntPtr memDC      = CreateCompatibleDC(desktopDC);
IntPtr hBitmap    = CreateCompatibleBitmap(desktopDC, width, height);
IntPtr oldBmp     = SelectObject(memDC, hBitmap);

bool ok = BitBlt(memDC, 0, 0, width, height, desktopDC, 0, 0, SRCCOPY);
Console.WriteLine($"BitBlt result: {ok}");

using var bmp = Image.FromHbitmap(hBitmap);
bmp.Save(savePath, ImageFormat.Png);

SelectObject(memDC, oldBmp);
DeleteObject(hBitmap);
DeleteDC(memDC);
ReleaseDC(desktopWnd, desktopDC);

bool fileExists = File.Exists(savePath);
long fileSize   = fileExists ? new FileInfo(savePath).Length : 0;
Console.WriteLine(fileExists
    ? $"✅ Screenshot saved! File size: {fileSize / 1024} KB"
    : "❌ File not found after save.");
