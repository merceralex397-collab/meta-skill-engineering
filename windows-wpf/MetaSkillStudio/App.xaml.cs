using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MetaSkillStudio.Services;
using MetaSkillStudio.Services.Interfaces;
using MetaSkillStudio.ViewModels;
using MetaSkillStudio.Views;

namespace MetaSkillStudio
{
    /// <summary>
    /// Application entry point with real Microsoft.Extensions.DependencyInjection container.
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private ServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Setup dependency injection container
            var services = new ServiceCollection();
            ConfigureServices(services);
            
            // Build service provider
            _serviceProvider = services.BuildServiceProvider();
            
            // Resolve and show main window
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        /// <summary>
        /// Configures all services for dependency injection.
        /// </summary>
        private void ConfigureServices(IServiceCollection services)
        {
            // Environment and Configuration services (infrastructure)
            services.AddSingleton<IEnvironmentProvider, EnvironmentProvider>();
            services.AddSingleton<IConfigurationStorage, ConfigurationStorage>();
            services.AddSingleton<IDispatcher, DispatcherService>();
            
            // Business services
            services.AddSingleton<IPythonRuntimeService, PythonRuntimeService>();
            
            // DialogService needs special registration to inject service provider
            services.AddSingleton<IDialogService>(provider => 
            {
                // Create a scope-aware provider for dialogs
                return new DialogService(provider);
            });
            
            // ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<AnalyticsViewModel>();
            
            // Views
            services.AddSingleton<MainWindow>(provider =>
            {
                var viewModel = provider.GetRequiredService<MainViewModel>();
                return new MainWindow { DataContext = viewModel };
            });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
