using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;

namespace DrawWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double _width = 0;
        private double _height = 0;

        private double Add = 0;


        #region WinApi

        [DllImport("gdi32.dll", SetLastError = true)]
        static extern IntPtr CreateDIBitmap([In] IntPtr hdc, [In] ref BITMAPINFOHEADER lpbmih, uint fdwInit, byte[] lpbInit, [In] ref BITMAPINFO lpbmi, uint fuUsage);

        [DllImport("gdi32.dll", SetLastError = true)]
        static extern int SetDIBits(IntPtr hdc, IntPtr hbmp, uint uStartScan, uint
           cScanLines, byte[] lpvBits, [In] ref BITMAPINFO lpbmi, uint fuColorUse);

        [DllImport("gdi32.dll", SetLastError = true)]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        enum TernaryRasterOperations : uint
        {

            SRCCOPY = 0x00CC0020,

            SRCPAINT = 0x00EE0086,

            SRCAND = 0x008800C6,

            SRCINVERT = 0x00660046,

            SRCERASE = 0x00440328,

            NOTSRCCOPY = 0x00330008,

            NOTSRCERASE = 0x001100A6,

            MERGECOPY = 0x00C000CA,

            MERGEPAINT = 0x00BB0226,

            PATCOPY = 0x00F00021,

            PATPAINT = 0x00FB0A09,

            PATINVERT = 0x005A0049,

            DSTINVERT = 0x00550009,

            BLACKNESS = 0x00000042,

            WHITENESS = 0x00FF0062,

            CAPTUREBLT = 0x40000000
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;

            public void Init()
            {
                biSize = (uint)Marshal.SizeOf(this);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RGBQUAD
        {
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;
            public RGBQUAD bmiColors;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        private System.IntPtr m_Bitmap;
        private BITMAPINFOHEADER m_Bmh;
        private BITMAPINFO m_Bmi = new BITMAPINFO();

        private void Init()
        {
            int errorNumber = Marshal.GetLastWin32Error();

            byte[] testGraphicArray = new byte[300];
            for (int i = 0; i < m_Bmh.biWidth; i++)
            {
                testGraphicArray[i * 3 + 0] = Convert.ToByte(i);
                testGraphicArray[i * 3 + 1] = Convert.ToByte(255 - i);
                testGraphicArray[i * 3 + 2] = Convert.ToByte(i);
            }

            IntPtr winPtr = GetDC((IntPtr)0);

            errorNumber = Marshal.GetLastWin32Error();


            if (m_Bitmap == (IntPtr)0)
                m_Bitmap = CreateDIBitmap(winPtr, ref m_Bmh, (uint)0L, testGraphicArray, ref m_Bmi, (uint)0L);

            errorNumber = Marshal.GetLastWin32Error();

            int retValue;

            for (int i = 0; i < 100; i++)
            {
                retValue = SetDIBits((System.IntPtr)winPtr, m_Bitmap, (uint)i, 1, testGraphicArray, ref m_Bmi, (uint)0L);
            }

            errorNumber = Marshal.GetLastWin32Error();


            if (m_Bitmap != (IntPtr)0)
            {
                IntPtr hMemDC;
                IntPtr Old;

                hMemDC = CreateCompatibleDC((System.IntPtr)winPtr);

                Old = SelectObject(hMemDC, m_Bitmap);

                bool success = BitBlt((System.IntPtr)winPtr, 10, 10, m_Bmh.biWidth, m_Bmh.biHeight, hMemDC, 0, 0, TernaryRasterOperations.SRCCOPY);
                errorNumber = Marshal.GetLastWin32Error();
                SelectObject(hMemDC, Old);

            }

        }

        #endregion



        public MainWindow()
        {
            m_Bmh.Init();
            m_Bmh.biPlanes = 1;
            m_Bmh.biBitCount = 24;
            m_Bmh.biCompression = 0;
            m_Bmh.biHeight = 100;
            m_Bmh.biWidth = 100;

            m_Bitmap = (IntPtr)0;

            InitializeComponent();
        }

        private void Draw()
        {
            Ellipse ellips = null;

            Random random = new Random();

            Dispatcher.BeginInvoke(new Action(delegate
            {

                ellips = new Ellipse();

                ellips.Width = 20;
                ellips.Height = 20;
                Canvas.SetTop(ellips, 50);
                Canvas.SetLeft(ellips, 50);
                ellips.Fill = Brushes.Yellow;

                canvas.Children.Add(ellips);

            }));

            int v = 2;
            double x = random.Next(20, 290);
            double y = random.Next(20, 240);
            double a = random.Next(0, 361);

            double vx = v * Math.Cos(a);
            double vy = v * Math.Sin(a);

            while (true)
            {
                x += vx;
                y += vy;

                if (Add != 0)
                {
                    x += Add;
                    y += Add;

                    Add = 0;
                }

                double[] data = ReadMemory().Split('|').Select(double.Parse).ToArray();

                x = data[0];

                y = data[1];

                vx = data[2];

                vy = data[3];

             /*   if (x < 10)
                {
                    vx = -vx;
                }

                if (x > _width - 10)
                {
                    vx = -vx;
                }

                if (y < 10)
                {
                    vy = -vy;
                }

                if (y > _height - 10)
                {
                    vy = -vy;
                }*/

                Dispatcher.BeginInvoke(new Action(delegate
                {

                    Canvas.SetLeft(ellips, x);
                    Canvas.SetTop(ellips, y);

                }));

                Thread.Sleep(10);
            }
        }

        static string ReadMemory()
        {

            char[] message;

            int size;



            MemoryMappedFile sharedMemory = MemoryMappedFile.OpenExisting("MemoryFile");


            using (MemoryMappedViewAccessor reader = sharedMemory.CreateViewAccessor(0, 4, MemoryMappedFileAccess.Read))
            {
                size = reader.ReadInt32(0);
            }



            using (MemoryMappedViewAccessor reader = sharedMemory.CreateViewAccessor(4, size * 2, MemoryMappedFileAccess.Read))
            {

                message = new char[size];
                reader.ReadArray<char>(0, message, 0, size);
            }


            return new string(message);


        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {

           // sys

             new Thread(Draw).Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _height = canvas.Height;

            _width = canvas.Width;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F)
            {
                Random random = new Random();

                Add += random.Next(10, 100);
            }

            else
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
