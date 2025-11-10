using DeskAssistant.Models;
using DeskAssistant.SecureService;
using DeskAssistant.Services;
using DeskAssistant.ViewModels;
using DeskAssistant.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using NLog;
using NLog.Extensions.Logging;
using ILogger = NLog.ILogger;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DeskAssistant
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost _host;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        public static Window MainWindow = new MainWindow();


        public ConnectionSettings ConnectionSettings { get; set; }
        public EmailSettings EmailSettings { get; set; }
        public EncryptionSettings EncryptionSettings { get; set; }
        public static UIElement? AppTitlebar { get; set; }
        private UIElement? _shell = null;
        public EmailService _emailService;
        public EncryptionHelper _encryptionHelper;


        public static T GetService<T>()
        where T : class
        {
            if ((App.Current as App)!._host.Services.GetService(typeof(T)) is not T service)
            {
                throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
            }

            return service;
        }


        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            try
            {
                _logger.Trace("Assistant start to load");

                this.InitializeComponent();

                var hostBuilder = Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        var env = context.HostingEnvironment;
                        _logger.Info($"App start with [{env.EnvironmentName}] environment");

                        //config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: true)
                        //.AddEnvironmentVariables();
                        config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "AppSettings/navigationSettings.json"));
                        config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "AppSettings/emailServiceSettings.json"));
                        config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "AppSettings/encryptionSettings.json"));

                        // add UserSecrets when Development
                        if (env.IsDevelopment())
                        {
                            config.AddUserSecrets<App>();
                        }

                        config.AddEnvironmentVariables();
                    })
                    .ConfigureServices((context, services) =>
                    {
                        var configuration = context.Configuration;
                        
                        services.AddSingleton<IActivationService, ActivationService>();

                        services.AddSingleton(this);
                        services.AddSingleton<ShellViewModel>();

                        services.AddScoped<CalendarPage>();
                        services.AddScoped<CalendarViewModel>();

                        services.AddTransient<ShellPage>();
                        services.AddTransient<BirthdayTrackerPage>();
                        services.AddTransient<BirthdayTrackerViewModel>();
                        services.AddTransient<NavigationPages>();
                        services.AddTransient<EmailService>();
                        services.AddTransient<EncryptionHelper>();

                        services.Configure<ConnectionSettings>(configuration.GetSection(nameof(ConnectionSettings)));
                        services.Configure<EmailSettings>(configuration.GetSection(nameof(EmailSettings)));
                        services.Configure<EncryptionSettings>(configuration.GetSection(nameof(EncryptionSettings)));
                        services.Configure<NavigationPages>(context.Configuration.GetSection("NavigationPages"));
                    })
                    .ConfigureLogging((context, logging) =>
                    {
                        logging.ClearProviders();
                        logging.AddNLog();
                    });
                                
                _host = hostBuilder.Build();

                _logger.Trace("App is loaded");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected async override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            await App.GetService<IActivationService>().ActivateAsync(args);
        }

        //private void SaveWindowState()
        //{
        //    _windowSizeSelectorService.SaveWindowSize(_localSettingsService);
        //}

        //private void SizeWindowChanged()
        //{
        //    _windowSizeSelectorService.SaveSizeWhenResizing(_localSettingsService);
        //}
    }
}
