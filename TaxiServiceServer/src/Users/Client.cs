using System;
using System.Collections.Generic;
using TaxiServiceServer.Common;
using TaxiServiceServer.Database;
using TaxiServiceServer.Enums;

namespace TaxiServiceServer.Users
{
    public class Client : User
    {
        public List<Address> Addresses { get; set; }

        public Client(uint id, string name) : base(id, name)
        {
            UserTypeId = UserType.USER_TYPE_CLIENT;
            Addresses = new List<Address>();
        }

        public void LoadAddresses()
        {
            Addresses.Clear();
            var mysql = MySQL.Instance();
            using (var reader = mysql.Execute($"SELECT `address` FROM `save_address` WHERE `ownerId` = {Id}"))
            {
                if (reader == null)
                    return;

                while (reader.Read())
                    Addresses.Add(Address.Parse(reader.GetString(0)));
            }
        }

        public override void Update()
        {
            base.Update();
        }

        public override void SaveToDB(bool trans = true)
        {
            base.SaveToDB(trans);
      
            var mysql = MySQL.Instance();

            mysql.PExecute($"DELETE FROM `save_address` WHERE `ownerId` = '{Id}'");
            foreach (var address in Addresses)
                mysql.PExecute($"INSERT INTO `save_address` (`ownerId`, `address`) VALUES ('{Id}', '{address}')"); 
        }
    }
}