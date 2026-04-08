using Microsoft.AspNetCore.Mvc;
using Mss.WorkForce.Code.WMSSimulatorWeb.Pages.Shared;

namespace WMSSimulatorWeb.Pages
{
    public class IndexModel : WorkForcePageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IndexModel> _logger;

        public List<Parameter> Parameters { get; set; } = new();

        [BindProperty]
        public Parameter NewParameter { get; set; } = new();

        public List<string> AvailableCodes { get; set; } = new();

        [BindProperty]
        public string SelectedCode { get; set; } = string.Empty;

        [BindProperty]
        public string NewCode { get; set; } = string.Empty;

        public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger, IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor) 
        {
            _httpClient = httpClientFactory.CreateClient("WMSSimulatorAPI");
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var updatedList = await _httpClient.GetFromJsonAsync<List<Parameter>>("parameters") ?? new List<Parameter>();

                Parameters = updatedList.OrderBy(p => p.Code).ToList();
                
                AvailableCodes = Parameters.Select(p => p.Code).OrderBy(c => c).ToList();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error in GET: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnPostAddParameterAsync()
        {
            

            try
            {
                if (SelectedCode == "new")
                {
                    NewParameter.Code = NewCode;
                }
                else
                {
                    NewParameter.Code = SelectedCode;
                }

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("parameters", NewParameter);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Index");
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

    public class Parameter
    {
        public decimal Value { get; set; }  
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

    }
}
