using CRSurveyAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace CRSurveyAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SurveyAPI : ControllerBase
    {
        [HttpGet(Name = "GetSurveyFilters")]
        public SurveyDetails GetSurveyFilters()
        {
            DataAccessLayer dataAccessLayer = new ();
            SurveyDetails surveys = dataAccessLayer.GetSurveyFilters();
            return surveys;
        }

        [HttpPost(Name = "GetSurveyDetails")]
        public List<Survey> GetSurveys(SurveyParam surveyParam)
        {
            DataAccessLayer dataAccessLayer = new();
            List<Survey> surveys = dataAccessLayer.GetFilteredSurveys(surveyParam);
            return surveys;
        }
    }
}
