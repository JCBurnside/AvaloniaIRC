using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Reactive.Linq;
using ReactiveUI;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace AvaloniaDerping
{
    public class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            bot = new IRCBot("irc.twitch.tv", 6667, "<channel>", (writer) =>
             {
                 writer.WriteLine("PASS oauth:<oauth>");
                 writer.Flush();
                 writer.WriteLine("NICK <nick>");
                 writer.Flush();
             });
            this.InitializeComponent();
            this.AttachDevTools();
        }

        private IRCBot bot;
        public static string output = " ";
        private async Task InitializeComponent()
        {

            await bot.Start().ConfigureAwait(true);
            await bot.WaitForStart().ConfigureAwait(false);

            this.DataContext = new ViewModel
            {
                test = Observable.Using(() => bot.reader, reader => Observable.FromAsync(reader.ReadLineAsync).Repeat().TakeWhile(s => s != null))
            };
            bot.OnMessageRecived += (sender, args) =>
            {
                output += String.Join(" ", args.line) + Environment.NewLine;
            };
            AvaloniaXamlLoader.Load(this);
        }

        IEnumerable<string> ReadLines(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    yield return reader.ReadLine();
                }
            }
        }

    }
    public class ViewModel : ReactiveObject
    {

        private IObservable<string> _test;
        public IObservable<string> test
        {
            get => _test; 
            set => this.RaiseAndSetIfChanged(ref _test, value);
        }

       
    }
}
