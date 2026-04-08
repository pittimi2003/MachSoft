using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Web.Common
{
    public class ColumnChoose
    {

        #region Constructors

        public ColumnChoose(string fieldName, string caption, bool isVisible, int index = 0, ComponentType componentType = ComponentType.None, LevelFilterType levelFilterType = LevelFilterType.AllLevels)
        {
            FieldName = fieldName;
            Caption = caption;
            IsVisible = isVisible;
            Index = index;
            ComponentType = componentType;
            LevelFilterType = levelFilterType;
        }

        #endregion

        #region Properties

        public string Caption { get; set; }
        public string FieldName { get; set; }
        public bool IsVisible { get; set; }
        public int Index { get; set; }
        public ComponentType ComponentType { get; set; }
        public LevelFilterType LevelFilterType { get; set; }

        #endregion
    }
}
