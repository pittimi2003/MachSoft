using DevExpress.Blazor;
using DevExpress.Blazor.Internal;
using Microsoft.AspNetCore.Components;
using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Web.Code;
using Mss.WorkForce.Code.Web.Common;
using Mss.WorkForce.Code.Web.Model;
using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web.Services;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;

namespace Mss.WorkForce.Code.Web.Components.PanelEditor
{
    public partial class PanelEditor<TEntity> where TEntity : IBaseModel, new()
    {

        #region Fields

        [Inject]
        private ILocalizationService Loc { get; set; }

        private List<IBaseModel> _data = new();
        private List<string> _lastValidatedMembers = new();
        private IReadOnlyList<TEntity>? _selectedDataGridItems;
        private bool cleanMultiSelec = false;
        private bool isEditing;
        private List<KeyValuePair<bool, string>> ListBoolOptions = new();

        private bool showActionAdd;
        private bool showActionDelete;
        private bool showActionEdit;
        private bool showActionReadOnly;
        private bool showActionSave;
        private bool showNewElementForm;
        public string MSG_FIELD_MANDATORY = "* Mandatory field";
        public string MSG_GREAT_THAN = "* The {0} cannot be less than 1";
        private string INBOUND = @"<span class=""tag-inbound""><i class=""mlxPanel mlx-ico-entrada""></i> Inbound</span>";
        private string OUTBOUND = @"<span class=""tag-outbound""><i class=""mlxPanel mlx-ico-salida""></i> Outbound</span>";
        private string MESSAGEDEPENDENCIESNOTIFICATION = "It is not possible to delete this profile, it is linked to another registry.";
        private string NOTIFICATIONTITLE = "Notification";

        private List<KeyValuePair<bool, MarkupString>> TypeBounds = new();

        #endregion

        #region Properties

        [Parameter]
        public List<IBaseModel> Data
        {
            get => _data;
            set
            {
                // Si tiene la misma referencia no actulizar el modelo
                if (ReferenceEquals(_data, value))
                {
                    OrderItemsData();
                    return;
                }

                _data = value ?? new List<IBaseModel>();
                Items = CloneElement(_data.Select(o => (TEntity)o).ToList());
                OrderItemsData();
            }
        }

        [Parameter] public bool IsChildren { get; set; } = false;
        [Parameter] public EventCallback<IEnumerable<IBaseModel>> OnSaveChanges { get; set; }
        [Parameter] public string Title { get; set; }
        [Parameter] public EventCallback OnShowBreakPopup { get; set; }
        [Parameter] public EventCallback<Guid> OnDeleteBreakProfile { get; set; }
        [Parameter] public EventCallback<Guid> OnEditBreakProfile { get; set; }
        [Parameter] public int PageSizeBase { get; set; } = 5;
        [Parameter] public Guid WarehouseId { get; set; }
        [Parameter] public UserFormatOptions UserFormat { get; set; } = new();
        [Parameter] public string ErrorMessage { get; set; } = string.Empty;
        [Parameter] public ePanelType PanelType { get; set; } = ePanelType.Any;
        [Inject] private IContextConfig _contextConfig { get; set; }
        private Dictionary<string, IEnumerable<SelectItemComboBox>> Catalogues { get; set; } = new Dictionary<string, IEnumerable<SelectItemComboBox>>();
        private string CssClasses => $"mlx-grid";
        private string CurrentHeigthPnlEditor => IsBodyVisible ? "pnlEditor-auto" : "pnlEditor-fixed";
        [Inject] private IMlxDialogService DialogService { get; set; }
        private TEntity ElementNew { get; set; }
        private bool EnableAddNewButton => ValidationErrors.Count == 0;
        private bool IsBodyVisible { get; set; } = true;
        private List<TEntity> Items { get; set; }
        private Dictionary<string, DisplayAttributes> Properties { get; set; }
        private IReadOnlyList<object>? SelectedDataGridItems
        {
            get => _selectedDataGridItems?.Cast<object>().ToList();
            set
            {
                _selectedDataGridItems = value?.Cast<TEntity>().ToList();
                showActionDelete = _selectedDataGridItems?.Count > 0;
            }
        }

        private string UserInput { get; set; } = "";
        private Dictionary<string, string> ValidationErrors { get; set; } = new();
        private Dictionary<Guid, Dictionary<string, string>> UpdateErrors { get; set; } = new();
        private Dictionary<string, ValidationType> Validations { get; set; }
        private int[] PageSizeOptions => new int[] { PageSizeBase, PageSizeBase * 2 };
        private Guid _previousWarehouseId;

        #endregion

        #region Methods

        public void GetCatalogues()
        {
            Catalogues.Clear();

            foreach (var property in Properties)
            {
                if (property.Value.FieldType == Model.Enums.ComponentType.DropDown)
                {
                    var instance = ElementNew.GetProperty(property.Key);

                    if (instance is CatalogEntity catalogue)
                        FillCatalogue(catalogue.CatalogName, property.Key);
                }
                else if (property.Value.FieldType == Model.Enums.ComponentType.Multiselec)
                    FillCatalogue(property.Value.CatalogueName, property.Key);
            }
        }

        protected override async Task OnInitializedAsync()
        {
            TranslateResoruce();
            CreateNewInstanceElement();
            GetAttributesProperties();
            SetComponentState(showActionAdd: true, showActionEdit: true, clearSelection: true);
        }

        private static string FormatNumbers(double value, char groupSep, char decimalSep)
        {
            var nfi = (System.Globalization.NumberFormatInfo)
                      System.Globalization.CultureInfo.InvariantCulture.NumberFormat.Clone();

            nfi.NumberGroupSeparator = groupSep.ToString();
            nfi.NumberDecimalSeparator = decimalSep.ToString();
            const string dynamicFormat = "#,##0.#####";
            return value.ToString(dynamicFormat, nfi);
        }

