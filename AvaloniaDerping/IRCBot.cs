
namespace AvaloniaDerping
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
    internal class IRCBot : IDisposable
    {
        internal static int RetryLimit = 3;
        private readonly string server;
        private readonly int port;
        private readonly string channel;
        private TcpClient irc;

        private NetworkStream _stream;
        internal NetworkStream stream
        {
            get
            {
                if (isStarted)
                    return _stream;
                throw new NotStartedException();
            }
            private set
            {
                _stream = value;
            }
        }

        private StreamReader _reader;
        internal StreamReader reader
        {
            get
            {
                if (isStarted)
                    return _reader;
                throw new NotStartedException();
            }
            private set
            {
                _reader = value;
            }
        }

        internal async Task WaitForStart()
        {
            while (!isStarted)
            {
                await Task.Delay(1000).ConfigureAwait(false);
            }
        }

        private StreamWriter _writer;
        internal StreamWriter writer
        {
            get
            {
                if (isStarted)
                    return _writer;
                throw new NotStartedException();
            }
            private set
            {
                _writer = value;
            }
        }

        private readonly Action<StreamWriter> login;
        public Action<StreamWriter, IEnumerable<string>> pong;
        private TextWriter output;
        private bool isStarted = false;

        public delegate void MessageRecived(object sender, MessageRecievedEventArgs args);
        public event MessageRecived OnMessageRecived;

        internal IRCBot(string server, uint port, string channel, Action<StreamWriter> login, Action<StreamWriter, IEnumerable<string>> pong = null, TextWriter output = null)
        {
            this.server = server; this.port = (int)port; this.channel = channel; this.login = login; this.pong = pong ?? DefaultPong; this.output = output ?? Console.Out;
        }
        public async Task Start()
        {
            bool retry = false;
            int retryCount = 0;
            do
            {
                try
                {
                    irc = new TcpClient(AddressFamily.InterNetwork);
                    await irc.ConnectAsync(server, port);

                    stream = irc.GetStream();
                    reader = new StreamReader(_stream);
                    writer = new StreamWriter(_stream);
                    login(_writer);
                    _writer.WriteLine($"JOIN #{channel}");
                    _writer.Flush();
                    isStarted = true;
                    while (true)
                    {
                        string inputLine;
                        while ((inputLine = await reader.ReadLineAsync()) != null)
                        {
                            output.WriteLine(inputLine);
                            string[] split = inputLine.Split(' ');
                            if (split[0] == "PING")
                            {
                                pong(writer, split.Skip(1));
                            }
                            else
                            {
                                OnMessageRecived?.Invoke(this, new MessageRecievedEventArgs
                                {
                                    line = split,
                                    channel = channel
                                });
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Thread.Sleep(5000);
                    retry = ++retryCount <= RetryLimit;
                }
            } while (retry);
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

        public void WriteLine(string format, object o)
        {
            if (!isStarted)
            {
                throw new NotStartedException();
            }
            string output = new Regex("{{1}.{1,}}{1}").Replace(format, m =>
            {

                string replaceWith = "";
                switch (m.Value)
                {
                    case "{channel}":
                        replaceWith = '#' + channel;
                        break;
                    case "{0}":
                        replaceWith = o.ToString();
                        break;
                }
                return replaceWith;
            });


            writer.WriteLine(output);
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

    internal class NotStartedException : Exception
    {
        public NotStartedException() : base("IRC not started") { }
        public NotStartedException(string message) : base(message) { }
        public NotStartedException(string message, Exception inner) : base(message, inner) { }
    }
    public class MessageRecievedEventArgs : EventArgs
    {
        public string[] line;
        public string channel;
    }
}
