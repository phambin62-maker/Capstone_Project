using System.ComponentModel.DataAnnotations;

namespace BE_Capstone_Project.Application.Payment.DTOs
{
    public class PaymentInfDto
    {
        public class PaymentInformationModel
        {
            [Required]
            public string OrderType { get; set; }

            [Range(1000, double.MaxValue, ErrorMessage = "Amount must be at least 1000 VND")]
            public double Amount { get; set; }

            [Required]
            [StringLength(255)]
            public string OrderDescription { get; set; }

            [Required]
            [StringLength(100)]
            public string Name { get; set; }
        }
        public class PaymentResponseModel
        {
            public string OrderDescription { get; set; }
            public string TransactionId { get; set; }
            public string OrderId { get; set; }
            public string PaymentMethod { get; set; }
            public string PaymentId { get; set; }
            public bool Success { get; set; }
            public string Token { get; set; }
            public string VnPayResponseCode { get; set; }
        }

    }
}
