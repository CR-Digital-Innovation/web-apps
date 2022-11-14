namespace CRSurveyApp.Models
{
    public class ProjectFilter
    {
        public List<ProjectNames>? Projects { get; set; }
        public List<ProjectStatus>? ProjectStatus { get; set; }
        public List<SurveyResponse>? SurveyResponse { get; set; }
        public ProjectParam? ProjectParam { get; set; }
        public int SelectedProjectId { get; set; }
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

    public class ProjectType
    {
        public int Id { get; set; }
        public string? Type { get; set; }
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

    public class AddProject
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<ProjectStatus>? ProjectStatus { get; set; }
        public int SelectedProjectStatus { get; set; }
        public List<ProjectType>? ProjectType { get; set; }
        public int SelectedProjectType { get; set; }
        public string? Customer { get; set; }
        public string? StartDate { get; set; }
        public string? Description { get; set; }
        public List<CustomerContacts>? CustomerContacts { get; set; }
        public string? UserIdentity { get; set; }
    }

    public class CustomerContacts
    {
        public int ContactId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }

    }

    public class SendSurveyModel
    {
        public int ProjectId { get; set; }
        public List<Survey>? Surveys { get; set; }
        public int SelectedSurvey { get; set; }
        public List<CustomerContacts>? CustomerContacts { get; set; }
        public string SelectedContacts { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public bool EmailSent { get; set; } 
        public string? UniqueID { get; set; }
    }

    public class SurveyEmailContent
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public string? UniqueID { get; set; }
    }
}
