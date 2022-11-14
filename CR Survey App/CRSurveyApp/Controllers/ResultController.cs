using Microsoft.AspNetCore.Mvc;

namespace CRSurveyApp.Controllers
{
    public class ResultController : Controller
    {
        public IActionResult Results()
        {
            return View();
        }
    }
}
