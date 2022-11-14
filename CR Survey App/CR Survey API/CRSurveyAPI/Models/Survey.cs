namespace CRSurveyAPI.Models
{
    public class SurveyParam
    {
        public int SurveyType { get; set; }
    }

    public class SurveyDetails
    {
        public List<SurveyType>? SurveyType { get; set; }
    }

    public class SurveyType
    {
        public int Id { get; set; }
        public string? Type { get; set; }
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
}
