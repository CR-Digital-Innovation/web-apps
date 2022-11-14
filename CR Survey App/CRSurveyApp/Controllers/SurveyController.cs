using CRSurveyApp.Models;
using CRSurveyApp.Helper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;

namespace CRSurveyApp.Controllers
{
    public class SurveyController : Controller
    {
        private readonly APIHelper helperAPI;
        private readonly ILogger logger;

        public SurveyController(ILogger<SurveyController> logger)
        {
            this.logger = logger;
            helperAPI = new();
        }

        /// <summary>
        /// Action method to load survey type dropdown values along with all the surveys 
        /// This action method is called on page load
        /// </summary>
        /// <returns>
        /// Survey type dropdown values and surveys
        /// </returns>
        public async Task<IActionResult> Surveys()
        {
            logger.LogInformation("Load Surveys - Call API(SurveyAPI) to load all surveys");
            SurveyModel? surveyModel = new SurveyModel();
            HttpClient client = helperAPI.GetSurveyAPI();
            HttpResponseMessage responseMessage = await client.GetAsync("SurveyAPI");
            if (responseMessage.IsSuccessStatusCode)
            {
                logger.LogInformation("Getting surveys from API(SurveyAPI) is successfull");
                var result = responseMessage.Content.ReadAsStringAsync().Result;
                surveyModel = JsonConvert.DeserializeObject<SurveyModel>(result);
            }
            return View(surveyModel);
        }

        public async Task<IActionResult> GetFilteredSurveys(SurveyParam surveyParam)
        {
            logger.LogInformation("Load filtered Surveys - Call API(SurveyAPI)");
            List<Survey> surveys = new List<Survey>();

            var projectFilter = JsonConvert.SerializeObject(surveyParam);
            var httpContent = new StringContent(projectFilter, Encoding.UTF8, "application/json");

            HttpClient client = helperAPI.GetSurveyAPI();
            HttpResponseMessage responseMessage = await client.PostAsync("SurveyAPI", httpContent);

            if (responseMessage.IsSuccessStatusCode)
            {
                logger.LogInformation("Load filtered Surveys - Call to API(SurveyAPI) successfull");
                var result = responseMessage.Content.ReadAsStringAsync().Result;
                surveys = JsonConvert.DeserializeObject<List<Survey>>(result);
            }

            return Json(new { data = surveys });
        }

        [HttpPost]
        public ActionResult Surveys(SurveyModel surveyModel)
        {
            logger.LogInformation("Redirect to AddSurvey action method");
            HttpContext.Session.SetInt32("SurveyID", surveyModel.SurveyId);
            HttpContext.Session.SetString("SurveyOperationType", JsonConvert.SerializeObject(surveyModel.surveyOperationType));
            return RedirectToAction("AddSurvey");
        }

        public ActionResult AddSurvey()
        {
            AddSurveyModel addSurveyModel = new();
            DataAccessLayer dal = new();
            SurveyOperationType surveyOperationType;
            int surveyId;

            // Get operation type from session 
            if (HttpContext.Session.GetString("SurveyOperationType") != null)
            {
                surveyOperationType = JsonConvert.DeserializeObject<SurveyOperationType>(HttpContext.Session.GetString("SurveyOperationType"));

                if (surveyOperationType == SurveyOperationType.AddNewSurvey)
                {
                    logger.LogInformation("Loging add survey page");
                    // Load new surey
                    surveyId = 0;
                    addSurveyModel.IsActive = true;

                    // Check if Survey details with question exists in session if exists remove 
                    if (HttpContext.Session.GetString("SurveyDetails") != null)
                    {
                        HttpContext.Session.Remove("SurveyDetails");
                    }
                }
                else if ((surveyOperationType == SurveyOperationType.EditSurvey))
                {
                    logger.LogInformation("Loging edit survey page");
                    if (HttpContext.Session.GetString("SurveyID") != null)
                    {
                        surveyId = (Int32)HttpContext.Session.GetInt32("SurveyID");
                        logger.LogInformation("Loging edit survey page for SurveyID" + surveyId);

                        //Get survey details using servey id 
                        addSurveyModel = dal.GetSurveyDetailsForASurvey(surveyId);

                        // Check if Survey details with question exists in session if exists remove 
                        if (HttpContext.Session.GetString("SurveyDetails") != null)
                        {
                            HttpContext.Session.Remove("SurveyDetails");
                        }

                        // Store survey details to session 
                        HttpContext.Session.SetString("SurveyDetails", JsonConvert.SerializeObject(addSurveyModel));
                    }
                }
                // remove SurveyOperationType from session 
                HttpContext.Session.Remove("SurveyOperationType");
            }
            else
            {
                // Get Survey details from session 
                if (HttpContext.Session.GetString("SurveyDetails") != null)
                {
                    addSurveyModel = JsonConvert.DeserializeObject<AddSurveyModel>(HttpContext.Session.GetString("SurveyDetails"));
                }
            }
            addSurveyModel.SurveyTypes = dal.GetSurveyTypes();
            return View(addSurveyModel);
        }

