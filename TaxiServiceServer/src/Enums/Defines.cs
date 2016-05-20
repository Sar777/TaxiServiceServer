namespace TaxiServiceServer.Enums
{
    public enum TaxiType
    {
        TAXI_TYPE_PASSENGER             = 0,
        TAXI_TYPE_TRUCK                 = 1,
        TAXI_TYPE_MAX
    }

    // Состояние заказа
    public enum OrderStatus
    {
        ORDERING_STATUS_NONE            = 0,
        ORDERING_STATUS_QUEUE           = 1,
        ORDERING_STATUS_WAIT            = 2,
        ORDERING_STATUS_IN_PROCESS      = 3,
        ORDERING_STATUS_DONE            = 4,
        ORDERING_STATUS_CANCELED        = 5,
    }

    // Типы пользователей
    public enum UserType
    {
        USER_TYPE_UNKNOWN               = 0,
        USER_TYPE_CLIENT                = 1,
        USER_TYPE_DISPATCHER            = 2,
        USER_TYPE_DRIVER                = 3,
    }

    // Состояния водителя
    public enum DriverStatus
    {
        DRIVER_STATUS_NONE              = 0,
        DRIVER_STATUS_WAIT              = 1,
        DRIVER_STATUS_IN_PROGRESS       = 2,
    }
}
