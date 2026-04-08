using Microsoft.AspNetCore.Mvc;
using Mss.WorkForce.Code.WMSSimulatorWeb.Pages.Shared;
using System.Text.Json;

namespace WMSSimulatorWeb.Pages
{
    public class WorkForceTaskModel : WorkForcePageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WorkForceTaskModel> _logger;
        public WorkForceTaskModel(IHttpClientFactory httpClientFactory, ILogger<WorkForceTaskModel> logger, IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor) // 🔹 Pasar IHttpContextAccessor al constructor base
        {
            _httpClient = httpClientFactory.CreateClient("WMSSimulatorAPI");
            _logger = logger;
        }

        public List<WorkForceTask> WorkForceTasks { get; set; } = new();

        [BindProperty]
        public List<Guid> SelectedOrders { get; set; } = new();
        public List<WarehouseData> Warehouses { get; set; } = new();

        [BindProperty]
        public string Action { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            try
            {

                var selectedWarehouse = SelectedWarehouse.ToString() ?? "";


                _logger.LogInformation($"🔍 Llamando a la API con warehouseId: {selectedWarehouse}");

                var response = await _httpClient.GetAsync($"workforcetasks?warehouseId={selectedWarehouse}");


                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"❌ Error al obtener datos: {response.StatusCode}");
                    return;
                }

