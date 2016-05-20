using TaxiServiceServer.Enums;

namespace TaxiServiceServer.Common
{
    public class Misc
    {
        public static string GetOrderStatusString(OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.ORDERING_STATUS_IN_PROCESS:
                    return "В процессе";
                case OrderStatus.ORDERING_STATUS_NONE:
                    return "Нету";
                case OrderStatus.ORDERING_STATUS_QUEUE:
                    return "В очереди";
                case OrderStatus.ORDERING_STATUS_WAIT:
                    return "Ожидание";
            }

            return "Ошибка";
        }

        public static string GetTaxiTypeString(TaxiType type)
        {
            switch (type)
            {
                case TaxiType.TAXI_TYPE_PASSENGER:
                    return "Пассажирское";
                case TaxiType.TAXI_TYPE_TRUCK:
                    return "Грузовое";
                default:
                    break;
            }

            return "Неизвестно";
        }
    }
}