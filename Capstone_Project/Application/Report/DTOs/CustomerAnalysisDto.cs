namespace BE_Capstone_Project.Application.Report.DTOs
{
    public class CustomerAnalysisDto
    {
        public int TotalCustomers { get; set; }

        public int LoyalCustomers { get; set; }

        public int NewCustomersInRange { get; set; }

        public decimal ReturnRate { get; set; }
    }
}