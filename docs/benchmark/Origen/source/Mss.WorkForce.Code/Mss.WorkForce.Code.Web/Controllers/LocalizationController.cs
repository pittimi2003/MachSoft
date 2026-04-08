using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Mss.WorkForce.Code.Web.Services;

namespace Mss.WorkForce.Code.Web.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class LocalizationController : ControllerBase
    {
        private static readonly object _lock = new object();

        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;

        public LocalizationController(ILocalizationService localizationService, ILogger<LocalizationController> logger) 
        {
            _localizationService = localizationService;
            _logger = logger;
        }

        public IActionResult AddMissingKey([FromBody] MissingKeyRequest request) {
            try
            {
                lock (_lock)
                {
                    _localizationService.AddMissingKey(request.Key, request.Value);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding missing key '{Key}' to resource files", request?.Key ?? "null");
                return StatusCode(500, "An unexpected error occurred while adding the key");
            }
        }

        public IActionResult ChangeCulture(string culture, string redirectUri)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(redirectUri))
                    redirectUri = "~/";

                var cookieOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = Request.IsHttps,
                    IsEssential = true
                };

                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture, culture)),
                    cookieOptions
                );

                _logger.LogInformation("Culture set to: {Culture}, redirecting to: {RedirectUri}", culture, redirectUri);

                return LocalRedirect(redirectUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting culture {Culture}", culture);

                //En caso de error, redirigir sin cambiar cultura
                return LocalRedirect(string.IsNullOrWhiteSpace(redirectUri) ? "~/" : redirectUri);
            }
        }

    }

    public class MissingKeyRequest
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}

