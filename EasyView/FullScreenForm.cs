using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

namespace EasyView
{
    public class FullScreenForm : Form
    {
        #region Win API
        [DllImport("user32.dll")]
        public static extern void SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int X, int Y, int width, int height, uint flags);
        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public bool Contains(int X, int Y)
            {
                return (X >= Left && X < Right && Y >= Top && Y < Bottom);
            }
        }
        
        [StructLayout(LayoutKind.Sequential)]
        struct MonitorInfo
        {
            public uint size;
            public Rect monitor;
            public Rect work;
            public uint flags;
        }

        public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        public static extern bool
            EnumDisplayMonitors(
                IntPtr hdc,                     //HDC hdc,                   // handle to display DC 
                IntPtr lprcClip,                //LPCRECT lprcClip,          // clipping rectangle 
                MonitorEnumDelegate lpfnEnum,   //MONITORENUMPROC lpfnEnum,  // callback function
                IntPtr dwData                   //LPARAM dwData              // data for callback function 
            );

        [DllImport("user32.dll")]
        static extern bool GetMonitorInfo(IntPtr hmon, ref MonitorInfo mi);
        private static IntPtr HWND_TOP = IntPtr.Zero;
        private const int SWP_SHOWWINDOW = 64; // 0×0040

        static bool MonitorEnum(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
        {
            MonitorInfo mi = new MonitorInfo();
            mi.size = (uint)Marshal.SizeOf(mi);
            bool success = GetMonitorInfo(hMonitor, ref mi);
            Monitors.Add(mi);
            return success;
        }

        static List<MonitorInfo> Monitors = new List<MonitorInfo>();
        static FullScreenForm()
        {
            MonitorEnumDelegate med = new MonitorEnumDelegate(MonitorEnum);
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, med, IntPtr.Zero);
        }

        public void SetWinFullScreen(IntPtr hwnd)
        {
            int X = Left + Width / 2;
            int Y = Top + Height / 2;
            foreach (MonitorInfo mi in Monitors)
            {
                if ( mi.monitor.Contains(X,Y) )
                {
                    SetWindowPos(hwnd, HWND_TOP, mi.monitor.Left, mi.monitor.Top, mi.monitor.Right - mi.monitor.Left, mi.monitor.Bottom - mi.monitor.Top, SWP_SHOWWINDOW);
                    break;
                }
            }
        }
        #endregion

        private struct StoreRestoreState
        {
            private FormWindowState winState;
            private FormBorderStyle brdStyle;
            private bool topMost;
            private Rectangle bounds;

            public void Save(Form form)
            {
                winState = form.WindowState;
                brdStyle = form.FormBorderStyle;
                topMost = form.TopMost;
                bounds = form.Bounds;
            }

            public void Restore(Form form)
            {
                form.WindowState = winState;
                form.FormBorderStyle = brdStyle;
                form.TopMost = topMost;
                form.Bounds = bounds;
            }
        }
        StoreRestoreState storedState;

        protected bool IsMaximized = false;

        public void Switch()
        {
            if (IsMaximized)
            {
                storedState.Restore(this);
                IsMaximized = false;
            }
            else
            {
                IsMaximized = true;
                storedState.Save(this);
                WindowState = FormWindowState.Maximized;
                FormBorderStyle = FormBorderStyle.None;
                SetWinFullScreen(Handle);
            }
        }
    }
}
