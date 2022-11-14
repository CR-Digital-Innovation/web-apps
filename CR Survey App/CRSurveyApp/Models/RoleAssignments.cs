namespace CRSurveyApp.Models
{
    public class RoleAssignmentsModel
    {
        public List<Role>? Roles { get; set; }
        public List<Users>? Users { get; set; }
        public int SelectedRoleId { get; set; }
        public int SelectedUserId { get; set; }
    }
    public class RoleAssignmentParam
    {
        public int SelectedRole { get; set; }
        public int SelectedUser { get; set; }
    }
    public class RoleAssignments
    {
        public int Id { get; set; }
        public string? RoleName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
    }
    public class Users
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
    }

    public class AddRoleAssignmentModel
    {
        public RoleAssignments? RoleAssignment { get; set; }
        public List<Role>? Roles { get; set; }
        public int SelectedRole { get; set; }
        public string? UserIdentity { get; set; }
    }
}
