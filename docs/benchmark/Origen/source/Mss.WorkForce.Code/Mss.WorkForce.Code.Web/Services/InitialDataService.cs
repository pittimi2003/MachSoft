using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.SignalR;

namespace Mss.WorkForce.Code.Web.Services
{
    public class Datauser
    {
        #region Constructors

        public Datauser(
            Guid id,
            string name,
            string lastname,
            string warehouse,
            string code,
            Guid? warehouseId,
            string timeZoneName,
            string language,
            string dateFormat,
            string timeFormat,
            char thousandFormat,
            char decimalFormat,
            string unitSystem,
            string? cultureCode)
        {
            Id = id;
            Name = name;
            LastName = lastname;
            Warehouse = warehouse;
            Code = code;
            WarehouseId = warehouseId;
            TimeZoneName = timeZoneName;
            Language = language;
            TimeFormat = timeFormat;
            ThousandFormat = thousandFormat;
            DecimalFormat = decimalFormat;
            DateFormat = dateFormat;
            UnitSystem = unitSystem;
            CultureCode = cultureCode;
        }

        #endregion

        #region Properties

        public string Code { get; }
        public Guid Id { get; }
        public string LastName { get; }
        public string Name { get; }
        public string Warehouse { get; }
        public Guid? WarehouseId { get; }
        public string TimeZoneName { get; }
        public string Language { get; }
        public string DateFormat { get; }
        public string TimeFormat { get; }
        public char ThousandFormat { get; }
        public char DecimalFormat { get; }
        public string UnitSystem { get; }
        public string? CultureCode { get; }

        #endregion
    }

    public class InitialDataService : IInitialDataService
    {
        #region Fields

        private readonly IContextConfig _contextConfig;
        private readonly ProtectedLocalStorage _localStorage;
        private readonly ILogger<WarehouseService> _logger;
        private readonly IUserService _userService;
        private Language _language;
        private OrganizationDto? _organization;
        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Constructors

        public InitialDataService(LocalStorageService localStorageService,
                                    IContextConfig contextConfig,
                                    IUserService userService,
                                    ProtectedLocalStorage localStorage,
                                    ILogger<WarehouseService> logger,
                                    IHttpContextAccessor contextAccessor)
        {
            _contextConfig = contextConfig;
            _userService = userService;
            _localStorage = localStorage;
            _logger = logger;
            _httpContextAccessor = contextAccessor;
        }

        #endregion

        #region Properties

        public OrganizationDto Organization
        {
            get
            {
                if (_organization == null)
                    _organization = GetOrganization();
                return _organization;
            }
        }

        private Datauser? _datauser { get; set; }

        #endregion

        #region Methods

        public void flushData()
        {
            _organization = null;
            _datauser = null;
            _localStorage.DeleteAsync("UserData");
        }

        public ClaimsPrincipal SetClaims()
        {
            var dataUser = GetDatauser();
            if (dataUser == null)
                throw new InvalidOperationException("No hay usuario cargado en memoria.");

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, dataUser.Code ?? dataUser.Name ?? "Usuario"),
                new(ClaimTypes.Role, "Administrador"),
                new("Culture", dataUser.CultureCode ?? "es-MX"),
                new("Warehouse", dataUser.Warehouse ?? "")
            };

