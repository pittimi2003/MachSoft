using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.DTO.Designer;
using Mss.WorkForce.Code.Web.Common;
using Mss.WorkForce.Code.Web.Components.BaseComponent;
using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web.Services;

namespace Mss.WorkForce.Code.Web.Components.Pages.Configuration.Projects
{
    public partial class Projects : PageOperations, IDisposable
    {
        #region selectMode
        private EventActions SelectMode = EventActions.UnSelected;
        #endregion

        #region Injections
        [Inject]
        private IEventServices eventServices { get; set; }
        [Inject]
        private IDesignerServices _designer { get; set; }
        [Inject]
        public IWarehouseService _wharehouse { get; set; }
        [Inject]
        public IMlxDialogService _dialog { get; set; }

        [Inject]
        public IInitialDataService _InitialDataService { get; set; }
        #endregion

        #region Properties
        private List<WarehouseDto> WarehouseDto { get; set; }
        private WarehouseDto? SelectedWarehouse { get; set; }
        private LayoutDto? SelectedLayout { get; set; } = null;
        private List<Guid> SelectedLayoutIds = new();
        private List<LayoutDto?> lstSelectedLayout { get; set; } = new();
        private bool IsEditMode { get; set; } = false;
        public bool ShowNewView { get; set; } = false;
        public bool ShowDesignerView { get; set; } = false;
        public bool ShowDetail { get; set; } = false;
        public bool IsProjectView { get; set; } = false;
        private GetAttributesDto<List<LayoutDto>> GetLayoutsDto { get; set; }
        private List<EventActions>? Actions { get; set; }
        private string EventListener { get; set; } = string.Empty;
        #endregion

        #region BaseComponent
        private BaseComponente<LayoutDto>? baseComponentLayout;
        private string BaseComponentContainer { get; set; }
        private bool savebtn { get; set; } = false;
        private GetAttributesDto<List<WarehouseDto>> GetParametersDto { get; set; }

        #endregion


        protected override void OnInitialized()
        {
            GetLayoutsDto = new(l);
            GetParametersDto = new(l);
            LoadData();
            GetLayoutsDto.NameContainer = nameof(Projects);
            GetLayoutsDto.GridPublish = $"{nameof(GridActions)}{nameof(Projects)}";
            GetLayoutsDto.ToolBarPublish = $"{nameof(ToolBarActions)}{nameof(Projects)}";
            GetLayoutsDto.DetailPublish = $"{nameof(DetailsAction)}{nameof(Projects)}";
            EventListener = $"{nameof(ToolBarActions)}{nameof(Projects)}";
            BaseComponentContainer = $"{nameof(Projects)}{nameof(BaseComponent)}";
            eventServices.Subscribe(GetLayoutsDto.NameContainer, GetLayoutsDto.GridPublish, GridActions);
            eventServices.Subscribe(GetLayoutsDto.NameContainer, GetLayoutsDto.ToolBarPublish, ToolBarActions);
            eventServices.Subscribe(GetLayoutsDto.NameContainer, GetLayoutsDto.DetailPublish, DetailsAction);
            eventServices.Subscribe(nameof(Projects), BaseComponentContainer, BaseComponentActions);
            Actions = new List<EventActions>()
            {
                {EventActions.New},
                {EventActions.Edit},
                {EventActions.Designer},
                { EventActions.Clone},
                {EventActions.Delete}
            };
        }

        protected override async Task OnInitializedAsync()
        {
            Navigation.LocationChanged += OnLocationChanged;
            _SendParamService.OnChange += OnSendParamChanged;
        }

        private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            if (_SendParamService.ActiveProjectName == null)
            {
                lstSelectedLayout.Clear();
                SelectedLayoutIds.Clear();
                SelectedLayout = LayoutDto.NewDto();
                ShowDesignerView = false;
                ShowNewView = false;
                IsProjectView = false;
                ShowDetail = false;
                IsEditMode = false;
            }

            StateHasChanged();
        }

        private async void OnSendParamChanged()
        {
            await InvokeAsync(async () =>
            {
                if (!string.IsNullOrEmpty(_SendParamService.ActiveProjectName))
                {
                    LoadProjectFromService();
                    StateHasChanged();
                }
            });
        }

