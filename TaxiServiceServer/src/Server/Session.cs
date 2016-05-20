using System;
using System.Collections.Generic;
using System.Timers;
using TaxiServiceServer.Common;
using TaxiServiceServer.Enums;
using TaxiServiceServer.Managers;
using TaxiServiceServer.Networking;
using TaxiServiceServer.Parser;
using TaxiServiceServer.Protocol;
using TaxiServiceServer.Users;

namespace TaxiServiceServer.Server
{
    public partial class Session
    {
        public TCPSocket Socket { get; private set; }
        public string Address { get; private set; }

        public User User;

        private bool _logout;
        public bool IsLogout
        {
            get { return _logout; }
            set
            {
                if (!_logout && _logout != value)
                    OnLogout();

                _logout = value;
            }
        }

        private readonly Queue<Packet> _packetQueue;

        public Session(string username, uint id, TCPSocket socket)
        {
            Socket = socket;
            Address = socket.Socket.RemoteEndPoint.ToString();

            IsLogout = false;

            _packetQueue = new Queue<Packet>();

            User = UserMgr.Instance.GetUserById(id);

            var saveSessionTimer = new Timer(Constants.SAVE_INTERVAL) { Enabled = true };
            saveSessionTimer.Elapsed += SaveSession;
        }

        public bool Update()
        {
            if (IsLogout)
                return false;

            if (_packetQueue.Count != 0)
            {
                lock (_packetQueue)
                {
                    while (_packetQueue.Count != 0)
                    {
                        var packet = _packetQueue.Dequeue();
                        Handler.SelectHandler(this, packet);
                    }
                }
            }

            return true;
        }

        public void SendPacket(Packet packet)
        {
            if (Socket == null || !Socket.Socket.Connected)
                return;

            Socket.SendPacket(packet);
        }

        public void QueuePacket(Packet packet)
        {
            lock (_packetQueue)
            {
                _packetQueue.Enqueue(packet);
            }
        }

        private void OnLogout()
        {
        }

        private void SaveSession(object source, ElapsedEventArgs e)
        {
        }
    }
}