        protected void TranslateResoruce()
        {
            MSG_FIELD_MANDATORY = Loc.Loc("* Mandatory field");
            INBOUND = @"<span class=""tag-inbound""><i class=""mlxPanel mlx-ico-entrada""></i>" + Loc.Loc("Inbound") + "</span>";
            OUTBOUND = @"<span class=""tag-outbound""><i class=""mlxPanel mlx-ico-salida""></i> " + Loc.Loc("Outbound") + "</span>";
            MESSAGEDEPENDENCIESNOTIFICATION = Loc.Loc("It is not possible to delete this profile, it is linked to another registry.");
            NOTIFICATIONTITLE = Loc.Loc("Notification");
            TypeBounds = [.. new List<KeyValuePair<bool, MarkupString>> {
                new KeyValuePair<bool, MarkupString>(false, (MarkupString)INBOUND),
                new KeyValuePair<bool, MarkupString>(true, (MarkupString)OUTBOUND)}];

            ListBoolOptions = [.. new List<KeyValuePair<bool, string>>{
                new KeyValuePair<bool, string>(true, Loc.Loc("Yes")),
                new KeyValuePair<bool, string>(false, Loc.Loc("No"))
                }];

        }

        private static List<TEntity> CloneElement<TEntity>(List<TEntity> source)
        {
            try
            {
                var json = JsonSerializer.Serialize(source);
                return JsonSerializer.Deserialize<List<TEntity>>(json);
            }
            catch (Exception ex)
            {
                //Log
                return new List<TEntity>();
            }
        }

        private void AddNewElement()
        {
            ElementNew.Id = Guid.NewGuid();
            ElementNew.DataOperationType = OperationType.Insert;
            Items.Add(ElementNew);

            cleanMultiSelec = true;
            CreateNewInstanceElement();
            CreateValidationErrors();
            OrderItemsData();
            StateHasChanged();
        }

        private void CreateNewInstanceElement()
        {
            ElementNew = new TEntity();
        }

        private void CreateValidationErrors()
        {
            foreach (var property in Properties)
            {
                if (!property.Value.Required) continue;

                var ft = property.Value.FieldType;
                bool isNumericType = ft == ComponentType.NumericSpin || ft == ComponentType.NumericSpinInt;

                if (isNumericType)
                    ValidationErrors[property.Key] = Loc.Loc(MSG_GREAT_THAN, property.Value.Caption);
                else
                    ValidationErrors[property.Key] = MSG_FIELD_MANDATORY;
            }
        }

        private void ExecuteStateAdd()
        {
            SetComponentState(showNewElementForm: true, showActionSave: true, showActionReadOnly: true);
            CreateValidationErrors();
            CreateNewInstanceElement();
        }

        private async Task ExecuteStateDelete()
        {
            if (_selectedDataGridItems?.Any() == true)
            {
                int selectedCount = _selectedDataGridItems.Count;

                if (_selectedDataGridItems.Any(e => e.IsDependencies))
                {
                    await DialogService.ShowDialogAsync(NOTIFICATIONTITLE, MESSAGEDEPENDENCIESNOTIFICATION, true, Loc.Loc("Cancel"), false);
                    return;
                }

                string message = ElementNew switch
                {
                    OrderProfilesDto => selectedCount == 1 ? Loc.Loc("Are you sure you want to delete an order profile?") : Loc.Loc("Are you sure you want to delete {0} order profiles?", selectedCount),
                    OrderLoadPropertiesDto => selectedCount == 1 ? Loc.Loc("Are you sure you want to delete a load profile?") : Loc.Loc("Are you sure you want to delete {0} load profiles?", selectedCount),
                    _ => Loc.Loc("Are you sure you want to delete {0}?", string.Join(", ", _selectedDataGridItems.Select(e => e.Name)))
                };

                bool resp = await DialogService.ShowDialogAsync(NOTIFICATIONTITLE, message);

                if (resp)
                {
                    foreach (var element in SelectedDataGridItems.OfType<TEntity>())
                    {
                        if (element.DataOperationType == OperationType.Insert)
                        {
                            Items.Remove(element);
                        }
                        else if (element is IBaseModel itemData)
                        {
                            itemData.DataOperationType = OperationType.Delete;
                        }
                    }

                    SetComponentState(showActionSave: true, showActionReadOnly: true, clearSelection: true);
                    await InvokeAsync(StateHasChanged);
                }
            }
        }

        private void ExecuteStateEdit()
        {
            SetComponentState(isEditing: true, showActionSave: true, showActionReadOnly: true);
        }

        private async Task ExecuteStateReadOnly()
        {
            bool hasChanges = IsDirtyErrors();

            if (hasChanges)
            {
                bool confirmDiscard = await DialogService.ShowDialogAsync(Loc.Loc("Notification"), Loc.Loc($"You have unsaved changes. Are you sure you want to leave and discard them?"));

                if (confirmDiscard)
                {
                    Items = CloneElement(_data.Select(o => (TEntity)o).ToList());
                    UpdateErrors.Clear();
                }
                else
                {
                    return;
                }
            }
            SetComponentState(showActionAdd: true, showActionEdit: true, clearSelection: true);
            await InvokeAsync(StateHasChanged);
        }

        private async Task ExecuteStateSave()
        {
            IEnumerable<IBaseModel> dataChanges;

            SetComponentState(showActionAdd: true, showActionEdit: true);
            dataChanges = Items.OfType<IBaseModel>().Where(x => x.DataOperationType != OperationType.None);

            if (dataChanges != null && dataChanges.Any())
                await OnSaveChanges.InvokeAsync(dataChanges);
        }