        /// <summary>
        /// Save survey along with questionnaires 
        /// </summary>
        /// <param name="newSurveyDetails"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddSurvey(AddSurveyModel newSurveyDetails)
        {
            SurveManager surveManager = new();
            AddSurveyModel? survey = new();
            DataAccessLayer dal = new();
            // Get Survey details from session 
            if (HttpContext.Session.GetString("SurveyDetails") != null)
            {
                logger.LogInformation("Submit the survey details");
                survey = JsonConvert.DeserializeObject<AddSurveyModel>(HttpContext.Session.GetString("SurveyDetails"));
                survey.Name = newSurveyDetails.Name;
                survey.SelectedSurveyType = newSurveyDetails.SelectedSurveyType;
                survey.Description = newSurveyDetails.Description;
                survey.StartDate = newSurveyDetails.StartDate;
                survey.EndDate = newSurveyDetails.EndDate;
                survey.IsActive = newSurveyDetails.IsActive;
                survey.UserIdentity = User.Identity?.Name;
                dal.SaveSurvey(survey);
            }
            return RedirectToAction("Surveys");
        }

        public PartialViewResult AddQuestionModelPopup(string id, string name, int surveyType, string description, int questionId, string stDate, string endDate, bool isActive, int sectionId)
        {
            int questionCounter = 0;
            AddSurveyModel addSurveyModel;
            // Get Survey details from session 
            if (HttpContext.Session.GetString("SurveyDetails") != null)
            {
                questionCounter = GetMaxQuestionCounter();

                /*addSurveyModel = JsonConvert.DeserializeObject<AddSurveyModel>(HttpContext.Session.GetString("SurveyDetails"));

                if (addSurveyModel.Sections.First().Questions == null)
                {
                    questionCounter = 0;
                }
                else
                {
                    if (addSurveyModel.Sections.First().Questions.Count == 0)
                        questionCounter = 0;
                    else
                        questionCounter = GetMaxQuestionCounter();//addSurveyModel.Sections.First().Questions.Max(x => x.QuestionId) + 1;// addSurveyModel.Questions.Count(); // need to modify to get max count  
                } */
            }
            else
            {
                questionCounter = 0;
                // Add survey to session 
                addSurveyModel = new()
                {
                    Id = Convert.ToInt32(id),
                    Name = name,
                    SelectedSurveyType = surveyType,
                    Description = description,
                    StartDate = stDate,
                    EndDate = endDate,
                    IsActive = isActive
                };

                // Update survey details 
                addSurveyModel.Name = name;
                addSurveyModel.SelectedSurveyType = surveyType;
                addSurveyModel.Description = description;
                addSurveyModel.StartDate = stDate;
                addSurveyModel.EndDate = endDate;
                addSurveyModel.IsActive = isActive;

                HttpContext.Session.SetString("SurveyDetails", JsonConvert.SerializeObject(addSurveyModel));
            }

            Question question = new();
            SurveManager surveManager = new();
            question = surveManager.AddDefaultQuestion(questionCounter);
            question.SectionId = sectionId == 0 ? GetMaxSectionCounter() : sectionId;
            question.DisplayOrder = GetQuestionMaxDisplayOrder(sectionId);
            return PartialView("_AddQuestion", question);
        }

