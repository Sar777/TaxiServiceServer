using TaxiServiceServer.Enums;

namespace TaxiServiceServer.Users
{
    public class Dispatcher : User
    {
        public Dispatcher(uint id, string name) : base(id, name)
        {
            UserTypeId = UserType.USER_TYPE_DISPATCHER;
        }
    }
}