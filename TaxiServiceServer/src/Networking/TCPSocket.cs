using System;
using System.Net.Sockets;
using TaxiServiceServer.Common;
using TaxiServiceServer.Common.Hashers;
using TaxiServiceServer.Database;
using TaxiServiceServer.Enums;
using TaxiServiceServer.Managers;
using TaxiServiceServer.Protocol;
using TaxiServiceServer.Protocol.ResponseCode;

namespace TaxiServiceServer.Networking
{
    public class TCPSocket
    {
        public Socket Socket { get; private set; }
        public byte[] Buffer { get; private set; }
        public bool IsClosed { get; set; }
        private Server.Session _session;

        public TCPSocket(Socket socket)
        {
            if (socket == null)
                throw new ArgumentNullException("socket");

            Socket = socket;
            Buffer = new byte[256];
            IsClosed = false;
            _session = null;
        }

        ~TCPSocket()
        {
            Socket.Close();
        }

        public void Receive(int bytes)
        {
            if (bytes == 0)
                return;

            byte[] temp = Buffer;
            Array.Resize(ref temp, bytes);
            byte[] decryptBytes = Cryptography.Decrypt(temp);
            var packet = ParsePacket(decryptBytes);
            Array.Clear(Buffer, 0, Buffer.Length);
            if (packet == null)
                return;

            Console.WriteLine($"Received packet {packet.Opcode} from client {Socket.RemoteEndPoint}");
            PacketReader(packet);
        }

        public void SendPacket(Packet packet)
        {
            WriteHeader(packet);
            var encryptBytes = Cryptography.Encrypt(packet.ToArray());
            Console.WriteLine($"Send packet {packet.Opcode} to client {Socket.RemoteEndPoint}");
            AsyncTcpServer.Instanse.Send(this, encryptBytes);
        }

        public void Close()
        {
            Socket.Close();
        }

        private Packet ParsePacket(byte[] bytes)
        {
            var buffer = new ByteBuffer(bytes);
            Opcode opcode = (Opcode)buffer.ReadUInt16();
            int size = buffer.ReadUInt16();

            if (opcode >= Opcode.MAX_OPCODE)
                return null;

            if (size > 1000)
                return null;

            var packet = new Packet(opcode);
            packet.WriteBytes(buffer.GetBytes(size));
            packet.ResetPos();
            return packet;
        }

        private void WriteHeader(Packet packet)
        {
            byte[] bytes = packet.ToArray();

            packet.Clear();
            packet.WriteUInt16((ushort) packet.Opcode);
            packet.WriteUInt16((ushort) bytes.Length);
            packet.WriteBytes(bytes);
        }

        private bool PacketReader(Packet packet)
        {
            if (packet.Opcode >= Opcode.MAX_OPCODE)
            {
                Console.WriteLine($"PacketReader: Unknown opcode {packet.Opcode} from client {Socket.RemoteEndPoint}");
                return true;
            }

            switch (packet.Opcode)
            {
                case Opcode.CMSG_AUTH:
                {
                    HandleAuth(packet);
                    break;
                }
                case Opcode.CMSG_REGISTRATION:
                {
                    HandleRegistration(packet);
                    break;
                }
                case Opcode.CMSG_LOGOUT:
                {
                    _session.IsLogout = true;
                    _session = null;
                    break;
                }
                case Opcode.CMSG_DISCONNECTED:
                {
                    IsClosed = true;
                    break;
                }
                default:
                {
                    if (_session == null)
                    {
                        Console.WriteLine($"PacketReader: Session is null for packet {packet.Opcode} from client {Socket.RemoteEndPoint}");
                        return false;
                    }

                    if (_session.IsLogout)
                        return false;

                    _session.QueuePacket(packet);
                    break;
                }
            }

            return true;
        }

        private void HandleAuth(Packet packet)
        {
            var username = packet.ReadUTF8String();
            var password = packet.ReadUTF8String();

            AuthResponse responseCode = AuthResponse.AUTH_RESPONSE_SUCCESS;

            var mysql = MySQL.Instance();
            var uName = string.Empty;
            uint accountId = 0;
            UserType type = UserType.USER_TYPE_UNKNOWN;
            using (var reader = mysql.Execute($"SELECT `Id`, `type`, `username` FROM `users` WHERE `username` = '{username}' AND `password` = '{MD5Hash.Get(username + ":" + password)}'"))
            {
                if (reader == null || _session != null)
                    responseCode = AuthResponse.AUTH_RESPONSE_UNKNOWN_ERROR;
                else if (!reader.Read())
                    responseCode = AuthResponse.AUTH_RESPONSE_UNKNOWN_USER;

                if (responseCode == AuthResponse.AUTH_RESPONSE_SUCCESS)
                {
                    accountId = reader.GetUInt32(0);
                    type = (UserType)reader.GetByte(1);
                    uName = reader.GetString(2);
                }
            }

            if (!string.IsNullOrEmpty(uName) && accountId != 0)
            {
                _session = new Server.Session(uName, accountId, this);
                Server.Server.Instance.AddSessionQueue(_session);
            }

            var response = new Packet(Opcode.SMSG_AUTH_RESPONSE);
            response.WriteUInt8((byte)responseCode);
            response.WriteUInt32(accountId);
            response.WriteUTF8String(uName);
            response.WriteUInt8((byte)type);
            SendPacket(response);
        }

        private void HandleRegistration(Packet packet)
        {
            string username = packet.ReadUTF8String();
            string password = packet.ReadUTF8String();

            RegistrationResponse responseCode = RegistrationResponse.REG_RESPONSE_SUCCESS;

            int accountId = 0;
            var mysql = MySQL.Instance();
            using (var reader = mysql.Execute($"SELECT `Id` FROM `users` WHERE `username` = '{username}'"))
            {
                if (reader == null)
                    responseCode = RegistrationResponse.REG_RESPONSE_UNKNOWN_ERROR;
                else if (reader.Read())
                    responseCode = RegistrationResponse.REG_RESPONSE_HERE_USER;
            }

            if (responseCode == RegistrationResponse.REG_RESPONSE_SUCCESS)
            {
                accountId = (int)mysql.PExecute($"INSERT INTO `users` ( `type`, `username`, `password`) VALUES ('{(int)UserType.USER_TYPE_CLIENT}', '{username}', '{MD5Hash.Get(username + ":" + password)}')");
                if (accountId != -1)
                {
                    UserMgr.Instance.AddNewUser(username, (uint) accountId, UserType.USER_TYPE_CLIENT);
                    _session = new Server.Session(username, (uint)accountId, this);
                    Server.Server.Instance.AddSessionQueue(_session);
                }
                else
                    responseCode = RegistrationResponse.REG_RESPONSE_UNKNOWN_ERROR;
            }

            var response = new Packet(Opcode.SMSG_REGISTRATION_RESPONSE);
            response.WriteUInt8((byte)responseCode);
            response.WriteUInt32((uint)accountId);
            response.WriteUTF8String(username);
            response.WriteUInt8((byte)UserType.USER_TYPE_CLIENT);
            SendPacket(response);
        }
    }
}
