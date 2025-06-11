using Arqanum.Pages;
using Arqanum.Services;
using ArqanumCore.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
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

        public void ShowImagePopup(BitmapImage image)
        {
            PopupImage.Source = image;

            PopupRoot.Width = RootGrid.ActualWidth;
            PopupRoot.Height = RootGrid.ActualHeight;

            ImagePopup.IsOpen = true;
        }

        private void ImagePopupGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ImagePopup.IsOpen = false;
            PopupImage.Source = null;
        }
        public MainWindow(AccountService accountService)
        {
            InitializeComponent();

            _hwnd = WindowNative.GetWindowHandle(this);

            _newWndProcDelegate = WindowProc;

            _prevWndProc = SetWindowLongPtr(_hwnd, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(_newWndProcDelegate));

            _accountService = accountService;

            this.Activated += MainWindow_Activated;

            ImagePreviewService.Initialize(this);

            this.SizeChanged += MainWindow_SizeChanged;

            RootGrid.LayoutUpdated += RootGrid_LayoutUpdated;
        }
        private async void SaveImageButton_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new FileSavePicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            savePicker.FileTypeChoices.Add("PNG Image", new[] { ".png" });
            savePicker.SuggestedFileName = "image";

            var file = await savePicker.PickSaveFileAsync();
            if (file == null)
                return;

            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(PopupImage);

            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();

            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(
                    BitmapEncoder.PngEncoderId, stream);

                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied,
                    (uint)renderTargetBitmap.PixelWidth,
                    (uint)renderTargetBitmap.PixelHeight,
                    96, 96,
                    pixelBuffer.ToArray());

                await encoder.FlushAsync();
            }
        }


        private bool _pendingPopupResize = false;

        private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (ImagePopup.IsOpen)
            {
                _pendingPopupResize = true;
            }
        }

        private void RootGrid_LayoutUpdated(object? sender, object e)
        {
            if (_pendingPopupResize && ImagePopup.IsOpen)
            {
                PopupRoot.Width = RootGrid.ActualWidth;
                PopupRoot.Height = RootGrid.ActualHeight;
                _pendingPopupResize = false;
            }
        }


        private void CloseImageButton_Click(object sender, RoutedEventArgs e)
        {
            ImagePopup.IsOpen = false;
            PopupImage.Source = null;
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
