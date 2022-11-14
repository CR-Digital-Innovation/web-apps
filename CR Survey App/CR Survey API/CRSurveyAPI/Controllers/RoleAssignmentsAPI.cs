using CRSurveyAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRSurveyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleAssignmentsAPI : ControllerBase
    {
        [HttpGet(Name = "GetRoleAssignmentsFilters")]
        public RoleAssignmentFilter GetRoleAssignmentsFilters()
        {
            DataAccessLayer dataAccessLayer = new();
            RoleAssignmentFilter roleFilter = new()
            {
                Roles = dataAccessLayer.GetDistinctRoles(),
                Users = dataAccessLayer.GetDistinctUsers()
            };
            return roleFilter;
        }

        [HttpPost(Name = "GetRoleAssignments")]
        public List<RoleAssignments> GetRoleAssignments(RoleAssignmentParam roleAssignmentParam)
        {
            DataAccessLayer dataAccessLayer = new();
            return dataAccessLayer.GetRoleAssignments(roleAssignmentParam);
        }
    }
}
