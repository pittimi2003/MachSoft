using DevExpress.Blazor.Localization;

namespace Mss.WorkForce.Code.Web.Services.LocalizationServices
{
    public class LocalizationDevExpressService : DxLocalizationService, IDxLocalizationService
    {
        private readonly ILocalizationService _loc;

        public LocalizationDevExpressService(ILocalizationService localizationService)
        {
            _loc = localizationService;
        }

        string IDxLocalizationService.GetString(string key)
        {
            var appValue = _loc?.Loc(key);
            if (!string.IsNullOrEmpty(appValue) &&
                !string.Equals(appValue, key, StringComparison.Ordinal))
            {
                return appValue;
            }

            return base.GetString(key) ?? key;
        }

    }
}