        public PartialViewResult EditQuestionModelPopup(int questionId)
        {
            Question question = new();
            AddSurveyModel addSurveyModel;
            SurveManager surveManager = new();
            // Get Survey details from session 
            if (HttpContext.Session.GetString("SurveyDetails") != null)
            {
                addSurveyModel = JsonConvert.DeserializeObject<AddSurveyModel>(HttpContext.Session.GetString("SurveyDetails"));

                //// Mark edited 
                //addSurveyModel.Questions.Where(x => x.QuestionId == questionId).FirstOrDefault().IsEdited = true;
                //// Add back to session 
                //HttpContext.Session.SetString("SurveyDetails", JsonConvert.SerializeObject(addSurveyModel));

                //question = addSurveyModel.Questions.Where(x => x.QuestionId == questionId).FirstOrDefault();// need to modufy to get survey id and question id 
                question = GetQuestionUsingId(questionId);
                question.QuestionTypes = surveManager.GetQuestionTypes();
                question.ScaleLowValues = new List<int>() { 0, 1 };
                question.ScaleHighValues = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            }
            return PartialView("_AddQuestion", question);
        }

        public PartialViewResult AddSection(int QuestionId, int QuestionOrder, int SectionId, string id, string name, int surveyType, string description, string stDate, string endDate, bool isActive)
        {
            if (HttpContext.Session.GetString("SurveyDetails") == null)
            {
                AddSurveyModel addSurveyModel = new()
                {
                    Id = Convert.ToInt32(id),
                    Name = name,
                    SelectedSurveyType = surveyType,
                    Description = description,
                    StartDate = stDate,
                    EndDate = endDate,
                    IsActive = isActive
                };

                HttpContext.Session.SetString("SurveyDetails", JsonConvert.SerializeObject(addSurveyModel));
            }
            Section section = new()
            {
                IsNew = true,
                StartQueOrder = QuestionOrder,
                StartQueId = QuestionId,
                OldSectionId = SectionId
            };
            return PartialView("_AddSection", section);
        }

        public PartialViewResult EditSectionModelPopup(int sectionId)
        {
            Section? section = new();
            AddSurveyModel? addSurveyModel;
            SurveManager surveManager = new();
            // Get Survey details from session 
            if (HttpContext.Session.GetString("SurveyDetails") != null)
            {
                addSurveyModel = JsonConvert.DeserializeObject<AddSurveyModel>(HttpContext.Session.GetString("SurveyDetails"));
                section = addSurveyModel.Sections.Where(x => x.SectionId == sectionId).FirstOrDefault();

            }
            return PartialView("_AddSection", section);
        }

        public JsonResult DeleteSection(int sectionId)
        {
            SurveManager surveManager = new();

            // Get Survey details from session 
            if (HttpContext.Session.GetString("SurveyDetails") != null)
            {
                AddSurveyModel addSurveyModel = JsonConvert.DeserializeObject<AddSurveyModel>(HttpContext.Session.GetString("SurveyDetails"));

                if (addSurveyModel.Sections.Where(x => x.SectionId == sectionId).Count() > 0)
                {
                    addSurveyModel.Sections.Where(x => x.SectionId == sectionId).FirstOrDefault().IsDeleted = true;
                }

                // Add back to session 
                HttpContext.Session.SetString("SurveyDetails", JsonConvert.SerializeObject(addSurveyModel));
            }
            return Json(true);
        }

