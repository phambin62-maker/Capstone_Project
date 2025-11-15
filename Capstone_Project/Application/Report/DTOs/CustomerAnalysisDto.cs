namespace BE_Capstone_Project.Application.Report.DTOs
{
    public class CustomerAnalysisDto
    {
        public int TotalCustomers { get; set; }

        public int LoyalCustomers { get; set; } // Khách hàng thân thiết >2 ( Các booking đã đặt và thanh toán thành công =>2 

        public int NewCustomersInRange { get; set; }

        public decimal ReturnRate { get; set; } // Tỷ lệ quay lại = (Số Khách hàng thân thiết / Tổng số khách hàng)
    }
}