using System.Linq;
using TaxiServiceServer.Common;
using TaxiServiceServer.Enums;
using TaxiServiceServer.Parser;
using TaxiServiceServer.Protocol;
using TaxiServiceServer.src.Managers;

namespace TaxiServiceServer.Server
{
    public class Handlers
    {
        [Parser(Opcode.CMSG_GET_TAXI_INFO)]
        public static void HandleGetCurrentOrder(Session session, Packet packet)
        {
            var order = OrderMgr.Instance.GetOrdersByOwner(session.User.Id).First();
            var response = new Packet(Opcode.SMSG_GET_TAXI_INFO_RESPONSE);
            response.WriteUInt32(order != null ? order.Id : 0);

            // Типы такси
            response.WriteUInt8((byte)TaxiType.TAXI_TYPE_MAX);
            for (uint i = 0; i < (uint)TaxiType.TAXI_TYPE_MAX; ++i)
                response.WriteUTF8String(Misc.GetTaxiTypeString((TaxiType)i));

            // Любимые адреса
            if (session.User.UserTypeId == UserType.USER_TYPE_CLIENT)
            {
                response.WriteUInt16((ushort)session.User.ToClient().Addresses.Count);
                foreach (var address in session.User.ToClient().Addresses)
                    response.WriteUTF8String(address.ToString());
            }
            else
                response.WriteUInt16(0);

            session.Socket.SendPacket(response);
        }

        [Parser(Opcode.CMSG_GET_ORDER)]
        public static void HandleGetOrder(Session session, Packet packet)
        {
            var orderId = packet.ReadUInt32();
            var order = OrderMgr.Instance.GetOrderingById(orderId);
            if (order == null)
                return;

            var response = new Packet(Opcode.SMSG_GET_ORDER_RESPONSE);
            order.WritePacket(response);
            session.Socket.SendPacket(response);
        }
    }
}
