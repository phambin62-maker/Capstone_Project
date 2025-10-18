namespace BE_Capstone_Project.Domain.Enums
{
    public enum PaymentStatus : byte
    {
        Failed = 0,
        Completed = 1,
        Pending = 2,
        Refunded = 3,
    }
}
