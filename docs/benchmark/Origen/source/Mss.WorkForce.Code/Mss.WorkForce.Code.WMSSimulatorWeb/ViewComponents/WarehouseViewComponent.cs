using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Mss.WorkForce.Code.WMSSimulatorWeb.Pages.Shared;

namespace Mss.WorkForce.Code.WMSSimulatorWeb.ViewComponents
{
    public class WarehouseViewComponent : ViewComponent
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<WarehouseViewComponent> _logger;

        public WarehouseViewComponent(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<WarehouseViewComponent> logger)
        {
            _httpClient = httpClientFactory.CreateClient("WMSSimulatorAPI");
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(string? warehouseId = null)
        {
            List<WarehouseData> warehouses = new();

            try
            {
                _logger.LogInformation("📡 Load list of warehouses from API...");

                warehouses = await _httpClient.GetFromJsonAsync<List<WarehouseData>>("warehouses") ?? new List<WarehouseData>();

                _logger.LogInformation($"✅ {warehouses.Count} warehouses obtained.");

                string sessionWarehouseId = _httpContextAccessor.HttpContext?.Session.GetString("SelectedWarehouse");

                _logger.LogInformation($"📦 Warehouse in Session before update: {sessionWarehouseId}");

                if (!string.IsNullOrEmpty(warehouseId) && warehouseId != "default")
                {
                    _httpContextAccessor.HttpContext?.Session.SetString("SelectedWarehouse", warehouseId);
                    sessionWarehouseId = warehouseId;
                    _logger.LogInformation($"✅ New warehouse saved in Session: {warehouseId}");
                }

                if (string.IsNullOrEmpty(sessionWarehouseId) || sessionWarehouseId == "default" && warehouses.Count > 0)
                {
                    _httpContextAccessor.HttpContext?.Session.Clear();
                    sessionWarehouseId = warehouses.First().Id.ToString();
                    _httpContextAccessor.HttpContext?.Session.SetString("SelectedWarehouse", sessionWarehouseId);
                    _logger.LogInformation($"🔄 No warehouse in Session. First will be assigned: {sessionWarehouseId}");
                }

                warehouses = warehouses
                    .OrderByDescending(w => w.Id.ToString() == sessionWarehouseId)
                    .ThenBy(w => w.Code)
                    .ToList();

                _logger.LogInformation($"🔄 Lista of Warehouses resorted, {sessionWarehouseId} is first.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"❌ Error obtaining warehouses: {ex.Message}");
            }

            return View(warehouses);
        }
    }

    }
