using Arqanum.Pages;
using Arqanum.Services;
using ArqanumCore.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using WinRT.Interop;

namespace Arqanum
{
    public sealed partial class MainWindow : Window
    {
        private readonly AccountService _accountService;
        private readonly ContactService _contactService;

        public MainWindow(AccountService accountService, ContactService contactService)
        {
            InitializeComponent();

            _hwnd = WindowNative.GetWindowHandle(this);

            _newWndProcDelegate = WindowProc;

            _prevWndProc = SetWindowLongPtr(_hwnd, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(_newWndProcDelegate));

            _accountService = accountService;

            _contactService = contactService;

            this.Activated += MainWindow_Activated;

            ImagePreviewService.Initialize(this);

            CameraService.Initialize(this);

            this.SizeChanged += MainWindow_SizeChanged;

            RootGrid.LayoutUpdated += RootGrid_LayoutUpdated;
        }


        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;

                MainFrame.Navigate(typeof(LoadingPage), null, new DrillInNavigationTransitionInfo());

                var accountExist = await _accountService.AccountExist();

                if (accountExist)
                {
                    await _contactService.LoadContactsAsync();
                    MainFrame.Navigate(typeof(MainPage), null, new DrillInNavigationTransitionInfo());
                }
                else
                {
                    MainFrame.Navigate(typeof(WelcomePage), null, new DrillInNavigationTransitionInfo());
                }
            }
        }

        #region Camera Popup

        private bool _pendingCameraPopupResize = false;

        private MediaFrameSourceGroup mediaFrameSourceGroup;

        private MediaCapture mediaCapture;

        private bool _isMirrored = false;

        public async void OpenCameraPopup()
        {
            var groups = await MediaFrameSourceGroup.FindAllAsync();

            foreach (var group in groups)
            {
                var colorSourceInfo = group.SourceInfos.FirstOrDefault(s => s.SourceKind == MediaFrameSourceKind.Color);
                if (colorSourceInfo != null)
                {
                    mediaFrameSourceGroup = group;
                    break;
                }
            }
            mediaCapture = new MediaCapture();

            var mediaCaptureInitializationSettings = new MediaCaptureInitializationSettings()
            {
                SourceGroup = this.mediaFrameSourceGroup,
                SharingMode = MediaCaptureSharingMode.SharedReadOnly,
                StreamingCaptureMode = StreamingCaptureMode.Video,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu
            };
            await mediaCapture.InitializeAsync(mediaCaptureInitializationSettings);

            var frameSource = mediaCapture.FrameSources[this.mediaFrameSourceGroup.SourceInfos[0].Id];
            PopupPhotoCameraElement.Source = MediaSource.CreateFromMediaFrameSource(frameSource);


            if (_isMirrored)
            {
                PopupPhotoCameraElement.RenderTransform = new ScaleTransform() { ScaleX = -1 };
                PopupPhotoCameraElement.RenderTransformOrigin = new Point(0.5, 0.5);
            }
            else
            {
                PopupPhotoCameraElement.RenderTransform = new ScaleTransform() { ScaleX = 1 };
                PopupPhotoCameraElement.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            CameraPopupRoot.Width = RootGrid.ActualWidth;
            CameraPopupRoot.Height = RootGrid.ActualHeight;
            CameraPopup.IsOpen = true;

        }

        private void ToggleMirrorButton_Click(object sender, RoutedEventArgs e)
        {
            _isMirrored = !_isMirrored;
            OpenCameraPopup();
        }

        async private void TakePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            // Capture a photo to a stream
            var imgFormat = ImageEncodingProperties.CreateJpeg();
            var stream = new InMemoryRandomAccessStream();
            await mediaCapture.CapturePhotoToStreamAsync(imgFormat, stream);
            stream.Seek(0);

            // Show the photo in an Image element
            BitmapImage bmpImage = new BitmapImage();
            await bmpImage.SetSourceAsync(stream);
            var image = new Image() { Source = bmpImage };
        }

        private void CloseCameraPopupButton_Click(object sender, RoutedEventArgs e)
        {
            CameraPopup.IsOpen = false;
            mediaCapture.Dispose();
            mediaFrameSourceGroup = null;
        }

        #endregion

        #region Image Popup

        private bool _pendingImagePreviewPopupResize = false;

        public void ShowImagePopup(BitmapImage image)
        {
            PopupImage.Source = image;

            ImagePreviewPopupRoot.Width = RootGrid.ActualWidth;
            ImagePreviewPopupRoot.Height = RootGrid.ActualHeight;

            ImagePreviewPopup.IsOpen = true;
        }

        private async void SaveImageButton_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new FileSavePicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            savePicker.FileTypeChoices.Add("PNG Image", new[] { ".png" });

            savePicker.SuggestedFileName = "arqanum_" + Guid.NewGuid().ToString("N");

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

        private void CloseImagePreviewButton_Click(object sender, RoutedEventArgs e)
        {
            ImagePreviewPopup.IsOpen = false;
            PopupImage.Source = null;
        }

        #endregion

        #region Popup Resize Handling

        private void RootGrid_LayoutUpdated(object? sender, object e)
        {
            if (_pendingImagePreviewPopupResize && ImagePreviewPopup.IsOpen)
            {
                ImagePreviewPopupRoot.Width = RootGrid.ActualWidth;
                ImagePreviewPopupRoot.Height = RootGrid.ActualHeight;
                _pendingImagePreviewPopupResize = false;
            }

            if (_pendingCameraPopupResize && CameraPopup.IsOpen)
            {
                CameraPopupRoot.Width = RootGrid.ActualWidth;
                CameraPopupRoot.Height = RootGrid.ActualHeight;
                _pendingCameraPopupResize = false;
            }

        }
        private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (ImagePreviewPopup.IsOpen)
            {
                _pendingImagePreviewPopupResize = true;
            }
            if (CameraPopup.IsOpen)
            {
                _pendingCameraPopupResize = true;
            }
        }

        #endregion

        #region Window Resize Handling

        private IntPtr _hwnd;
        private IntPtr _prevWndProc;
        private WndProc _newWndProcDelegate;
        private bool _isLoaded = false;

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

        #endregion
    }
}