        private void FillCatalogue(string catalogueName, string propertyName)
        {
            switch (catalogueName)
            {
                case EntityNamesConst.Rol:
                    Catalogues.TryAdd(propertyName, SelectItemComboBox.FillItemsComboBox(_contextConfig.GetRoles(WarehouseId).ToList()));
                    break;
                case EntityNamesConst.Team:
                    Catalogues.TryAdd(propertyName, SelectItemComboBox.FillItemsComboBox(_contextConfig.GetTeams(WarehouseId).ToList()));
                    break;
                case EntityNamesConst.BreakProfile:
                    Catalogues.TryAdd(propertyName, SelectItemComboBox.FillItemsComboBox(_contextConfig.GetBreakProfiles(WarehouseId).ToList()));
                    break;
                case EntityNamesConst.Process:
                    Catalogues.TryAdd(propertyName, SelectItemComboBox.FillItemsComboBox(_contextConfig.GetProcessesByWarehouse(WarehouseId).ToList()));
                    break;
                case EntityNamesConst.Break:
                    Catalogues.TryAdd(propertyName, SelectItemComboBox.FillItemsComboBox(_contextConfig.GetBreaksByWarehouse(WarehouseId).ToList()));
                    break;
                case EntityNamesConst.Shift:
                    Catalogues.TryAdd(propertyName, SelectItemComboBox.FillItemsComboBox(_contextConfig.GetShifts(WarehouseId).ToList()));
                    break;
                case EntityNamesConst.VehicleProfile:
                    Catalogues.TryAdd(propertyName, SelectItemComboBox.FillItemsComboBox(_contextConfig.GetVehicleProfileCatalogue(WarehouseId).ToList()));
                    break;
                case EntityNamesConst.LoadProfile:
                    Catalogues.TryAdd(propertyName, SelectItemComboBox.FillItemsComboBox(_contextConfig.GetLoadProfileCatalogue(WarehouseId).ToList()));
                    break;
                case EntityNamesConst.Area:
                    Catalogues.TryAdd(propertyName, SelectItemComboBox.FillItemsComboBox(_contextConfig.GetAreasByWarehouse(WarehouseId).ToList()));
                    break;
                case EntityNamesConst.TypeEquipment:
                    Catalogues.TryAdd(propertyName, SelectItemComboBox.FillItemsComboBox(_contextConfig.GetTypeEquipmentCatalogue(WarehouseId).ToList()));
                    break;
            }
        }

        private void GetAttributesProperties()
        {
            Properties = new Dictionary<string, DisplayAttributes>();
            Validations = new Dictionary<string, ValidationType>();

            if (Items != null)
            {
                Properties = new GetAttributesDto<List<TEntity>>(Loc) { Model = Items }.GetProperties();
                foreach (var item in Properties)
                {
                    item.Value.Caption = l.Loc(item.Value.Caption);
                }
                foreach (var prop in typeof(TEntity).GetProperties())
                {
                    var validation = prop.GetCustomAttribute<ValidationAttributes>();
                    if (validation != null)
                        Validations.Add(prop.Name, validation.Validation);
                }
            }
        }

        protected override void OnParametersSet() {
            GetCatalogues();

            if (_previousWarehouseId != WarehouseId)
            {
                SetComponentState(showActionAdd: true, showActionEdit: true, clearSelection: true);
                _previousWarehouseId = WarehouseId;
            }
        } 

        private Guid GetCatalogPropertyId(object model, string propertyName)
        {
            var property = model.GetType().GetProperty(propertyName);
            if (property != null && property.PropertyType == typeof(CatalogEntity))
            {
                var catalogEntity = property.GetValue(model) as CatalogEntity;
                return catalogEntity?.Id ?? Guid.Empty;
            }
            return Guid.Empty;
        }

        private IEnumerable<SelectItemComboBox>? GetDropDowpItems(string propertyName) => Catalogues?.GetValueOrDefault(propertyName).OrderBy(x => x.Value);

        private Guid GetDropDowpValue(string propertyName)
        {
            var instance = ElementNew.GetProperty(propertyName);

            if (instance is CatalogEntity catalog)
                return catalog.Id;

            return Guid.Empty;
        }

        private IEnumerable<SelectItemComboBox> GetListBoxValues(IDropDownBox dropDownBox)
        {
            return dropDownBox.Value as IEnumerable<SelectItemComboBox>;
        }

        private string GetMaskForProperty(string property)
        {
            var prop = Validations.FirstOrDefault(x => x.Key == property);
            return MaskControllers.GetMaskForProperty(prop.Value);
        }

        private IEnumerable<SelectItemComboBox>? GetMultiselec(string propertyName, object model = null)
        {
            List<SelectItemComboBox> resultList = new List<SelectItemComboBox>();
            var catalogueValues = GetDropDowpItems(propertyName);
            if (model != null)
            {
                var propertyInfo = model.GetType().GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    var value = propertyInfo.GetValue(model);
                    if (value is Multiselect element)
                    {
                        foreach (var item in element.Items)
                        {
                            resultList.Add(new SelectItemComboBox
                            {
                                Key = catalogueValues.FirstOrDefault(x => x.Key == item.Key).Key,
                                Value = catalogueValues.FirstOrDefault(x => x.Key == item.Key).Value,
                            });
                        }
                    }
                }
            }
            return resultList;
        }

        private string GetPropertiesMultiSelecName(string propertyName, object obItem)
        {
            var propertyInfo = obItem.GetType().GetProperty(propertyName);
            var value = propertyInfo.GetValue(obItem);
            if (value is Multiselect element)
            {
                return element.Name;
            }
            return string.Empty;
        }

        private string GetPropertyString(string propertyName, object model)
        {
            var property = model.GetType().GetProperty(propertyName);
            if (property != null)
            {
                return property.GetValue(model)?.ToString() ?? string.Empty;
            }
            return string.Empty;
        }

