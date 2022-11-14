using Microsoft.AspNetCore.Mvc;
using SeatAllocation.Models;

namespace SeatAllocation.Controllers
{
    public class SeatBookingController : Controller
    {
        public IActionResult Index()
        {
            SeatBooking seatBooking = new SeatBooking();
            return View(seatBooking.GetSeatAvailability());
        }

        public IActionResult Index1()
        {
            return View();
        }
    }
}
