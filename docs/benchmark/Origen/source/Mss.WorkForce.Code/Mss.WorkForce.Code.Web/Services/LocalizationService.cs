using DevExpress.CodeParser;
using Google.Protobuf.WellKnownTypes;
using Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using Mss.WorkForce.Code.Web.Common;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Mss.WorkForce.Code.Web.Services
{
    public class LocalizationService : ILocalizationService
    {

        #region Fields

        private const string ResourceRootPath = "Localization";
        private readonly IConfiguration _configuration;
        // Configuración
        private readonly string _defaultCulture;

        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ILogger<LocalizationService> _logger;
        private readonly NavigationManager _navigationManager;
        private readonly string[] _supportedCultures;
        private IList<string> _ResourceNames = new List<string> { "SharedResource.resx", "SharedResource.es.resx", "SharedResource.fr.resx" };
        private const string JS_PREFIX = "JS.";
        private bool _enableLocalizable;

        #endregion

        #region Constructors

        public LocalizationService(
            IStringLocalizer<SharedResource> localizer,
            ILogger<LocalizationService> logger,
            IConfiguration configuration,
            NavigationManager navigationManager)
        {
            _logger = logger;
            _localizer = localizer;
            _configuration = configuration;
            _navigationManager = navigationManager;

            // Obtengo la culture por defecto desde appsettings.json, si no esta definido ponemos por defecto "en".
            _defaultCulture = _configuration.GetValue<string>("Localization:DefaultCulture") ?? "en-US";

            // Varialbe para activar/desactivar que los recursos se puedan añadir automaticamente a los archivos de recursos
            _enableLocalizable = _configuration.GetValue<bool>("GlobalSettings:EnableLocalizableAuto");

            // Obtengo las cultures soportadas desde appsettings.json.
            _supportedCultures = _configuration
                .GetSection("Localization:SupportedCultures")
                .Get<string[]>() ?? new[] { "en", "es", "fr" };
        }

        #endregion

        #region Methods

        public void AddMissingKey(string key, string value)
        {
            foreach (var fileName in _ResourceNames)
                AddKeyToXmlResourceFile(fileName, key, $"* {value}");
        }

        public async Task ChangeCultureAsync(string culture)
        {
            var currentUri = _navigationManager.ToAbsoluteUri(_navigationManager.Uri).PathAndQuery;
            await ChangeCultureAsync(culture, currentUri);
        }

        public async Task ChangeCultureAsync(string culture, string redirectUri)
        {
            try
            {
                var cultureToSet = _defaultCulture;

                // Valido que la Uri no sea null o vacia.
                if (string.IsNullOrWhiteSpace(redirectUri))
                    redirectUri = "~/";

                // Valido si la cultura es soportada, por los archivos de resources
                if (!string.IsNullOrWhiteSpace(culture))
                {
                    var isValidCulture = _supportedCultures.Contains(culture, StringComparer.OrdinalIgnoreCase);

                    if (isValidCulture)
                        cultureToSet = culture;
                    else
                        _logger.LogWarning("An attempt was made to set an unsupported culture: {Culture}. Using default culture: {DefaultCulture}",
                            culture, _defaultCulture);
                }
                else
                    _logger.LogInformation("No culture provided, using default culture: {DefaultCulture}", _defaultCulture);

                // Construir URL del controlador de cultura
                var cultureUrl = BuildCultureUrl(cultureToSet, redirectUri);

                // Navegar al controlador que manejará el cambio de cultura
                _navigationManager.NavigateTo(cultureUrl, forceLoad: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing culture to: {Culture}", culture);
                throw;
            }
        }

        public async Task ChangeCultureToDefaultAsync()
        {
            await ChangeCultureAsync(_defaultCulture);
        }

        public string GetCurrentCultureName() => CultureInfo.CurrentCulture.Name;

        public string GetDefaultCulture() => _defaultCulture;

        public async Task<IReadOnlyDictionary<string, string>> GetAllJavaScriptResourcesAsync()
        {
            var translations = new Dictionary<string, string>();

            // Aqui obtengo todos los recursos para javascript con el prefijo "JS.[Name]"
            translations = _localizer.GetAllStrings()
                .Where(r => r.Name.StartsWith(JS_PREFIX))
                .ToDictionary(elemnt => elemnt.Name, elemnt => elemnt.Value);

            return translations;
        }

        /// <summary>
        /// Localiza un recurso a partir de la clave especificada.
        /// Si el recurso no existe en el archivo de localización, devuelve el valor por defecto proporcionado.
        /// Además, si la opción <c>_enableLocalizable</c> está habilitada, agrega la clave y el valor por defecto
        /// al archivo de recursos para su posterior localización.
        /// </summary>
        /// <param name="key">Clave de localización a buscar.</param>
        /// <param name="defaultValue">Valor por defecto que se utilizará si no se encuentra la clave.</param>
        /// <param name="arguments">Parámetros opcionales para formatear el texto localizado o el valor por defecto.</param>
        /// <returns>El texto localizado correspondiente a la clave, o el valor por defecto si la clave no existe.</returns>
        public string LocByKey(string key, string defaultValue, params object[] arguments)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    return defaultValue;

                LocalizedString result = (arguments == null || arguments.Length == 0)
                    ? _localizer.GetString(key) : _localizer.GetString(key, arguments);


                if (result.ResourceNotFound)
                {
                    if (_enableLocalizable)
                        AddMissingKey(key, defaultValue);

                    string formattedValue = FormatStringWithArguments(defaultValue, arguments);

                    return (_enableLocalizable) ? $"* {formattedValue}" : formattedValue;
                }

                return result.Value ?? defaultValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Localization error with key '{Key}'. Fallback to default value '{defaultValue}'", key, defaultValue);
                return defaultValue;
            }
        }

        public string Loc(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    return string.Empty;

                var generatedKeyName = GenerateKeyName(key);

                var localizedString = _localizer[generatedKeyName];

                if (localizedString.ResourceNotFound)
                {
                    if (_enableLocalizable)
                    {
                        AddMissingKey(generatedKeyName, key);
                        return $"* {key}";
                    }

                    return key;
                }

                return localizedString.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving localized string for key: {key}");
                return key;
            }
        }

        public string Loc(string key, params object[] arguments)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    return string.Empty;

                var normalizedKey = GenerateKeyName(key);

                var result = _localizer.GetString(normalizedKey, arguments);

                if (result.ResourceNotFound)
                {
                    if (_enableLocalizable)
                    {
                        AddMissingKey(normalizedKey, key);
                        return $"* {key}";
                    }
                    else
                        return FormatStringWithArguments(key, arguments);
                }

                return result.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with key: {Key}", key);
                return key;
            }
        }

        public void Loc(XDocument element)
        {
            try
            {
                if (element?.Root == null)
                    return;
                // Traducir recursivamente los elemementos
                LocalizeElementRecursive(element.Root);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error translating XDocument");
            }
        }

        private void AddKeyToXmlResourceFile(string fileName, string keyName, string value)
        {
            string filePath = Path.Combine(ResourceRootPath, fileName);

            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogDebug($"The file path does not exist: {filePath}, to add the resource: {keyName}");
                    return;
                }

                var resouceDoc = XDocument.Load(filePath);

                // Verifico que la keyName no exista en el archivo del recurso
                var existingKey = resouceDoc.Root?
                    .Elements("data")
                    .FirstOrDefault(e => e.Attribute("name")?.Value == keyName);

                if (existingKey != null)
                    return;

                var xmlNamespace = resouceDoc.Root.GetNamespaceOfPrefix("xml");

                var dataElement = new XElement("data",
                    new XAttribute("name", keyName),
                    new XAttribute(xmlNamespace + "space", "preserve"),
                    new XElement("value", value)
                );

                resouceDoc.Root?.Add(dataElement);
                resouceDoc.Save(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding key to XML resource file '{FilePath}'", filePath);
                return;
            }
        }

        private string BuildCultureUrl(string culture, string redirectUri)
        {
            var parameters = new List<string>();

            // Solo agregar parámetro culture si no está vacío
            // Si está vacío, el controlador usará la cultura por defecto
            if (!string.IsNullOrWhiteSpace(culture))
                parameters.Add($"culture={Uri.EscapeDataString(culture)}");

            parameters.Add($"redirectUri={Uri.EscapeDataString(redirectUri)}");

            return $"/Localization/ChangeCulture?{string.Join("&", parameters)}";
        }

        private string GenerateKeyName(string originalKey)
        {
            if (string.IsNullOrWhiteSpace(originalKey))
                return originalKey;

            // Convertir en mayusculas y quitar espacios
            var keyName = originalKey.ToUpperInvariant().Replace(" ", "");

            //Reemplzo lo que no es letra o número por guion bajo
            keyName = Regex.Replace(keyName, @"[^A-Z0-9]", "_");

            return keyName;
        }

        private void LocalizeElementRecursive(XElement element)
        {
            switch (element.Name.LocalName.ToLower())
            {
                case "chart":
                    TranslateAttibuteElement(element, "Name");
                    LocalizeChartElement(element);
                    break;
                case "grid":
                    TranslateAttibuteElement(element, "Name");
                    LocalizeGridElement(element);
                    break;
                case "pivot":
                    TranslateAttibuteElement(element, "Name");
                    LocalizePivotElement(element);
                    break;
            }

            // Procesar elementos hijos recursivamente
            foreach (var child in element.Elements())
            {
                LocalizeElementRecursive(child);
            }
        }

        private void LocalizeChartElement(XElement element)
        {
            foreach (var child in element.Elements())
            {
                if (child.Name.LocalName == "Simple")
                    TranslateAttibuteElement(child, "Name");
                else if (child.Name.LocalName == "DashboardDescription")
                    TranslateDashboardDescription(child);

                LocalizeChartElement(child);
            }
        }

        private void LocalizeGridElement(XElement element)
        {
            foreach (var child in element.Elements())
            {
                if (child.Name.LocalName == "GridDimensionColumn")
                    TranslateAttibuteElement(child, "Name");
                else if (child.Name.LocalName == "GridSparklineColumn")
                    TranslateAttibuteElement(child, "Name");
                else if (child.Name.LocalName == "DashboardDescription")
                    TranslateDashboardDescription(child);

                LocalizeGridElement(child);
            }
        }
 
        private void LocalizePivotElement(XElement element)
        {
            foreach (var child in element.Elements())
            {
                if (child.Name.LocalName == "Measure")
                    TranslateAttibuteElement(child, "Name");
                else if (child.Name.LocalName == "PivotItemFormatRule")
                    TranslateAttibuteElement(child, "Name");
                else if (child.Name.LocalName == "DashboardDescription")
                    TranslateDashboardDescription(child);

                LocalizePivotElement(child);
            }
        }

        private void TranslateDashboardDescription(XElement element)
        {
            if (element == null || element.Name.LocalName != "DashboardDescription")
                return;

            var keyResource = element.Attribute("KeyResource");

            if (keyResource != null && !string.IsNullOrWhiteSpace(keyResource.Value))
            {
                var generatedKeyName = GenerateKeyName(keyResource.Value);
                var valueResource = element.Value;

                var localizedString = _localizer[generatedKeyName];

                if (localizedString.ResourceNotFound)
                {
                    if (_enableLocalizable)
                    {
                        AddMissingKey(generatedKeyName, valueResource);
                        element.Value = $"* {valueResource}";
                    }
                }
                else
                {
                    element.Value = localizedString;
                }
            }
        }

        private void TranslateAttibuteElement(XElement element, string attrName)
        {
            var attr = element.Attribute(attrName);

            if (attr != null && !string.IsNullOrWhiteSpace(attr.Value))
            {
                var value = attr.Value;

                if (value != null && !string.IsNullOrWhiteSpace(value))
                {
                    var translated = Loc(value);
                    attr.Value = translated;
                }
            }
        }

        private string FormatStringWithArguments(string templete, params object[] arguments)
        {
            try
            {
                if (arguments == null || arguments.Length == 0)
                    return templete;

                var result = new StringBuilder(templete);

                for(int i = 0; i < arguments.Length; i++)
                {
                    var indexArg = $"{{{i}}}";

                    var value = arguments[i]?.ToString() ?? string.Empty;
                    result.Replace(indexArg, value);
                }

                return result.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error formatting string with StringBuilder");
                return templete;
            }
        }

        public Dictionary<string, string> GetTranslationsForKeys(string culture, List<string> keys)
        {
            var originalCulture = CultureInfo.CurrentCulture;
            var originalUICulture = CultureInfo.CurrentUICulture;

            try
            {
                var tempCulture = new CultureInfo(culture);
                CultureInfo.CurrentCulture = tempCulture;
                CultureInfo.CurrentUICulture = tempCulture;

                return keys.ToDictionary(key => key, key => Loc(key));
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUICulture;
            }
        }


        #endregion

    }
}
