
namespace AvaloniaDerping
{
    using System;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Diagnostics;
    using Avalonia.Logging.Serilog;
    using Avalonia.Themes.Default;
    using Avalonia.Markup.Xaml;
    using Serilog;

    class App : Application
    {

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            base.Initialize();
        }

        static void Main(string[] args)
        {
            InitializeLogging();
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .Start<MainWindow>();
        }

        public static void AttachDevTools(Window window)
        {
#if DEBUG
#pragma warning disable GU0011   
            DevTools.Attach(window);
#pragma warning restore GU0011
#endif
        }

        private static void InitializeLogging()
        {
#if DEBUG
            SerilogLogger.Initialize(new LoggerConfiguration()
                .MinimumLevel.Warning()
                .WriteTo.Trace(outputTemplate: "{Area}: {Message}")
                .CreateLogger());
#endif
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug();
    }
}
