using CRSurveyApp.Helper;
using CRSurveyApp.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace CRSurveyApp.Controllers
{
    public class RoleController : Controller
    {
        private readonly APIHelper helperAPI;
        private readonly ILogger logger;
        public RoleController(ILogger<RoleController> logger)
        {
            this.logger = logger;
            helperAPI = new();
        }
        public async Task<IActionResult> Roles()
        {
            logger.LogInformation("Load roles - Call API(RoleAPI) to load all roles");
            RoleModel? model = new();
            HttpClient client = helperAPI.GetSurveyAPI();
            HttpResponseMessage responseMessage = await client.GetAsync("api/RoleAPI");
            if (responseMessage.IsSuccessStatusCode)
            {
                logger.LogInformation("Getting roles from API(RoleAPI) is successfull");
                var result = responseMessage.Content.ReadAsStringAsync().Result;
                model = JsonConvert.DeserializeObject<RoleModel>(result);
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetFilteredRoles(RoleParam roleParam)
        {
            logger.LogInformation("Load filtered roles - Call API(RoleAPI) to load filtered roles");
            List<Role>? roles = new();

            var roleFilter = JsonConvert.SerializeObject(roleParam);
            var httpContent = new StringContent(roleFilter, Encoding.UTF8, "application/json");

            HttpClient client = helperAPI.GetSurveyAPI();
            HttpResponseMessage responseMessage = await client.PostAsync("api/RoleAPI", httpContent);
            if (responseMessage.IsSuccessStatusCode)
            {
                logger.LogInformation("Load filtered roles - Call to API(RoleAPI) successfull");
                var result = responseMessage.Content.ReadAsStringAsync().Result;
                roles = JsonConvert.DeserializeObject<List<Role>>(result);
            }
            return Json(new { data = roles });
        }

        [HttpPost]
        public ActionResult Roles(RoleModel roleModel)
        {
            HttpContext.Session.SetInt32("RoleID", roleModel.SelectedRoleId);
            return RedirectToAction("CreateRole");
        }

        public ActionResult CreateRole()
        {
            int roleId = 0;

            // Get project id  from session 
            if (HttpContext.Session.GetString("RoleID") != null)
            {
                roleId = (int)HttpContext.Session.GetInt32("RoleID");
            }

            DataAccessLayer dal = new();
            AddRoleModel model = dal.GetRoleDetails(roleId);

            return View(model);
        }

        public JsonResult SaveRole(AddRoleModel AddRoleModel)
        {
            DataAccessLayer dal = new();
            dal.SaveRole(AddRoleModel);
            return Json(true);
        }

    }
}
