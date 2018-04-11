namespace AvaloniaIRC
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;
    using System;
    using System.ComponentModel;
    using System.Linq;

    public class ScrollingText : UserControl
    {
        public ScrollingText()
        {
            DataContext = this;
            this.InitializeComponent();
        }


        private int maxLines;
        public int MaxLines
        {
            get => maxLines;
            set
            {
#pragma warning disable GU0011,INPC003 // Don't ignore the return value.,Notify when property changes.
                SetAndRaise(MaxLinesProperty, ref maxLines, value);
#pragma warning restore INPC003,GU0011 // Notify when property changes.,Don't ignore the return value.
                string prevText = text;
                RaisePropertyChanged(TextProperty, prevText, Text);
            }
        }

        private int maxCharacters;
        public int MaxCharacters
        {
            get => maxCharacters;
            set
            {
#pragma warning disable GU0011, INPC003 // Don't ignore the return value.,Notify when property changes.
                SetAndRaise(MaxCharactersProperty, ref maxCharacters, value);
#pragma warning restore INPC003, GU0011 // Notify when property changes.,Don't ignore the return value.
                string prevText = text;
                RaisePropertyChanged(TextProperty, prevText, Text);
            }
        }

        private string text;
        public string Text
        {
            get
            {
                int lineCount;
                if ((lineCount = text.Count(c => c == '\n')) >= MaxLines)
                {
                    text = string.Join(Environment.NewLine, Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Skip(MaxLines - lineCount));
                }
                while (text.Length > MaxCharacters)
                {
                    text = string.Join(Environment.NewLine, Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Skip(1));
                }
                return text;
            }
#pragma warning disable INPC003 // Notify when property changes.
            set => SetAndRaise(TextProperty, ref text, value);
#pragma warning restore INPC003 // Notify when property changes.
        }


        public static readonly AvaloniaProperty<int> MaxLinesProperty = AvaloniaProperty.Register<ScrollingText, int>(nameof(MaxLines), 100);
        public static readonly AvaloniaProperty<string> TextProperty = AvaloniaProperty.Register<ScrollingText, string>(nameof(Text));
        public static readonly AvaloniaProperty<int> MaxCharactersProperty = AvaloniaProperty.Register<ScrollingText, int>(nameof(MaxCharacters), 100_000);
        

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
