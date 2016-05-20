using System;
using MySql.Data.MySqlClient;
using TaxiServiceServer.Database;
using TaxiServiceServer.Enums;
using TaxiServiceServer.Managers;
using TaxiServiceServer.Protocol;
using TaxiServiceServer.Users;

namespace TaxiServiceServer.Common
{
    public class Order
    {
        enum DBStatus
        {
            ORDER_NONE = 0,
            ORDER_NEW = 1,
            ORDER_CHANGED = 2,
            ORDER_MAX,
        }

        public uint Id { get; private set; }
        public Address SAddress { get; set; }
        public Address EAddress { get; set; }
        public DateTime Date { get; set; }
        public OrderStatus Status { get; set; }
        public TaxiType Type { get; set; }
        public User Driver { get; set; }
        public User Owner { get; set; }

        // Соятоние заказ для сохранения
        private DBStatus _dbStatus;

        private Order()
        {
            Id = 0;
            Status = OrderStatus.ORDERING_STATUS_NONE;
            Type = TaxiType.TAXI_TYPE_MAX;
            Driver = null;
            Owner = null;
        }

        public Order(MySqlDataReader reader)
        {
            // `Id`, `type`, `status`, `date`, `s_address`, `e_address`, `driverId`, `ownerId`
            Id = reader.GetUInt32(0);
            Type = (TaxiType)reader.GetByte(1);
            Status = (OrderStatus)reader.GetByte(2);
            Date = Time.UnixTimeStampToDateTime(reader.GetUInt32(3));
            SAddress = Address.Parse(reader.GetString(4));
            EAddress = Address.Parse(reader.GetString(5));
            Driver = UserMgr.Instance.GetUserById(reader.GetUInt32(6));
            Owner = UserMgr.Instance.GetUserById(reader.GetUInt32(7));
        }

        public Order(uint id, User owner, User driver, TaxiType taxiType, OrderStatus status, Address sAddress,
            Address eAddress, DateTime date)
        {
            Id = id;
            Owner = owner;
            Driver = driver;
            SAddress = sAddress;
            EAddress = eAddress;
            Date = date;
            Type = taxiType;
            Status = status;
        }

        public Order(User owner, TaxiType taxiType, Address sAddress, Address eAddress)
        {
            Id = 0;
            Owner = owner;
            Driver = null;
            SAddress = sAddress;
            EAddress = eAddress;
            Type = taxiType;
            Date = new DateTime();
            Status = OrderStatus.ORDERING_STATUS_QUEUE;

            _dbStatus = DBStatus.ORDER_NEW;
        }

        public void WritePacket(Packet packet)
        {
            packet.WriteUInt32(Id);
            packet.WriteUTF8String(Misc.GetTaxiTypeString(Type));
            packet.WriteUTF8String(Misc.GetOrderStatusString(Status));
            packet.WriteUInt32((uint)Time.UnixTimeFromDataTime(Date));
            packet.WriteUTF8String(SAddress.ToString());
            packet.WriteUTF8String(EAddress.ToString());
            packet.WriteUTF8String(Owner.Username);

            if (Driver != null)
            {
                packet.WriteUInt8(1);
                packet.WriteUTF8String(Driver.Username);
                packet.WriteUTF8String(Driver.ToDriver().Car.Model);
                packet.WriteUTF8String(Driver.ToDriver().Car.Color);
                packet.WriteUTF8String(Driver.ToDriver().Car.Number);
                packet.WriteUTF8String(Misc.GetTaxiTypeString(Driver.ToDriver().Car.Type));
            }
            else
                packet.WriteUInt8(0);
        }

        public void SaveToDB(bool trans = true)
        {
            if (_dbStatus == DBStatus.ORDER_NONE)
                return;

            var mysql = MySQL.Instance();

            if (trans)
                mysql.BeginTransaction();

            var driverId = Driver?.Id ?? 0;
            var ownerId = Owner?.Id ?? 0;

            switch (_dbStatus)
            {
                case DBStatus.ORDER_NEW:
                {
                    var orderId =(int)mysql.PExecute($"INSERT INTO `orders` (`type`, `status`, `date`, `s_address`, `e_address`, `driverId`, `ownerId`) VALUES ('{(int) Type}', '{(int) Status}', '{Time.UnixTimeNow()}', '{SAddress}', '{EAddress}', '{driverId}', '{ownerId}')");
                    if (orderId == -1)
                        return;

                    Id = (uint)orderId;
                    break;
                }
                case DBStatus.ORDER_CHANGED:
                {
                    mysql.PExecute($"DELETE FROM `orders` WHERE `Id` = {Id}");
                    mysql.PExecute($"INSERT INTO `orders` (`Id`, `type`, `status`, `date`, `s_address`, `e_address`, `driverId`, `ownerId`) VALUES ('{Id}', '{(int) Type}', '{(int) Status}', '{Time.UnixTimeFromDataTime(Date)}', '{SAddress}', '{EAddress}', '{driverId}', '{ownerId}')");
                    break;
                }
                default:
                    break;
            }

            _dbStatus = DBStatus.ORDER_NONE;

            if (trans)
                mysql.CommitTransaction();
        }

        public bool IsQueue()
        {
            return Status == OrderStatus.ORDERING_STATUS_QUEUE;
        }

        public bool IsProggress()
        {
            return Status == OrderStatus.ORDERING_STATUS_IN_PROCESS;
        }

        public bool IsValid()
        {
            return Id != 0 && SAddress != null &&
                   EAddress != null && SAddress != EAddress && Owner != null &&
                   Type == TaxiType.TAXI_TYPE_MAX;
        }

        public void Cancel()
        {
            if (Status != OrderStatus.ORDERING_STATUS_QUEUE)
                return;

            Status = OrderStatus.ORDERING_STATUS_CANCELED;
            _dbStatus = DBStatus.ORDER_CHANGED;
            //   SaveToDb(true);
        }

        public bool Validate(out string errorText, bool _new = false)
        {
            errorText = "";
            if (Id == 0 && _new)
                errorText = "Неизвестный номер";
            else if (Owner == null)
                errorText = "Неизвестный заказчик";
            else if (Type == TaxiType.TAXI_TYPE_MAX)
                errorText = "Не указан тип такси";
            else if (SAddress == null)
                errorText = "Неизвестный адрес заказ";
            else if (EAddress == null)
                errorText = "Неизвестный адрес назначения";
            else if (SAddress == EAddress)
                errorText = "Адрес заказа и адрес назначения совпадают";

            return errorText.Length == 0;
        }
    }
}