using CRSurveyAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRSurveyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsAPI : ControllerBase
    {
        [HttpGet(Name = "GetProjectFilters")]
        public ProjectDetails GetProjectFilters()
        {
            DataAccessLayer dataAccessLayer = new();
            ProjectDetails projectFilters = dataAccessLayer.GetProjectFilters();
            return projectFilters;
        }

        [HttpPost(Name = "GetProjectDetails")]
        public List<Project> GetProjectDetails(ProjectParam projectParam)
        {
            DataAccessLayer dataAccessLayer = new();
            List<Project> projectFilters = dataAccessLayer.GetProjectDetails(projectParam);
            return projectFilters;
        }
    }
}
