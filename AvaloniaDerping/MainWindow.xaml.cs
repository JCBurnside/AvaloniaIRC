
namespace AvaloniaDerping
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;

    public class MainWindow : Window
    {
        public MainWindow()
        {
            this.DataContext = new MainWindowViewModel();
            bot = new IRCBot("irc.twitch.tv", 6667, "digital_light", (writer) =>
            {
                writer.WriteLine("PASS oauth:jdayjjidikti7o3nh4aslgssiyrcbx");
                writer.Flush();
                writer.WriteLine("NICK digital_light");
                writer.Flush();
            });
#pragma warning disable GU0011,CS4014
            this.InitializeComponent();
#pragma warning disable GU0011,CS4014
            this.AttachDevTools();
        }

        private IRCBot bot;
        public static string output = " ";
        private async Task InitializeComponent()
        {

            await bot.Start().ConfigureAwait(true);
            await bot.WaitForStart().ConfigureAwait(false);


            bot.OnMessageRecived += (sender, args) =>
            {
                ((MainWindowViewModel)DataContext).Chat += String.Join(" ", args.line) + Environment.NewLine;
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
    public class MainWindowViewModel : INotifyPropertyChanged
    {

        private string chat;

        public string Chat
        {
            get => chat;
            set
            {
                if (value != chat)
                {
                    chat = value;
                    OnPropertyChanged();
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
