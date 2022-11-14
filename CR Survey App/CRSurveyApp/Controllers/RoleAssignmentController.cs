using CRSurveyApp.Helper;
using CRSurveyApp.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace CRSurveyApp.Controllers
{
    public class RoleAssignmentController : Controller
    {
        private readonly ILogger logger;
        private readonly APIHelper helperAPI;
        public RoleAssignmentController(ILogger<RoleAssignmentController> logger)
        {
            this.logger = logger;
            helperAPI = new();
        }

        public async Task<IActionResult> RoleAssignments()
        {
            logger.LogInformation("Load roles assignments - Call API(RoleAssignmentsAPI) to load all roles assignments");
            RoleAssignmentsModel? model = new();
            HttpClient client = helperAPI.GetSurveyAPI();
            HttpResponseMessage responseMessage = await client.GetAsync("api/RoleAssignmentsAPI");
            if (responseMessage.IsSuccessStatusCode)
            {
                logger.LogInformation("Getting roles assignments from API(RoleAssignmentsAPI) is successfull");
                var result = responseMessage.Content.ReadAsStringAsync().Result;
                model = JsonConvert.DeserializeObject<RoleAssignmentsModel>(result);
            }
            else
            {
                logger.LogError("Error getting roles assignments from API(RoleAssignmentsAPI)");
            }
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> GetFilteredRoleAssignments(RoleAssignmentParam roleAssignmentParam)
        {
            logger.LogInformation("Load filtered roles assignments - Call API(RoleAssignmentsAPI) to load filtered roles assignments");
            List<RoleAssignments>? roleAssignments = new();

            var roleAssignmentFilter = JsonConvert.SerializeObject(roleAssignmentParam);
            var httpContent = new StringContent(roleAssignmentFilter, Encoding.UTF8, "application/json");

            HttpClient client = helperAPI.GetSurveyAPI();
            HttpResponseMessage responseMessage = await client.PostAsync("api/RoleAssignmentsAPI", httpContent);
            if (responseMessage.IsSuccessStatusCode)
            {
                logger.LogInformation("Load filtered roles assignments - Call to API(RoleAssignmentsAPI) successfull");
                var result = responseMessage.Content.ReadAsStringAsync().Result;
                roleAssignments = JsonConvert.DeserializeObject<List<RoleAssignments>>(result);
            }
            return Json(new { data = roleAssignments });
        }

        [HttpPost]
        public ActionResult RoleAssignments(RoleAssignmentsModel roleAssignmentModel)
        {
            return RedirectToAction("CreateRoleAssignment");
        }

        public ActionResult CreateRoleAssignment()
        {
            DataAccessLayer dal = new();
            AddRoleAssignmentModel model = new();
            model.Roles =  dal.GetDistinctRoles();

            return View(model);
        }

        public JsonResult SaveRoleAssignment(AddRoleAssignmentModel AddRoleAssignmentModel)
        {
            DataAccessLayer dal = new();
            AddRoleAssignmentModel.UserIdentity = User.Identity?.Name;
            dal.SaveRoleAssignment(AddRoleAssignmentModel);
            return Json(true);
        }
    }
}
