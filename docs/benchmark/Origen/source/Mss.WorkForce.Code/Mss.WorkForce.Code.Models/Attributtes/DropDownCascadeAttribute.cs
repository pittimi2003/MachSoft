namespace Mss.WorkForce.Code.Models
{
    public class DropDownCascadeAttribute : Attribute
    {

        public DataSourceDropdownType DataSourceType { get; set; }

        public string PropertyNameCascade { get; set; }


        public DropDownCascadeAttribute(DataSourceDropdownType dataSourceType, string cascadePropertyName)
        {
            DataSourceType = dataSourceType;
            PropertyNameCascade = cascadePropertyName;
        }
    }
}
