using System;

namespace CRSurveyApp.Helper
{
    public class APIHelper
    {
        public IConfiguration Configuration { get; set; }
        private string? ApiUrl { get; set; }
        internal APIHelper()
        {
            var builder = new ConfigurationBuilder()
           .AddJsonFile("appSettings.json");
            Configuration = builder.Build();
            ApiUrl = Configuration["WebAPIs:CRSurveyAPI"];
        }
        public HttpClient GetSurveyAPI()
        {
            var client = new HttpClient();
            //client.BaseAddress = new Uri("https://crsurveyapiappservice.azurewebsites.net");
            client.BaseAddress = new Uri(ApiUrl);
            return client;
        }
    }
}