        public JsonResult SaveSection(Section Section)
        {
            SurveManager surveManager = new();
            // Get Survey details from session 
            if (HttpContext.Session.GetString("SurveyDetails") != null)
            {
                AddSurveyModel addSurveyModel = JsonConvert.DeserializeObject<AddSurveyModel>(HttpContext.Session.GetString("SurveyDetails"));

                // Add first section along with default section 
                if (addSurveyModel.Sections == null)
                {
                    List<Section> sections = new List<Section>();

                    Section defaultSection = new();
                    defaultSection.SectionId = 1;
                    defaultSection.SectionName = "Default";
                    sections.Add(defaultSection);

                    Section newSection = new();
                    newSection.SectionId = 2;
                    newSection.IsNew = Section.IsNew;
                    newSection.SectionName = Section.SectionName;
                    newSection.SectionDescription = Section.SectionDescription;
                    sections.Add(newSection);

                    addSurveyModel.Sections = sections;
                }
                else
                {
                    if (addSurveyModel.Sections.Where(x => x.SectionId == Section.SectionId).Count() > 0)
                    {
                        // edit existing section 
                        addSurveyModel.Sections.Where(x => x.SectionId == Section.SectionId).FirstOrDefault().SectionName = Section.SectionName;
                        addSurveyModel.Sections.Where(x => x.SectionId == Section.SectionId).FirstOrDefault().SectionDescription = Section.SectionDescription;
                    }
                    else
                    {
                        //Add new section 
                        Section newSection = new();
                        newSection.SectionId = addSurveyModel.Sections.Max(x => x.SectionId) + 1;
                        newSection.SectionName = Section.SectionName;
                        newSection.SectionDescription = Section.SectionDescription;
                        newSection.IsNew = Section.IsNew;

                        // Move Questions 
                        List<Question> newQuestions = new();
                        //newSection.Questions = MoveQuestions(addSurveyModel.Sections.Where(x=>x.SectionId == Section.OldSectionId).FirstOrDefault(), Section.StartQueOrder, Section.StartQueId);

                        foreach (Section section in addSurveyModel.Sections.Where(x => x.SectionId == Section.OldSectionId))
                        {
                            foreach (Question question in section.Questions.Where(x => x.DisplayOrder > Section.StartQueOrder))
                            {
                                newQuestions.Add(question);
                            }
                            if (section.Questions.Where(x => x.DisplayOrder > Section.StartQueOrder).Count() > 0)
                            {
                                addSurveyModel.Sections.Where(x => x.SectionId == Section.OldSectionId).FirstOrDefault().Questions.RemoveAll(x => x.DisplayOrder > Section.StartQueOrder);
                            }
                        }
                        newSection.Questions = newQuestions;
                        addSurveyModel.Sections.Add(newSection);
                    }
                }

                // Need to modify here
                /*if (addSurveyModel.Questions == null)
                {
                    List<Question> questions = new();
                    questions.Add(Question);
                    addSurveyModel.Questions = questions;
                }
                else
                {
                    // Check is question exists or not, if exists then modify 

                    if (addSurveyModel.Questions.Where(x => x.QuestionId == Question.QuestionId).Count() == 0)
                    {
                        // Add new question 
                        Question.IsEdited = true;
                        addSurveyModel.Questions.Add(Question);
                    }
                    else
                    {
                        // Edit existing question 
                        addSurveyModel.Questions.Where(x => x.QuestionId == Question.QuestionId).FirstOrDefault().QuestionName = Question.QuestionName;
                        addSurveyModel.Questions.Where(x => x.QuestionId == Question.QuestionId).FirstOrDefault().SelectedQuestionTypes = Question.SelectedQuestionTypes;
                        addSurveyModel.Questions.Where(x => x.QuestionId == Question.QuestionId).FirstOrDefault().Options = Question.Options;
                        addSurveyModel.Questions.Where(x => x.QuestionId == Question.QuestionId).FirstOrDefault().Scale = Question.Scale;
                        addSurveyModel.Questions.Where(x => x.QuestionId == Question.QuestionId).FirstOrDefault().IsEdited = true;
                    }

                } */
                // Add back to session 
                HttpContext.Session.SetString("SurveyDetails", JsonConvert.SerializeObject(addSurveyModel));
            }
            return Json(true);
        }

