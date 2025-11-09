namespace FE_Capstone_Project.Models
{
    public class PaymentResultViewModel
    {
        public bool Success { get; set; }
        public string OrderDescription { get; set; }
        public string Amount { get; set; }
        public string TransactionId { get; set; }
        public string PaymentMethod { get; set; }
        public string Error { get; set; }
    }
}
