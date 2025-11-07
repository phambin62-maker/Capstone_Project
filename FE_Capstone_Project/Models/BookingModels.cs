using BE_Capstone_Project.Domain.Enums;

namespace FE_Capstone_Project.Models
{
    public class TravelerDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string IdentityID { get; set; }
        public CustomerType CustomerType { get; set; }

        public override string ToString()
        {
            return $"Traveler: {FirstName} {LastName}, Email: {Email}, Phone: {PhoneNumber}, " +
                   $"ID: {IdentityID}, Type: {CustomerType}";
        }
    }

    public class BookingRequest
    {
        public int Tour_Schedule { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public int Infants { get; set; }

        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Certificate_Id { get; set; }

        public List<TravelerDTO> Travelers { get; set; }

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
