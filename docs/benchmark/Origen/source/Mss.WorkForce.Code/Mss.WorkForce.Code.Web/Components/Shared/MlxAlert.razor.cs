using DevExpress.CodeParser;
using Microsoft.AspNetCore.Components;
using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Model;
using Mss.WorkForce.Code.Web.Services;
using System.Globalization;
using System.Reflection;

namespace Mss.WorkForce.Code.Web.Components.Shared
{
    public partial class MlxAlert
    {

        #region Fields

        private List<KeyValuePair<bool, string>> ListBoolOptions = new();

        private string Field = string.Empty;
        private string Operator = string.Empty;
        private string Reference = string.Empty;
        private string ReferenceFixedValue = string.Empty;
        private string Fixed = string.Empty;
        private string SelectedField = string.Empty;
        private string SelectedOperator = string.Empty;
        private string SelectReference = string.Empty;

        #endregion

        #region Properties

        [Parameter] public EventCallback<AlertFilterDto> AlertFilterChanged { get; set; }

        [Parameter] public string EntityName { get; set; }

        [Parameter] public bool IsMultiEdition { get; set; }
        public IEnumerable<SelectItemEnum> Fields { get; set; }
        public IEnumerable<SelectItemEnum> FieldReferences { get; set; }
        [Parameter]
        public bool IsEditing { get; set; }

        [Parameter]
        public int Columns { get; set; }

        [Parameter] public AlertFilterDto ModelFilter { get; set; } = new();
        public IEnumerable<SelectItemEnum> Operators { get; set; }
        [Parameter] public UserFormatOptions userFormat { get; set; } = new();

        private FieldTypeEnum FieldType { get; set; } = FieldTypeEnum.None;

        private string multiplePhrase = "";

        private bool _hasInitializedField = false;

        [Inject]
        private IFieldLabelService FieldLabelService { get; set; }

        public string FilterFieldLabel => string.IsNullOrEmpty(ModelFilter.FilterField) ? string.Empty : L.Loc(FieldLabelService.GetLabel(EntityName, ModelFilter.FilterField));
        public string FilterReferenceLabel => string.IsNullOrEmpty(ModelFilter.FilterReference) ? string.Empty : L.Loc(FieldLabelService.GetLabel(EntityName, ModelFilter.FilterReference));

        public string OperatorSymbol => EnumHelper.GetItemDescription(ModelFilter.Operator);

        #endregion

        #region Methods

        protected override void OnInitialized()
        {
            Transalate();
            Operators = EnumHelper.GetSelectItems<AlertOperator>().OrderBy(x => x.Text);
        }

        private void Transalate()
        {
            if (!string.IsNullOrEmpty(ModelFilter.FilterReference))
                FieldReferences = TranslateFields(GetPropertiesValues(EntityName, ModelFilter.FilterReference));

            ListBoolOptions = [.. new List<KeyValuePair<bool, string>>
            {
                new KeyValuePair<bool, string>(true, L.Loc("True")),
                new KeyValuePair<bool, string>(false, L.Loc("False"))
            }.OrderBy(x => x.Value)];

            Fields = TranslateFields(GetPropertiesValues(EntityName, string.Empty));
            Field = L.Loc("Field");
            Operator = L.Loc("Operator");
            Reference = L.Loc("Reference");
            ReferenceFixedValue = L.Loc("Reference fixed value");
            Fixed = L.Loc("Fixed");
            SelectedField = L.Loc("Select field...");
            SelectedOperator = L.Loc("Select operator...");
            SelectReference = L.Loc("Select reference...");
            multiplePhrase = L.Loc("<Multiple values>");
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

        private string FormatTimeValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            if (DateTime.TryParseExact(value, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDateTime))
            {
                return parsedDateTime.ToUserTime(userFormat.HourFormatWithoutSeconds);
            }

            return value;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !_hasInitializedField && !string.IsNullOrEmpty(ModelFilter?.FilterField) && ModelFilter.IsFixed)
            {
                _hasInitializedField = true;
                FieldType = EntityHelper.GetFieldType<OrderFields>(ModelFilter.FilterField);
                StateHasChanged();
            }
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
                ModelFilter.FilterReference = string.Empty;
            else
                ModelFilter.FilterFixedValue = string.Empty;

            await OnFieldChanged(ModelFilter.FilterField);
        }

      

        private async Task OnFieldChanged(string fieldName)
        {
            ModelFilter.FilterField = fieldName;

            FieldReferences = TranslateFields(GetPropertiesValues(EntityName, fieldName));
            if (!string.IsNullOrEmpty(fieldName) && ModelFilter.IsFixed)
            {
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
            await AlertFilterChanged.InvokeAsync(ModelFilter);

        }

        private async Task OnReferenceChanged(string newValue)
        {
            ModelFilter.FilterReference = newValue;
            await AlertFilterChanged.InvokeAsync(ModelFilter); 
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

        private async Task SetProperty(Decimal value)
        {
            ModelFilter.FilterFixedValue = value.ToString();
            await AlertFilterChanged.InvokeAsync(ModelFilter);
        }
        private async Task SetProperty(String value)
        {
            ModelFilter.FilterFixedValue = value.ToString();
            await AlertFilterChanged.InvokeAsync(ModelFilter);
        }

        private async Task SetProperty(bool value)
        {
            ModelFilter.FilterFixedValue = value.ToString();
            await AlertFilterChanged.InvokeAsync(ModelFilter);
        }

        private async Task SetProperty(DateTime value)
        {
            ModelFilter.FilterFixedValue = value.ToString("yyyy-MM-dd");
            await AlertFilterChanged.InvokeAsync(ModelFilter); 
        }

        private async Task SetProperty(TimeSpan value)
        {
            var dateTime = DateTime.Today.Add(value);
            ModelFilter.FilterFixedValue = dateTime.ToString("HH:mm");
            await AlertFilterChanged.InvokeAsync(ModelFilter);
        }

        private string GetValidationError(string fieldName)
        {
            if (fieldName == nameof(ModelFilter.FilterField))
            {
                if (string.IsNullOrEmpty(ModelFilter.FilterField))
                    return L.Loc("* Mandatory field");
            }

            if (fieldName == nameof(ModelFilter.FilterReference) && !ModelFilter.IsFixed)
            {
                if (string.IsNullOrEmpty(ModelFilter.FilterReference))
                    return L.Loc("* Mandatory field");
            }

            if (fieldName == nameof(ModelFilter.FilterFixedValue) && ModelFilter.IsFixed)
            {
                if (FieldType == FieldTypeEnum.Text && string.IsNullOrWhiteSpace(ModelFilter.FilterFixedValue))
                    return L.Loc("* Mandatory field");
            }

            return string.Empty;
        }

        #endregion

    }
}
