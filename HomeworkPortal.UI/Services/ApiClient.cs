namespace HomeworkPortal.UI.Services
{
    public class ApiClient
    {
        public readonly HttpClient Client;
        public ApiClient(HttpClient client) => Client = client;
    }
}