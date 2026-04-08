using System.Text;

namespace Mss.WorkForce.Code.WFMConnector
{
    public class ApiServiceApiClient(HttpClient client)
    {
        public async Task<string?> PostNotification(string Request)
        {
            try
            {
                var content = new StringContent(Request, Encoding.UTF8);
                var response = await client.PostAsync("/recalculation", content);

                string? databaseResponse = await response.Content.ReadFromJsonAsync<string>();

                return databaseResponse;
            }

            catch (Exception ex)
            {
                return string.Empty;
            }
        }

    }
}
