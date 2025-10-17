namespace BE_Capstone_Project.Domain.Enums
{
    public enum BookingStatus : byte
    {
        Cancelled = 0,     // Đã hủy
        Completed = 1,   // hoàn thành
        Pending = 2,        // chờ xác nhận 
        Confirmed = 3,   // đã xác nhận
        
    }
   


}
