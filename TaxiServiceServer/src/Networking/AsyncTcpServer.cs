using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace TaxiServiceServer.Networking
{
    public class AsyncTcpServer : IDisposable
    {
        private TcpListener _listener;
        private List<TCPSocket> _clients;

        private static object syncRoot = new object();
        private static AsyncTcpServer _instance;

        private Timer _updateClients = new Timer(10000);
        public static AsyncTcpServer Instanse
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                        {
                            var addr = new IPAddress(new byte[] { 0, 0, 0, 0 });
                            _instance = new AsyncTcpServer(addr, 30000);
                        }
                    }
                }
                return _instance;
            }
        }

        public AsyncTcpServer(IPAddress localaddr, int port) : this()
        {
            _updateClients.Enabled = true;
            _updateClients.Elapsed += UpdateClientsTimer;

            _listener = new TcpListener(localaddr, port);
        }

        private AsyncTcpServer()
        {
            _updateClients.Enabled = true;
            _updateClients.Elapsed += UpdateClientsTimer;

            Encoding = Encoding.Default;
            _clients = new List<TCPSocket>();
        }

        public Encoding Encoding { get; set; }

        public void Start()
        {
            Console.WriteLine($"Listener started {_listener.LocalEndpoint.ToString()}");
            _listener.Start();
            _listener.BeginAcceptTcpClient(AcceptSocketCallback, null);
        }

        public void Stop()
        {
            _listener.Stop();
            lock (_clients)
            {
                foreach (var client in this._clients)
                    client.Socket.Shutdown(SocketShutdown.Both);

                _clients.Clear();
            }
        }

        public void Send(byte[] bytes)
        {
            lock (_clients)
            {
                foreach (var client in _clients)
                {
                    Send(client, bytes);
                }
            }
        }

        public void Send(TCPSocket tcpSocket, byte[] bytes)
        {
            try
            {
                tcpSocket.Socket.BeginSend(bytes, 0, bytes.Length, 0, SendCallback, tcpSocket);
            }
            catch (Exception)
            {
                lock (_clients)
                {
                    _clients.RemoveAll(x => x.Socket == tcpSocket.Socket);
                }
            }
        }

        private void SendCallback(IAsyncResult result)
        {
            var socket = result.AsyncState as TCPSocket;
            if (socket == null || socket.IsClosed)
                return;

            socket.Socket.EndSend(result);
        }

        private void AcceptSocketCallback(IAsyncResult result)
        {
            var socket = _listener.EndAcceptSocket(result);
            var client = new TCPSocket(socket);
            lock (_clients)
                _clients.Add(client);

            socket.BeginReceive(client.Buffer, 0, client.Buffer.Length, 0, ReadCallback, client);
            _listener.BeginAcceptTcpClient(AcceptSocketCallback, null);
        }

        private void ReadCallback(IAsyncResult result)
        {
            var client = result.AsyncState as TCPSocket;
            if (client == null)
                return;

            // Client disconnected
            if (client.IsClosed || !client.Socket.Connected)
                return;

            int bytesRead = 0;
            try
            {
                bytesRead = client.Socket.EndReceive(result);
                client.Receive(bytesRead);

                Array.Clear(client.Buffer, 0, client.Buffer.Length);
                client.Socket.BeginReceive(client.Buffer, 0, client.Buffer.Length, 0, ReadCallback, client);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        private void UpdateClientsTimer(object source, ElapsedEventArgs e)
        {
            lock (_clients)
            {
                for (var i = 0; i < _clients.ToArray().Length; ++i)
                {
                    var client = _clients.ToArray()[i];
                    if (!client.Socket.Connected || client.IsClosed)
                        _clients.Remove(client);
                }
            }
        }

        public void Dispose()
        {
            _updateClients.Dispose();
        }
    }
}
