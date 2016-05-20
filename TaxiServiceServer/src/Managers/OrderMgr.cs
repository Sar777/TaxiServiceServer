using System.Collections.Generic;
using System.Linq;
using TaxiServiceServer.Common;
using TaxiServiceServer.Database;
using TaxiServiceServer.Enums;

namespace TaxiServiceServer.src.Managers
{
    public class OrderMgr
    {
        private static OrderMgr _instance;
        private readonly List<Order> _orderings;

        private OrderMgr()
        {
            _orderings = new List<Order>();
        }

        public static OrderMgr Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new OrderMgr();
                return _instance;
            }
        }

        public void AddOrdering(Order ordering)
        {
            _orderings.Add(ordering);
        }

        public void RemoveOrdering(Order ordering)
        {
            _orderings.Remove(ordering);
        }

        public Order GetOrderingById(uint id)
        {
            return _orderings.Find(x => x.Id == id);
        }

        public void SaveAll()
        {
            var mysql = MySQL.Instance();
            mysql.BeginTransaction();

            foreach (var order in _orderings)
                order.SaveToDB(false);

            mysql.CommitTransaction();
        }

        public void LoadFromDB()
        {
            var mysql = MySQL.Instance();
            using (var reader = mysql.Execute("SELECT `Id`, `type`, `status`, `date`, `s_address`, `e_address`, `driverId`, `ownerId` FROM `orders`"))
            {
                while (reader.Read())
                    _orderings.Add(new Order(reader));
            }
        }

        public List<Order> GetOrdersByOwner(uint ownerId)
        {
            return _orderings.Where(
                order =>
                    order.Status != OrderStatus.ORDERING_STATUS_DONE &&
                    order.Status != OrderStatus.ORDERING_STATUS_CANCELED).Where(order => order.Owner.Id == ownerId).ToList();
        }
    }
}
