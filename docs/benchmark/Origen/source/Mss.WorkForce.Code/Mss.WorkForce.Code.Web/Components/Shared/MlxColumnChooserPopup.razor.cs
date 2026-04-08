using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Mss.WorkForce.Code.Web.Common;

namespace Mss.WorkForce.Code.Web.Components.Shared
{
    public partial class MlxColumnChooserPopup : IDisposable
    {
        
        [Parameter]
        public List<ColumnChoose>? ColumnChooseProperties { get; set; }

        [Parameter]
        public bool Visible { get; set; }

        [Parameter]
        public EventCallback<bool> VisibleChanged { get; set; }

        [Parameter]
        public EventCallback<string[]> OnSave { get; set; }

        [Parameter]
        public EventCallback OnCancel { get; set; }

        [Parameter]
        public EventCallback OnClose { get; set; }


        private bool _wasSaved = false;

        private const string JsInitFunction = "initSingleContainerReorder";
        private const string JsCleanupFunction = "cleanupSingleContainerReorder";
        private const string JsGetReorderedFieldsFunction = "getReorderedFieldNames";

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (Visible)
            {
                await JS.InvokeVoidAsync(JsInitFunction);
            }
        }

        private async Task HandlePopupClosed()
        {
            if (!_wasSaved)
            {
                await OnClose.InvokeAsync();
            }

            _wasSaved = false;
        }

        private async Task SaveAndNotify()
        {
            _wasSaved = true;
            var reorderedFieldNames = await JS.InvokeAsync<string[]>(JsGetReorderedFieldsFunction);
            await OnSave.InvokeAsync(reorderedFieldNames);
        }

        public void Dispose()
        {
            _ = JS.InvokeVoidAsync(JsCleanupFunction);
        }
    }
}
