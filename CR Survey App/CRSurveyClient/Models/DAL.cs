using System.Data;
using System.Data.SqlClient;

namespace CRSurveyClient.Models
{
    public class DAL
    {
        public IConfiguration Configuration { get; set; }
        private string? ConnString { get; set; }

        internal DAL()
        {
            var builder = new ConfigurationBuilder()
            .AddJsonFile("appSettings.json");
            Configuration = builder.Build();
            ConnString = Configuration["ConnectionStrings:DefaultConnection"];
        }

        internal SurveyModel LoadSurvey(string uniqueID, string userIdentity, int pageIndex)
        {
            SurveyModel surveyModel = new();

            Section section = new();

            List<Question> questions = new();
            Question question;

            List<Option> options = new();
            Option option;

            List<Scale> scales = new();
            Scale scale;

            DataTable dtResponse = new DataTable();

            ExecutionResponse executionResponse = new();
            try
            {
                surveyModel.UniqueId = uniqueID;
                using (SqlConnection connection = new SqlConnection(ConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("dbo.usp_GetSurveyForClient"))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = connection;
                        cmd.Parameters.AddWithValue("@UniqueID", uniqueID);
                        cmd.Parameters.AddWithValue("@PageIndex", pageIndex);
                        cmd.Parameters.AddWithValue("@User", userIdentity == null ? "1" : userIdentity);
                        // Opening SQL Connection 
                        connection.Open();

                        cmd.Parameters.Add("@ExecutionStatus", SqlDbType.Bit);
                        cmd.Parameters["@ExecutionStatus"].Direction = ParameterDirection.Output;

                        cmd.Parameters.Add("@OutMessage", SqlDbType.VarChar, 200);
                        cmd.Parameters["@OutMessage"].Direction = ParameterDirection.Output;

                        // Executing the SQL query 
                        SqlDataReader sdr = cmd.ExecuteReader();

                        // Read survey details
                        while (sdr.Read())
                        {
                            surveyModel.SurveyId = Convert.ToInt32(sdr["ID"].ToString());
                            surveyModel.ProjectId = Convert.ToInt32(sdr["ProjectId"].ToString());
                            surveyModel.SurveyName = sdr["Name"].ToString();
                            surveyModel.Type = Convert.ToInt32(sdr["Type"].ToString());
                            surveyModel.Description = sdr["Description"].ToString();
                        }

                        // Read section 
                        if (sdr.NextResult())
                        {
                            while (sdr.Read())
                            {
                                section = new();
                                section.SectionId = Convert.ToInt32(sdr["ID"].ToString());
                                section.SectionName = sdr["SectionName"].ToString();
                                section.SectionDescription = sdr["Description"].ToString();
                                surveyModel.Section = section;
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
                                question.QuestionTypes = Convert.ToInt32(sdr["Type"].ToString());
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

                        // Read pagination 
                        if (sdr.NextResult())
                        {
                            while (sdr.Read())
                            {
                                surveyModel.TotalPages = Convert.ToInt32(sdr["TotalPages"].ToString());
                                surveyModel.PageIndex = Convert.ToInt32(sdr["CurrentPage"].ToString());
                            }
                        }

                        // Read response 
                        if (sdr.NextResult())
                        {
                            dtResponse.Load(sdr);
                        }

                        // Close the reader and get output parameters 
                        sdr.Close();
                        executionResponse.ExecutionStatus = Convert.ToBoolean(cmd.Parameters["@ExecutionStatus"].Value) == true ? ExecutionStatus.Success : ExecutionStatus.Fail;
                        executionResponse.Message = cmd.Parameters["@OutMessage"].Value.ToString();
                        surveyModel.ExecutionResponse = executionResponse;

                        // read option by questions and add to question list 
                        foreach (Question que in questions.Where(x => x.QuestionTypes == 1 || x.QuestionTypes == 2 || x.QuestionTypes == 3))
                        {
                            List<Option> questionOptions = new List<Option>();
                            foreach (Option op in options.Where(x => x.QuestionId == que.QuestionId))
                            {
                                questionOptions.Add(op);
                            }
                            questions.Where(x => x.QuestionId == que.QuestionId).First().Options = questionOptions;
                        }

                        // read scale by questions and add to question list 
                        foreach (Question que in questions.Where(x => x.QuestionTypes == 6))
                        {
                            questions.Where(x => x.QuestionId == que.QuestionId).First().Scale = scales.Where(x => x.QuestionId == que.QuestionId).First();
                        }

                        // Add questions to survey 
                        surveyModel.Questions = questions;

                        // Update previous submitted response foreach question
                        foreach (Question que in surveyModel.Questions)
                        {
                            if (dtResponse.AsEnumerable().Where(x => x.Field<int>("QuestionID").Equals(que.QuestionId)).Count() > 0)
                            {
                                if (que.QuestionTypes == 1 || que.QuestionTypes == 6 || que.QuestionTypes == 3)
                                {
                                    que.SelectedMultiChoiceOption = dtResponse.AsEnumerable().Where(x => x.Field<int>("QuestionID").Equals(que.QuestionId)).FirstOrDefault().Field<int>("OptionID");
                                    surveyModel.Questions.Where(x => x.QuestionId == que.QuestionId).FirstOrDefault().SelectedMultiChoiceOption = que.SelectedMultiChoiceOption;
                                }
                                else if (que.QuestionTypes == 4 || que.QuestionTypes == 5)
                                {
                                    que.TextResponse = dtResponse.AsEnumerable().Where(x => x.Field<int>("QuestionID").Equals(que.QuestionId)).FirstOrDefault().Field<string>("ResponseText");
                                    surveyModel.Questions.Where(x => x.QuestionId == que.QuestionId).FirstOrDefault().TextResponse = que.TextResponse;
                                }
                                else if (que.QuestionTypes == 2)
                                {
                                    foreach (Option op in que.Options)
                                    {
                                        if (dtResponse.AsEnumerable().Where(x => x.Field<int>("OptionID").Equals(op.Id)).Count() > 0)
                                        {
                                            surveyModel.Questions.Where(x => x.QuestionId == que.QuestionId).FirstOrDefault().Options.Where(y => y.Id == op.Id).FirstOrDefault().IsSelected = true;
                                        }
                                    }
                                }

                                //Get Comments 
                                surveyModel.Questions.Where(x => x.QuestionId == que.QuestionId).FirstOrDefault().Comments = dtResponse.AsEnumerable().Where(x => x.Field<int>("QuestionID").Equals(que.QuestionId)).FirstOrDefault().Field<string>("Comments");
                            }
                        }

                    }
                    connection.Close();
                }
            }
            catch (Exception)
            {

            }
            return surveyModel;
        }

        internal void PushResponsetoDB(SurveyModel response)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.usp_SaveSurveyResponse"))
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Name", response.Client.Name);
                        cmd.Parameters.AddWithValue("@EmailId", response.Client.Email);
                        cmd.Parameters.AddWithValue("@Company", response.Client.Company == null ? "" : response.Client.Company);
                        cmd.Parameters.AddWithValue("@SurveyId", response.SurveyId);
                        cmd.Parameters.AddWithValue("@ProjectID", response.ProjectId == null ? 0 : response.ProjectId);
                        cmd.Parameters.AddWithValue("@IsRespCompleted", response.EventType == EventType.Submit? 1 : 0);

                        var paramQuestions = new SqlParameter("@Response", SqlDbType.Structured);
                        paramQuestions.TypeName = "dbo.tblResponse";
                        paramQuestions.Value = QuestionListToDataTable(response);
                        cmd.Parameters.Add(paramQuestions);


                        cmd.ExecuteNonQuery();
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {

            }
        }