            var identity = new ClaimsIdentity(claims, "WFMAuthCookie");
            return new ClaimsPrincipal(identity);
        }

        public Datauser GetDatauser()
        {
            return _datauser;
        }

        public async Task GetDataUserLocal()
        {
            try
            {
                var resultData = await _localStorage.GetAsync<Datauser>("UserData");

                _datauser = resultData.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InitialDataService::GetDataUserLocal =>  Error retrieving user data from local storage");
            }
        }


        public Language GetLanguage() => _language;

        /// <summary>
        /// aqui llenamos los datos de inicio, eventualmente NO se pasara el user y el pass, se cambiara por el tokken
        /// </summary>
        /// <param name="organization"></param>
        public bool PullData(string user, string pass)
        {
            LoadDataInfo(user, pass);
            return true;
        }

        public void RefreshData(string user, string pass) => LoadDataInfo(user, pass);

        public bool UpdateOrganizationData()
        {
            try
            {
                _organization = GetOrganization();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private OrganizationDto GetOrganization()
        {
            var organization = _contextConfig.GetOnlyOrganization();
            if (organization != null)
            {
                return new OrganizationDto
                {
                    Id = organization.Id,
                    Name = organization.Name,
                    RegionalSettings = new RegionalSettingsOrganization
                    {
                        ThousandsSeparator = organization.ThousandsSeparator,
                        DecimalSeparator = organization.DecimalSeparator,
                        SystemOfMeasurement = organization.SystemOfMeasurement,
                        Language = organization.Language,
                        DateFormat = organization.DateFormat,
                        HourFormat = organization.HourFormat
                    }
                };
            }
            return new OrganizationDto();
        }

        private void LoadDataInfo(string user, string pass)
        {
            try
            {
                if (_organization == null)
                    _organization = GetOrganization();

                if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
                {
                    if (_userService.GetUsers(_organization.Id).FirstOrDefault(x => x.Code == user && x.Password == pass) is UserDto logUser)
                    {
                        var defaultWarehouse = logUser.WarehouseDefaultId != null && logUser.Warehouses != null
                                                ? logUser.Warehouses.FirstOrDefault(x => x.Id == logUser.WarehouseDefaultId)
                                                : null;
                        var warehouseCode = defaultWarehouse?.Code ?? "";
                        var timeZoneName = defaultWarehouse?.TimeZone_?.Name ?? "";
                        var thousandsSeparator = !string.IsNullOrEmpty(logUser.RegionalSettings.ThousandsSeparator.Name) ? logUser.RegionalSettings.ThousandsSeparator.Name[0] : '.';
                        var decimalSeparator = !string.IsNullOrEmpty(logUser.RegionalSettings.DecimalSeparator?.Name) ? logUser.RegionalSettings.DecimalSeparator.Name[0] : '.';
                        var dateFormat = logUser.RegionalSettings.DateFormat?.DateTimeFormat ?? "dd/MM/yyyy";
                        var hourFormat = logUser.RegionalSettings.HourFormat?.HourTimeFormat ?? "hh:mm:ss tt";
                        _datauser = new Datauser(
                            logUser.Id,
                            logUser.Name,
                            logUser.Lastname,
                            warehouseCode,
                            logUser.Code,
                            logUser.WarehouseDefaultId,
                            timeZoneName,
                            logUser.RegionalSettings.Language.Name,
                            dateFormat,
                            hourFormat,
                            thousandsSeparator,
                            decimalSeparator,
                            "International units",
                            logUser.RegionalSettings.Language?.InternationalCode
                            );

                        if (_datauser != null)
                            _localStorage.SetAsync("UserData", _datauser);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InitialDataService::GetDataUserLocal => Error to load data user and organization");
            }
        }

        private string GetGanttFilterKey(GanttView viewName)
        {
            var idUser = _datauser?.Id.ToString() ?? "Anonymous";
            return $"gantt-filter:{viewName.ToString().ToLowerInvariant()}:{idUser}";
        }

        public async Task SaveGanttFilterForViewAsync(GanttFilterSettingsDto filter, GanttView viewName)
        {
            try
            {
                var key = GetGanttFilterKey(viewName);
                await _localStorage.SetAsync(key, filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"InitialDataService::SaveGanttFilterForViewAsync => Failed to save Gantt filter settings for view '{viewName}'.");
            }
        }

        public async Task<GanttFilterSettingsDto?> GetGanttFilterForViewAsync(GanttView viewName)
        {
            try
            {
                var resultData = await _localStorage.GetAsync<Datauser>("UserData");
                _datauser = resultData.Value;

                var key = GetGanttFilterKey(viewName);
                var result = await _localStorage.GetAsync<GanttFilterSettingsDto>(key);
                return result.Success ? result.Value : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"InitialDataService::GetGanttFilterForViewAsync => An error occurred while loading Gantt filter settings for view '{viewName}'.");
                return null;
            }
        }

        public UserFormatOptions GetUserFormat()
        {
            if (_datauser == null)
                return new UserFormatOptions();

            return new UserFormatOptions
            {
                DateFormat = _datauser.DateFormat,
                HourFormat = _datauser.TimeFormat,
                UnitSystem = _datauser.UnitSystem,
                ThousandSeparator = _datauser.ThousandFormat,
                DecimalSeparator = _datauser.DecimalFormat,
                CultureCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName ?? "en"
            };
        }



        public async Task ClearGanttFilterAsync(GanttView viewName)
        {
            try
            {
                var key = GetGanttFilterKey(viewName);
                await _localStorage.DeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"InitialDataService::ClearGanttFilterAsync => An error occurred while removing Gantt filter settings for view '{viewName}'.");
            }
        }

        public async Task ReloadDataUserLocal()
        {
            try
            {
                if (_organization == null)
                    _organization = GetOrganization();

                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    string? userCode = user.Identity?.Name;
                    UserDto? logUser = _userService.GetUsers(_organization.Id).FirstOrDefault(x => x.Code == userCode);

                    if (logUser != null)
                    {
                        var defaultWarehouse = logUser.WarehouseDefaultId != null && logUser.Warehouses != null
                                                ? logUser.Warehouses.FirstOrDefault(x => x.Id == logUser.WarehouseDefaultId)
                                                : null;
                        var warehouseCode = defaultWarehouse?.Code ?? "";
                        var timeZoneName = defaultWarehouse?.TimeZone_?.Name ?? "";
                        var thousandsSeparator = !string.IsNullOrEmpty(logUser.RegionalSettings.ThousandsSeparator.Name) ? logUser.RegionalSettings.ThousandsSeparator.Name[0] : '.';
                        var decimalSeparator = !string.IsNullOrEmpty(logUser.RegionalSettings.DecimalSeparator.Name) ? logUser.RegionalSettings.DecimalSeparator.Name[0] : '.';
                        var dateFormat = logUser.RegionalSettings.DateFormat?.DateTimeFormat ?? "dd/MM/yyyy";
                        var hourFormat = logUser.RegionalSettings.HourFormat?.HourTimeFormat ?? "hh:mm:ss tt";
                        _datauser = new Datauser(
                            logUser.Id,
                            logUser.Name,
                            logUser.Lastname,
                            warehouseCode,
                            logUser.Code,
                            logUser.WarehouseDefaultId,
                            timeZoneName,
                            logUser.RegionalSettings.Language.Name,
                            dateFormat,
                            hourFormat,
                            thousandsSeparator,
                            decimalSeparator,
                            "International units",
                            logUser.RegionalSettings.Language?.InternationalCode
                            );

                        if (_datauser != null)
                            _localStorage.SetAsync("UserData", _datauser);

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InitialDataService::GetDataUserLocal => Error to reload data user and organization");
            }
        }

        #endregion
    }
}

