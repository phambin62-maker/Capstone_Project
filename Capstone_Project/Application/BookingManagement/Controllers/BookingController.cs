using BE_Capstone_Project.Application.BookingManagement.DTOs;
using BE_Capstone_Project.Application.BookingManagement.Services.Interfaces;
using BE_Capstone_Project.Application.Services;
using BE_Capstone_Project.Application.TourManagement.Services.Interfaces;
using BE_Capstone_Project.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace BE_Capstone_Project.Application.BookingManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IUserService _userService;
        private readonly ITourService _tourService;

        public BookingController(IBookingService bookingService, IUserService userService, ITourService tourService)
        {
            _bookingService = bookingService;
            _userService = userService;
            _tourService = tourService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _bookingService.GetAllAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null) return NotFound();
            return Ok(booking);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var list = await _bookingService.GetByUserIdAsync(userId);
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BookingRequest request, [FromQuery] string username)
        {
            try
            {
                Console.WriteLine($"===========================================================");
                Console.WriteLine($"{request.ToString()}");
                Console.WriteLine($"===========================================================");
                var user = await _userService.GetUserByUsername(username);
                var tour = await _tourService.GetTourByScheduleId(request.Tour_Schedule);
                decimal totalPrice = CalculateTotalPrice(request, tour);


                CreateBookingDTO booking = new CreateBookingDTO
                {
                    UserId = user.Id,
                    TourScheduleId = request.Tour_Schedule,
                    FirstName = request.First_Name,
                    LastName = request.Last_Name,
                    PhoneNumber = request.Phone,
                    Email = request.Email,
                    CertificateId = request.Certificate_Id,

                    PaymentStatus = Domain.Enums.PaymentStatus.Pending,
                    BookingStatus = Domain.Enums.BookingStatus.Pending,

                    RefundAmount = null,
                    RefundDate = null,
                    PaymentMethod = null,
                    PaymentDate = null,
                    ExpirationTime = DateTime.Now.AddHours(1),
                    BookingDate = DateTime.Now,
                    TotalPrice = totalPrice
                };

                var bookingId = await _bookingService.CreateAsync(booking);
                if (bookingId == 0)
                {
                    return BadRequest(new { message = "Failed to create booking.", success = false });
                }
                else
                {
                    await _bookingService.AddBookingCustomersToBookId(bookingId, request.Travelers);
                    return Ok(new BookingSuccessResponse { 
                        BookingId = bookingId,
                        TotalPrice = (int)totalPrice,
                        TourName = tour.Name,
                        FirstName = request.First_Name,
                        LastName = request.Last_Name,
                        Message = "Booking created successfully", 
                        Success = true });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest(new { message = ex.Message, success = false });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateBookingDTO dto)
        {
            var success = await _bookingService.UpdateAsync(id, dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _bookingService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        private decimal CalculateTotalPrice(BookingRequest request, Tour tour)
        {
            if (tour.Price == null)
                throw new Exception("Tour price is not set.");

            int totalPeople = request.Adults + request.Children + request.Infants;

            decimal basePrice = tour.Price.Value;
            decimal childPrice = basePrice * (1 - (tour.ChildDiscount ?? 0) / 100);

            decimal groupDiscount = 0;
            if (totalPeople >= 6 && tour.GroupDiscount.HasValue)
            {
                groupDiscount = tour.GroupDiscount.Value / 100;
            }

            decimal totalPrice = (request.Adults * basePrice) +
                                 ((request.Children + request.Infants) * childPrice);

            totalPrice *= (1 - groupDiscount);

            return totalPrice;
        }
    }
}