        private void LoadProjectFromService()
        {
            lstSelectedLayout.Clear();
            SelectedLayoutIds.Clear();
            SelectedLayout = LayoutDto.NewDto();

            SelectedLayout = GetLayouts().FirstOrDefault(x =>
                x.Name == _SendParamService.ActiveProjectName);

            if (SelectedLayout != null)
            {
                lstSelectedLayout.Add(SelectedLayout);
                SelectedLayoutIds = lstSelectedLayout.Select(l => l.Id).ToList();

                ShowDesignerView = true;
                ShowNewView = true;
                IsProjectView = true;
                ShowDetail = false;
                IsEditMode = false;
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                GetLayoutsDto.Model = GetLayouts();
                eventServices.Publish(GetLayoutsDto.NameContainer, new EventArguments(GetLayoutsDto.NameContainer, EventActions.Refresh, null));
            }

            if (firstRender && !string.IsNullOrEmpty(_SendParamService.ActiveProjectName))
            {
                lstSelectedLayout.Clear();
                SelectedLayoutIds.Clear();
                SelectedLayout = LayoutDto.NewDto();

                SelectedLayout = GetLayouts().FirstOrDefault(x => x.Name == _SendParamService.ActiveProjectName);
                lstSelectedLayout.Add(SelectedLayout);
                SelectedLayoutIds = lstSelectedLayout.Select(l => l.Id).ToList();

                ShowDesignerView = true;
                ShowNewView = true;
                IsProjectView = true;
                ShowDetail = false;
                IsEditMode = false;
                StateHasChanged();
            }
        }

        public void BaseComponentActions(EventArguments eventArguments)
        {
            savebtn = (eventArguments.EventActions == EventActions.SaveValid && eventArguments.EventData is bool flag && flag);
            StateHasChanged();
        }

        private List<LayoutDto> GetLayouts()
        {
            // Interface to Layout
            List<LayoutDto> layoutList = _designer.GetLayoutsDto(_InitialDataService.GetDatauser()?.Id ?? Guid.Empty).OrderBy(x => x.Name).ToList();
            return layoutList;
        }

        protected override void LoadData()
        {
            // Lógica específica para este componente
            WarehouseDto = _wharehouse.GetWarehouses().ToList();
        }

        public override bool ToolBarActionsNew(EventArguments eventArguments)
        {
            lstSelectedLayout.Clear();
            SelectedLayoutIds.Clear();
            ShowNewView = true;
            ShowDetail = false;

            AddNewItem();
            lstSelectedLayout.Add(SelectedLayout);
            return true;
        }

        private void AddNewItem()
        {
            SelectedLayout = LayoutDto.NewDto();
        }
        public override void CancelEvent()
        {
            ShowNewView = false;
            SelectMode = EventActions.UnSelected;
        }

