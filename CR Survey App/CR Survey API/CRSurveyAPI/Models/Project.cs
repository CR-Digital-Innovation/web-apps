namespace CRSurveyAPI.Models
{
    public class ProjectDetails
    {
        public List<ProjectNames>? Projects { get; set; }
        public List<ProjectStatus>? ProjectStatus { get; set; }
        public List<SurveyResponse>? SurveyResponse { get; set; }
        public List<Project>? ProjectsDetails { get; set; } 
        public SurveyStats? SurveyStats { get; set; }
    }
    public class SurveyStats
    {
        public int SurvySentCount { get; set; }
        public int SurvyCompletedCount { get; set; }
        public decimal AvgRating { get; set; }
        public int ProjectAtRiskCount { get; set; }
    }
    public class ProjectStatus
    {
        public int Id { get; set; }
        public string? Status { get; set; }
    }

    public class ProjectNames
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class SurveyResponse
    {
        public int Id { get; set; }
        public string? Response { get; set; }
    }

    public class ProjectParam
    {
        public int StatusId { get; set; }
        public int ProjectId { get; set; }
        public int ResponseId { get; set; }
    }
    public class Project
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Account { get; set; }
        public string? GoLiveDate { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public string? CreatedBy { get; set; }
        public string? SurveyResponse { get; set; }
        public string? ResponseDate { get; set; }
    }

}
