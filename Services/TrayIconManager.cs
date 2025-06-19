using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace Arqanum.Services
{
    internal sealed class TrayIconManager : IDisposable
    {
        private const int WM_USER = 0x0400;
        private const int WM_TRAYICON = WM_USER + 1;
        private const int WM_DESTROY = 0x0002;

        private const int ID_TRAY_OPEN = 1001;
        private const int ID_TRAY_EXIT = 1002;
        private readonly Action _exitCallback;

        private IntPtr _messageHwnd;
        private Window _uiWindow;
        private NotifyIconData _nid;
        private WndProcDelegate _wndProcDelegate;

        public TrayIconManager(Window uiWindow, Action exitCallback, string tooltip = "Arqanum Messenger")
        {
            _uiWindow = uiWindow ?? throw new ArgumentNullException(nameof(uiWindow));
            _exitCallback = exitCallback ?? throw new ArgumentNullException(nameof(exitCallback));
            _wndProcDelegate = WndProc;

            RegisterWindowClass();
            _messageHwnd = CreateMessageWindow();

            string iconPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "tray.ico");

            _nid = new NotifyIconData
            {
                cbSize = (uint)Marshal.SizeOf<NotifyIconData>(),
                hWnd = _messageHwnd,
                uID = 1,
                uFlags = 0x00000001 | 0x00000002 | 0x00000004, // NIF_MESSAGE | NIF_ICON | NIF_TIP
                uCallbackMessage = WM_TRAYICON,
                hIcon = LoadIconFromFile(iconPath),
                szTip = tooltip ?? "Tray Icon"
            };


            bool res = Shell_NotifyIcon(0x00000000, ref _nid); // NIM_ADD
            if (!res)
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "Shell_NotifyIcon NIM_ADD failed");
        }
        private static IntPtr LoadIconFromFile(string path)
        {
            IntPtr hIcon = LoadImage(IntPtr.Zero, path, IMAGE_ICON, 0, 0, LR_LOADFROMFILE);
            if (hIcon == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            return hIcon;
        }

        

        const uint IMAGE_ICON = 1;
        const uint LR_LOADFROMFILE = 0x00000010;
        public void Dispose()
        {
            Shell_NotifyIcon(0x00000002, ref _nid); // NIM_DELETE
            if (_messageHwnd != IntPtr.Zero)
            {
                DestroyWindow(_messageHwnd);
                _messageHwnd = IntPtr.Zero;
            }
        }

        private void RegisterWindowClass()
        {
            var wc = new WNDCLASS
            {
                style = 0,
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
                cbClsExtra = 0,
                cbWndExtra = 0,
                hInstance = IntPtr.Zero,
                hIcon = IntPtr.Zero,
                hCursor = IntPtr.Zero,
                hbrBackground = IntPtr.Zero,
                lpszMenuName = null,
                lpszClassName = "ArqanumTrayClass"
            };

            ushort atom = RegisterClass(ref wc);
            if (atom == 0 && Marshal.GetLastWin32Error() != 0)
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "RegisterClass failed");
            }
        }

        private IntPtr CreateMessageWindow()
        {
            IntPtr hwnd = CreateWindowEx(
                0,
                "ArqanumTrayClass",
                string.Empty,
                0,
                0,
                0,
                0,
                0,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero);

            if (hwnd == IntPtr.Zero)
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "CreateWindowEx failed");
            }

            return hwnd;
        }

        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_TRAYICON)
            {
                int mouseEvent = lParam.ToInt32();
                if (mouseEvent == 0x0203) // WM_LBUTTONDBLCLK
                    ShowMainWindow();
                else if (mouseEvent == 0x0204) // WM_RBUTTONDOWN
                    ShowContextMenu();
            }
            else if (msg == WM_DESTROY)
            {
                PostQuitMessage(0);
            }

            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private void ShowMainWindow()
        {
            if (_uiWindow != null)
            {
                var hwnd = WindowNative.GetWindowHandle(_uiWindow);
                ShowWindow(hwnd, 5); // SW_SHOW
                SetForegroundWindow(hwnd);
                _uiWindow.Activate();
            }
        }

        

        private void ShowContextMenu()
        {
            IntPtr hMenu = CreatePopupMenu();
            AppendMenu(hMenu, 0, ID_TRAY_OPEN, "Open");
            AppendMenu(hMenu, 0, ID_TRAY_EXIT, "Exit");

            GetCursorPos(out POINT pt);
            SetForegroundWindow(_messageHwnd);

            const uint TPM_RIGHTBUTTON = 0x0002;
            const uint TPM_RETURNCMD = 0x0100;

            int cmd = TrackPopupMenu(hMenu, TPM_RIGHTBUTTON | TPM_RETURNCMD, pt.X, pt.Y, 0, _messageHwnd, IntPtr.Zero);
            if (cmd != 0)
            {
                OnMenuCommand(cmd);
            }

            PostMessage(_messageHwnd, 0x0000, IntPtr.Zero, IntPtr.Zero); // WM_NULL
        }

        private void OnMenuCommand(int cmd)
        {
            switch (cmd)
            {
                case ID_TRAY_OPEN:
                    ShowMainWindow();
                    break;
                case ID_TRAY_EXIT:
                    _exitCallback.Invoke();
                    break;
            }
        }


        #region WinAPI

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X, Y; }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NotifyIconData
        {
            public uint cbSize;
            public IntPtr hWnd;
            public uint uID;
            public uint uFlags;
            public uint uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WNDCLASS
        {
            public uint style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool Shell_NotifyIcon(uint dwMessage, ref NotifyIconData data);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr CreatePopupMenu();

        [DllImport("user32.dll")]
        private static extern bool AppendMenu(IntPtr hMenu, uint uFlags, uint uIDNewItem, string lpNewItem);

        [DllImport("user32.dll")]
        private static extern int TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int reserved, IntPtr hWnd, IntPtr prcRect);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CreateWindowEx(
            int dwExStyle, string lpClassName, string lpWindowName, int dwStyle,
            int x, int y, int nWidth, int nHeight,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern void PostQuitMessage(int nExitCode);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadImage(IntPtr hInst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

        #endregion
    }
}
