using Microsoft.AspNetCore.Mvc.Rendering;

namespace SeatAllocation.Models
{
    public class SeatBookingModel
    {
        public string UserLogged { get; set; }
        public List<SelectListItem> OfficeLocations { get; set; }

        public DateTime SelectedDate { get; set; }


    }

    public class SeatBooking
    {
        public SeatBookingModel GetSeatAvailability()
        {
            SeatBookingModel seatBookingModel = new SeatBookingModel();
            try
            {
                seatBookingModel.UserLogged = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                List<SelectListItem> listOfficeLocation = new List<SelectListItem>();
                SelectListItem bangalore = new SelectListItem { Text = "Bangalore", Value = Guid.NewGuid().ToString() };
                SelectListItem jaipur = new SelectListItem { Text = "Jaipur", Value = Guid.NewGuid().ToString() };
                listOfficeLocation.Add(bangalore);
                listOfficeLocation.Add(jaipur);

                seatBookingModel.OfficeLocations = listOfficeLocation;
                seatBookingModel.SelectedDate = DateTime.Today;
            }
            catch(Exception ex)
            {

            }
            return seatBookingModel;
        }
    }
}
