using System.Collections.Generic;
using TaxiServiceServer.Cars;
using TaxiServiceServer.Common;
using TaxiServiceServer.Database;

namespace TaxiServiceServer.Managers
{
    class CarsMgr
    {
        private static CarsMgr _instance;
        private readonly List<Car> _cars;

        private CarsMgr()
        {
            _cars = new List<Car>();
        }

        public static CarsMgr Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new CarsMgr();
                return _instance;
            }
        }

        public void SaveAll()
        {
            var mysql = MySQL.Instance();
            mysql.BeginTransaction();

            foreach (var order in _cars)
                order.SaveToDB(false);

            mysql.CommitTransaction();
        }

        public void LoadFromDB()
        {
            var mysql = MySQL.Instance();
            using (var reader = mysql.Execute("SELECT `Id`, `model`,  `number`, `color`, `type` FROM `cars`"))
            {
                while (reader.Read())
                    _cars.Add(new Car(reader));
            }
        }
    }
}
