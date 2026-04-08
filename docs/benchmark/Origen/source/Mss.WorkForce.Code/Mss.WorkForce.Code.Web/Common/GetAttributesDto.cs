using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Web.Services;
namespace Mss.WorkForce.Code.Web.Common
{
	public class GetAttributesDto<T>
	{
		public T? Model { get; set; }
		public bool ShowActionBar { get; set; } = true;
		public bool ShowPager { get; set; } = true;
		public string NameContainer { get; set; }
		public string ToolBarPublish { get; set; }
		public string GridPublish { get; set; }
		public string DetailPublish { get; set; }

		private readonly ILocalizationService _l;

		public GetAttributesDto(ILocalizationService localizationService) => _l = localizationService;

		public Dictionary<string, DisplayAttributes> GetProperties()
		{
			var resp = new Dictionary<string, DisplayAttributes>();

			if (Model == null) return resp;

			var props = typeof(T).GetProperties();
			if (Model is IEnumerable)
			{
				// Obtiene el tipo de los elementos dentro de la lista
				Type itemType = Model.GetType().GetGenericArguments()[0];
				props = itemType.GetProperties();
			}

			foreach (var prop in props)
			{
				var displayAttributes = GetDisplayAttribute(prop);
				if (displayAttributes != null && displayAttributes.FieldType != Web.Model.Enums.ComponentType.None)
					resp.Add(prop.Name, displayAttributes);


				// Si la propiedad es una clase compleja, obtener sus propiedades recursivamente
				if (displayAttributes != null && displayAttributes.TypeGroup != Web.Model.Enums.GroupTypes.None)
				{
					// Evitar referencias cíclicas
					if (prop.PropertyType != typeof(T))
					{
						var nestedModel = Activator.CreateInstance(prop.PropertyType);
						var nestedProps = GetNestedProperties(nestedModel, prop.Name);
						foreach (var nestedProp in nestedProps)
							resp.Add(nestedProp.Key, nestedProp.Value);
					}
				}
			}

			return resp;
		}

		private Dictionary<string, DisplayAttributes> GetNestedProperties(object model, string parentName)
		{
			Dictionary<string, DisplayAttributes> nestedResp = new Dictionary<string, DisplayAttributes>();

			var props = model.GetType().GetProperties();
			foreach (var prop in props)
			{
				DisplayAttributes displayAttributes = prop.GetCustomAttribute<DisplayAttributes>();
				CaptionTranslate(displayAttributes);
				if (displayAttributes != null && displayAttributes.FieldType != Web.Model.Enums.ComponentType.None)
				{
					nestedResp.Add($"{parentName}.{prop.Name}", displayAttributes);
				}

				// Manejar propiedades anidadas recursivamente
				if (displayAttributes.TypeGroup != Web.Model.Enums.GroupTypes.None)
				{
					var nestedModel = Activator.CreateInstance(prop.PropertyType);
					var deeperProps = GetNestedProperties(nestedModel, $"{parentName}.{prop.Name}");
					foreach (var deeperProp in deeperProps)
					{
						nestedResp.Add(deeperProp.Key, deeperProp.Value);
					}
				}
			}
			return nestedResp;
		}

		public Dictionary<string, DisplayAttributes> GetTranslateCaptions()
		{
			var result = new Dictionary<string, DisplayAttributes>();

			var props = typeof(T).GetProperties();
			foreach (var prop in typeof(T).GetProperties())
			{
				var attr = GetDisplayAttribute(prop);
				if (attr != null)
					result[prop.Name] = attr;
			}

			return result;
		}

		private DisplayAttributes GetDisplayAttribute(PropertyInfo prop)
		{
			var attr = prop.GetCustomAttribute<DisplayAttributes>();
			if (attr != null && !string.IsNullOrEmpty(attr.Caption))
				attr.Caption = _l.Loc(attr.Caption);
			return attr;
		}

		public PropertyInfo? GetKeyProperty()
		{
			if (Model == null)
			{
				return null;
			}

			Type modelType = Model is IEnumerable && Model.GetType().IsGenericType
				? Model.GetType().GetGenericArguments()[0]
				: typeof(T);

			return modelType.GetProperties()
				.FirstOrDefault(p => p.GetCustomAttributes(typeof(KeyAttribute), true).Any());
		}

		private void CaptionTranslate(DisplayAttributes displayAttributes)
		{
			if (displayAttributes is DisplayAttributes display)
				displayAttributes.Caption = displayAttributes.Caption != string.Empty && displayAttributes.TypeGroup == Web.Model.Enums.GroupTypes.None ? _l.Loc(displayAttributes.Caption) : "";
		}

	}



}


