using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Web.Services;
using System.Security.Claims;

namespace Mss.WorkForce.Code.Web.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly DataAccess _dataAccess;
        private readonly IInitialDataService _initialDataService;

        public AuthController(DataAccess dataAccess, IInitialDataService initialDataService)
        {
            _dataAccess = dataAccess;
            _initialDataService = initialDataService;
        }

        /// <summary>
        /// Endpoint principal de login.
        /// Valida usuario, crea cookie y redirige.
        /// </summary>
        [IgnoreAntiforgeryToken]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string user, [FromForm] string password, [FromQuery] string? ReturnUrl = null)
        {
            if (!_dataAccess.ValidateUser(user, password))
                return Redirect("/?loginError=1");


            _initialDataService.PullData(user, password);
            var dataUser = _initialDataService.GetDatauser();

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, dataUser.Code ?? user),
                new(ClaimTypes.Role, "Administrador"),
                new("Culture", dataUser.CultureCode ?? "es-MX")
            };

            var identity = new ClaimsIdentity(claims, "WFMAuthCookie");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("WFMAuthCookie", principal);

            // Aplica la cookie de cultura (idioma)
            var culture = dataUser.CultureCode ?? "es-MX";
            var cultureCookie = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture));
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                cultureCookie,
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true, // evita bloqueos por políticas de cookies
                    HttpOnly = false
                });

            // Intenta obtener la URL pendiente desde sesión si no vino en query
            if (string.IsNullOrWhiteSpace(ReturnUrl))
                ReturnUrl = HttpContext.Session.GetString("PendingReturnUrl");

            HttpContext.Session.Remove("PendingReturnUrl"); // Limpia el valor usado

            if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                return LocalRedirect(ReturnUrl);

            return LocalRedirect("/Home");

        }


        /// <summary>
        /// Cierra sesión y redirige al login
        /// </summary>
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync("WFMAuthCookie");
            }
            catch { /* Ignorar si no hay cookie */ }

            HttpContext.Response.Cookies.Delete(".AspNetCore.Antiforgery");
            return Redirect("/");
        }
    }
}

