using System.Data;
using System.Data.SqlClient;

namespace CRSurveyApp.Models
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

        internal List<SurveyType> GetSurveyTypes()
        {
            List<SurveyType> surveysTypes = new();
            SurveyType surveyType;
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
            }
            return surveysTypes;
        }

        internal AddSurveyModel GetSurveyDetailsForASurvey(int surveyId)
        {
            AddSurveyModel addSurveyModel = new();

            List<Section> sections = new();
            Section section;

            List<Question> questions = new();
            Question question;

            List<Option> options = new();
            Option option;

            List<Scale> scales = new();
            Scale scale;
            using (SqlConnection connection = new SqlConnection(ConnString))
            {
                using (SqlCommand cmd = new SqlCommand("dbo.usp_GetSurveyDetailsUsingSurveyID"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = connection;
                    cmd.Parameters.AddWithValue("@SurveyId", surveyId);
                    // Opening SQL Connection 
                    connection.Open();

                    // Executing the SQL query 
                    SqlDataReader sdr = cmd.ExecuteReader();

                    // Read survey details
                    while (sdr.Read())
                    {
                        addSurveyModel.Id = Convert.ToInt32(sdr["ID"].ToString());
                        addSurveyModel.Name = sdr["Name"].ToString();
                        addSurveyModel.SelectedSurveyType = Convert.ToInt32(sdr["Type"].ToString());
                        addSurveyModel.Description = sdr["Description"].ToString();
                        addSurveyModel.IsActive = Convert.ToBoolean(sdr["Active"].ToString());
                        addSurveyModel.StartDate = sdr["StartDate"].ToString();
                        addSurveyModel.EndDate = sdr["EndDate"].ToString();
                    }

                    // Read sections 
                    if (sdr.NextResult())
                    {
                        while (sdr.Read())
                        {
                            section = new();
                            section.SectionId = Convert.ToInt32(sdr["ID"].ToString());
                            section.SectionName = sdr["SectionName"].ToString();
                            section.SectionDescription = sdr["Description"].ToString();
                            sections.Add(section);
                        }
                    }

                    // Read questions 
                    if (sdr.NextResult())
                    {
                        while (sdr.Read())
                        {
                            question = new();
                            question.QuestionId = Convert.ToInt32(sdr["ID"].ToString());
                            question.QuestionName = sdr["Title"].ToString();
                            question.SelectedQuestionTypes = Convert.ToInt32(sdr["Type"].ToString());
                            question.SectionId = Convert.ToInt32(sdr["SectionID"].ToString());
                            question.DisplayOrder = Convert.ToInt32(sdr["DisplayOrder"].ToString());
                            question.AddComments = Convert.ToBoolean(sdr["AddComments"].ToString());
                            questions.Add(question);
                        }
                    }

                    // Read options 
                    if (sdr.NextResult())
                    {
                        while (sdr.Read())
                        {
                            option = new();
                            option.Id = Convert.ToInt32(sdr["ID"].ToString());
                            option.OptionName = sdr["Title"].ToString();
                            option.QuestionId = Convert.ToInt32(sdr["QuestionID"].ToString());
                            options.Add(option);
                        }
                    }

                    // Read scale 
                    if (sdr.NextResult())
                    {
                        while (sdr.Read())
                        {
                            scale = new();
                            scale.Id = Convert.ToInt32(sdr["ID"].ToString());
                            scale.MinScale = Convert.ToInt32(sdr["MinValue"].ToString());
                            scale.MaxScale = Convert.ToInt32(sdr["MaxValue"].ToString());
                            scale.MinLabel = sdr["MinValueLabel"].ToString();
                            scale.MaxLabel = sdr["MaxValueLabel"].ToString();
                            scale.QuestionId = Convert.ToInt32(sdr["QuestionID"].ToString());
                            scales.Add(scale);
                        }
                    }

                    // read option by questions and add to question list 
                    foreach (Question que in questions.Where(x => x.SelectedQuestionTypes == 1 || x.SelectedQuestionTypes == 2 || x.SelectedQuestionTypes == 3))
                    {
                        List<string> questionOptions = new List<string>();
                        foreach (Option op in options.Where(x => x.QuestionId == que.QuestionId))
                        {
                            questionOptions.Add(op.OptionName);
                        }
                        questions.Where(x => x.QuestionId == que.QuestionId).First().Options = questionOptions;
                    }

                    // read scale by questions and add to question list 
                    foreach (Question que in questions.Where(x => x.SelectedQuestionTypes == 6))
                    {
                        questions.Where(x => x.QuestionId == que.QuestionId).First().Scale = scales.Where(x => x.QuestionId == que.QuestionId).First();
                    }

                    // arrange questions by sections 
                    foreach (Section queSec in sections)
                    {
                        List<Question> arrangedQueList = new List<Question>();
                        //Question arrangeQue;
                        foreach (Question que in questions.Where(x => x.SectionId == queSec.SectionId))
                        {
                            //arrangeQue = new Question();
                            //arrangeQue = que;
                            arrangedQueList.Add(que);
                        }
                        sections.Where(x => x.SectionId == queSec.SectionId).First().Questions = arrangedQueList;
                    }

                    // Add questions to survey 
                    //addSurveyModel.Questions = questions;
                    addSurveyModel.Sections = sections;
                }
                connection.Close();
            }
            return addSurveyModel;
        }


        internal void SaveSurvey(AddSurveyModel addSurveyModel)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.usp_SaveSurvey"))
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@SurveyId", addSurveyModel.Id);
                        cmd.Parameters.AddWithValue("@SurveyName", addSurveyModel.Name);
                        cmd.Parameters.AddWithValue("@SurveyDescription", addSurveyModel.Description);
                        cmd.Parameters.AddWithValue("@SurveyType", addSurveyModel.SelectedSurveyType);
                        cmd.Parameters.AddWithValue("@User", addSurveyModel.UserIdentity == null ? "1" : addSurveyModel.UserIdentity);
                        cmd.Parameters.AddWithValue("@StDate", addSurveyModel.StartDate == null ? DBNull.Value : addSurveyModel.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", addSurveyModel.EndDate == null ? DBNull.Value : addSurveyModel.EndDate);
                        cmd.Parameters.AddWithValue("@Active", addSurveyModel.IsActive);
                        //cmd.Parameters.AddWithValue("@User", System.Security.Principal.User.Identity?.Name);

                        var paramSections = new SqlParameter("@Sections", SqlDbType.Structured);
                        paramSections.TypeName = "dbo.tblSections";
                        paramSections.Value = SectionListToDataTable(addSurveyModel.Sections);
                        cmd.Parameters.Add(paramSections);

                        var paramQuestions = new SqlParameter("@Questions", SqlDbType.Structured);
                        paramQuestions.TypeName = "dbo.tblQuestions";
                        paramQuestions.Value = QuestionListToDataTable(addSurveyModel.Sections);
                        cmd.Parameters.Add(paramQuestions);

                        var paramOptions = new SqlParameter("@Options", SqlDbType.Structured);
                        paramOptions.TypeName = "dbo.tblOptions";
                        paramOptions.Value = OptionListToDataTable(addSurveyModel.Sections);
                        cmd.Parameters.Add(paramOptions);

                        var paramScale = new SqlParameter("@Scale", SqlDbType.Structured);
                        paramScale.TypeName = "dbo.tblScale";
                        paramScale.Value = ScaleOptionListToDataTable(addSurveyModel.Sections);
                        cmd.Parameters.Add(paramScale);

                        cmd.ExecuteNonQuery();
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {

            }
        }

        internal DataTable SectionListToDataTable(List<Section>? sections)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("SectionId");
            dt.Columns.Add("Name");
            dt.Columns.Add("Description");
            dt.Columns.Add("IsEdited");
            dt.Columns.Add("IsDeleted");

            foreach (Section section in sections)
            {
                if(!section.IsNew || !section.IsDeleted)
                {
                    var row = dt.NewRow();
                    row["SectionId"] = section.SectionId;
                    row["Name"] = section.SectionName;
                    row["Description"] = section.SectionDescription;
                    row["IsDeleted"] = section.IsDeleted;
                    //row["IsEdited"] = question.IsEdited;
                    dt.Rows.Add(row);
                }            
            }
            return dt;
        }

        internal DataTable QuestionListToDataTable(List<Section>? sections)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("QuestionId");
            dt.Columns.Add("SectionId");
            dt.Columns.Add("Title");
            dt.Columns.Add("QuestionType");
            dt.Columns.Add("AddComments");
            dt.Columns.Add("DisplayOrder");
            dt.Columns.Add("IsEdited");
            dt.Columns.Add("IsDeleted");

            foreach (Section section in sections)
            {
                if (!section.IsNew || !section.IsDeleted)
                {
                    if (section.Questions != null)
                    {
                        foreach (Question question in section.Questions.Where(x => x.IsEdited || x.IsDeleted))
                        {
                            var row = dt.NewRow();
                            row["QuestionId"] = question.QuestionId;
                            row["SectionId"] = question.SectionId;
                            row["Title"] = question.QuestionName;
                            row["QuestionType"] = question.SelectedQuestionTypes;
                            row["AddComments"] = question.AddComments;
                            row["DisplayOrder"] = question.DisplayOrder;
                            row["IsEdited"] = question.IsEdited;
                            row["IsDeleted"] = question.IsDeleted;
                            dt.Rows.Add(row);
                        }
                    }
                }
            }
            return dt;
        }

        internal DataTable OptionListToDataTable(List<Section>? sections)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("QuestionId");
            dt.Columns.Add("Title");
            foreach (Section section in sections)
            {
                if (section.Questions != null)
                {
                    foreach (Question question in section.Questions.Where(x => x.SelectedQuestionTypes == 1 || x.SelectedQuestionTypes == 2 || x.SelectedQuestionTypes == 3))
                    {
                        if (question.IsEdited)
                        {
                            foreach (string option in question.Options)
                            {
                                var row = dt.NewRow();
                                row["QuestionId"] = question.QuestionId;
                                row["Title"] = option;
                                dt.Rows.Add(row);
                            }
                        }
                    }
                }
            }
            return dt;
        }
        internal DataTable ScaleOptionListToDataTable(List<Section>? sections)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("QuestionId");
            dt.Columns.Add("MinValue");
            dt.Columns.Add("MaxValue");
            dt.Columns.Add("MinValueLabel");
            dt.Columns.Add("MaxValueLabel");

            foreach (Section section in sections)
            {
                if (section.Questions != null)
                {
                    foreach (Question question in section.Questions.Where(x => x.SelectedQuestionTypes == 6))
                    {
                        if (question.IsEdited)
                        {
                            var row = dt.NewRow();
                            row["QuestionId"] = question.QuestionId;
                            row["MinValue"] = question.Scale.MinScale;
                            row["MaxValue"] = question.Scale.MaxScale;
                            row["MinValueLabel"] = question.Scale.MinLabel;
                            row["MaxValueLabel"] = question.Scale.MaxLabel;
                            dt.Rows.Add(row);
                        }
                    }
                }
            }
            return dt;
        }

        internal void DeleteSurvey(int id)
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("dbo.usp_DeleteSurvey"))
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@SurveyId", id);

                    cmd.ExecuteNonQuery();
                }

                conn.Close();
            }
        }

        #region Project
        internal List<ProjectStatus> GetProjectStatus()
        {
            List<ProjectStatus> projectStatuses = new();
            ProjectStatus ProjectStatus;
            using (SqlConnection connection = new(ConnString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("dbo.usp_GetProjectStatus"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = connection;

                    // Executing the SQL query 
                    SqlDataReader sdr = cmd.ExecuteReader();

                    while (sdr.Read())
                    {
                        ProjectStatus = new();
                        ProjectStatus.Id = Convert.ToInt32(sdr["ID"].ToString());
                        ProjectStatus.Status = sdr["Name"].ToString();
                        projectStatuses.Add(ProjectStatus);
                    }
                }
            }
            return projectStatuses;
        }

        internal List<ProjectType> GetProjectType()
        {
            List<ProjectType> projectTypes = new();
            ProjectType projectType;

            using (SqlConnection connection = new(ConnString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("dbo.usp_GetProjectType"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = connection;

                    // Executing the SQL query 
                    SqlDataReader sdr = cmd.ExecuteReader();

                    while (sdr.Read())
                    {
                        projectType = new();
                        projectType.Id = Convert.ToInt32(sdr["ID"].ToString());
                        projectType.Type = sdr["Name"].ToString();
                        projectTypes.Add(projectType);
                    }
                }
            }
            return projectTypes;
        }

        internal AddProject GetProjectDetails(int projectId)
        {
            AddProject addProject = new AddProject();

            List<CustomerContacts> contacts = new();
            CustomerContacts contact;

            try
            {
                if (projectId == 0)
                {
                    // add default project contact if new project 
                    contact = new()
                    {
                        ContactId = 0
                    };
                    contacts.Add(contact);
                    addProject.CustomerContacts = contacts;
                }
                else
                {
                    using (SqlConnection connection = new SqlConnection(ConnString))
                    {
                        using (SqlCommand cmd = new SqlCommand("dbo.usp_GetProjectDetailsUsingProjectID"))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Connection = connection;
                            cmd.Parameters.AddWithValue("@ProjectId", projectId);
                            // Opening SQL Connection 
                            connection.Open();

                            // Executing the SQL query 
                            SqlDataReader sdr = cmd.ExecuteReader();

                            // Read survey details
                            while (sdr.Read())
                            {
                                addProject.Id = Convert.ToInt32(sdr["ID"].ToString());
                                addProject.Name = sdr["Name"].ToString();
                                addProject.SelectedProjectStatus = Convert.ToInt32(sdr["Status"].ToString());
                                addProject.SelectedProjectType = Convert.ToInt32(sdr["Type"].ToString());
                                addProject.Description = sdr["Description"].ToString();
                            }

                            // Read customer contacts  
                            if (sdr.NextResult())
                            {
                                while (sdr.Read())
                                {
                                    contact = new();
                                    contact.ContactId = Convert.ToInt32(sdr["Id"].ToString());
                                    contact.FirstName = sdr["FirstName"].ToString();
                                    contact.LastName = sdr["LastName"].ToString();
                                    contact.Email = sdr["Email"].ToString();
                                    contacts.Add(contact);
                                }
                            }
                            // Add contacts
                            addProject.CustomerContacts = contacts;
                        }
                        connection.Close();
                    }
                }

                // Get Project status values for dropdown bind 
                addProject.ProjectStatus = GetProjectStatus();

                // Get Project type values for dropdown bind
                addProject.ProjectType = GetProjectType();
            }
            catch (Exception)
            {

            }

            return addProject;
        }

        internal void SaveProject(AddProject addProject)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.usp_SaveProject"))
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ProjectId", addProject.Id);
                        cmd.Parameters.AddWithValue("@ProjectName", addProject.Name);
                        cmd.Parameters.AddWithValue("@Status", addProject.SelectedProjectStatus);
                        cmd.Parameters.AddWithValue("@Type", addProject.SelectedProjectType);
                        cmd.Parameters.AddWithValue("@ProjectStartDate", String.IsNullOrEmpty(addProject.StartDate) ? "" : addProject.StartDate);
                        cmd.Parameters.AddWithValue("@ProjectDescription", String.IsNullOrEmpty(addProject.Description) ? "" : addProject.Description);
                        cmd.Parameters.AddWithValue("@User", addProject.UserIdentity == null ? "1" : addProject.UserIdentity);

                        var paramContacts = new SqlParameter("@CustomerContacts", SqlDbType.Structured);
                        paramContacts.TypeName = "dbo.tblCustomerContacts";
                        paramContacts.Value = ContactsListToDataTable(addProject.CustomerContacts);
                        cmd.Parameters.Add(paramContacts);

                        cmd.ExecuteNonQuery();
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {

            }
        }

        internal void DeleteProject(int id)
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("dbo.usp_DeleteProject"))
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@ProjectId", id);

                    cmd.ExecuteNonQuery();
                }

                conn.Close();
            }
        }

        internal SendSurveyModel GetSendSurveyDetails(int projectId)
        {
            SendSurveyModel sendSurvey = new();
            sendSurvey.ProjectId = projectId;

            List<Survey> surveys = new();
            Survey survey;

            List<CustomerContacts> contacts = new();
            CustomerContacts contact;
            using (SqlConnection connection = new(ConnString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("dbo.usp_GetSurveysAndContacts"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = connection;
                    cmd.Parameters.AddWithValue("@ProjectId", projectId);

                    // Executing the SQL query 
                    SqlDataReader sdr = cmd.ExecuteReader();

                    while (sdr.Read())
                    {
                        survey = new();
                        survey.Id = Convert.ToInt32(sdr["ID"].ToString());
                        survey.Name = sdr["Name"].ToString();
                        surveys.Add(survey);
                    }
                    sendSurvey.Surveys = surveys;

                    if (sdr.NextResult())
                    {
                        while (sdr.Read())
                        {
                            contact = new();
                            contact.ContactId = Convert.ToInt32(sdr["Id"].ToString());
                            contact.FirstName = sdr["FirstName"].ToString();
                            contact.LastName = sdr["LastName"].ToString();
                            contact.Email = sdr["Email"].ToString();
                            contact.FullName = sdr["FirstName"].ToString() + " " + sdr["LastName"].ToString();
                            contacts.Add(contact);
                        }
                    }
                    sendSurvey.CustomerContacts = contacts;
                }
            }
            return sendSurvey;
        }

        internal SurveyEmailContent GetEmailContent(int surveyId)
        {
            SurveyEmailContent content = new SurveyEmailContent();
            using (SqlConnection connection = new(ConnString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("dbo.usp_GetEmailContent"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = connection;
                    cmd.Parameters.AddWithValue("@SurveyId", surveyId);

                    // Executing the SQL query 
                    SqlDataReader sdr = cmd.ExecuteReader();

                    while (sdr.Read())
                    {
                        content.Id = Convert.ToInt32(sdr["ID"].ToString());
                        content.SurveyId = Convert.ToInt32(sdr["SurveyId"].ToString());
                        content.Subject = sdr["Subject"].ToString();
                        content.UniqueID = Guid.NewGuid().ToString("N");
                        content.Body = sdr["Body"].ToString() + "/" + content.UniqueID;
                    }
                }
            }
            return content;
        }


        internal void SaveSentSurvey(SendSurveyModel sendSurveyModel)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.usp_SaveSurveySent"))
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@SurveyId", sendSurveyModel.SelectedSurvey);
                        cmd.Parameters.AddWithValue("@ProjectId", sendSurveyModel.ProjectId);
                        cmd.Parameters.AddWithValue("@ContactEmail", sendSurveyModel.SelectedContacts);
                        cmd.Parameters.AddWithValue("@Subject", sendSurveyModel.Subject);
                        cmd.Parameters.AddWithValue("@Body", sendSurveyModel.Body);
                        cmd.Parameters.AddWithValue("@EmailSent", sendSurveyModel.EmailSent);
                        cmd.Parameters.AddWithValue("@User", "1");
                        cmd.Parameters.AddWithValue("@UniqueID", sendSurveyModel.UniqueID == null ? "" : sendSurveyModel.UniqueID);
                        //cmd.Parameters.AddWithValue("@User", System.Security.Principal.User.Identity?.Name);

                        cmd.ExecuteNonQuery();
                    }

                    conn.Close();
                }
            }
            catch (Exception)
            { }

        }
        internal DataTable ContactsListToDataTable(List<CustomerContacts>? contacts)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ContactId");
            dt.Columns.Add("FirstName");
            dt.Columns.Add("LastName");
            dt.Columns.Add("Email");

            foreach (CustomerContacts contact in contacts)
            {
                var row = dt.NewRow();
                row["ContactId"] = contact.ContactId;
                row["FirstName"] = contact.FirstName;
                row["LastName"] = contact.LastName;
                row["Email"] = contact.Email;
                dt.Rows.Add(row);
            }
            return dt;
        }

        #endregion

        #region Roles
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
            catch (Exception)
            {

            }
            return roles;
        }

        internal AddRoleModel GetRoleDetails(int roleId)
        {
            AddRoleModel addRoleModel = new();
            Role role = new();

            List<ProjectNames> availableProjects = new();
            ProjectNames project;
            List<SurveyNames> availablesurveys = new();
            SurveyNames survey;

            List<ProjectNames> assignedProjects = new();
            List<SurveyNames> assignedSurveys = new();

            List<Permission> permissions = new();
            Permission permission;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("dbo.usp_GetRoleDetails"))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = connection;
                        cmd.Parameters.AddWithValue("@RoleId", roleId);
                        // Opening SQL Connection 
                        connection.Open();

                        // Executing the SQL query 
                        SqlDataReader sdr = cmd.ExecuteReader();

                        //if (roleId != 0)
                        //{
                        // Read survey details
                        while (sdr.Read())
                        {
                            role.Id = Convert.ToInt32(sdr["ID"].ToString());
                            role.RoleName = sdr["Role"].ToString();
                            role.Description = sdr["Description"].ToString();
                            role.Permission = Convert.ToInt32(sdr["Permission"].ToString());
                        }
                        addRoleModel.Role = role;

                        // Get assigned projects & surveys 
                        // Read assigned projects 
                        if (sdr.NextResult())
                        {
                            while (sdr.Read())
                            {
                                project = new();
                                project.Id = Convert.ToInt32(sdr["ID"].ToString());
                                project.Name = sdr["Name"].ToString();
                                assignedProjects.Add(project);
                            }
                            addRoleModel.SelectedProjects = assignedProjects;
                        }
                        // Read assigned surveys 
                        if (sdr.NextResult())
                        {
                            while (sdr.Read())
                            {
                                survey = new();
                                survey.Id = Convert.ToInt32(sdr["ID"].ToString());
                                survey.Name = sdr["Name"].ToString();
                                assignedSurveys.Add(survey);
                            }
                            addRoleModel.SelectedSurveys = assignedSurveys;
                        }
                        // Get unassigned projects & surveys 
                        // Read unassigned(available) projects 
                        if (sdr.NextResult())
                        {
                            while (sdr.Read())
                            {
                                project = new();
                                project.Id = Convert.ToInt32(sdr["ID"].ToString());
                                project.Name = sdr["Name"].ToString();
                                availableProjects.Add(project);
                            }
                            addRoleModel.AvailableProjects = availableProjects;
                        }

                        // Read unassigned(available) surveys 
                        if (sdr.NextResult())
                        {
                            while (sdr.Read())
                            {
                                survey = new();
                                survey.Id = Convert.ToInt32(sdr["ID"].ToString());
                                survey.Name = sdr["Name"].ToString();
                                availablesurveys.Add(survey);
                            }
                            addRoleModel.AvailableSurveys = availablesurveys;
                        }
                        // Read permissions 
                        if (sdr.NextResult())
                        {
                            while (sdr.Read())
                            {
                                permission = new();
                                permission.PermissionId = Convert.ToInt32(sdr["ID"].ToString());
                                permission.PermissionName = sdr["Permission"].ToString();
                                permissions.Add(permission);
                            }
                            addRoleModel.Permissions = permissions;
                        }
                        //}
                        //else
                        //{
                        //    // Get default role 
                        //    Role defaultRole = new Role()
                        //    {
                        //        Id = 0,
                        //        RoleName="",
                        //        Description="",
                        //        Permission = 0
                        //    };
                        //    addRoleModel.Role = defaultRole;

                        //    // Get unassigned projects & surveys 
                        //    // Read unassigned projects 
                        //    while (sdr.Read())
                        //    {
                        //        project = new();
                        //        project.Id = Convert.ToInt32(sdr["ID"].ToString());
                        //        project.Name = sdr["Name"].ToString();
                        //        projects.Add(project);
                        //    }
                        //    addRoleModel.AvailableProjects = projects;

                        //    // Read unassigned surveys 
                        //    if (sdr.NextResult())
                        //    {
                        //        while (sdr.Read())
                        //        {
                        //            survey = new();
                        //            survey.Id = Convert.ToInt32(sdr["ID"].ToString());
                        //            survey.Name = sdr["Name"].ToString();
                        //            surveys.Add(survey);
                        //        }
                        //        addRoleModel.AvailableSurveys = surveys;
                        //    }
                        //    // Read permissions 
                        //    if (sdr.NextResult())
                        //    {
                        //        while (sdr.Read())
                        //        {
                        //            permission = new();
                        //            permission.PermissionId = Convert.ToInt32(sdr["ID"].ToString());
                        //            permission.PermissionName = sdr["Permission"].ToString();
                        //            permissions.Add(permission);
                        //        }
                        //        addRoleModel.Permissions = permissions;
                        //    }
                        //}
                    }
                    connection.Close();
                }
            }
            catch (Exception)
            {

            }
            return addRoleModel;
        }

        internal void SaveRole(AddRoleModel addRoleModel)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.usp_SaveRole"))
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@RoleId", addRoleModel.Role.Id);
                        cmd.Parameters.AddWithValue("@RoleName", addRoleModel.Role.RoleName);
                        cmd.Parameters.AddWithValue("@Description", addRoleModel.Role.Description);
                        cmd.Parameters.AddWithValue("@Permission", addRoleModel.SelectedPermission);
                        cmd.Parameters.AddWithValue("@User", "1");
                        //cmd.Parameters.AddWithValue("@User", System.Security.Principal.User.Identity?.Name);
                        cmd.Parameters.AddWithValue("@SelectedSurveys", string.Join<int>(",", addRoleModel.Surveys));
                        cmd.Parameters.AddWithValue("@SelectedProjects", string.Join<int>(",", addRoleModel.Projects));

                        cmd.ExecuteNonQuery();
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region RoleAssignment
        internal void SaveRoleAssignment(AddRoleAssignmentModel addRoleAssignmentModel)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.usp_SaveRoleAssignment"))
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@UserName", addRoleAssignmentModel.RoleAssignment.UserName);
                        cmd.Parameters.AddWithValue("@Email", addRoleAssignmentModel.RoleAssignment.Email);
                        cmd.Parameters.AddWithValue("@RoleId", addRoleAssignmentModel.SelectedRole);
                        cmd.Parameters.AddWithValue("@User", addRoleAssignmentModel.UserIdentity == null ? "1" : addRoleAssignmentModel.UserIdentity);

                        cmd.ExecuteNonQuery();
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion
    }
}
