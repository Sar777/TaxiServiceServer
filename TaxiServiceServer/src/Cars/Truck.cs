using TaxiServiceServer.Enums;

namespace TaxiServiceServer.Cars
{
    public class Truck : Car
    {
        private Truck() : base()
        {
            this.Type = TaxiType.TAXI_TYPE_TRUCK;
        }
    }
}
