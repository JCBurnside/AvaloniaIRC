namespace AvaloniaIRC.Controls
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;
    using AvaloniaIRC.ViewModels;
    using System;
    using System.ComponentModel;
    using System.Linq;

    public class ScrollingText : UserControl
    {
        public ScrollingText()
        {
            DataContext = new ScrollingTextViewModel();
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
