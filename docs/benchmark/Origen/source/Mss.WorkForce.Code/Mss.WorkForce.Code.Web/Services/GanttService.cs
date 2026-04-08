using Microsoft.JSInterop;
namespace Mss.WorkForce.Code.Web.Services
{

    public class GanttJsService : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private IJSObjectReference? _moduleGantt;
        private IJSObjectReference? _modulePivot;


        public GanttJsService(IJSRuntime jsRuntime) => _jsRuntime = jsRuntime;

        public async Task<IJSObjectReference> GetModuleAsync(IJSRuntime jsRuntime)
        {
            if (_moduleGantt == null)
                _moduleGantt = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/Gantt/mlx-gantt.js");
            
            return _moduleGantt;
        }

        public async Task<IJSObjectReference> GetModulePivotAsync(IJSRuntime jsRuntime)
        {
            if (_modulePivot == null)            
                _modulePivot = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/Gantt/mlx-filter.js");
            
            return _modulePivot;
        }

        public async ValueTask DisposeAsync()
        {
            if (_moduleGantt != null)            
                await _moduleGantt.DisposeAsync();
            

            if (_modulePivot != null)            
                await _modulePivot.DisposeAsync();
            
        }
    }

}
