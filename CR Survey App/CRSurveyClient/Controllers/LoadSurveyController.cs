using CRSurveyClient.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
//using Microsoft.AspNet.Identity;

namespace CRSurveyClient.Controllers
{
    public class LoadSurveyController : Controller
    {
        public IActionResult Survey(string id)
        {
            //id = "27ee020106c3463586b91603b056b07d";
            //id = "a7e82c48e9534a61910e4cfd87b8fc2e";
            //id = "5843383a87804474a192d31aa09fd73e";//Maturity Assessment 
            //id = "fbee663859de4147ba7f13ad48001367"; //CSAT
            //id = "ed96c3b8f12d4dfdbd5626a2098d75a8"; // CSAT Azure
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("SurveyNotFound");
            }
            DAL dal = new();
            SurveyModel survey = dal.LoadSurvey(id, User.Identity?.Name, -1);// User.Identity?.Name
            if (survey.ExecutionResponse == null)
            {
                return RedirectToAction("SurveyNotFound");
            }
            else
            {
                if (survey.ExecutionResponse.ExecutionStatus == ExecutionStatus.Success)
                {
                    return View(survey);
                }
                else
                {
                    if (survey.ExecutionResponse.Message == "USER_NOT_HAVE_ACCESS_TO_SURVEY")
                        return RedirectToAction("NotAuthorized");
                    else
                    {
                        return RedirectToAction("SurveyNotFound");
                    }
                }
            }
        }

        [HttpPost]
        public IActionResult Survey(SurveyModel response)
        {
            DAL dal = new();
            // Check event type 
            if (response.EventType == EventType.Back)
            {
                SurveyModel survey = dal.LoadSurvey(response.UniqueId, User.Identity?.Name, response.PageIndex-1);
                if (survey == null)
                {
                    return RedirectToAction("SurveyNotFound");
                }
                else
                {
                    if (survey.ExecutionResponse.ExecutionStatus == ExecutionStatus.Success)
                    {
                        return View(survey);
                    }
                    else
                    {
                        if (survey.ExecutionResponse.Message == "USER_NOT_HAVE_ACCESS_TO_SURVEY")
                            return RedirectToAction("NotAuthorized");
                        else
                        {
                            return RedirectToAction("SurveyNotFound");
                        }
                    }
                }
            }
            if (response.EventType == EventType.Next)
            {
                // Push the page response 
                Client client = new()
                {
                    Name = GetFullName(),
                    Email = User.Identity?.Name
                };
                response.Client = client;
                dal.PushResponsetoDB(response);

                SurveyModel survey = dal.LoadSurvey(response.UniqueId, User.Identity?.Name, response.PageIndex + 1);
                if (survey == null)
                {
                    return RedirectToAction("SurveyNotFound");
                }
                else
                {
                    if (survey.ExecutionResponse.ExecutionStatus == ExecutionStatus.Success)
                    {
                        return View(survey);
                    }
                    else
                    {
                        if (survey.ExecutionResponse.Message == "USER_NOT_HAVE_ACCESS_TO_SURVEY")
                            return RedirectToAction("NotAuthorized");
                        else
                        {
                            return RedirectToAction("SurveyNotFound");
                        }
                    }
                }
            }
            else
            {
                response.EventType = EventType.Submit;
                Client client = new()
                {
                    Name = GetFullName(),
                    Email = User.Identity?.Name
                };
                response.Client = client;
                dal.PushResponsetoDB(response);
                return RedirectToAction("ResponseRecorded");
            }
        }

        public IActionResult ResponseRecorded()
        {
            return View();
        }

        public IActionResult SurveyNotFound()
        {
            return View();
        }

        public IActionResult NotAuthorized()
        {
            return View();
        }

        public string GetFullName()
        {
            try
            {
                var fullNameClaim = ((ClaimsIdentity)User.Identity).FindFirst("name");
                if (fullNameClaim != null)
                    return fullNameClaim.Value;
            }
            catch (Exception)
            { }
            return "";
        }
    }
}
