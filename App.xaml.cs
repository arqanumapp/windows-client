using Arqanum.CoreImplementations;
using Arqanum.Pages;
using Arqanum.Services;
using Arqanum.ViewModels;
using Arqanum.Windows;
using ArqanumCore;
using ArqanumCore.Interfaces;
using ArqanumCore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Arqanum
{
    public partial class App : Application
    {
        public static IServiceProvider Services;

        private IHost _host;

        public static Window Window;

        private Mutex _appMutex;

        private const string MutexName = "Arqanum_SingleInstance_Mutex";

        private TrayIconManager _trayIconManager;

        public App()
        {
            bool createdNew;
            _appMutex = new Mutex(true, MutexName, out createdNew);

            if (!createdNew)
            {
                Environment.Exit(0);
            }
            InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddTransient<IDbPasswordProvider, DbPasswordProvider>();

                services.AddTransient<ICaptchaProvider, CaptchaProvider>();

                services.AddTransient<IShowNotyficationService, ShowNotyficationService>();

                services.AddTransient<IFileCacheService, FileCacheService>();

                services.AddHttpClient<FileCacheService>();

                services.AddArqanumCore();

                services.AddSingleton<ThemeService>();

                services.AddTransient<WelcomePage>();

                services.AddTransient<CreateAccountPage>();

                services.AddTransient<CreateAccountViewModel>();

                services.AddTransient<CaptchaWindow>();

                services.AddSingleton<MainWindow>();
            })
            .Build();

            Services = _host.Services;

            var themeService = Services.GetRequiredService<ThemeService>();
            var mainWindow = Services.GetRequiredService<MainWindow>();

            Window = mainWindow;

            Window.Closed += OnWindowClosed;
            Window.ExtendsContentIntoTitleBar = true;

            Window.Activate();

            _trayIconManager = new TrayIconManager(mainWindow, ExitApplication);

            themeService.RestoreTheme();
        }

        private void OnWindowClosed(object sender, WindowEventArgs args)
        {
            args.Handled = true;

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(Window);
            ShowWindow(hwnd, 0); 
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public void ExitApplication()
        {
            _trayIconManager?.Dispose();
            _host?.Dispose();
            Environment.Exit(0);
        }
    }
}
