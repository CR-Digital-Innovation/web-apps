namespace CRSurveyApp.Models
{
    public class RoleModel
    {
        public List<Role>? Roles { get; set; }
        public int SelectedRoleId { get; set; }
    }
    public class RoleParam
    {
        public int SelectedRole { get; set; }
    }

    public class Role
    {
        public int Id { get; set; }
        public string? RoleName { get; set; }
        public string? Description { get; set; }
        public string? LastModifiedBy { get; set; }
        public string? LastModifiedOn { get; set; }
        public int Permission { get; set; }
    }

    public class AddRoleModel
    {
        public Role? Role { get; set; }
        public List<ProjectNames>? AvailableProjects { get; set; }
        public List<ProjectNames>? SelectedProjects { get; set; }
        public List<SurveyNames>? AvailableSurveys { get; set; }
        public List<SurveyNames>? SelectedSurveys { get; set; }
        public List<Permission>? Permissions { get; set; }
        public int SelectedPermission { get; set; }
        public List<int>? Projects { get; set; }
        public List<int>? Surveys { get; set; }
    }

    public class Permission
    {
        public int PermissionId { get; set; }
        public string? PermissionName { get; set; }
    }

}
