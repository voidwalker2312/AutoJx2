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
        private ServiceProvider _serviceProvider;
        
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

            // Khởi tạo MainWindow với DI
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
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
            services.AddSingleton<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
