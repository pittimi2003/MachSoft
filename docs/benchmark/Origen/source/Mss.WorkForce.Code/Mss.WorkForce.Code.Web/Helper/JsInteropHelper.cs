using Microsoft.JSInterop;

namespace Mss.WorkForce.Code.Web.Helper
{
    public static class JsInteropHelper
    {
		public static ValueTask CallJs(this IJSRuntime js, string group, string method, params object[] args) { return js.InvokeVoidAsync("invokeFunction", new object[] { group, method }.Concat(args).ToArray()); }

		//public static ValueTask<T> CallJs<T>(this IJSRuntime js, string group, string method, params object[] args) => js.InvokeAsync<T>("invokeFunction", new object[] { group, method }.Concat(args).ToArray());
		
	}
}
