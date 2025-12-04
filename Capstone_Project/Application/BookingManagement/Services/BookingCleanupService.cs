using BE_Capstone_Project.Application.BookingManagement.Services.Interfaces;

namespace BE_Capstone_Project.Application.BookingManagement.Services
{
    public class BookingCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

        public BookingCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                    // Delete all expired pending bookings
                    await bookingService.DeleteExpiredPendingBookingsAsync();
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
