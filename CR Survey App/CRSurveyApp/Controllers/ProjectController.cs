using CRSurveyApp.Helper;
using CRSurveyApp.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace CRSurveyApp.Controllers
{
    public class ProjectController : Controller
    {
        private readonly ILogger logger;
        private readonly APIHelper helperAPI;

        public ProjectController(ILogger<ProjectController> logger)
        {
            this.logger = logger;
            helperAPI = new APIHelper();
        }

        public async Task<IActionResult> Projects()
        {
                logger.LogInformation("Load projects - Call API(ProjectsAPI) to load all projects");
                ProjectFilter? projectFilters = new();
                HttpClient client = helperAPI.GetSurveyAPI();
                HttpResponseMessage responseMessage = await client.GetAsync("api/ProjectsAPI");
                if (responseMessage.IsSuccessStatusCode)
                {
                    logger.LogInformation("Getting projects from API(ProjectsAPI) is successfull");
                    var result = responseMessage.Content.ReadAsStringAsync().Result;
                    projectFilters = JsonConvert.DeserializeObject<ProjectFilter>(result);
                }

            return View(projectFilters);
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectDetails(ProjectParam projectParam)
        {
            logger.LogInformation("Load projects - Call API(ProjectsAPI) to load filtered projects");
            List<Project>? projects = new();

            var projectFilter = JsonConvert.SerializeObject(projectParam);
            var httpContent = new StringContent(projectFilter, Encoding.UTF8, "application/json");

            HttpClient client = helperAPI.GetSurveyAPI();
            HttpResponseMessage responseMessage = await client.PostAsync("api/ProjectsAPI", httpContent);
            if (responseMessage.IsSuccessStatusCode)
            {
                logger.LogInformation("Load projects - Call to API(ProjectsAPI) to load filtered projects is successfull");
                var result = responseMessage.Content.ReadAsStringAsync().Result;
                projects = JsonConvert.DeserializeObject<List<Project>>(result);
            }
            return Json(new { data = projects });
        }

        [HttpPost]
        public ActionResult Projects(ProjectFilter project)
        {
            logger.LogInformation("Redirect to CreateProject action method");
            HttpContext.Session.SetInt32("ProjectID", project.SelectedProjectId);
            return RedirectToAction("CreateProject");
        }

        public ActionResult CreateProject()
        {
            int projectId = 0;

            // Get project id  from session 
            if (HttpContext.Session.GetString("ProjectID") != null)
            {
                projectId = (Int32)HttpContext.Session.GetInt32("ProjectID");
            }

            DataAccessLayer dal = new();
            AddProject addProject = dal.GetProjectDetails(projectId);

            return View(addProject);
        }

        //[HttpPost]
        public JsonResult SaveProject(AddProject AddProject)
        {
            DataAccessLayer dal = new();
            AddProject.UserIdentity = User.Identity?.Name;
            dal.SaveProject(AddProject);
            return Json(true);
        }

        public JsonResult DeleteProject(int id)
        {
            DataAccessLayer dal = new();
            dal.DeleteProject(id);
            return Json("");
        }

        public PartialViewResult SendSurveyModelPopup(int projectId)
        {
            DataAccessLayer dal = new();
            SendSurveyModel sendSurvey = dal.GetSendSurveyDetails(projectId);

            return PartialView("_SendSurvey", sendSurvey);
        }

        public JsonResult GetEmailContent(int surveyId)
        {
            DataAccessLayer dal = new();
            SurveyEmailContent content = dal.GetEmailContent(surveyId);
            return Json(new { data = content });
        }

        public JsonResult SendSurvey(SendSurveyModel SendSurveyModel)
        {
            SendSurveyModel.EmailSent = EmailTrigger.SendEmail(SendSurveyModel);
            DataAccessLayer dal = new();
            dal.SaveSentSurvey(SendSurveyModel);
            return Json(true);
        }
    }
}
