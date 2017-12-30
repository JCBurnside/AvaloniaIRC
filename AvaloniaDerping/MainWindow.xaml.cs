using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace AvaloniaDerping
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            bot = new IRCBot("irc.twitch.tv", 6667, "<channel>", (writer) =>
             {
                 writer.WriteLine("PASS oauth:<oauth>");//tyoxiymgeednn4keov4023lae0zh6v
                 writer.Flush();
                 writer.WriteLine("NICK <nick>");
                 writer.Flush();
             });
            this.InitializeComponent();
            this.AttachDevTools();
        }

        private IRCBot bot;
        public static string output=" ";
        public IObservable<string> test;
        private void InitializeComponent()
        {
            
            AvaloniaXamlLoader.Load(this);
            bot.Start();
            bot.WaitForStart();
            test = ReadLines(bot.stream).ToObservable();
                    
            bot.OnMessageRecived += (sender,args)=> {
                output += String.Join(" ", args.line)+Environment.NewLine;
            };
        }
        
        IEnumerable<string> ReadLines(Stream stream){
            using (StreamReader reader = new StreamReader(stream)){
                while(!reader.EndOfStream){
                    yield return reader.ReadLine();
                }
            }
        }
    }
}
