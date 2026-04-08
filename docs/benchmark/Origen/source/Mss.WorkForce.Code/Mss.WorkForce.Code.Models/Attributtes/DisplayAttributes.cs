using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Web
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple =false)]
    public class DisplayAttributes:Attribute
    {
        public int Index { get; set; } = 0;
        public string Caption { get; set; } = string.Empty;
        public bool Required { get; set; } = false;
        public string Group { get; set; } = string.Empty;
        public GroupTypes TypeGroup { get; set; } = GroupTypes.None;
        public ComponentType FieldType { get; set; } = ComponentType.TextBox;
        public bool IsMultiEdit { get; set; } = true;
        public bool IsVisible { get; set; } = true;
        public bool IsVisibleDefault { get; set; }

        // Propiedades para valores mínimo y máximo
        public double? MinValue { get; set; } = null;
        public double? MaxValue { get; set; } = null;

        public string CatalogueName {  get; set; } = string.Empty;

        public bool ShowIconEnum { get; set; } = false;

		public bool TraslateCaption { get; set; } = false;

        public bool TextAlignment { get; set; } = false;

        public LevelFilterType LevelFilterType { get; set; }

		public DisplayAttributes(int index, string caption, bool required, ComponentType fieldType = ComponentType.TextBox, string group ="", GroupTypes typeGroup  = GroupTypes.None, bool isMultiEdit = true, bool isVisible =true, bool isVisibleDefault = false, bool showIconEnum = false, bool traslateCaption = false, bool textAlignment = false, LevelFilterType levelFilterType = LevelFilterType.AllLevels)
        { 
            Index = index;
            Caption = caption;
            Required = required;
            FieldType = fieldType;
            Group = group;
            TypeGroup = typeGroup;
            IsMultiEdit = isMultiEdit;
            IsVisible = isVisible;
            IsVisibleDefault = isVisibleDefault;
            ShowIconEnum = showIconEnum;
            TraslateCaption = traslateCaption;
			TextAlignment = textAlignment;
            LevelFilterType = levelFilterType;
		}
        public DisplayAttributes(bool required,bool isVisible,  bool isVisibleDefault)
        {
            Required = required;
            IsVisible=isVisible;
            IsVisibleDefault=isVisibleDefault;
        }

        public DisplayAttributes(DisplayAttributes displayAttributes)
        { 
            Index=displayAttributes.Index;
            Caption=displayAttributes.Caption;
            Required = displayAttributes.Required;
            Group = displayAttributes.Group;
            TypeGroup = displayAttributes.TypeGroup;
            FieldType = displayAttributes.FieldType;
            IsMultiEdit= displayAttributes.IsMultiEdit;
            IsVisible = displayAttributes.IsVisible;
            IsVisibleDefault = displayAttributes.IsVisibleDefault;

        }

        // Constructor específico para NumericSpin
        public DisplayAttributes(
            int index,
            string caption,
            bool required,
            double minValue,
            double maxValue,
            string group = "",           
            bool isVisible = true,
            bool isVisibleDefault = false,
            ComponentType fieldType = ComponentType.NumericSpin)
        {
            Index = index;
            Caption = caption;
            Required = required;
            FieldType = fieldType;
            MinValue = minValue;
            MaxValue = maxValue;
            Group = group;            
            IsVisible = isVisible;
            IsVisibleDefault = isVisibleDefault;
        }


        public DisplayAttributes(int index, string caption, bool required, ComponentType fieldType,  bool isVisible, string catalogueName)
        {
            Index = index;
            Caption = caption;
            Required = required;
            FieldType = fieldType;
            IsVisible = isVisible;
            CatalogueName = catalogueName;
        }

		public DisplayAttributes(int index, string caption, bool isVisible, bool required, bool isVisibleDefault, ComponentType fieldType, int minValue, int maxValue, bool textAlignment = false)
		{
			Index = index;
			Caption = caption;
			IsVisible = isVisible;
			Required = required;
			IsVisibleDefault = isVisibleDefault;
			FieldType = fieldType;
			MinValue = minValue;
			MaxValue = maxValue;
			TextAlignment = textAlignment;
		}
	}

   
}
