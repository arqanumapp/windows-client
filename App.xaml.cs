using Arqanum.CoreImplementations;
using Arqanum.Pages;
using Arqanum.Services;
using Arqanum.ViewModels;
using Arqanum.Windows;
using ArqanumCore;
using ArqanumCore.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using System;
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

            var mainWindow = Services.GetRequiredService<MainWindow>();
            var themeService = Services.GetRequiredService<ThemeService>();

            Window = mainWindow;
            Window.Activate();
            themeService.RestoreTheme();
        }

    }
}
