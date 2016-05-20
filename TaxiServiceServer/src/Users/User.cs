using TaxiServiceServer.Database;
using TaxiServiceServer.Enums;

namespace TaxiServiceServer.Users
{
    public class User
    {
        public uint Id { get; private set; }
        public string Username { get; private set; }

        public UserType UserTypeId { get; protected set; }

        public User()
        {
            Id = 0;
            UserTypeId = UserType.USER_TYPE_CLIENT;
            Username = "User";
        }

        public virtual void Update()
        {
            
        }

        public User(uint id, string username)
        {
            this.Id = id;
            this.Username = username;
        }

        public User(string username)
        {
            this.Id = 0;
            this.Username = username;
        }

        public Users.Client ToClient() { return this as Users.Client; }
        public Dispatcher ToManager() { return this as Dispatcher; }
        public Driver ToDriver() { return this as Driver; }

        public virtual void SaveToDB(bool trans = true)
        {
            var mysql = MySQL.Instance();
            if (trans)
                mysql.BeginTransaction();

            mysql.PExecute($"DELETE FROM `users` WHERE `Id` = '{Id}'");
            mysql.PExecute($"INSERT INTO `users` (`Id`, `type`, `name`) VALUES ('{Id}', '{(int)UserTypeId}', '{Username}')");

            if (trans)
                mysql.CommitTransaction();
        }
    }
}
