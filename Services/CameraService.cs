using System;

namespace Arqanum.Services
{
    public static class CameraService
    {
        private static MainWindow _mainWindow;

        public static void Initialize(MainWindow window)
        {
            _mainWindow = window;
        }

        public static void OpenPhotoCamera()
        {
            _mainWindow?.OpenCameraPopup();
        }
    }
}