        public JsonResult SaveQuestion(Question Question)
        {
            SurveManager surveManager = new();
            // Get Survey details from session 
            if (HttpContext.Session.GetString("SurveyDetails") != null)
            {
                AddSurveyModel addSurveyModel = JsonConvert.DeserializeObject<AddSurveyModel>(HttpContext.Session.GetString("SurveyDetails"));

                // if no sections are found 
                if (addSurveyModel.Sections == null)
                {
                    // Add default section
                    List<Section> sections = new();
                    Section section = new()
                    {
                        SectionId = 0,
                        SectionName = "default",
                    };
                    sections.Add(section);
                    addSurveyModel.Sections = sections;

                    // Add new question 
                    List<Question> questions = new();
                    Question.IsEdited = true;
                    questions.Add(Question);
                    addSurveyModel.Sections.Where(x => x.SectionId == 0).FirstOrDefault().Questions = questions;
                }
                // Add question to default section (last)
                /*if (Question.SectionId == 0)
                {
                    // Add new question 
                    if (addSurveyModel.Sections[addSurveyModel.Sections.Count - 1].Questions == null)
                    {
                        List<Question> questions = new();
                        questions.Add(Question);
                        addSurveyModel.Sections[addSurveyModel.Sections.Count - 1].Questions = questions;
                    }
                    else
                    {
                        Question.IsEdited = true;
                        addSurveyModel.Sections[addSurveyModel.Sections.Count - 1].Questions.Add(Question);
                    }
                }*/
                else
                {
                    Question.SectionId = Question.SectionId == 0 ? GetMaxSectionCounter() : Question.SectionId;
                    // if section exists and no question exists
                    if (addSurveyModel.Sections.Where(x => x.SectionId == Question.SectionId).FirstOrDefault().Questions == null)
                    {
                        List<Question> questions = new();
                        Question.IsEdited = true;
                        questions.Add(Question);
                        addSurveyModel.Sections.Where(x => x.SectionId == Question.SectionId).FirstOrDefault().Questions = questions;
                    }
                    else
                    {
                        // If section id is not equal o then add question to specific section 
                        if (addSurveyModel.Sections.Where(x => x.SectionId == Question.SectionId).FirstOrDefault().Questions.Where(x => x.QuestionId == Question.QuestionId).Count() == 0)
                        {
                            // Add new question 
                            Question.IsEdited = true;
                            addSurveyModel.Sections.Where(x => x.SectionId == Question.SectionId).FirstOrDefault().Questions.Add(Question);
                        }
                        else
                        {
                            // Edit existing question 
                            addSurveyModel.Sections.Where(x => x.SectionId == Question.SectionId).FirstOrDefault().Questions.Where(x => x.QuestionId == Question.QuestionId).FirstOrDefault().QuestionName = Question.QuestionName;
                            addSurveyModel.Sections.Where(x => x.SectionId == Question.SectionId).FirstOrDefault().Questions.Where(x => x.QuestionId == Question.QuestionId).FirstOrDefault().SelectedQuestionTypes = Question.SelectedQuestionTypes;
                            addSurveyModel.Sections.Where(x => x.SectionId == Question.SectionId).FirstOrDefault().Questions.Where(x => x.QuestionId == Question.QuestionId).FirstOrDefault().AddComments = Question.AddComments;
                            addSurveyModel.Sections.Where(x => x.SectionId == Question.SectionId).FirstOrDefault().Questions.Where(x => x.QuestionId == Question.QuestionId).FirstOrDefault().Options = Question.Options;
                            addSurveyModel.Sections.Where(x => x.SectionId == Question.SectionId).FirstOrDefault().Questions.Where(x => x.QuestionId == Question.QuestionId).FirstOrDefault().Scale = Question.Scale;
                            addSurveyModel.Sections.Where(x => x.SectionId == Question.SectionId).FirstOrDefault().Questions.Where(x => x.QuestionId == Question.QuestionId).FirstOrDefault().IsEdited = true;
                        }
                    }
                }


                // Need to modify here
                /*if (addSurveyModel.Questions == null)
                {
                    List<Question> questions = new();
                    questions.Add(Question);
                    addSurveyModel.Questions = questions;
                }
                else
                {
                    // Check is question exists or not, if exists then modify 

                    if (addSurveyModel.Questions.Where(x => x.QuestionId == Question.QuestionId).Count() == 0)
                    {
                        // Add new question 
                        Question.IsEdited = true;
                        addSurveyModel.Questions.Add(Question);
                    }
                    else
                    {
                        // Edit existing question 
                        addSurveyModel.Questions.Where(x => x.QuestionId == Question.QuestionId).FirstOrDefault().QuestionName = Question.QuestionName;
                        addSurveyModel.Questions.Where(x => x.QuestionId == Question.QuestionId).FirstOrDefault().SelectedQuestionTypes = Question.SelectedQuestionTypes;
                        addSurveyModel.Questions.Where(x => x.QuestionId == Question.QuestionId).FirstOrDefault().Options = Question.Options;
                        addSurveyModel.Questions.Where(x => x.QuestionId == Question.QuestionId).FirstOrDefault().Scale = Question.Scale;
                        addSurveyModel.Questions.Where(x => x.QuestionId == Question.QuestionId).FirstOrDefault().IsEdited = true;
                    }

                } */
                // Add back to session 
                HttpContext.Session.SetString("SurveyDetails", JsonConvert.SerializeObject(addSurveyModel));
            }
            //HttpContext.Session.SetInt32("SurveyID", 1);
            return Json(true);
        }

