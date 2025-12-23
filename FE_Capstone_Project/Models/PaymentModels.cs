namespace FE_Capstone_Project.Models
{
    public class PaymentRequest
    {
        public string OrderType { get; set; }
        public decimal Amount { get; set; }
        public string OrderDescription { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"PaymentRequest: {{ OrderType = {OrderType}, Amount = {Amount:C}, OrderDescription = {OrderDescription}, Name = {Name} }}";
        }
    }

    public class PaymentResponse
    {
        public string PaymentUrl { get; set; }
    }

    public class PaymentDTO
    {
        public int BookingId { get; set; }
        public bool Success { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
