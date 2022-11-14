namespace CRSurveyClient.Models
{
    public class SurveyModel
    {
        public string? UniqueId { get; set; }
        public int SurveyId { get; set; }
        public string? SurveyName { get; set; }
        public int ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? Description { get; set; }
        public int Type { get; set; }
        public Section? Section { get; set; }
        public List<Question>? Questions { get; set; }
        public Client? Client { get; set; }
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public ExecutionResponse? ExecutionResponse { get; set; }
        public EventType? EventType { get; set; }
    }

    public class Section
    {
        public int SectionId { get; set; }
        public string? SectionName { get; set; }
        public string? SectionDescription { get; set; }
    }

    public class Client
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Company { get; set; }
    }

    public class Question
    {
        public int QuestionId { get; set; }
        public string? QuestionName { get; set; }
        public int QuestionTypes { get; set; }
        public List<Option>? Options { get; set; }
        public Scale? Scale { get; set; }
        public bool IsResponded { get; set; }
        public int SelectedMultiChoiceOption { get; set; }
        public bool AddComments { get; set; }
        public string? TextResponse { get; set; }
        public string? Comments { get; set; }
    }

    public class Option
    {
        public int Id { get; set; }
        public string? OptionName { get; set; }
        public int QuestionId { get; set; }
        public bool IsSelected { get; set; }
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
}
