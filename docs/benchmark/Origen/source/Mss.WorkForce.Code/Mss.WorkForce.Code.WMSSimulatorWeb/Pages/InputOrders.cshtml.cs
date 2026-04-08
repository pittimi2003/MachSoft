using Microsoft.AspNetCore.Mvc;
using Mss.WorkForce.Code.WMSSimulatorWeb.Pages.Shared;

namespace WMSSimulatorWeb.Pages
{
    public class InputOrdersModel : WorkForcePageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<InputOrdersModel> _logger;

        public InputOrdersModel(IHttpClientFactory httpClientFactory, ILogger<InputOrdersModel> logger, IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor) 
        {
            _httpClient = httpClientFactory.CreateClient("WMSSimulatorAPI");
            _logger = logger;
        }
        public List<InputOrder> InputOrders { get; set; } = new();

        [BindProperty]
        public List<Guid> SelectedOrders { get; set; } = new(); 

        [BindProperty]
        public string Action { get; set; } = string.Empty; 

        public async Task OnGetAsync()
        {
            try
            {

                var selectedWarehouse = SelectedWarehouse.ToString() ?? "";

                InputOrders = await _httpClient.GetFromJsonAsync<List<InputOrder>>($"inputorders?warehouseId={selectedWarehouse}") ?? new List<InputOrder>();
                InputOrders = InputOrders.OrderBy(p => p.OrderCode).ToList();

                foreach(InputOrder inputOrder in InputOrders)
                {
                    inputOrder.Progress = inputOrder.Progress;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error in GET: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync()
        {
            try
            {
                var requestData = new TableRequest { Data = SelectedOrders, Action = Action };

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("inputorders", requestData);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/InputOrders"); 
                }
                else
                {
                    _logger.LogError($"Error in POST: {response.StatusCode}");
                    return Page();
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error in POST: {ex.Message}");
                return Page();
            }
        }
    }

    public class InputOrder
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public bool IsOut { get; set; }
        public string? PreferedDockCode { get; set; }
        public string Status { get; set; } = string.Empty;
        public double Progress { get; set; }
        public int NumLines { get; set; }
        public DateTime UpdateDate { get; set; }
    }

  
}
