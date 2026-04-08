using Microsoft.AspNetCore.Components;
using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Model;
using Mss.WorkForce.Code.Web.Services;
using System.Reflection;

namespace Mss.WorkForce.Code.Web.Components.Shared
{
    public partial class MlxAlertFilter
    {
        #region Fields
        private AlertFilterDto _modelFilter;

        private List<KeyValuePair<bool, string>> ListBoolOptions = new();

        private bool EnableAddNewButton = false;

        private string Field = string.Empty;
        private string Operator = string.Empty;
        private string Reference = string.Empty;
        private string ReferenceFixedValue = string.Empty;
        private string Fixed = string.Empty;
        private string IsFixed = string.Empty;
        private string Actions = string.Empty;
        private string SelectedField = string.Empty;
        private string SelectedOperator = string.Empty;
        #endregion

        #region Properties

        [Parameter] public EventCallback<ICollection<AlertFilterDto>> AlertFilterChanged { get; set; }

        [Parameter]
        public ICollection<AlertFilterDto> AlertFilters { get; set; } = new List<AlertFilterDto>();

        [Parameter]
        public string EntityName { get; set; }

        [Parameter]
        public int Columns { get; set; }

        public IEnumerable<SelectItemEnum> Fields { get; set; }
        public IEnumerable<SelectItemEnum> FieldReferences { get; set; }
        [Parameter]
        public UserFormatOptions userFormat { get; set; } = new();
        [Parameter]
        public bool IsEditing { get; set; }
        public IEnumerable<SelectItemEnum> Operators { get; set; }

        private FieldTypeEnum FieldType { get; set; } = FieldTypeEnum.None;
        private AlertFilterDto ModelFilter { get; set; } = new();

        [Inject]
        private IFieldLabelService FieldLabelService { get; set; }

        #endregion

        #region Methods

        protected override void OnInitialized()
        {
            Transalate();
            var _fields = GetPropertiesValues(EntityName, string.Empty);
            Fields = TranslateFields(_fields); 
            Operators = EnumHelper.GetSelectItems<AlertOperator>();
        }

        private IEnumerable<SelectItemEnum> TranslateFields(IEnumerable<SelectItemEnum> items)
        {
            if (items == null)
                return Enumerable.Empty<SelectItemEnum>();

            return items.Select(f => new SelectItemEnum
            {
                Value = f.Value,
                Text = L.Loc(f.Text)
            }).OrderBy(x => x.Text);
        }

        private void Transalate()
        {
            ListBoolOptions = [.. new List<KeyValuePair<bool, string>>
            {
                new KeyValuePair<bool, string>(true, L.Loc("True")),
                new KeyValuePair<bool, string>(false, L.Loc("False"))
            }.OrderBy(x => x.Value)];

            Field = L.Loc("Field");
            Operator = L.Loc("Operator");
            Reference = L.Loc("Reference");
            ReferenceFixedValue = L.Loc("Reference fixed value");
            Fixed = L.Loc("Fixed");
            IsFixed = L.Loc("Is fixed");
            Actions = L.Loc("Actions");
            SelectedField = L.Loc("Select field...");
            SelectedOperator = L.Loc("Select operator...");
        }

        private async Task AddAlertFilter()
        {
            ModelFilter.Id = Guid.NewGuid();

            if (ModelFilter.IsFixed)
                ModelFilter.FilterReference = string.Empty;
            else
                ModelFilter.FilterFixedValue = string.Empty;

            //ModelFilter.FilterField = L.Loc(FieldLabelService.GetLabel(EntityName, ModelFilter.FilterField));
            //ModelFilter.FilterReference = L.Loc(FieldLabelService.GetLabel(EntityName, ModelFilter.FilterReference));
            AlertFilters.Add(ModelFilter);

            await AlertFilterChanged.InvokeAsync(AlertFilters);
            ModelFilter = new();
            EnableAddNewButton = false;
        }


        private IEnumerable<SelectItemEnum> GetPropertiesValues(string entityName, string fieldNameReference)
        {
            switch (entityName)
            {
                case nameof(EntityTypeEnum.WorkOrderPlanning):
                    if (string.IsNullOrEmpty(fieldNameReference))
                        return EntityHelper.GetFields<OrderFields>(entityName, FieldLabelService);
                    else
                    {
                        FieldInfo fieldInfo = typeof(OrderFields).GetField(fieldNameReference, BindingFlags.Public | BindingFlags.Instance);
                        return EntityHelper.GetFields<OrderFields>(fieldInfo.FieldType, entityName, FieldLabelService);
                    }
            }

            return null;
        }

