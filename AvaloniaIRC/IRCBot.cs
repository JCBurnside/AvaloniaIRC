
namespace AvaloniaIRC
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

#pragma warning disable INPC001
    public class IRCBot : IDisposable
    {
        public static int RetryLimit { get; set; } = 3;
        private readonly string server;
        private readonly int port;
        private readonly string channel;
        private TcpClient irc;

        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;

        private readonly Action<StreamWriter> login;
        public Action<StreamWriter, IEnumerable<string>> pong;
        private bool isStarted = false;
        private bool running = false;

        public delegate void MessageRecieved(object sender, MessageRecievedEventArgs args);
        public event MessageRecieved OnMessageRecieved;

        public delegate void Connected(object sender, ConnectedEventArgs args);
        public event Connected OnConnected;


        internal IRCBot(string server, uint port, string channel, Action<StreamWriter> login, Action<StreamWriter, IEnumerable<string>> pong = null)
        {
            this.server = server; this.port = (int)port; this.channel = channel; this.login = login; this.pong = pong ?? DefaultPong;
        }

        public void Stop()
        {
            running = false;
        }

        public async Task Start(CancellationToken token = default)
        {
            bool retry = false;
            int retryCount = 0;
            do
            {
                try
                {
                    irc = new TcpClient(AddressFamily.InterNetwork);
                    await irc.ConnectAsync(server, port);
                    token.ThrowIfCancellationRequested();
                    stream = irc.GetStream();
                    reader = new StreamReader(stream);
                    writer = new StreamWriter(stream);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Thread.Sleep(5000);
                    retry = ++retryCount <= RetryLimit;
                }
            } while (retry);
            running = true;
            login(writer);
            writer.WriteLine($"JOIN #{channel}");
            writer.Flush();
            OnConnected?.Invoke(this, new ConnectedEventArgs(server, channel));
            isStarted = true;
            while (running)
            {
                token.ThrowIfCancellationRequested();
                string inputLine;
                while ((inputLine = await reader.ReadLineAsync()) != null)
                {
                    string[] splits = inputLine.Split(' ');
                    if (splits[0] == "PING")
                    {
                        pong(writer, splits.Skip(1));
                    }
                    else
                    {
                        OnMessageRecieved?.Invoke(this, new MessageRecievedEventArgs(String.Join(" ", splits), channel));
                    }
                }
            }
        }


        private static void DefaultPong(StreamWriter writer, IEnumerable<string> args)
        {
            writer.WriteLine(args.First());
            writer.Flush();
        }

        public void WriteLine(object o)
        {
            if (!isStarted)
            {
                throw new NotStartedException();
            }
            writer.WriteLine(o);
            writer.Flush();
        }

        public void WriteLine(string format, params object[] o)
        {
            if (!isStarted)
            {
                throw new NotStartedException();
            }
            string output = format.Replace("{channel}", '#' + channel);
            writer.WriteLine(output, o);
            writer.Flush();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    writer.Dispose();
                    reader.Dispose();
                    stream.Dispose();
                    irc.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~IRCBot() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
#pragma warning restore INPC001

    public class ConnectedEventArgs
    {
        public string Channel { get; private set; }
        public string Server { get; private set; }
        public ConnectedEventArgs(string server, string channel)
        {
            Channel = channel; Server = server;
        }
    }

    internal class NotStartedException : Exception
    {
        public NotStartedException() : base("IRC not started") { }
        public NotStartedException(string message) : base(message) { }
        public NotStartedException(string message, Exception inner) : base(message, inner) { }
    }

    public class MessageRecievedEventArgs : EventArgs
    {
        public MessageRecievedEventArgs(string line, string channel)
        {
            Line = line;
            Channel = channel;
        }

        public string Line { get; private set; }
        public string Channel { get; private set; }
    }
}
