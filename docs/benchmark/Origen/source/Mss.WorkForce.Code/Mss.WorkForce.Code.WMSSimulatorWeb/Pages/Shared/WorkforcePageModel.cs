using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Mss.WorkForce.Code.WMSSimulatorWeb.Pages.Shared
{
    public class WorkForcePageModel : PageModel
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WorkForcePageModel(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public string SelectedWarehouse { get; private set; } = "No selected";

        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            if (_httpContextAccessor.HttpContext?.Session != null)
            {
                SelectedWarehouse = _httpContextAccessor.HttpContext.Session.GetString("SelectedWarehouse") ?? "No selected";
            }
            base.OnPageHandlerExecuting(context);
        }

        [IgnoreAntiforgeryToken]
        public IActionResult OnGetSetWarehouse(string warehouseId)
        {
            if (string.IsNullOrEmpty(warehouseId))
            {
                return BadRequest(new { success = false, message = "Warehouse ID is required" });
            }

            _httpContextAccessor.HttpContext?.Session.SetString("SelectedWarehouse", warehouseId);
            return new JsonResult(new { success = true, warehouseId });
        }
    }
}
