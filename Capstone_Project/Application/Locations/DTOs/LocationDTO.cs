namespace BE_Capstone_Project.Application.Locations.DTOs
{
    public class LocationDTO
    {
        public int Id { get; set; }
        public string? LocationName { get; set; }
    }

    public class CreateLocationDTO
    {
        public string? LocationName { get; set; }
    }

    public class UpdateLocationDTO
    {
        public string? LocationName { get; set; }
    }
}
