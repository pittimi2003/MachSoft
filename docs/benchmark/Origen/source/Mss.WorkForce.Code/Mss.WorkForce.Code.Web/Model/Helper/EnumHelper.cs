using System.ComponentModel;
using System.Reflection;

namespace Mss.WorkForce.Code.Web.Model
{
    public static class EnumHelper
    {
        #region Methods

        /// <summary>
        /// Funcion para obtener todos los elementos de un enum
        /// </summary>
        /// <typeparam name="E">El tipo de enum</typeparam>
        /// <returns>Lista de elementos del enum</returns>
        public static List<SelectItemEnum> GetSelectItems<E>() where E : Enum
        {
            return Enum.GetValues(typeof(E))
                .Cast<E>()
                .Select(e => new SelectItemEnum
                {
                    Value = e,
                    Text = e.GetType()
                            .GetField(e.ToString())?
                            .GetCustomAttribute<DescriptionAttribute>()?.Description ?? e.ToString()
                }).OrderBy(e => e.Text).ToList();
        }

        public static E? ConvertStringToEnum<E>(string valor) where E : struct, Enum
        {
            if (Enum.TryParse<E>(valor, true, out E resultado))
                return resultado;

            return null; 
        }

        public static string GetItemDescription<T>(T enumValue) where T : Enum
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            var descriptionAttribute = fieldInfo?.GetCustomAttribute<DescriptionAttribute>();

            return descriptionAttribute?.Description ?? enumValue.ToString();
        }

        public static T GetEnumValueFromDescription<T>(string description)
        {
            foreach (var field in typeof(T).GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;

                if (attribute?.Description == description)
                    return (T)field.GetValue(null);
            }

            return default!;
        }

        #endregion
    }
}
