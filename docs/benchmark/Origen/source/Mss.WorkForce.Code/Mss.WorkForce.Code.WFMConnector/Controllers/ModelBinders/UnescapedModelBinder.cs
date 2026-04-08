using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Mss.WorkForce.Code.WFMConnector.Controllers.ModelBinders
{
    public class UnescapedHeaderModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext context)
        {

            if (!context.HttpContext.Request.Headers.TryGetValue(context.ModelName, out var values))
            {
                context.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            var rawValue = values.FirstOrDefault();

            if (rawValue == null)
            {
                context.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            var decoded = Uri.UnescapeDataString(rawValue);

            context.Result = ModelBindingResult.Success(decoded);
            return Task.CompletedTask;

        }
    }


    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromUnescapedHeaderAttribute : ModelBinderAttribute
    {
        public FromUnescapedHeaderAttribute()
            : base(typeof(UnescapedHeaderModelBinder))
        {
            BindingSource = BindingSource.Header;
        }
    }
}
