using System.Xml;
using System.Xml.Linq;

namespace Mss.WorkForce.Code.Web.Services
{
    public interface ILocalizationService
    {

        #region Methods

        void AddMissingKey(string key, string value);

        Task ChangeCultureAsync(string culture);

        Task ChangeCultureAsync(string culture, string redirectUri);

        Task ChangeCultureToDefaultAsync();

        Task<IReadOnlyDictionary<string, string>> GetAllJavaScriptResourcesAsync();

        string GetCurrentCultureName();

        string GetDefaultCulture();
        string Loc(string key);

        string Loc(string key, params object[] arguments);

        string LocByKey(string key, string defaultValue, params object[] arguments);

        void Loc(XDocument element);

        Dictionary<string, string> GetTranslationsForKeys(string culture, List<string> keys);

        #endregion

    }
}
