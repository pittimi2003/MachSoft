using Microsoft.AspNetCore.Mvc;
using Mss.WorkForce.Code.WMSSimulator.WMSModel;
using Mss.WorkForce.Code.WMSSimulatorWeb.Pages.Shared;

namespace WMSSimulatorWeb.Pages
{
    public class SimulationResultsPage : WorkForcePageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SimulationResultsPage> _logger;

        public SimulationResultsPage(IHttpClientFactory httpClientFactory, ILogger<SimulationResultsPage> logger, IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor) 
        {
            _httpClient = httpClientFactory.CreateClient("WMSSimulatorAPI");
            _logger = logger;
        }

        public List<SimulationResults> simulationResults { get; set; } = new();

        public async Task<IActionResult> OnGetLoadDataAsync()
        {
            try
            {
                var selectedWarehouse = SelectedWarehouse.ToString() ?? "";

                var results = await _httpClient.GetFromJsonAsync<List<SimulationResults>>($"results?warehouseId={selectedWarehouse}") ?? new List<SimulationResults>();

                return new JsonResult(results); 
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = "Error loading data", message = ex.Message });
            }
        }

    }




}