        public override async Task SaveEvent()
        {
            try
            {
                bool isValid = baseComponentLayout != null ? baseComponentLayout.Validate() : false;
                if (ShowNewView && SelectedLayout != null && isValid)
                {
                    await _designer.AddLayout(SelectedLayout);
                    ShowNewView = false;
                    SelectedLayout = null;
                    SelectedWarehouse = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving the record: {ex.Message}");
            }

            GetLayoutsDto.Model = GetLayouts();

            StateHasChanged();
        }

        public override bool GridActionsRefresh(EventArguments eventArguments)
        {
            GetLayoutsDto.Model = GetLayouts();
            eventServices.Publish(GetLayoutsDto.NameContainer, new EventArguments(GetLayoutsDto.NameContainer, EventActions.Refresh, null));
            return true;
        }

        public override bool GridActionsCollapseGrid(EventArguments eventArguments)
        {
            ShowDetail = true;
            isExpandGrid = false;
            StateHasChanged();
            return true;
        }

        public override bool GridActionsExpandGrid(EventArguments eventArguments)
        {
            ShowDetail = false;
            isExpandGrid = true;
            StateHasChanged();
            return true;
        }

        public override bool GridActionsSelected(EventArguments eventArguments)
        {
            isExpandGrid = true;
            SelectedLayout = null;
            SelectedWarehouse = null;
            lstSelectedLayout.Clear();
            SelectedLayoutIds.Clear();
            ShowDetail = false;
            SelectMode = EventActions.UnSelected;

            if (eventArguments.EventData is List<object> selectedItems && selectedItems.Count == 1)
            {
                lstSelectedLayout.Add(selectedItems.FirstOrDefault() as LayoutDto);
                SelectedLayout = selectedItems.FirstOrDefault() as LayoutDto;
                SelectedLayoutIds = lstSelectedLayout.Select(l => l.Id).ToList();
                ShowDetail = true;
                SelectMode = EventActions.Selected;
                isExpandGrid = false;
            }

            return true;
        }

        public override bool GridActionsMultiSelected(EventArguments eventArguments)
        {
            if (eventArguments.EventData is List<object> selectedItems && selectedItems.Count >= 1)
            {
                lstSelectedLayout.RemoveAll(item => !selectedItems.Contains(item)); //Delete unselected elements

                foreach (object selectedItem in selectedItems)
                {
                    if (!lstSelectedLayout.Contains(selectedItem))
                        lstSelectedLayout.Add(selectedItem as LayoutDto);
                }

                SelectedLayout = selectedItems.FirstOrDefault() as LayoutDto;
                SelectedLayoutIds = lstSelectedLayout.Select(w => w.Id).ToList();
                SelectMode = EventActions.MultiSelected;
                ShowDetail = true;
                isExpandGrid = false;
            }

            return true;
        }

        public void ClearView()
        {
            IsEditMode = false;
            IsProjectView = false;
            ShowDesignerView = false;
            ShowNewView = false;
            ShowDetail = false;
            SelectedWarehouse = null;
            SelectedLayout = null;
            GetLayoutsDto.Model = GetLayouts();
            lstSelectedLayout.Clear();
            SelectedLayoutIds.Clear();
            SelectMode = EventActions.UnSelected;
        }

        public override bool ToolBarActionsUpdate(EventArguments eventArguments)
        {
            if (SelectedLayout != null)
            {
                IsEditMode = true;
                isExpandGrid = false;
                ShowDetail = true;
            }
            return true;
        }

        public async override Task<bool> ToolBarActionsClone(EventArguments eventArguments)
        {
            if (SelectedLayout != null)
            {
                await _designer.CloneLayout(SelectedLayout);
                SelectedLayout = null;
                GetLayoutsDto.Model = GetLayouts();
                ShowDetail = false;
                SelectMode = EventActions.UnSelected;
                eventServices.Publish(GetLayoutsDto.NameContainer, new EventArguments(GetLayoutsDto.NameContainer, EventActions.Refresh, null)); //Deselects the record on deletion
                StateHasChanged();
            }
            return true;
        }

        public override async Task<bool> DetailsActionsUpdate(EventArguments eventArguments)
        {
            try
            {
                if (eventArguments.EventData is List<LayoutDto> layoutDto)
                {
                    if (layoutDto != null)
                    {
                        await _designer.UpdateListLayout(layoutDto);
                        GetLayoutsDto.Model = GetLayouts();
                        IsEditMode = false;
                        if (layoutDto.Count == 1) //Assign the current selection after editing to get the current object to display in DesignerComponent.
                            SelectedLayout = layoutDto[0];
                        eventServices.Publish(GetLayoutsDto.NameContainer, new EventArguments(GetLayoutsDto.NameContainer, EventActions.Refresh, null));
                        StateHasChanged();
                    }
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

        public override async Task<bool> ToolBarActionsDelete(EventArguments eventArguments)
        {
            if (lstSelectedLayout != null && lstSelectedLayout.Any())
            {
                bool confirmed = await _dialog.ShowDialogAsync(
                    Loc("DELETE"),
                    Loc("Are you sure you want to delete {0}?", string.Join(", ", lstSelectedLayout?.Select(x => x.Name))));
                if (confirmed)
                {
                    await _designer.DeleteListLayout(lstSelectedLayout);
                    SelectedLayout = null;
                    ShowDetail = false;
                    IsEditMode = false;
                    GetLayoutsDto.Model = GetLayouts();
                    SelectMode = EventActions.UnSelected;
                    eventServices.Publish(GetLayoutsDto.NameContainer, new EventArguments(GetLayoutsDto.NameContainer, EventActions.Refresh, null)); //Deselects the record on deletion
                    StateHasChanged();
                }
            }

            return true;
        }

        public override bool DetailsActionsCancel(EventArguments eventArguments)
        {
            IsEditMode = false;
            StateHasChanged();
            return true;
        }

        public override bool ToolBarActionsDesigner(EventArguments eventArguments)
        {
            ShowDesignerView = true;
            ShowNewView = true;
            IsProjectView = true;
            ShowDetail = false;
            IsEditMode = false;

            _SendParamService.ActiveProjectName = SelectedLayout.Name;
            _SendParamService.ReloadInitData = true;

            return true;
        }

        public void Dispose()
        {
            _SendParamService.ActiveProjectName = null;
            _SendParamService.ActiveProjectId = Guid.Empty;
            eventServices.Unsubscribe(GetLayoutsDto.NameContainer, GetLayoutsDto.GridPublish, GridActions);
            eventServices.Unsubscribe(GetLayoutsDto.NameContainer, GetLayoutsDto.ToolBarPublish, ToolBarActions);
            eventServices.Unsubscribe(GetLayoutsDto.NameContainer, GetLayoutsDto.DetailPublish, DetailsAction);
            eventServices.Unsubscribe(nameof(Projects), BaseComponentContainer, BaseComponentActions);
            Navigation.LocationChanged -= OnLocationChanged;
            _SendParamService.OnChange -= OnSendParamChanged;
        }
    }
}
