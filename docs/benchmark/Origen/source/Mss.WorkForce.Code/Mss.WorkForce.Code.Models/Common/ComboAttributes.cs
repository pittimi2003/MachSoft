namespace Mss.WorkForce.Code.Web.Common
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ComboAttributes : Attribute
    {
        public string DataKey { get; } // Clave para identificar el catálogo
        public string ValueField { get; } // Campo del valor (ID)
        public string TextField { get; } // Campo del texto

        public ComboAttributes(string dataKey, string valueField = "Id", string textField = "Name")
        {
            DataKey = dataKey;
            ValueField = valueField;
            TextField = textField;
        }
    }
}
