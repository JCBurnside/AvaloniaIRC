
namespace AvaloniaIRC
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Diagnostics.ViewModels;
    using Avalonia.Markup.Xaml;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reactive;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;

    public class MainWindow : Window
    {
        public MainWindow()
        {

            bot = new IRCBot("irc.twitch.tv", 6667, "inthelittlewood", (writer) =>
            {
                writer.WriteLine("PASS oauth:jdayjjidikti7o3nh4aslgssiyrcbx");
                writer.Flush();
                writer.WriteLine("NICK digital_light");
                writer.Flush();
            });
            this.DataContext = new MainWindowViewModel(bot.WriteLine);
            this.InitializeComponent();
            this.AttachDevTools();
        }

        private IRCBot bot;
        private readonly Regex incoming = new Regex("^:.+!.+@.+ PRIVMSG #.+ :");
        private void InitializeComponent()
        {

            AvaloniaXamlLoader.Load(this);
            if (DataContext is MainWindowViewModel context)
            {
                bot.OnConnected += (sender, args) =>
                {
                    context.HasStarted = true;
                    context.Chat = "Connected" + Environment.NewLine;
                };

                bot.OnMessageRecieved += (sender, args) =>
                {
                    if (incoming.IsMatch(args.Line))
                    {
                        string output = $"{args.Line.Substring(1, args.Line.IndexOf('!') - 1)}>{incoming.Replace(args.Line, String.Empty)}";
                        context.Chat += output + Environment.NewLine;
                        while (context.Chat.Length > 100000 || context.Chat.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Count() > 300)
                        {
                            context.Chat = context.Chat.Substring(context.Chat.IndexOf(Environment.NewLine) + Environment.NewLine.Length);
                        }
                    }
                };
            }
#pragma warning disable GU0011
            bot.Start().ConfigureAwait(true);
#pragma warning restore GU0011
        }
    }

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel(Action<string> sendMessage)
        {
            this.sendMessage = () =>
            {
                sendMessage?.Invoke(Message);
                Message = null;
            };
        }

        private string chat = "Waiting for connection";
        private bool hasStarted;

        public string Chat
        {
            get => chat;
            set
            {

                if (value == chat)
                {
                    return;
                }

                chat = value;
                this.OnPropertyChanged();

            }
        }

        public bool HasStarted
        {
            get => this.hasStarted;
            set
            {
                if (value == this.hasStarted)
                {
                    return;
                }

                this.hasStarted = value;
                this.OnPropertyChanged();
            }
        }

        private Action sendMessage { get; }

        public void SendMessage()
        {
            sendMessage?.Invoke();
        }

#pragma warning disable INPC002 // Mutable public property should notify.
        public string Message { get; set; }
#pragma warning restore INPC002 // Mutable public property should notify.

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
