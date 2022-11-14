using CRSurveyAPI.Models;
using System.Data;
using System.Data.SqlClient;

namespace CRSurveyAPI
{
    public class DataAccessLayer
    {
        public IConfiguration Configuration { get; set; }
        private string? ConnString { get; set; }

        internal DataAccessLayer()
        {
            var builder = new ConfigurationBuilder()
            .AddJsonFile("appSettings.json");
            Configuration = builder.Build();
            ConnString = Configuration["ConnectionStrings:DefaultConnection"];
        }

        #region Survey
        internal SurveyDetails GetSurveyFilters()
        {
            SurveyDetails surveyDetails = new();

            List<SurveyType> surveysTypes = new();
            SurveyType surveyType;

            //Get Survey types 
            using (SqlConnection connection = new(ConnString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("dbo.usp_GetSurveyTypes"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = connection;

                    // Executing the SQL query 
                    SqlDataReader sdr = cmd.ExecuteReader();

                    while (sdr.Read())
                    {
                        surveyType = new();
                        surveyType.Id = Convert.ToInt32(sdr["Id"].ToString());
                        surveyType.Type = sdr["Type"].ToString();
                        surveysTypes.Add(surveyType);
                    }
                }
                connection.Close();
            }
            surveyDetails.SurveyType = surveysTypes;

            return surveyDetails;
        }

        internal List<Survey> GetFilteredSurveys(SurveyParam surveyParam)
        {
            List<Survey> surveys = new();
            Survey survey;

            // Get surveys 
            using (SqlConnection connection = new(ConnString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("dbo.usp_GetFilteredSurveys"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = connection;

                    cmd.Parameters.AddWithValue("@Type", surveyParam.SurveyType);

                    // Executing the SQL query 
                    SqlDataReader sdr = cmd.ExecuteReader();

                    while (sdr.Read())
                    {
                        survey = new();
                        survey.Id = Convert.ToInt32(sdr["ID"].ToString());
                        survey.Name = sdr["Name"].ToString();
                        survey.Type = sdr["Type"].ToString();
                        survey.Description = sdr["Description"].ToString();
                        survey.Active = Convert.ToBoolean(sdr["Active"].ToString());
                        survey.CreatedBy = sdr["CreatedBy"].ToString();
                        survey.CreatedOn = sdr["CreatedOn"].ToString();
                        survey.ResponseCount = Convert.ToInt32(sdr["ResponseCount"].ToString());
                        surveys.Add(survey);
                    }
                }
                connection.Close();
            }
            return surveys;
        }
        #endregion

        #region Project
        internal ProjectDetails GetProjectFilters()
        {
            ProjectDetails projectFilter = new();

            List<ProjectStatus> projectStatuses = new();
            ProjectStatus projectStatus;

            List<ProjectNames> projects = new();
            ProjectNames project;

            List<SurveyResponse> responses = new();
            SurveyResponse response;

            SurveyStats surveyStats;
            // Get project status 
            using (SqlConnection connection = new(ConnString))
            {
                connection.Open();
                using SqlCommand cmd = new SqlCommand("dbo.usp_GetProjectStatus");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = connection;

                // Executing the SQL query 
                SqlDataReader sdr = cmd.ExecuteReader();

                while (sdr.Read())
                {
                    projectStatus = new();
                    projectStatus.Id = Convert.ToInt32(sdr["ID"].ToString());
                    projectStatus.Status = sdr["Name"].ToString();
                    projectStatuses.Add(projectStatus);
                }
                connection.Close();
            }

            // Get projects 
            using (SqlConnection connection = new(ConnString))
            {
                connection.Open();
                using SqlCommand cmd = new SqlCommand("dbo.usp_GetDistinctProject");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = connection;

                // Executing the SQL query 
                SqlDataReader sdr = cmd.ExecuteReader();

                while (sdr.Read())
                {
                    project = new();
                    project.Id = Convert.ToInt32(sdr["ID"].ToString());
                    project.Name = sdr["Name"].ToString();
                    projects.Add(project);
                }
                connection.Close();
            }

            // Get projects 
            using (SqlConnection connection = new(ConnString))
            {
                connection.Open();
                using SqlCommand cmd = new SqlCommand("dbo.usp_GetSurveyResponse");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = connection;

                // Executing the SQL query 
                SqlDataReader sdr = cmd.ExecuteReader();

                while (sdr.Read())
                {
                    response = new();
                    response.Id = Convert.ToInt32(sdr["ID"].ToString());
                    response.Response = sdr["Response"].ToString();
                    responses.Add(response);
                }
                connection.Close();
            }

            // Get stats 
            using (SqlConnection connection = new(ConnString))
            {
                connection.Open();
                using SqlCommand cmd = new SqlCommand("dbo.usp_GetSurveyStats");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = connection;

                // Executing the SQL query 
                SqlDataReader sdr = cmd.ExecuteReader();

                while (sdr.Read())
                {
                    surveyStats = new SurveyStats();
                    surveyStats.SurvySentCount = Convert.ToInt32(sdr["SurveySent"].ToString());
                    surveyStats.SurvyCompletedCount = Convert.ToInt32(sdr["CompletedSurvey"].ToString());
                    surveyStats.AvgRating = Convert.ToDecimal(sdr["AvgRating"].ToString());
                    surveyStats.ProjectAtRiskCount = Convert.ToInt32(sdr["RiskCount"].ToString());
                    projectFilter.SurveyStats = surveyStats;
                }
                connection.Close();
            }

            projectFilter.ProjectStatus = projectStatuses;
            projectFilter.Projects = projects;
            projectFilter.SurveyResponse = responses;
            
            return projectFilter;
        }

        internal List<Project> GetProjectDetails(ProjectParam projectParam)
        {
            List<Project> projects = new();
            Project project;
            using (SqlConnection connection = new(ConnString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("dbo.usp_GetProjectDetails"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = connection;

                    cmd.Parameters.AddWithValue("@StatusId", projectParam.StatusId);
                    cmd.Parameters.AddWithValue("@ProjectId", projectParam.ProjectId);
                    cmd.Parameters.AddWithValue("@ResponseId", projectParam.ResponseId);

                    // Executing the SQL query 
                    SqlDataReader sdr = cmd.ExecuteReader();

                    while (sdr.Read())
                    {
                        project = new();
                        project.Id = Convert.ToInt32(sdr["ID"].ToString());
                        project.Name = sdr["Name"].ToString();
                        project.Account = sdr["Account"].ToString();
                        project.GoLiveDate = sdr["GoLiveDate"].ToString();
                        project.Status = sdr["Status"].ToString();
                        project.Type = sdr["Type"].ToString();
                        project.CreatedBy = sdr["CreatedBy"].ToString();
                        project.SurveyResponse = sdr["SurveyResponse"].ToString();
                        project.ResponseDate = sdr["ResponseDate"].ToString();
                        projects.Add(project);
                    }
                }
            }
            return projects;
        }
        #endregion

        #region Role 
        internal List<Role> GetDistinctRoles()
        {
            List<Role> roles = new(); 
            Role role;
            try
            {
                using (SqlConnection connection = new(ConnString))
                {
                    connection.Open();
                    using SqlCommand cmd = new SqlCommand("dbo.usp_GetDistinctRoles");
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = connection;

                    // Executing the SQL query 
                    SqlDataReader sdr = cmd.ExecuteReader();

                    while (sdr.Read())
                    {
                        role = new();
                        role.Id = Convert.ToInt32(sdr["ID"].ToString());
                        role.RoleName = sdr["Role"].ToString();
                        roles.Add(role);
                    }
                    connection.Close();
                }
            }
            catch(Exception)
            {

            }
            return roles;
        }
        internal List<Role> GetRoles(RoleParam roleParam)
        {
            List<Role> roles = new();
            Role role;
            try
            {
                using (SqlConnection connection = new(ConnString))
                {
                    connection.Open();
                    using SqlCommand cmd = new SqlCommand("dbo.usp_GetRoles");
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = connection;
                    cmd.Parameters.AddWithValue("@RoleId", roleParam.SelectedRole);

                    // Executing the SQL query 
                    SqlDataReader sdr = cmd.ExecuteReader();

                    while (sdr.Read())
                    {
                        role = new();
                        role.Id = Convert.ToInt32(sdr["ID"].ToString());
                        role.RoleName = sdr["Role"].ToString();
                        role.Description = sdr["Description"].ToString();
                        role.LastModifiedBy = sdr["LastModifiedBy"].ToString();
                        role.LastModifiedOn = sdr["LastModifiedOn"].ToString();
                        roles.Add(role);
                    }
                    connection.Close();
                }
            }
            catch (Exception)
            {

            }
            return roles;
        }
        #endregion

        #region RoleAssignment
        internal List<User> GetDistinctUsers()
        {
            List<User> users = new();
            User user;
            try
            {
                using (SqlConnection connection = new(ConnString))
                {
                    connection.Open();
                    using SqlCommand cmd = new SqlCommand("dbo.usp_GetDistinctUsers");
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = connection;

                    // Executing the SQL query 
                    SqlDataReader sdr = cmd.ExecuteReader();

                    while (sdr.Read())
                    {
                        user = new();
                        user.Id = Convert.ToInt32(sdr["ID"].ToString());
                        user.UserName = sdr["Name"].ToString();
                        users.Add(user);
                    }
                    connection.Close();
                }
            }
            catch (Exception)
            {

            }
            return users;
        }

        internal List<RoleAssignments> GetRoleAssignments(RoleAssignmentParam roleAssignmentParam)
        {
            List<RoleAssignments> rolesAssignments = new();
            RoleAssignments rolesAssignment;
            try
            {
                using (SqlConnection connection = new(ConnString))
                {
                    connection.Open();
                    using SqlCommand cmd = new SqlCommand("dbo.usp_GetRoleAssignments");
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = connection;
                    cmd.Parameters.AddWithValue("@RoleId", roleAssignmentParam.SelectedRole);
                    cmd.Parameters.AddWithValue("@UserId", roleAssignmentParam.SelectedUser);

                    // Executing the SQL query 
                    SqlDataReader sdr = cmd.ExecuteReader();

                    while (sdr.Read())
                    {
                        rolesAssignment = new();
                        rolesAssignment.Id = Convert.ToInt32(sdr["ID"].ToString());
                        rolesAssignment.RoleName = sdr["Role"].ToString();
                        rolesAssignment.UserName = sdr["Name"].ToString();
                        rolesAssignment.Email = sdr["EmailID"].ToString();
                        rolesAssignment.CreatedBy = sdr["CreatedBy"].ToString();
                        rolesAssignment.CreatedOn = sdr["CreatedOn"].ToString();
                        rolesAssignments.Add(rolesAssignment);
                    }
                    connection.Close();
                }
            }
            catch (Exception)
            {

            }
            return rolesAssignments;
        }
        #endregion
    }
}
