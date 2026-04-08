
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    namespace Mss.WorkForce.Code.WMSSimulatorWeb.Controllers
    {
        [Route("ServiceControl")]
        public class ServiceController : Controller
        {
            private readonly HttpClient _httpClient;
            private readonly ILogger<ServiceController> _logger;

            public ServiceController(IHttpClientFactory httpClientFactory, ILogger<ServiceController> logger)
            {
                _httpClient = httpClientFactory.CreateClient("WMSSimulatorAPI");
                _logger = logger;
            }

            [HttpPost("SendServiceAction")]
            public async Task<IActionResult> SendServiceAction([FromBody] ServiceActionRequest actionRequest)
            {
                if (actionRequest == null || string.IsNullOrEmpty(actionRequest.Action))
                {
                    return BadRequest(new { success = false, message = "No valid action." });
                }

                _logger.LogInformation($"Sending action '{actionRequest.Action}' to backend...");

                var json = JsonSerializer.Serialize(actionRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    var response = await _httpClient.PostAsync("/service", content);
                    var responseData = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"Action '{actionRequest.Action}' executed successfully.");
                        return Ok(new { success = true, message = "Action executed successfully.", action = actionRequest.Action });
                    }
                    else
                    {
                        _logger.LogError($"Error in action '{actionRequest.Action}': {response.StatusCode}");
                        return BadRequest(new { success = false, message = "Error in server." });
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError($"Connection error: {ex.Message}");
                    return BadRequest(new { success = false, message = "Connection error with server." });
                }
            }

        [HttpGet("GetServiceStatus")]
        public async Task<IActionResult> GetServiceStatus()
        {
            try
            {
                var response = await _httpClient.GetAsync("/service");
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    bool isRunning = JsonSerializer.Deserialize<bool>(jsonResponse);
                    _logger.LogInformation($"Service status: {(isRunning ? "ON" : "OFF")}");
                    return Ok(new { success = true, isRunning });
                }
                else
                {
                    _logger.LogError($"Error obtaining status service: {response.StatusCode}");
                    return BadRequest(new { success = false, message = "Error obtaining status service." });
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Connection error: {ex.Message}");
                return BadRequest(new { success = false, message = "Connection error with server." });
            }
        }
    }

        public class ServiceActionRequest
        {
            public string Action { get; set; }
        }
    }

