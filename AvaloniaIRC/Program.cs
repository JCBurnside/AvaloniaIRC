namespace AvaloniaIRC
{
    using Avalonia;
    using Avalonia.Logging.Serilog;
    using AvaloniaIRC;
    using System;
    using System.Collections.Generic;
    using System.Text;

    class Program
    {
        static void Main(string[] args)
        {
            BuildAvaloniaApp().Start<MainWindow>();
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug();
    }
}
