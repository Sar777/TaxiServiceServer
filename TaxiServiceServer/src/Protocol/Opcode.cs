namespace TaxiServiceServer.Protocol
{
    public enum Opcode : uint
    {
        NULL_OPCODE                         = 0x000,
        CMSG_AUTH                           = 0x001,
        CMSG_REGISTRATION                   = 0x002,
        SMSG_AUTH_RESPONSE                  = 0x003,
        SMSG_REGISTRATION_RESPONSE          = 0x004,
        CMSG_LOGOUT                         = 0x005,
        CMSG_DISCONNECTED                   = 0x006,
        CMSG_GET_TAXI_INFO                  = 0x007,
        SMSG_GET_TAXI_INFO_RESPONSE         = 0x008,
        CMSG_GET_ORDER                      = 0x009,
        SMSG_GET_ORDER_RESPONSE             = 0x00A,
        MAX_OPCODE,
    }
}
