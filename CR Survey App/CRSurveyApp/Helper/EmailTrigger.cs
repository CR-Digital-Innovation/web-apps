using CRSurveyApp.Models;
using System.Net.Mail;

namespace CRSurveyApp.Helper
{
    public class EmailTrigger
    {
        public static bool SendEmail(SendSurveyModel SendSurveyModel)
        {
            try
            {
                SmtpClient emailClient = new SmtpClient();

                emailClient.Port = 587;
                emailClient.EnableSsl = true;
                emailClient.Host = "smtp.office365.com";
                emailClient.UseDefaultCredentials = false;
                emailClient.Credentials = new System.Net.NetworkCredential("email id", "password");

                emailClient.Send("email id", SendSurveyModel.SelectedContacts, SendSurveyModel.Subject, SendSurveyModel.Body);
                emailClient.Dispose();
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}
