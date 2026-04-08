using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mss.WorkForce.Code.WMSSimulatorWeb.ViewComponents
{
    public class ServiceControlViewComponent : ViewComponent
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ServiceControlViewComponent> _logger;

        public ServiceControlViewComponent(IHttpClientFactory httpClientFactory, ILogger<ServiceControlViewComponent> logger)
        {
            _httpClient = httpClientFactory.CreateClient("WMSSimulatorAPI");
            _logger = logger;
        }

        public IViewComponentResult Invoke()
        {
            return View();
        }

        public async Task<JsonResult> SendServiceAction(string action)
        {
            _logger.LogInformation($"📡 Sending action '{action}' to server...");

            var requestBody = new { action = action };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("/service", content);
                var responseData = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"✅ Action '{action}' executed successfully.");
                    return new JsonResult(new { success = true, message = "Action executed successfully.", action = action });
                }
                else
                {
                    _logger.LogError($"❌ Error in action '{action}': {response.StatusCode}");
                    return new JsonResult(new { success = false, message = "Error in server." });
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"❌ Connection error: {ex.Message}");
                return new JsonResult(new { success = false, message = "Connection error with server." });
            }
        }
    }
}
