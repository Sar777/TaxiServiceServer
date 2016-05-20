using TaxiServiceServer.Enums;

namespace TaxiServiceServer.Cars
{
    public class Passenger : Car
    {
        public int Passengers { get; private set; }

        private Passenger() : base()
        {
            this.Passengers = 0;
            this.Type = TaxiType.TAXI_TYPE_PASSENGER;
        }
    }
}
