using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Web.Services;
using System.Reflection;

namespace Mss.WorkForce.Code.Web.Model
{
    public static class EntityHelper
    {

        #region Methods

        public static IEnumerable<SelectItemEnum>? GetProperties<T>()
        {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(prop => new SelectItemEnum
                {
                    Value = prop.Name,
                    Text = prop.Name
                });
        }

        public static IEnumerable<SelectItemEnum>? GetFields<T>(Type type, string entityName, IFieldLabelService labelService)
        {

            return typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(field => field.FieldType == type)
                .Select(field => new SelectItemEnum
                {
                    Value = field.Name,
                    Text = labelService.GetLabel(entityName, field.Name)
                });
        }

        public static IEnumerable<SelectItemEnum>? GetFields<T>()
        {

            return typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Select(field => new SelectItemEnum
                {
                    Value = field.Name,
                    Text = field.Name
                });
        }

        public static IEnumerable<SelectItemEnum>? GetFields<T>(string entityName, IFieldLabelService labelService)
        {

            return typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Select(field => new SelectItemEnum
                {
                    Value = field.Name,
                    Text = labelService.GetLabel(entityName, field.Name)
                });
        }

        public static FieldTypeEnum GetFieldType<T>(string fieldName)
        {

            FieldInfo fieldInfo = typeof(T).GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);

            if (fieldInfo == null)
                return FieldTypeEnum.None;

            Type type = fieldInfo.FieldType;

            if (type == typeof(string))
                return FieldTypeEnum.Text;
            else if (type == typeof(bool))
                return FieldTypeEnum.Bool;
            else if (type == typeof(DateTime))
                return FieldTypeEnum.Date;
            else if (type == typeof(int) || type == typeof(decimal) || type == typeof(double))
                return FieldTypeEnum.Number;

            return FieldTypeEnum.None;

        }




        #endregion
    }
}
