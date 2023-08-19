using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScreenCaptureService
{
    public class ScreenshotCaptureService
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern int DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern int DeleteObject(IntPtr hObject);


        private static string _outputPath = string.Empty;
        private static string _fileNameFormat;
        private static int _intervalSeconds;
        private static bool isStart = true;

        public CancellationTokenSource cancellationTokenSource;

        public void Start()
        {
            cancellationTokenSource = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    CaptureScreenToFile();
                    await Task.Delay(_intervalSeconds * 1000);
                }
            });

        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }


        public static void CaptureScreenToFile()
        {
            LoadConfig();
            string fileName = string.Format(_fileNameFormat, DateTime.Now);
            IntPtr desktopHdc = GetDC(IntPtr.Zero);
            IntPtr hdc = CreateCompatibleDC(desktopHdc);

            int width = GetSystemMetrics(0); // Screen width
            int height = GetSystemMetrics(1); // Screen height

            IntPtr bmp = CreateCompatibleBitmap(desktopHdc, width, height);
            SelectObject(hdc, bmp);

            BitBlt(hdc, 0, 0, width, height, desktopHdc, 0, 0, 13369376); // Capture entire screen

            using (Bitmap screenshot = Image.FromHbitmap(bmp))
            {
                string filePath = Path.Combine(_outputPath, fileName);
                screenshot.Save(filePath, ImageFormat.Png);
            }

            DeleteObject(bmp);
            DeleteDC(hdc);
            ReleaseDC(IntPtr.Zero, desktopHdc);
        }
        private static void LoadConfig()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            if (File.Exists(configPath))
            {
                string configJson = File.ReadAllText(configPath);
                dynamic config = JsonConvert.DeserializeObject(configJson);
                _outputPath = config.OutputPath;
                _fileNameFormat = config.FileNameFormat;
                _intervalSeconds = config.IntervalSeconds;
            }
            else
            {
                _outputPath = @"D:\Screenshots\";
                _fileNameFormat = $"Screenshot_{DateTime.Now:yyyyMMddHHmmss}.png";
                _intervalSeconds = 30;
            }

            if (!Directory.Exists(_outputPath))
            {
                Directory.CreateDirectory(_outputPath);
            }
        }
    }
}
