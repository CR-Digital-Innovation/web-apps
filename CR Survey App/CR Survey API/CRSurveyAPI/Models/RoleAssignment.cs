namespace CRSurveyAPI.Models
{
    public class RoleAssignmentFilter
    {
        public List<Role>? Roles { get; set; }
        public List<User>? Users { get; set; }
        public int SelectedRoleId { get; set; }
        public int SelectedUserId { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
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

}
