using System.ComponentModel;
 

namespace Mss.WorkForce.Code.Web
{
   internal static class GetEnumDescription
    {

        public static string GetItemDescription(EnumGanttLevels enumValue)
        {
            var descriptionAttribute = enumValue.GetType()
                .GetField(enumValue.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;

            return descriptionAttribute != null ? descriptionAttribute.Description : enumValue.ToString();
        }
    }
}