                // Leer el contenido de la respuesta como JSON
                var jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"📥 Respuesta recibida: {jsonResponse}");

                // Deserializar la respuesta JSON
                var data = JsonSerializer.Deserialize<ApiResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (data == null)
                {
                    _logger.LogWarning("⚠️ La respuesta deserializada es NULL.");
                    return;
                }

                // Asignar los datos obtenidos
                WorkForceTasks = data.WorkForceTasks ?? new List<WorkForceTask>();
                Warehouses = data.Warehouses ?? new List<WarehouseData>();

                _logger.LogInformation($"✅ {WorkForceTasks.Count} WorkForceTasks y {Warehouses.Count} Warehouses cargados.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Excepción en OnGetAsync: {ex.Message}");
            }
        }




        public async Task<IActionResult> OnPostUpdateTaskAsync()
        {
            try
            {

                Action = Request.Form["Action"];

                if (Action == "Update")
                {
                    _logger.LogInformation("OnPostUpdateTaskAsync called.");

                    if (!Guid.TryParse(Request.Form["Id"], out Guid taskId))
                    {
                        _logger.LogWarning("⚠️ Invalid ID.");
                        ModelState.AddModelError("", "Invalid Task ID.");
                        return Page();
                    }

                    string warehouseIdStr = Request.Form["WarehouseId"];
                    if (!Guid.TryParse(warehouseIdStr, out Guid warehouseId))
                    {
                        _logger.LogWarning("⚠️ Invalid Warehouse ID.");
                        ModelState.AddModelError("", "Invalid Warehouse ID.");
                        return Page();
                    }

                    _logger.LogInformation($"📦 WarehouseId received: {warehouseId}");

                    var updateTask = new WorkForceTask
                    {
                        Id = taskId,
                        InitHour = TimeSpan.Parse(Request.Form["InitHour"]),
                        EndHour = TimeSpan.Parse(Request.Form["EndHour"]),
                        NumOrders = double.Parse(Request.Form["NumOrders"]),
                        NumOrdersCompleted = int.Parse(Request.Form["NumOrdersCompleted"]),
                        LinesPerOrder = int.Parse(Request.Form["LinesPerOrder"]),
                        IsOut = Request.Form["IsOut"] == "true",
                        WarehouseId = warehouseId,
                        Date = DateTime.Parse(Request.Form["Date"])
                    };

                    _logger.LogInformation($"📤 Sending data to API: {JsonSerializer.Serialize(updateTask)}");

                    var requestData = new TaskRequest
                    {
                        Action = "Update",
                        Data = new List<Guid> { taskId },
                        TaskData = updateTask
                    };

                    HttpResponseMessage response = await _httpClient.PostAsJsonAsync("workforcetasks", requestData);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("✅ Task updated successfully.");
                        return RedirectToPage("/WorkForceTasks");
                    }

                    string errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"❌ Error in API: {response.StatusCode} - {errorMsg}");
                    ModelState.AddModelError("", $"API Error: {response.StatusCode} - {errorMsg}");
                    return Page();
                }

                if (Action == "Delete")
                {
                    if (!Guid.TryParse(Request.Form["Id"], out Guid taskId))
                    {
                        _logger.LogWarning("⚠️ Invalid ID Task.");
                        ModelState.AddModelError("", "Invalid Task ID.");
                        return Page();
                    }

                    var requestData = new TaskRequest
                    {
                        Action = "Delete",
                        Data = new List<Guid> { taskId }
                    };

                    HttpResponseMessage response = await _httpClient.PostAsJsonAsync("workforcetasks", requestData);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToPage("/WorkForceTasks");
                    }

                    else
                    {
                        string errorMsg = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"❌ API Error: {response.StatusCode} - {errorMsg}");
                        return Page();
                    }


                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Exception in OnPostUpdateTaskAsync: {ex.Message}");
                ModelState.AddModelError("", $"Unexpected error: {ex.Message}");
                return Page();
            }

        }

        public async Task<IActionResult> OnPostAddTaskAsync()
        {
            try
            {
                _logger.LogInformation("📝 OnPostAddTaskAsync called.");

                // Validar datos del formulario
                if (!Guid.TryParse(Request.Form["WarehouseId"], out Guid warehouseId))
                {
                    _logger.LogWarning("⚠️ Invalid WarehouseId.");
                    ModelState.AddModelError("", "Invalid WarehouseId.");
                    return Page();
                }

                string initHourStr = Request.Form["InitHour"];
                string endHourStr = Request.Form["EndHour"];
                string numOrdersStr = Request.Form["NumOrders"];
                string linesPerOrderStr = Request.Form["LinesPerOrder"];
                string dateStr = Request.Form["Date"];

                if (!TimeSpan.TryParse(initHourStr, out TimeSpan initHour) ||
                    !TimeSpan.TryParse(endHourStr, out TimeSpan endHour) ||
                    !double.TryParse(numOrdersStr, out double numOrders) ||
                    !int.TryParse(linesPerOrderStr, out int linesPerOrder) ||
                    !DateTime.TryParse(dateStr, out DateTime date))
                {
                    ModelState.AddModelError("", "Invalid input data.");
                    return Page();
                }

                bool isOut = Request.Form["IsOut"] == "true";

                _logger.LogInformation("🔍 Calling API to obtain data...");

                // Realizar la solicitud HTTP GET
                var responseWarehouse = await _httpClient.GetAsync("workforcetasks");

                // Leer el contenido de la respuesta como JSON
                var jsonResponse = await responseWarehouse.Content.ReadAsStringAsync();

                _logger.LogInformation($"📥 Answer received: {jsonResponse}");

                // Deserializar la respuesta JSON
                var data = JsonSerializer.Deserialize<ApiResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                Warehouses = data.Warehouses ?? new List<WarehouseData>();

                _logger.LogInformation($"✅ {WorkForceTasks.Count} WorkForceTasks and {Warehouses.Count} Warehouses loaded.");
            
   

            var newTask = new WorkForceTask
                {
                    Id = Guid.NewGuid(),
                    InitHour = initHour,
                    EndHour = endHour,
                    NumOrders = numOrders,
                    NumOrdersCompleted = 0,
                    LinesPerOrder = linesPerOrder,
                    IsOut = isOut,
                    WarehouseId = warehouseId,
                    WarehouseCode = Warehouses.FirstOrDefault(x => x.Id == warehouseId).Code,
                    Date = date
                };

                var requestData = new TaskRequest
                {
                    Action = "Add",
                    TaskData = newTask
                };

                _logger.LogInformation($"📤 Sending data to API: {JsonSerializer.Serialize(requestData)}");

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("workforcetasks", requestData);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Task added successfully.");
                    return RedirectToPage("/WorkForceTasks");
                }

                string errorMsg = await response.Content.ReadAsStringAsync();
                _logger.LogError($"❌ Error in API: {response.StatusCode} - {errorMsg}");
                ModelState.AddModelError("", $"API Error: {response.StatusCode} - {errorMsg}");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Exception in OnPostAddTaskAsync: {ex.Message}");
                ModelState.AddModelError("", $"Unexpected error: {ex.Message}");
                return Page();
            }
        }

        public class ApiResponse
        {
            public List<WorkForceTask> WorkForceTasks { get; set; } = new();
            public List<WarehouseData> Warehouses { get; set; } = new();
        }

    }
}