        private async void OnCheckFixedChanged(bool value)
        {
            ModelFilter.IsFixed = value;

            if (ModelFilter.IsFixed)
            {
                ModelFilter.FilterReference = string.Empty;
                EnableAddNewButton = !string.IsNullOrEmpty(ModelFilter.FilterFixedValue);
            }
            else
            {
                ModelFilter.FilterFixedValue = string.Empty;
                EnableAddNewButton = !string.IsNullOrEmpty(ModelFilter.FilterField) && !string.IsNullOrEmpty(ModelFilter.FilterReference);
            }

            OnFieldChanged(ModelFilter.FilterField);
        }

        private void ValidateFixedValue(string value)
        {
            ModelFilter.FilterFixedValue = value;
            EnableAddNewButton = !string.IsNullOrWhiteSpace(value);
        }

        private string GetOperatorIcon(object alertOperator)
        {
            switch ((AlertOperator)alertOperator)
            {
                case AlertOperator.GreaterThan:
                    return "mlx-ico-greater-than";

                case AlertOperator.LessThan:
                    return "mlx-ico-less-than";

                case AlertOperator.Equal:
                    return "mlx-ico-equal";

                case AlertOperator.NotEqual:
                    return "mlx-ico-distinct";

                case AlertOperator.GreaterOrEqual:
                    return "mlx-ico-greater-than-or-equal-to";

                case AlertOperator.LessOrEqual:
                    return "mlx-ico-less-than-or-equal-to";
            }

            return alertOperator.ToString();
        }

        private void OnFieldChanged(string fieldName)
        {
            if (!string.IsNullOrEmpty(fieldName))
            {
                ModelFilter.FilterField = fieldName;
                var _FieldReferences = GetPropertiesValues(EntityName, fieldName);
                FieldReferences = TranslateFields(_FieldReferences);

                FieldType = EntityHelper.GetFieldType<OrderFields>(fieldName);

                switch (FieldType)
                {

                    case FieldTypeEnum.Text:
                        ModelFilter.FilterFixedValue = string.Empty;
                        break;
                    case FieldTypeEnum.Number:
                        ModelFilter.FilterFixedValue = "0";
                        break;
                    case FieldTypeEnum.Bool:
                        ModelFilter.FilterFixedValue = "false";
                        break;
                    case FieldTypeEnum.Date:
                        ModelFilter.FilterFixedValue = DateTime.Now.ToString("HH:mm");
                        break;
                }
            }
        }

        private void OnReferenceChanged(string referenceValue)
        {
            ModelFilter.FilterReference = referenceValue;

            if (!ModelFilter.IsFixed && !string.IsNullOrEmpty(ModelFilter.FilterField) && !string.IsNullOrEmpty(ModelFilter.FilterReference))
                EnableAddNewButton = true;
            else if (ModelFilter.IsFixed)
                EnableAddNewButton = true;
            else
                EnableAddNewButton = false;
        }

        private async Task RemoveAlertFilter(Guid id)
        {
            if (AlertFilters.FirstOrDefault(a => a.Id == id) is AlertFilterDto alertRemove)
            {
                AlertFilters.Remove(alertRemove);

                await AlertFilterChanged.InvokeAsync(AlertFilters);
            }
        }

        private void SetProperty(Decimal value) => ModelFilter.FilterFixedValue = value.ToString();
        
        private void SetProperty(TimeSpan value)
        {
            var dateTime = DateTime.Today.Add(value);
            ModelFilter.FilterFixedValue = dateTime.ToString("HH:mm");
        }

        private string FormatFilterFixedValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^\d{1,2}:\d{2}(:\d{2})?$"))
            {
                if (TimeSpan.TryParse(value, out var time))
                {
                    var dateTime = DateTime.Today.Add(time);
                    return dateTime.ToUserTime(userFormat.HourFormat);
                }
            }
            return value;
        }


        private void SetProperty(bool value) => ModelFilter.FilterFixedValue = value.ToString();
        private void SetProperty(DateTime value) => ModelFilter.FilterFixedValue = value.ToString();

        #endregion
    }
}
