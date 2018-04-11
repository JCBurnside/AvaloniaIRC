using ReactiveUI;
using System;
using System.Linq;

namespace AvaloniaIRC.ViewModels
{
    class ScrollingTextViewModel : ReactiveObject
    {
        private int maxLines;
        public int MaxLines
        {
            get => maxLines;
            set
            {
                this.RaiseAndSetIfChanged(ref maxLines, value);

                this.RaisePropertyChanged(nameof(Text));
            }
        }

        private int maxCharacters;
        public int MaxCharacters
        {
            get => maxCharacters;
            set
            {
                this.RaiseAndSetIfChanged(ref maxCharacters, value);

                this.RaisePropertyChanged(nameof(Text));
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

            set => this.RaiseAndSetIfChanged(ref text, value);
        }
    }
}
