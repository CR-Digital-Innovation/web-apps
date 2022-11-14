using Microsoft.AspNetCore.Mvc.Rendering;

namespace CRSurveyApp.Models
{
    public class SurveyParam
    {
        public int SurveyType { get; set; }
    }

    public class SurveyModel
    {
        public List<SurveyType>? SurveyType { get; set; }
        public string? SelectedSurvey { get; set; }
        public int SurveyId { get; set; }
        public SurveyOperationType surveyOperationType { get; set; }
    }
    public class Survey
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public bool Active { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public int ResponseCount { get; set; }
    }
    public class SurveyNames
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
    public enum SurveyOperationType
    {
        AddNewSurvey = 1,
        EditSurvey = 2,
        LoadSessionSurvey = 3
    }

    public class AddSurveyModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? SelectedSurveyType { get; set; }
        public string? Description { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public bool IsActive { get; set; }
        public List<SurveyType>? SurveyTypes { get; set; }
        public string? UserIdentity { get; set; }
        
        public List<Section>? Sections { get; set; }

        //public List<Question>? Questions { get; set; }

        //public int SelectedQuestionId { get; set; }
        //public List<int>? ScaleLowValues { get; set; }
        //public List<int>? ScaleHighValues { get; set; }
        //public ActionType ActionType { get; set; } // One reference 
    }

    public class Section
    {
        public int SectionId { get; set; }
        public string? SectionName { get; set; }
        public string? SectionDescription { get; set; }
        public List<Question>? Questions { get; set; }
        public int StartQueOrder { get; set; }
        public int StartQueId{ get; set; }
        public int OldSectionId { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsNew { get; set; }
    }

    public class Question
    {
        public int QuestionId { get; set; }
        public int SectionId { get; set; }
        public string? QuestionName { get; set; }
        public List<QuestionType>? QuestionTypes { get; set; }
        public int SelectedQuestionTypes { get; set; }
        //public List<Option>? Options { get; set; }
        public List<string>? Options { get; set; }
        public Scale? Scale { get; set; }
        public List<int>? ScaleLowValues { get; set; }
        public List<int>? ScaleHighValues { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsEdited { get; set; }
        public int DisplayOrder { get; set; }
        public bool AddComments { get; set; }
    }

    public class Option
    {
        public int Id { get; set; }
        public string? OptionName { get; set; }
        public int QuestionId { get; set; }
    }

    public class QuestionType
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class Scale
    {
        public int Id { get; set; }
        public int MinScale { get; set; }
        public int MaxScale { get; set; }
        public string? MinLabel { get; set; }
        public string? MaxLabel { get; set; }
        public int QuestionId { get; set; }
    }

    public class NewQuestion
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int SelectedQuestionTypes { get; set; }
        public List<string>? Options { get; set; }
        public Scale? Scale { get; set; }
    }

    public class SurveyType
    {
        public int Id { get; set; }
        public string? Type { get; set; }
    }

    public enum ActionType
    {
        SaveSurvey = 1,
        AddQuestion = 2,
        EditQuestion = 3,
        DeleteQuestion = 4,
        ChangeOption = 5,
        AddOption = 6,
        AddOtherOption = 7
    }

    public class SurveManager
    {
        public Question AddDefaultQuestion(int questionCounter)
        {
            Question question = new()
            {
                QuestionId = questionCounter,
                QuestionTypes = GetQuestionTypes(),
                SelectedQuestionTypes = 1,
                ScaleLowValues = new List<int>() { 0, 1 },
                ScaleHighValues = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }
            };
            question.Options = new List<string>() { "option"};// AddDefaultMutichoiceOption();
            return question;
        }

        public List<Question> AddDefaultQuestions()
        {
            List<Question> questionsList = new List<Question>();
            Question question = new()
            {
                QuestionId = 0,
                QuestionTypes = GetQuestionTypes(),
                SelectedQuestionTypes = 1,
            };
            //question.Options = AddDefaultMutichoiceOption();
            questionsList.Add(question);
            return questionsList;
        }

        public List<Question> AddNewQuestion(List<Question> existingQuestions)
        {
            Question question = new()
            {
                QuestionId = existingQuestions.Count,
                QuestionTypes = GetQuestionTypes(),
                SelectedQuestionTypes = 1,
            };
            //question.Options = AddDefaultMutichoiceOption();
            existingQuestions.Add(question);
            return existingQuestions;
        }

        public List<QuestionType> GetQuestionTypes()
        {
            List<QuestionType> questionTypes = new List<QuestionType>()
            {
                new QuestionType { Id = 1, Name = "Multiple choice"},
                new QuestionType { Id = 2, Name = "Checkboxes"},
                new QuestionType { Id = 3, Name = "Dropdown"},
                new QuestionType { Id = 4, Name = "Short answer"},
                new QuestionType { Id = 5, Name = "Paragraph"},
                new QuestionType { Id = 6, Name = "Linear Scale"}
            };
            return questionTypes;
        }

        public List<Option> AddDefaultMutichoiceOption()
        {
            List<Option> optionList = new List<Option>();
            Option question = new()
            {
                Id = 0,
                OptionName = "option"
            };
            optionList.Add(question);
            return optionList;
        }

        public List<Option> AddNewMutichoiceOption(List<Option> options)
        {
            Option option = new()
            {
                Id = options.Count,
                OptionName = "option " + options.Count
            };
            options.Add(option);
            return options;
        }
    }
}
