using Arqanum.Pages;
using ArqanumCore.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Runtime.InteropServices;
using Windows.Storage;
using WinRT.Interop;

namespace Arqanum
{
    public sealed partial class MainWindow : Window
    {
        private readonly AccountService _accountService;
        private IntPtr _hwnd;
        private IntPtr _prevWndProc;
        private WndProc _newWndProcDelegate;
        private bool _isLoaded = false;

        private const string ThemeSettingKey = "AppTheme";

        public MainWindow(AccountService accountService)
        {
            InitializeComponent();
            _hwnd = WindowNative.GetWindowHandle(this);

            _newWndProcDelegate = WindowProc;

            _prevWndProc = SetWindowLongPtr(_hwnd, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(_newWndProcDelegate));
            _accountService = accountService;
            this.Activated += MainWindow_Activated;
        }
       
        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;

                var accountExist = await _accountService.AccountExist();

                if(accountExist)
                {
                    MainFrame.Navigate(typeof(MainPage), null, new DrillInNavigationTransitionInfo());
                }
                else
                {
                    MainFrame.Navigate(typeof(WelcomePage), null, new DrillInNavigationTransitionInfo());
                }
            }
        }


        private delegate IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        private const int GWL_WNDPROC = -4;
        private const uint WM_GETMINMAXINFO = 0x0024;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        private IntPtr WindowProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_GETMINMAXINFO)
            {
                MINMAXINFO mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);
                mmi.ptMinTrackSize.x = 1000;
                mmi.ptMinTrackSize.y = 700;
                Marshal.StructureToPtr(mmi, lParam, true);
            }

            return CallWindowProc(_prevWndProc, hwnd, msg, wParam, lParam);
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "CallWindowProcW")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    }
}
