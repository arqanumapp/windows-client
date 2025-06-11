using System;

namespace Arqanum.Services
{
    public static class ImagePreviewService
    {
        private static MainWindow _mainWindow;

        public static void Initialize(MainWindow window)
        {
            _mainWindow = window;
        }

        public static void Show(string imageUrl)
        {
            _mainWindow?.ShowImagePopup(new(new Uri(imageUrl)));
        }
    }
}