        public JsonResult DeleteQuestion(int sectionId, int QuestionId)
        {
            SurveManager surveManager = new();

            // Get Survey details from session 
            if (HttpContext.Session.GetString("SurveyDetails") != null)
            {
                AddSurveyModel addSurveyModel = JsonConvert.DeserializeObject<AddSurveyModel>(HttpContext.Session.GetString("SurveyDetails"));

                if (addSurveyModel.Sections.Where(x => x.SectionId == sectionId).Count() > 0)
                {
                    addSurveyModel.Sections.Where(x => x.SectionId == sectionId).FirstOrDefault().Questions.Where(x => x.QuestionId == QuestionId).FirstOrDefault().IsDeleted = true;
                }

                // Add back to session 
                HttpContext.Session.SetString("SurveyDetails", JsonConvert.SerializeObject(addSurveyModel));
            }
            return Json(true);
        }

        public JsonResult DeleteSurvey(int id)
        {
            DataAccessLayer dal = new();
            dal.DeleteSurvey(id);
            return Json("");
        }

        private List<SelectListItem>? GetSurveyDropdown(List<Survey>? surveys)
        {
            List<SelectListItem> listSurveys = new List<SelectListItem>();
            SelectListItem surveyItem;
            foreach (Survey survey in surveys)
            {
                surveyItem = new SelectListItem { Text = survey.Type, Value = survey.Type };
                listSurveys.Add(surveyItem);
            }
            return listSurveys;
        }

        #region Utility
        private Question GetQuestionUsingId(int questionId)
        {
            try
            {
                AddSurveyModel addSurveyModel = JsonConvert.DeserializeObject<AddSurveyModel>(HttpContext.Session.GetString("SurveyDetails"));
                foreach (Section section in addSurveyModel.Sections)
                {
                    if (section.Questions != null)
                    {
                        if (section.Questions.Where(x => x.QuestionId == questionId).Count() > 0)
                        {
                            return section.Questions.Where(x => x.QuestionId == questionId).FirstOrDefault();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        private int GetMaxQuestionCounter()
        {
            int maxQuestionCounter = 0;
            AddSurveyModel addSurveyModel = JsonConvert.DeserializeObject<AddSurveyModel>(HttpContext.Session.GetString("SurveyDetails"));

            if (addSurveyModel.Sections == null)
                return 0;
            foreach (Section section in addSurveyModel.Sections)
            {
                if (section.Questions != null && section.Questions.Count > 0)
                {
                    maxQuestionCounter = section.Questions.Max(x => x.QuestionId) > maxQuestionCounter ? section.Questions.Max(x => x.QuestionId) : maxQuestionCounter;
                }
            }
            return maxQuestionCounter + 1;
        }

        private int GetMaxSectionCounter()
        {
            AddSurveyModel addSurveyModel = JsonConvert.DeserializeObject<AddSurveyModel>(HttpContext.Session.GetString("SurveyDetails"));

            if (addSurveyModel.Sections == null)
                return 0;
            else if (addSurveyModel.Sections.Count == 0)
                return 0;
            else
                return addSurveyModel.Sections.Max(x => x.SectionId);
        }
        private int GetQuestionMaxDisplayOrder(int sectionId)
        {
            int maxDisplayOrder = 1;
            AddSurveyModel addSurveyModel = JsonConvert.DeserializeObject<AddSurveyModel>(HttpContext.Session.GetString("SurveyDetails"));

            if (addSurveyModel.Sections == null)
                return maxDisplayOrder;

            if (addSurveyModel.Sections.Where(x => x.SectionId == sectionId).Count() > 0)
            {
                if (addSurveyModel.Sections.Where(x => x.SectionId == sectionId).FirstOrDefault().Questions != null)
                {
                    if (addSurveyModel.Sections.Where(x => x.SectionId == sectionId).FirstOrDefault().Questions.Count > 0)
                    {
                        maxDisplayOrder = addSurveyModel.Sections.Where(x => x.SectionId == sectionId).FirstOrDefault().Questions.Max(X => X.DisplayOrder) + 1;
                    }
                }
            }
            return maxDisplayOrder;
        }
        #endregion
    }
}
