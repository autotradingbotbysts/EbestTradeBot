using ControlzEx.Theming;
using EbestTradeBot.Client.Services.Log;
using EbestTradeBot.Client.Services.OpenApi;
using EbestTradeBot.Client.Services.Trade;
using EbestTradeBot.Client.Services.XingApi;
using EbestTradeBot.Client.ViewModels;
using EbestTradeBot.Client.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Prism.Ioc;
using System.IO;
using System.Windows;

namespace EbestTradeBot.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private IConfiguration _configuration;

        protected override Window CreateShell()
        {
            SetTheme("Red");
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            _configuration = builder.Build();

            containerRegistry.RegisterInstance(_configuration);

            var services = new ServiceCollection();
            services.Configure<XingApiOptions>(_configuration.GetSection("XingApiOptions"));
            services.Configure<OpenApiOptions>(_configuration.GetSection("OpenApiOptions"));
            services.Configure<DefaultOptions>(_configuration.GetSection("DefaultOptions"));

            var serviceProvider = services.BuildServiceProvider();
            var xingApiOptionsMonitor = serviceProvider.GetService<IOptionsMonitor<XingApiOptions>>();
            var openApiOptionsMonitor = serviceProvider.GetService<IOptionsMonitor<OpenApiOptions>>();
            var defaultOptionsMonitor = serviceProvider.GetService<IOptionsMonitor<DefaultOptions>>();

            containerRegistry.RegisterInstance(xingApiOptionsMonitor);
            containerRegistry.RegisterInstance(openApiOptionsMonitor);
            containerRegistry.RegisterInstance(defaultOptionsMonitor);

            containerRegistry.RegisterSingleton<IXingApiService, XingApiService>();
            containerRegistry.RegisterSingleton<IOpenApiService, OpenApiService>();
            containerRegistry.RegisterSingleton<ITradeService, TradeService>();
            containerRegistry.RegisterSingleton<ILogService, LogService>();

            containerRegistry.RegisterForNavigation<MainWindow, MainWindowViewModel>();
            containerRegistry.RegisterForNavigation<ConfigurationWindow, ConfigurationViewModel>();
        }

        public void SetTheme(string color)
        {
            SetDarkMode(color);
        }

        private void SetDarkMode(string color)
        {
            ThemeManager.Current.ChangeTheme(this, $"Dark.{color}");
        }
    }
}
