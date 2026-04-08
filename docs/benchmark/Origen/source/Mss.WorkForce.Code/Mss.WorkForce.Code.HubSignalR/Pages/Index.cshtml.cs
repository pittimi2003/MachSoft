using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Mss.WorkForce.Code.HubSignalR.SignalR;
using Mss.WorkForce.Code.Models.SignalR;

namespace Mss.WorkForce.Code.HubSignalR.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IHubContext<SignalServerHub> _hubContext;

    [BindProperty]
    public GanttView SelectedView { get; set; }

    public IEnumerable<SelectListItem> GanttViews { get; set; }

    public IndexModel(ILogger<IndexModel> logger, IHubContext<SignalServerHub> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    public void OnGet()
    {
        GanttViews = Enum.GetValues(typeof(GanttView))
                       .Cast<GanttView>()
                       .Select(v => new SelectListItem
                       {
                           Value = v.ToString(),
                           Text = v.ToString()
                       });

        if (TempData["LastSelectedView"] is string viewString &&
        Enum.TryParse(viewString, out GanttView parsedView))
        {
            SelectedView = parsedView;
        }

    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _hubContext.Clients.All.SendAsync(SignalREventNames.ReloadDataGantt, SelectedView.ToString());

        // Por ahora solo redireccionamos
        TempData["Message"] = $"Notification sent for: {SelectedView}";
        TempData["LastSelectedView"] = SelectedView.ToString();
        return RedirectToPage();
    }
}
