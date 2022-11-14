using CRSurveyAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRSurveyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleAPI : ControllerBase
    {
        [HttpGet(Name = "GetRoleFilters")]
        public RoleFilter GetRoleFilters()
        {
            DataAccessLayer dataAccessLayer = new();
            RoleFilter roleFilter = new();
            roleFilter.Roles = dataAccessLayer.GetDistinctRoles();
            return roleFilter;
        }

        [HttpPost(Name = "GetRoles")]
        public List<Role> GetRoles(RoleParam roleParam)
        {
            DataAccessLayer dataAccessLayer = new();
            return dataAccessLayer.GetRoles(roleParam);;
        }
    }
}
