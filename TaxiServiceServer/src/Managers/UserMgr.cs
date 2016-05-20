using System;
using System.Collections.Generic;
using TaxiServiceServer.Database;
using TaxiServiceServer.Enums;
using TaxiServiceServer.Users;

namespace TaxiServiceServer.Managers
{
    internal class UserMgr
    {
        private static UserMgr _instance;
        private readonly List<User> _users;

        public UserMgr()
        {
            _users = new List<User>();
        }

        public static UserMgr Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UserMgr();
                return _instance;
            }
        }

        public List<User> Users
        {
            get
            {
                return _users;
            }
        }

        public void SaveAll()
        {
            var mysql = MySQL.Instance();
            mysql.BeginTransaction();

            foreach (var user in _users)
                user.SaveToDB(false);

            mysql.CommitTransaction();
        }

        public void LoadFromDB()
        {
            var mysql = MySQL.Instance();
            using (var reader = mysql.Execute("SELECT `Id`, `type`, `username` FROM `users`"))
            {
                while (reader.Read())
                {
                    User user = null;
                    switch ((UserType)reader.GetByte(1))
                    {
                        case UserType.USER_TYPE_CLIENT:
                            user =  new Client(reader.GetUInt32(0), reader.GetString(2));
                            break;
                        case UserType.USER_TYPE_DRIVER:
                        case UserType.USER_TYPE_DISPATCHER:
                        default:
                            Console.WriteLine($"Not supported usertype {(UserType)reader.GetByte(1)}. skeep...");
                            continue;
                    }

                    _users.Add(user);
                }
            }

            foreach (var user in _users)
            {
                if (user.UserTypeId == UserType.USER_TYPE_CLIENT)
                    user.ToClient().LoadAddresses();
            }
        }

        public User GetUserById(uint userId)
        {
            return _users.Find(x => x.Id == userId);
        }

        public void RemoveUserById(uint userId)
        {
            _users.RemoveAll(x => x.Id == userId);
        }

        public void AddNewUser(string username, uint accountId, UserType type)
        {
            User user;
            switch (type)
            {
                case UserType.USER_TYPE_CLIENT:
                    user = new Client(accountId, username);
                    break;
                case UserType.USER_TYPE_DRIVER:
                    user = new Driver(accountId, username);
                    break;
                case UserType.USER_TYPE_DISPATCHER:
                    user = new Dispatcher(accountId, username);
                    break;
                default:
                    Console.WriteLine($"Not supported usertype {type}");
                    return;
            }

            _users.Add(user);
        }
    }
}
