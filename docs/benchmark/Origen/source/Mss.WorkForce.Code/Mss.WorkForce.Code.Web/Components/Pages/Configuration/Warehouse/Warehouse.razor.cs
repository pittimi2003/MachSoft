using Microsoft.AspNetCore.Components;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Web.Common;
using Mss.WorkForce.Code.Web.Components.BaseComponent;
using Mss.WorkForce.Code.Web.Services;
using System.Collections.ObjectModel;
using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Web.Components.Pages.Configuration.Warehouse
{
    public partial class Warehouse : PageOperations, IDisposable
    {

        #region Fields

        private BaseComponente<WarehouseDto>? baseComponentRef;
        private List<Guid> SelectedWarehouseIds = new();
        private EventActions SelectMode = EventActions.UnSelected;

        #endregion

        #region Properties

        [Inject] private IContextConfig _contextConfig { get; set; }
        [Inject] private IInitialDataService _InitialDataService { get; set; }
        [Inject] private SendParamService _SendParamService { get; set; }
        [Inject] private LocalStorageService _storageService { get; set; }
        [Inject] private IEventServices eventServices { get; set; }
        [Inject] private IUserService UserService { get; set; }
        [Inject] private IWarehouseService WarehouseService { get; set; }

        private List<EventActions> Actions { get; set; }
        private string Cancel = string.Empty;
        private string Data = string.Empty;
        [Inject] private IMlxDialogService DialogService { get; set; }
        private string EmiterContainer { get; set; }
        private string EventListener { get; set; } = string.Empty;
        private GetAttributesDto<List<WarehouseDto>> GetParametersDto { get; set; }
        private bool IsEditMode { get; set; } = false;
        private bool IsNewMode { get; set; } = false;
        private List<WarehouseDto?> lstSelectedWarehouses { get; set; } = new();
        private string Save = string.Empty;
        private bool savebtn { get; set; } = false;
        private WarehouseDto? SelectedWarehouse { get; set; } = null;
        private ReadOnlyCollection<TimeZoneInfo> TimeZones { get; set; }

        #endregion

        #region Methods

        public void BaseComponentActions(EventArguments eventArguments)
        {
            savebtn = (eventArguments.EventActions == EventActions.SaveValid && eventArguments.EventData is bool flag && flag);
            StateHasChanged();
        }

        public override void CancelEvent()
        {
            IsNewMode = false;
            SelectMode = EventActions.UnSelected;
        }

        public override bool DetailsActionsCancel(EventArguments eventArguments)
        {
            IsEditMode = false;
            StateHasChanged();
            return true;
        }

        public override async Task<bool> DetailsActionsUpdate(EventArguments eventArguments)
        {
            try
            {
                if (eventArguments.EventData is List<WarehouseDto> warehouseDto)
                {
                    await WarehouseService.UpdateWarehouse(warehouseDto);
                    GetParametersDto.Model = GetWarehouses();
                    IsEditMode = false;
                    eventServices.Publish(GetParametersDto.NameContainer, new EventArguments(GetParametersDto.NameContainer, EventActions.Refresh, null));
                    _SendParamService.ReloadInitData = true;
                    StateHasChanged();
                    return true;
                }
                else
                {
                    throw new InvalidOperationException("Invalid data type for update action.");
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void Dispose()
        {
            eventServices.Unsubscribe(GetParametersDto.NameContainer, GetParametersDto.GridPublish, GridActions);
            eventServices.Unsubscribe(GetParametersDto.NameContainer, GetParametersDto.ToolBarPublish, ToolBarActions);
            eventServices.Unsubscribe(GetParametersDto.NameContainer, GetParametersDto.DetailPublish, DetailsAction);
            eventServices.Unsubscribe(nameof(Warehouse), EmiterContainer, BaseComponentActions);
        }

        public override bool GridActionsCollapseGrid(EventArguments eventArguments)
        {
            IsDetailsVisible = true;
            isExpandGrid = false;
            StateHasChanged();
            return true;
        }

        public override bool GridActionsExpandGrid(EventArguments eventArguments)
        {
            IsDetailsVisible = false;
            isExpandGrid = true;
            StateHasChanged();
            return true;
        }

        public override bool GridActionsMultiSelected(EventArguments eventArguments)
        {
            lstSelectedWarehouses.Clear();
            SelectedWarehouseIds.Clear();
            if (eventArguments.EventData is List<object> selectedItems && selectedItems.Count >= 1)
            {
                foreach (object selectedItem in selectedItems)
                {
                    if (!lstSelectedWarehouses.Contains(selectedItem))
                        lstSelectedWarehouses.Add(selectedItem as WarehouseDto);
                }
                SelectedWarehouse = selectedItems.FirstOrDefault() as WarehouseDto;
                SelectedWarehouseIds = lstSelectedWarehouses.Select(w => w.Id).ToList();
                SelectMode = EventActions.MultiSelected;
                IsDetailsVisible = true;
                isExpandGrid = false;
            }

            return true;
        }

        public override bool GridActionsRefresh(EventArguments eventArguments)
        {
            GetParametersDto.Model = GetWarehouses();
            eventServices.Publish(GetParametersDto.NameContainer, new EventArguments(GetParametersDto.NameContainer, EventActions.Refresh, null));
            return true;
        }

        public override bool GridActionsSelected(EventArguments eventArguments)
        {
            isExpandGrid = true;
            SelectedWarehouse = null;
            lstSelectedWarehouses.Clear();
            SelectedWarehouseIds.Clear();
            IsDetailsVisible = false;
            SelectMode = EventActions.UnSelected;

            if (eventArguments.EventData is List<object> selectedItems && selectedItems.Count == 1)
            {
                lstSelectedWarehouses.Add(selectedItems.FirstOrDefault() as WarehouseDto);
                SelectedWarehouse = selectedItems.FirstOrDefault() as WarehouseDto;
                SelectedWarehouseIds = lstSelectedWarehouses.Select(w => w.Id).ToList();
                IsDetailsVisible = true;
                SelectMode = EventActions.Selected;
                isExpandGrid = false;
            }

            return true;
        }

        public override async Task SaveEvent()
        {
            try
            {
                var isValid = baseComponentRef != null ? baseComponentRef.Validate() : false;

                if (IsNewMode && SelectedWarehouse != null && isValid)
                {
                    await WarehouseService.AddWarehouse(SelectedWarehouse);
                    IsNewMode = false;
                    SelectedWarehouse = null;

                    GetParametersDto.Model = GetWarehouses();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar el registro: {ex.Message}");
            }

            StateHasChanged();
        }

        public override async Task<bool> ToolBarActionsDelete(EventArguments eventArguments)
        {
            var code = _InitialDataService.GetDatauser().Code;
            var user = UserService.GetUserByCode(code);

            if (lstSelectedWarehouses.Any(x => x.Id == user.WarehouseDefaultId))
            {
                bool respWarehouse = await DialogService.ShowDialogAsync(Loc("NOTIFICATION"), Loc("Unable to delete current warehouse!"), false, "", true, Loc("Close"));
                return false;
            }

            bool resp = await DialogService.ShowDialogAsync(Loc("NOTIFICATION"), Loc("Are you sure you want to delete {0}?", string.Join(", ", lstSelectedWarehouses.Select(e => e.Code))));

            if (resp && lstSelectedWarehouses != null)
            {
                await WarehouseService.DeleteWarehouse(lstSelectedWarehouses);
                SelectedWarehouse = null;
                GetParametersDto.Model = GetWarehouses();
                IsDetailsVisible = false;
                IsEditMode = false;
                SelectMode = EventActions.UnSelected;
                eventServices.Publish(GetParametersDto.NameContainer, new EventArguments(GetParametersDto.NameContainer, EventActions.Refresh, null));
                _SendParamService.ReloadInitData = true;
                StateHasChanged();
            }

            return true;
        }

        public override bool ToolBarActionsNew(EventArguments eventArguments)
        {
            lstSelectedWarehouses.Clear();
            SelectedWarehouseIds.Clear();
            IsNewMode = true;
            IsDetailsVisible = false;
            AddNewItem();
            lstSelectedWarehouses.Add(SelectedWarehouse);
            return true;
        }

        public override bool ToolBarActionsUpdate(EventArguments eventArguments)
        {
            if (SelectedWarehouse != null)
            {
                IsEditMode = true;
                isExpandGrid = false;
                IsDetailsVisible = true;
            }
            return true;
        }
        protected override void LoadData()
        {
            // Lógica específica para este componente

        }

        protected override void OnInitialized()
        {
            TranslateFields();
            GetParametersDto = new(l);
            GetParametersDto.Model = GetWarehouses();

            GetParametersDto.NameContainer = nameof(Warehouse);
            GetParametersDto.GridPublish = $"{nameof(GridActions)}{nameof(Warehouse)}";
            GetParametersDto.ToolBarPublish = $"{nameof(ToolBarActions)}{nameof(Warehouse)}";
            GetParametersDto.DetailPublish = $"{nameof(DetailsAction)}{nameof(Warehouse)}";

            EventListener = $"{nameof(ToolBarActions)}{nameof(Warehouse)}";
            EmiterContainer = $"{nameof(Warehouse)}{nameof(BaseComponent)}";
            eventServices.Subscribe(GetParametersDto.NameContainer, GetParametersDto.GridPublish, GridActions);
            eventServices.Subscribe(GetParametersDto.NameContainer, GetParametersDto.ToolBarPublish, ToolBarActions);
            eventServices.Subscribe(GetParametersDto.NameContainer, GetParametersDto.DetailPublish, DetailsAction);
            eventServices.Subscribe(nameof(Warehouse), EmiterContainer, BaseComponentActions);

            Actions = new List<EventActions>()
        {
            {EventActions.New},
            {EventActions.Edit},
            {EventActions.Delete},
        };

        }


        private void TranslateFields()
        {
            Data = Loc("DATA");
            Cancel = Loc("Cancel");
            Save = Loc("Save");
        }
        private void AddNewItem()
        {
            SelectedWarehouse = WarehouseDto.NewDto();
        }



        private List<WarehouseDto> GetWarehouses()
        {
            return WarehouseService.GetWarehouses().OrderBy(x => x.Code).ToList();
        }

        private void OnRightSideVisibilityChanged(bool isVisible)
        {
            Console.WriteLine($"Right sidebar visibility changed: {isVisible}");
        }


        #endregion
    }
}
