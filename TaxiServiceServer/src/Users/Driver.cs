using TaxiServiceServer.Cars;
using TaxiServiceServer.Enums;

namespace TaxiServiceServer.Users
{
    public class Driver : User
    {
        public Car Car { get; private set; }

        public Driver(uint id, string name) : base(id, name)
        {
            UserTypeId = UserType.USER_TYPE_DRIVER;
        }
    }
}