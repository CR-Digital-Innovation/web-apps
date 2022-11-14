namespace CRSurveyAPI.Models
{
    public class RoleFilter
    {
        public List<Role>? Roles { get; set; }
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
    }


}
