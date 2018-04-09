
namespace AvaloniaIRC
{
    using System;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Diagnostics;
    using Avalonia.Logging.Serilog;
    using Avalonia.Themes.Default;
    using Avalonia.Markup.Xaml;
    using Serilog;

    public class App : Application
    {

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static void AttachDevTools(Window window)
        {
#if DEBUG
#pragma warning disable GU0011   
            DevTools.Attach(window);
#pragma warning restore GU0011
#endif
        }


        
    }
}