        internal DataTable QuestionListToDataTable(SurveyModel response)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("SurveyID");
            dt.Columns.Add("QuestionID");
            dt.Columns.Add("QuestionType");
            dt.Columns.Add("OptionID");
            dt.Columns.Add("QuestionTitle");
            dt.Columns.Add("ResponseText");
            dt.Columns.Add("Comments");

            if (response.Questions != null)
            {
                foreach (Question question in response.Questions)
                {
                    if (question.QuestionTypes == 2)
                    {
                        foreach (Option option in question.Options.Where(x => x.IsSelected == true))
                        {
                            var row = dt.NewRow();
                            row["SurveyID"] = response.SurveyId;
                            row["QuestionId"] = question.QuestionId;
                            row["QuestionType"] = question.QuestionTypes;
                            row["OptionId"] = option.Id;
                            row["Comments"] = question.Comments;
                            dt.Rows.Add(row);
                        }
                    }
                    else
                    {
                        var row = dt.NewRow();
                        row["SurveyID"] = response.SurveyId;
                        row["QuestionId"] = question.QuestionId;
                        row["QuestionType"] = question.QuestionTypes;
                        if (question.QuestionTypes == 1 || question.QuestionTypes == 3 || question.QuestionTypes == 6)
                            row["OptionId"] = question.SelectedMultiChoiceOption;
                        else if (question.QuestionTypes == 4 || question.QuestionTypes == 5)
                            row["ResponseText"] = question.TextResponse;
                        row["Comments"] = question.Comments;
                        dt.Rows.Add(row);
                    }
                }
            }
            return dt;
        }
    }
}
