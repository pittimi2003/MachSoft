using Mss.WorkForce.Code.Web.Services;
using System.Reflection;

namespace Mss.WorkForce.Code.Web.Model
{
    public class SelectItemComboBox
    {
        public Guid Key { get; set; }
        public string Value { get; set; } = string.Empty;

        public static IEnumerable<SelectItemComboBox> FillItemsComboBox(object catalogue, string key = "Id", string value = "Name", bool localized = false, ILocalizationService localizationService = null)
        {
            if (catalogue is IEnumerable<object> items)
            {

                var sortedItems = items.Select(item =>
                {
                    var keyProperty = item.GetType().GetProperty(key, BindingFlags.Public | BindingFlags.Instance);
                    var valueProperty = item.GetType().GetProperty(value, BindingFlags.Public | BindingFlags.Instance);

                    if (keyProperty == null || valueProperty == null)
                        {throw new ArgumentException($"Las propiedades '{key}' o '{value}' no existen en el tipo '{item.GetType().Name}'.");}

                    var rawValue = valueProperty.GetValue(item)?.ToString();
                    var displayValue = localized && localizationService != null && !string.IsNullOrWhiteSpace(rawValue)
                        ? localizationService.Loc(rawValue)
                        : rawValue;

                    return new
                    {
                        Item = item,
                        Key = keyProperty.GetValue(item) is Guid id ? id : Guid.Empty,
                        Value = displayValue
                    };}).OrderBy(x => x.Value).ToList();
                
                foreach (var sortedItem in sortedItems)
                {
                    yield return new SelectItemComboBox
                    {
                        Key = sortedItem.Key,
                        Value = sortedItem.Value
                    };
                }
            }
            else
            {
                throw new ArgumentException("El parámetro 'catalogue' no es una colección de objetos.");
            }
        }
    }


}