        private bool GetPropertyValueBool(string propertyName)
        {
            try
            {
                var propertyValue = ElementNew.GetProperty(propertyName);

                if (propertyValue == null)
                    return false;
                return (bool)propertyValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private bool GetPropertyValueBool(string propertyName, object model)
        {
            try
            {
                var propertyInfo = model.GetType().GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    var value = propertyInfo.GetValue(model);
                    return (bool)value;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private bool? GetPropertyValueBoolNullable(string propertyName)
        {
            try
            {
                var propertyValue = ElementNew.GetProperty(propertyName);

                if (propertyValue == null)
                    return null;
                return (bool)propertyValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private decimal GetPropertyValueDecimal(string propertyName)
        {
            try
            {
                var propertyValue = ElementNew.GetProperty(propertyName) ?? 0;
                return decimal.Parse(propertyValue.ToString() ?? "0");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        private int GetPropertyValueInt(string propertyName)
        {
            try
            {
                var propertyValue = ElementNew.GetProperty(propertyName) ?? 0;
                return int.Parse(propertyValue.ToString() ?? "0");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        private double GetPropertyValueDouble(string propertyName)
        {
            try
            {
                var propertyValue = ElementNew.GetProperty(propertyName) ?? 0;
                return double.Parse(propertyValue.ToString() ?? "0");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        private decimal GetPropertyValueDecimal(string propertyName, object model)
        {
            try
            {
                var propertyInfo = model.GetType().GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    var value = propertyInfo.GetValue(model);
                    return decimal.Parse(value?.ToString() ?? "0");
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        private int GetPropertyValueInt(string propertyName, object dataItem)
        {
            try
            {
                var propertyValue = dataItem.GetType().GetProperty(propertyName)?.GetValue(dataItem) ?? 0;
                return int.TryParse(propertyValue.ToString(), out var result) ? result : 0;
            }
            catch
            {
                return 0;
            }
        }

        private double GetPropertyValueDouble(string propertyName, object dataItem)
        {
            try
            {
                var propertyValue = dataItem.GetType().GetProperty(propertyName)?.GetValue(dataItem) ?? 0;
                return double.TryParse(propertyValue.ToString(), out var result) ? result : 0;
            }
            catch
            {
                return 0;
            }
        }

        private string GetPropertyValueString(string propertyName) => ElementNew.GetProperty(propertyName)?.ToString() ?? string.Empty;

        private TimeSpan GetPropertyValueTime(string propertyName)
        {
            try
            {
                var propertyValue = ElementNew.GetProperty(propertyName) ?? new TimeSpan(0, 0, 0);
                return (TimeSpan)propertyValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new TimeSpan(0, 0, 0);
            }
        }

        private TimeSpan GetPropertyValueTime(string propertyName, object model)
        {
            try
            {
                var propertyInfo = model.GetType().GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    var value = propertyInfo.GetValue(model);
                    return (TimeSpan)value;
                }
                return new TimeSpan(0, 0, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new TimeSpan(0, 0, 0);
            }
        }

        private string GetValidationError(string propertyName)
        {
            return ValidationErrors.TryGetValue(propertyName, out var error) ? error : string.Empty;
        }

        private string? GetValidationError(string propertyName, object dataItem)
        {
            if (dataItem.GetType().GetProperty("Id").GetValue(dataItem) is Guid guid)
            {
                return UpdateErrors.TryGetValue(guid, out var fieldErrors) &&
                       fieldErrors.TryGetValue(propertyName, out var error)
                    ? error
                    : string.Empty;
            }
            return string.Empty;
        }


        private void HideOrShowAccordion()
        {
            IsBodyVisible = !IsBodyVisible;
        }

        private bool IsDirty() => Items.OfType<IBaseModel>().Any(x => x.DataOperationType != OperationType.None) && !UpdateErrors.Any();
        private bool IsDirtyErrors() => Items.OfType<IBaseModel>().Any(x => x.DataOperationType != OperationType.None);

        private bool HasData() => Items.Any(x => x.DataOperationType != OperationType.Delete);

        private void ListBoxValuesChanged(IEnumerable<SelectItemComboBox> values, IDropDownBox dropDownBox, string propertyName, object model = null)
        {
            dropDownBox.BeginUpdate();
            ValidateErrorsMultiSelec(values, propertyName, model);
            dropDownBox.Value = values;
            UpdateListMultiSelect(propertyName, model, values);
            UpdateNameMultiSelect(values, propertyName);
            if (model is IBaseModel o)
                o.DataOperationType = OperationType.Update;

            dropDownBox.EndUpdate();
        }

        private void ValidateErrorsMultiSelec(IEnumerable<SelectItemComboBox> values, string propertyName, object model = null)
        {
            if (model != null)
                ValidatePropertyModeEdit(values, propertyName, model);
            else
                ValidatePropertyMultiSelec(values, propertyName);
        }

        private void OnSelectedDataItemsChanged(IReadOnlyList<object> selectedItems)
        {
            SelectedDataGridItems = selectedItems;
        }

        private object OrderItemsData()
        {
            try
            {
                if (Items.Any())
                {
                    var type = Items.First().GetType();

                    // Buscar la propiedad con menor index y visible
                    var orderProp = type.GetProperties()
                        .Select(p => new
                        {
                            Prop = p,
                            Attr = p.GetCustomAttributes(typeof(DisplayAttributes), false)
                                     .Cast<DisplayAttributes>()
                                     .FirstOrDefault()
                        })
                        .Where(x => x.Attr != null && x.Attr.IsVisible)
                        .OrderBy(x => x.Attr.Index)
                        .Select(x => x.Prop)
                        .FirstOrDefault();

                    if (orderProp == null)
                    {
                        orderProp = type.GetProperty("Name");
                    }

                    if (orderProp != null)
                    {
                        Items = Items
                            .Where(x => x.DataOperationType != OperationType.Delete)
                            .OrderBy(item =>
                            {
                                var value = orderProp.GetValue(item, null);

                                if (value is IComparable comparable)
                                    return comparable;

                                // Solo si es un combo
                                if (value != null)
                                {
                                    var nameProp = value.GetType().GetProperty("Name");
                                    var nameVal = nameProp?.GetValue(value, null);
                                    if (nameVal is IComparable nameComparable)
                                        return nameComparable;
                                }

                                return null;
                            })
                            .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Items;
        }

        private string QueryText(DropDownBoxQueryDisplayTextContext arg)
        {
            var names = (arg.Value as IEnumerable<SelectItemComboBox>)?.Select(x => x.Value);
            return names != null ? string.Join(",", names) : string.Empty;
        }

        private void SetCatalogPropertyValue(object model, string propertyName, Guid? selectedItem)
        {
            var property = model.GetType().GetProperty(propertyName);
            if (property != null && property.PropertyType == typeof(CatalogEntity))
            {
                var catalogEntity = property.GetValue(model) as CatalogEntity;
                if (catalogEntity != null)
                {
                    catalogEntity.Id = (Guid)selectedItem;
                    if (Catalogues.TryGetValue(propertyName, out IEnumerable<SelectItemComboBox> items))
                        catalogEntity.Name = items.FirstOrDefault(x => x.Key == selectedItem.Value)?.Value ?? string.Empty;
                    if (model is IBaseModel o)
                        o.DataOperationType = OperationType.Update;
                }
            }
        }

        private void SetPropertyValue(string propertyName, string newValue, object model)
        {
            var propertyInfo = model.GetType().GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                string oldValue = propertyInfo.GetValue(model) as string;

                if (oldValue != newValue)
                {
                    propertyInfo.SetValue(model, newValue);
                    if (model is IBaseModel o)
                        o.DataOperationType = OperationType.Update;
                }
                newValue = newValue.Trim() ?? string.Empty;

                SetValidUpdateData(propertyName, newValue, model);
            }
        }

        private void SetValidUpdateData(string propertyName, string newValue, object model)
        {
            SetMandatoryAtributeUpdate(propertyName, newValue, model);
            SetUniqueAtributeUpdate(propertyName, newValue, model);
        }

        private void SetNumericAtributeUpdate(string propertyName, decimal newValue, object model)
        {
            if (!Properties.TryGetValue(propertyName, out var prop) || !prop.Required)
                return;

            bool isNumericType = prop.FieldType == ComponentType.NumericSpin
                               || prop.FieldType == ComponentType.NumericSpinInt;

            if (!isNumericType) return;

            var idProp = model.GetType().GetProperty("Id");
            var guid = (Guid)(idProp?.GetValue(model) ?? Guid.Empty);

            if (!UpdateErrors.TryGetValue(guid, out var fieldErrors))
                fieldErrors = new Dictionary<string, string>();

            if (newValue <= 0)
            {
                fieldErrors[propertyName] = Loc.Loc(MSG_GREAT_THAN, prop.Caption);
                UpdateErrors[guid] = fieldErrors;
            }
            else
            {
                if (fieldErrors.Remove(propertyName) && fieldErrors.Count == 0)
                    UpdateErrors.Remove(guid);
            }

            SetUniqueAtributeUpdate(propertyName, newValue.ToString(), model);
            InvokeAsync(StateHasChanged);
        }


        private void SetPropertyValue(string propertyName, TimeSpan newValue, object model)
        {
            var propertyInfo = model.GetType().GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                TimeSpan oldValue = (TimeSpan)propertyInfo.GetValue(model);

                if (oldValue != newValue)
                {
                    propertyInfo.SetValue(model, newValue);

                    if (model is IBaseModel o)
                        o.DataOperationType = OperationType.Update;

                    GreaterThanUpdate(model);
                }
            }
        }

        private void SetPropertyValue(string propertyName, TimeSpan newValue)
        {
            var propertyInfo = ElementNew.GetType().GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                TimeSpan oldValue = (TimeSpan)propertyInfo.GetValue(ElementNew);

                if (oldValue != newValue)
                {
                    propertyInfo.SetValue(ElementNew, newValue);
                    GreaterThan();
                }
            }
        }

        private void SetPropertyValue(string propertyName, Guid? newValue)
        {
            if (ElementNew.GetProperty(propertyName) is CatalogEntity catalogue)
            {
                catalogue.Id = newValue.Value;

                if (Catalogues.TryGetValue(propertyName, out IEnumerable<SelectItemComboBox> items))
                {
                    catalogue.Name = items.FirstOrDefault(x => x.Key == newValue.Value)?.Value ?? string.Empty;
                    ValidationErrors.Remove(propertyName);
                }
            }
        }

        private async void SetStateAction(ActionState actionState)
        {
            switch (actionState)
            {
                case ActionState.Add:
                    ExecuteStateAdd();
                    break;
                case ActionState.Edit:
                    ExecuteStateEdit();
                    break;
                case ActionState.Delete:
                    await ExecuteStateDelete();
                    break;
                case ActionState.Save:
                    await ExecuteStateSave();
                    break;
                case ActionState.ReadOnly:
                    await ExecuteStateReadOnly();
                    break;
            }
        }

        private void SetValueProperty(string propertyName, string newValue)
        {
            var propertyInfo = ElementNew.GetType().GetProperty(propertyName);

            newValue = string.IsNullOrWhiteSpace(newValue) ? string.Empty : newValue.Trim();

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                string oldValue = propertyInfo.GetValue(ElementNew)?.ToString() ?? string.Empty;

                if (oldValue != newValue)
                {
                    propertyInfo.SetValue(ElementNew, newValue);

                    if (Properties.TryGetValue(propertyName, out var property))
                    {
                        if (string.IsNullOrEmpty(newValue) && property.Required)
                        {
                            ValidationErrors[propertyName] = Loc.Loc(MSG_FIELD_MANDATORY);
                        }
                        else
                        {
                            ValidationErrors.Remove(propertyName);
                            SetUniqueAtribute(propertyName, newValue);
                        }
                    }
                }
            }
        }

        private void GreaterThan()
        {
            if (ElementNew is ICustomValidation customValidatable)
            {
                var result = customValidatable.CustomValidation();

                if (result != ValidationResult.Success)
                {
                    _lastValidatedMembers = result.MemberNames.ToList();

                    foreach (var member in _lastValidatedMembers)
                    {
                        if (Properties.TryGetValue(member, out var property))
                        {
                            ValidationErrors[member] = Loc.Loc(result.ErrorMessage);
                        }
                    }
                }
                else
                {
                    foreach (var member in _lastValidatedMembers)
                    {
                        if (ValidationErrors.ContainsKey(member))
                        {
                            ValidationErrors.Remove(member);
                        }
                    }
                    _lastValidatedMembers.Clear();
                }
            }
        }

        private void GreaterThanUpdate(object model)
        {
            if (model is ICustomValidation customValidatable)
            {
                var result = customValidatable.CustomValidation();
                var idProp = model.GetType().GetProperty("Id");
                var idValue = (Guid?)idProp?.GetValue(model) ?? Guid.Empty;

                if (result != null && result != ValidationResult.Success)
                {
                    foreach (var member in result.MemberNames)
                    {
                        if (Properties.TryGetValue(member, out var property))
                        {
                            if (!UpdateErrors.TryGetValue(idValue, out var fieldErrors))
                            {
                                fieldErrors = new Dictionary<string, string>();
                                UpdateErrors[idValue] = fieldErrors;
                            }

                            fieldErrors[member] = Loc.Loc(result.ErrorMessage);
                        }
                    }
                }
                else
                {
                    if (UpdateErrors.TryGetValue(idValue, out var fieldErrors))
                    {
                        fieldErrors.Clear();
                        if (fieldErrors.Count == 0)
                            UpdateErrors.Remove(idValue);
                    }
                }
            }
        }

        private void SetUniqueAtribute(string propertyName, string NewValue)
        {
            if (string.IsNullOrWhiteSpace(NewValue))
                return;

            string ValueNormalizated = NewValue.Trim();
            var propertyInfo = ElementNew.GetType().GetProperty(propertyName);
            var uniqueAttr = propertyInfo.GetCustomAttributes(typeof(UniqueAttributes), inherit: true)
                                         .FirstOrDefault() as UniqueAttributes;

            if (uniqueAttr != null && uniqueAttr.IsUnique)
            {
                bool existsInDb = ValidUnique(propertyName, ValueNormalizated);

                bool existsInMemory = Items.Any(item =>
                {
                    var value = item.GetType().GetProperty(propertyName)?.GetValue(item)?.ToString();
                    return string.Equals(value, ValueNormalizated, StringComparison.OrdinalIgnoreCase);
                });

                if ((existsInDb || existsInMemory) && Properties.TryGetValue(propertyName, out var property))
                    ValidationErrors[propertyName] = Loc.Loc("*The {0} must be unique.", property.Caption);
                else
                    ValidationErrors.Remove(propertyName);
            }
        }

        private void SetUniqueAtributeUpdate(string propertyName, string newValue, object model)
        {

            if (string.IsNullOrWhiteSpace(newValue))
            {
                var caption0 = Properties.TryGetValue(propertyName, out var p0) ? p0.Caption : propertyName;
                var uniqueMsg0 = Loc.Loc("*The {0} must be unique.", caption0);

                foreach (var kvp in UpdateErrors.ToList())
                {
                    var id = kvp.Key;
                    var fieldErrors = kvp.Value;

                    if (fieldErrors.TryGetValue(propertyName, out var currentMsg) && currentMsg == uniqueMsg0)
                    {
                        fieldErrors.Remove(propertyName);
                        if (fieldErrors.Count == 0)
                            UpdateErrors.Remove(id);
                    }
                }
                return;
            }

            var propertyInfo = ElementNew.GetType().GetProperty(propertyName);
            var uniqueAttr = propertyInfo?.GetCustomAttributes(typeof(UniqueAttributes), true)
                                         .FirstOrDefault() as UniqueAttributes;
            if (uniqueAttr == null || !uniqueAttr.IsUnique)
                return;


            var duplicateIds = NoUnique(propertyName);


            var currentId = (Guid)(model.GetType().GetProperty("Id")?.GetValue(model) ?? Guid.Empty);
            if (!duplicateIds.Contains(currentId))
            {
                var val = model.GetType().GetProperty(propertyName)?.GetValue(model)?.ToString()?.Trim()?.ToLowerInvariant();
                if (!string.IsNullOrEmpty(val))
                {
                    var same = Items.Any(x => x.Id != currentId &&
                                              x.GetType().GetProperty(propertyName)?.GetValue(x)?.ToString()?.Trim()?.ToLowerInvariant() == val);
                    if (same)
                        duplicateIds.Add(currentId);
                }
            }

            var caption = Properties.TryGetValue(propertyName, out var prop) ? prop.Caption : propertyName;
            var uniqueMsg = Loc.Loc("*The {0} must be unique.", caption);

            foreach (var item in Items)
            {
                var id = item.Id;
                var esDuplicado = duplicateIds.Contains(id);

                if (esDuplicado)
                {

                    if (!UpdateErrors.TryGetValue(id, out var fieldErrors))
                    {
                        fieldErrors = new Dictionary<string, string>();
                        UpdateErrors[id] = fieldErrors;
                    }
                    fieldErrors[propertyName] = uniqueMsg;
                }
                else
                {
                    if (UpdateErrors.TryGetValue(id, out var fieldErrors))
                    {
                        if (fieldErrors.TryGetValue(propertyName, out var current) && current == uniqueMsg)
                        {
                            fieldErrors.Remove(propertyName);
                            if (fieldErrors.Count == 0)
                                UpdateErrors.Remove(id);
                        }
                    }
                }
            }
        }

        private void SetMandatoryAtributeUpdate(string propertyName, string newValue, object model)
        {
            var propertyInfo = ElementNew.GetType().GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {

                if (Properties.TryGetValue(propertyName, out var property))
                {
                    if (string.IsNullOrEmpty(newValue) && property.Required)
                    {
                        if (model.GetType().GetProperty("Id").GetValue(model) is Guid guid)
                        {
                            if (!UpdateErrors.TryGetValue(guid, out var fieldErrors))
                            {
                                fieldErrors = new Dictionary<string, string>();
                                UpdateErrors[guid] = fieldErrors;
                            }

                            fieldErrors[propertyName] = MSG_FIELD_MANDATORY;
                        }
                    }
                    else
                        ClearValidationError(propertyName, model);
                }

            }
        }

        private void ClearValidationError(string propertyName, object model)
        {
            if (model.GetType().GetProperty("Id").GetValue(model) is Guid guid)
            {
                if (UpdateErrors.TryGetValue(guid, out var fieldErrors))
                {
                    fieldErrors.Remove(propertyName);
                    if (fieldErrors.Count == 0)
                        UpdateErrors.Remove(guid);
                }
            }
        }

        private bool ValidUnique(string propertyName, string NewValue)
        {
            foreach (var i in _data)
            {
                var property = i.GetType().GetProperty(propertyName);
                if (property != null)
                {
                    var value = property.GetValue(i);
                    if (value?.ToString() == NewValue)
                        return true;
                }
            }
            return false;
        }

        private List<Guid> NoUnique(string propertyName)
        {
            var valueMap = new Dictionary<Guid, string>();
            foreach (var item in Items)
            {
                var property = item.GetType().GetProperty(propertyName);
                if (property == null)
                    continue;

                var value = property.GetValue(item)?.ToString();
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                valueMap[item.Id] = value.Trim().ToLowerInvariant();
            }

            var valueCounts = valueMap.GroupBy(kvp => kvp.Value).Where(g => g.Count() > 1).SelectMany(g => g.Select(kvp => kvp.Key)).ToList();

            return valueCounts;
        }

        private void SetValueProperty(string propertyName, bool newValue, object model)
        {
            var propertyInfo = ElementNew.GetType().GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                bool oldValue = (bool)propertyInfo.GetValue(model);

                if (oldValue != newValue)
                {
                    propertyInfo.SetValue(model, newValue);

                    if (model is IBaseModel o)
                        o.DataOperationType = OperationType.Update;
                }
            }
        }

        private void SetValueProperty(string propertyName, bool newValue)
        {
            var propertyInfo = ElementNew.GetType().GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                bool oldValue = (bool)propertyInfo.GetValue(ElementNew);

                if (oldValue != newValue)
                {
                    propertyInfo.SetValue(ElementNew, newValue);
                    if (ValidationErrors.TryGetValue(propertyName, out var value))
                        ValidationErrors.Remove(propertyName);
                }
            }
        }

        private void SetValueProperty(string propertyName, bool? newValue)
        {
            var propertyInfo = ElementNew.GetType().GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                bool? oldValue = (bool?)propertyInfo.GetValue(ElementNew);

                if (oldValue != newValue)
                {
                    propertyInfo.SetValue(ElementNew, newValue);

                    if (newValue != null)
                        ValidationErrors.Remove(propertyName);
                    else
                        ValidationErrors[propertyName] = MSG_FIELD_MANDATORY;
                }
            }
        }

        private void SetValueProperty(string propertyName, decimal newValue)
        {
            var propertyInfo = ElementNew.GetType().GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                decimal oldValue = decimal.Parse(propertyInfo.GetValue(ElementNew)?.ToString() ?? "0");

                if (oldValue != newValue)
                {
                    Type propertyType = propertyInfo.PropertyType;

                    if (propertyType == typeof(int))
                        propertyInfo.SetValue(ElementNew, (int)newValue);
                    else if (propertyType == typeof(decimal))
                        propertyInfo.SetValue(ElementNew, newValue);
                    else if (propertyType == typeof(float))
                        propertyInfo.SetValue(ElementNew, (float)newValue);
                    else if (propertyType == typeof(double))
                        propertyInfo.SetValue(ElementNew, (double)newValue);

                    if (newValue <= 0)
                        ValidationErrors[propertyName] = Loc.Loc(MSG_GREAT_THAN, Properties.TryGetValue(propertyName, out var meta) ? meta.Caption : propertyName);
                    else
                        ValidationErrors.Remove(propertyName);

                    var uniqueAttr = propertyInfo.GetCustomAttributes(typeof(UniqueAttributes), inherit: true)
                                 .FirstOrDefault() as UniqueAttributes;

                    if (uniqueAttr != null && uniqueAttr.IsUnique)
                    {
                        bool existsInMemory = Items.Any(item =>
                        {
                            var value = item.GetType().GetProperty(propertyName)?.GetValue(item);
                            return value != null && Convert.ToDecimal(value) == newValue;
                        });

                        if (existsInMemory && Properties.TryGetValue(propertyName, out var property))
                            ValidationErrors[propertyName] = Loc.Loc("*The {0} must be unique.", property.Caption);
                        else
                            ValidationErrors.Remove(propertyName);
                    }
                }
            }
        }

        private void SetValueProperty(string propertyName, decimal newValue, object model)
        {
            var propertyInfo = model.GetType().GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                var oldValue = Convert.ToDecimal(propertyInfo.GetValue(model) ?? 0);

                if (oldValue != newValue)
                {
                    Type propertyType = propertyInfo.PropertyType;

                    if (propertyType == typeof(int))
                        propertyInfo.SetValue(model, (int)newValue);
                    else if (propertyType == typeof(decimal))
                        propertyInfo.SetValue(model, newValue);
                    else if (propertyType == typeof(float))
                        propertyInfo.SetValue(model, (float)newValue);
                    else if (propertyType == typeof(double))
                        propertyInfo.SetValue(model, (double)newValue);

                    if (model is IBaseModel o)
                        o.DataOperationType = OperationType.Update;
                }
            }
            SetValidUpdateData(propertyName, newValue.ToString(), model);
            SetNumericAtributeUpdate(propertyName, newValue, model);
        }

        private void UpdateListMultiSelect(string propertyName, object model, IEnumerable<SelectItemComboBox> values)
        {
            if (model != null)
            {
                var propertyInfo = model.GetType().GetProperty(propertyName);
                var value = propertyInfo.GetValue(model);
                if (value is Multiselect elementModel)
                    elementModel.Items = values.ToDictionary(item => item.Key, item => item.Value);
            }
        }

        private void UpdateNameMultiSelect(IEnumerable<SelectItemComboBox> values, string propertyName)
        {
            string valueString = string.Join(",", values.Select(x => x.Value));
            if (ElementNew.GetProperty(propertyName) is Multiselect element)
            {
                element.Name = valueString;
                element.Items = values.ToDictionary(item => item.Key, item => item.Value);
            }
        }

        private void ValidatePropertyMultiSelec(IEnumerable<SelectItemComboBox> values, string propertyName)
        {
            if (values.Any())
                ValidationErrors.Remove(propertyName);
            else
                ValidationErrors[propertyName] = MSG_FIELD_MANDATORY;
        }

        private void ValidatePropertyModeEdit(IEnumerable<SelectItemComboBox> values, string propertyName, object model)
        {
            if (!Properties.TryGetValue(propertyName, out var property) || !property.Required)
                return;

            if (model is not IBaseModel baseModel || baseModel.Id == Guid.Empty)
                return;

            Guid guid = baseModel.Id;

            UpdateErrors.TryGetValue(guid, out var fieldErrors);
            fieldErrors ??= new Dictionary<string, string>();

            if (values == null || !values.Any())
            {
                fieldErrors[propertyName] = MSG_FIELD_MANDATORY;
                UpdateErrors[guid] = fieldErrors;
            }
            else if (fieldErrors.Remove(propertyName) && fieldErrors.Count == 0)
            {
                UpdateErrors.Remove(guid);
            }
        }

        private MarkupString GetInboudType(bool value) => value ? (MarkupString)OUTBOUND : (MarkupString)INBOUND;

        private bool HasRowError(object dataItem)
        {
            var idProp = dataItem.GetType().GetProperty("Id");
            var id = (Guid?)idProp?.GetValue(dataItem) ?? Guid.Empty;
            if (id == Guid.Empty) return false;

            return UpdateErrors.TryGetValue(id, out var errs)
                   && errs.Values.Any(v => !string.IsNullOrWhiteSpace(v));
        }

        private bool GetIsCatalogueFillData()
        {
            if (string.IsNullOrWhiteSpace(ErrorMessage))
                return false;

            if (Catalogues == null || !Catalogues.Any())
                return false;

            return IsCatalogueEmpty();
        }

        private bool IsCatalogueEmpty() => PanelType switch
        {
            ePanelType.Area or ePanelType.Process => !(Catalogues.TryGetValue(PanelType.ToString(), out var item) && item.ToList() is List<SelectItemComboBox> i && i.Any()),
            ePanelType.Any => true,
            _ => true,
        };

        void OnCustomizeFilterMenu(GridCustomizeFilterMenuEventArgs e)
        {
            var boolFields = new[]
            {
                nameof(BreaksDto.IsPaid),
                nameof(BreaksDto.IsRequiered),
                nameof(ResourceWorkerScheduleDto.Available)
            };

            if (e.DataColumn.FieldName == nameof(OrderProfilesDto.IsOut))
            {
                e.DataItems.ForEach(di => {
                    if (di.Value is bool value)
                        di.DisplayText = (bool)di.Value ? l.Loc("OutBound") : l.Loc("Inbound");
                });
                return;
            }

            if (boolFields.Contains(e.DataColumn.FieldName))
            {
                e.DataItems.ForEach(di => {
                    if (di.Value is bool value)
                        di.DisplayText = (bool)di.Value ? l.Loc("Yes") : l.Loc("No");
                });
            }
        }

        private void SetComponentState(
            bool isEditing = false,
            bool showNewElementForm = false,
            bool showActionAdd = false,
            bool showActionEdit = false,
            bool showActionDelete = false,
            bool showActionSave = false,
            bool showActionReadOnly = false,
            bool clearSelection = false)
        {
            this.isEditing = isEditing;
            this.showNewElementForm = showNewElementForm;
            this.showActionAdd = showActionAdd;
            this.showActionEdit = showActionEdit;
            this.showActionDelete = showActionDelete;
            this.showActionSave = showActionSave;
            this.showActionReadOnly = showActionReadOnly;

            if (clearSelection)
                _selectedDataGridItems = null;
        }

        #endregion
    }

}