using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Windows;
using AutoJx2.Interfaces;
using AutoJx2.Services;

namespace AutoJx2
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider = null!;
        
        public ServiceProvider ServiceProvider => _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Cấu hình Serilog
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "autojx2-.log");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
                .CreateLogger();

            // Cấu hình Dependency Injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Khởi tạo MainWindow thủ công với DI
            var logger = _serviceProvider.GetRequiredService<ILogger<MainWindow>>();
            var credentialManager = _serviceProvider.GetRequiredService<ICredentialManager>();
            var gameAutomation = _serviceProvider.GetRequiredService<IGameAutomation>();
            var configurationManager = _serviceProvider.GetRequiredService<IConfigurationManager>();
            var gameProcessManager = _serviceProvider.GetRequiredService<IGameProcessManager>();
            
            var mainWindow = new MainWindow(logger, credentialManager, gameAutomation, configurationManager, gameProcessManager);
            mainWindow.Show();
            
            // Đặt MainWindow làm MainWindow của ứng dụng
            MainWindow = mainWindow;
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Đăng ký các service
            services.AddLogging(builder =>
            {
                builder.AddSerilog(dispose: true);
            });

            services.AddSingleton<ICredentialManager, CredentialManager>();
            services.AddSingleton<IGameAutomation, GameAutomation>();
            services.AddSingleton<IConfigurationManager, ConfigurationManager>();
            services.AddSingleton<IGameProcessManager, GameProcessManager>();
            // MainWindow sẽ được tạo thủ công
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
