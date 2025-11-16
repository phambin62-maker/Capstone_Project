namespace BE_Capstone_Project.Application.BookingManagement.DTOs
{
    public class BookingRequest
    {
        public int Tour_Schedule { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public int Infants { get; set; }

        public string First_Name { get; set; } = string.Empty;
        public string Last_Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Certificate_Id { get; set; } = string.Empty;

        public string PaymentMethod {  get; set; } = string.Empty;

        public List<BookingCustomerDTO> Travelers { get; set; } = new();

        public override string ToString()
        {
            string travelerInfo = Travelers != null && Travelers.Count > 0
                ? string.Join("\n  ", Travelers.Select(t => t.ToString()))
                : "No travelers";

            return $"BookingRequest:\n" +
                   $"  Tour Schedule: {Tour_Schedule}\n" +
                   $"  Adults: {Adults}, Children: {Children}, Infants: {Infants}\n" +
                   $"  Name: {First_Name} {Last_Name}\n" +
                   $"  Email: {Email}, Phone: {Phone}\n" +
                   $"  Certificate ID: {Certificate_Id}\n" +
                   $"  Travelers:\n  {travelerInfo}";
        }
    }
}
