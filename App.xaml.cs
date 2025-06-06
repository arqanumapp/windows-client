using Arqanum.CoreImplementations;
using Arqanum.Pages;
using Arqanum.ViewModels;
using ArqanumCore;
using ArqanumCore.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using System;
namespace Arqanum
{
    public partial class App : Application
    {
        public static IServiceProvider Services;
        private IHost _host;

        private Window? _window;

        public App()
        {
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

                services.AddTransient<WelcomePage>();

                services.AddTransient<CreateAccountPage>();

                services.AddTransient<CreateAccountViewModel>();

                services.AddSingleton<MainWindow>();
            })
            .Build();

            Services = _host.Services;
            var mainWindow = Services.GetRequiredService<MainWindow>();
            _window = mainWindow;
            _window.Activate();
        }

    }
}
