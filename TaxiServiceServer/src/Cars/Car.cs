using System;
using MySql.Data.MySqlClient;
using TaxiServiceServer.Common;
using TaxiServiceServer.Database;
using TaxiServiceServer.Enums;
using TaxiServiceServer.Managers;
using TaxiServiceServer.Users;

namespace TaxiServiceServer.Cars
{
    public class Car
    {
        public uint ID { get; private set; }
        public string Model { get; set; }
        public string Number { get; set; }
        public string Color { get; set; }
        public TaxiType Type { get; protected set; }

        protected Car()
        {
            this.Model = "Unknown";
            this.Number = "Unknown";
            this.Color = "Unknown";
            this.Type = TaxiType.TAXI_TYPE_MAX;
        }

        public Car(MySqlDataReader reader)
        {
            //  `Id`, `model`,  `number`, `color`, `type`
            ID = reader.GetUInt32(0);
            Model = reader.GetString(1);
            Number = reader.GetString(2);
            Type = (TaxiType)reader.GetByte(3);
        }

        public void SaveToDB(bool trans = true)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 
        {
            MySQL mysql = MySQL.Instance();

            if (trans)
                mysql.BeginTransaction();

            mysql.PExecute($"DELETE FROM `cars` WHERE `Id` = {ID}");
            mysql.PExecute($"INSERT INTO `cars` (`Id`, `model`, `number`, `color`, `type`) VALUES ('{ID}', '{Model}', '{Number}', '{Color}', '{(int)Type}'");

            if (trans)
                mysql.CommitTransaction();
        }
    }
}
