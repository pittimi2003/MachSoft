using Microsoft.AspNetCore.Components;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Web.Common;
using Mss.WorkForce.Code.Web.Services;
using Microsoft.JSInterop;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;
using Mss.WorkForce.Code.Models.DTO.Designer;
using Mss.WorkForce.Code.Models.DTO.Designer.LocationZones;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.Text.Json;

namespace Mss.WorkForce.Code.Web.Components.DesignerComponent
{
    public partial class AreaComponent : PageOperations, ITrackChanges, IChildSaver, IResetBaseline, IRestorable, IModalAware
    {

        #region selectMode
        private EventActions SelectMode = EventActions.UnSelected;
        #endregion

        #region Parameters

        [Parameter]
        public EventCallback OnClickSideBar { get; set; }

        [Parameter]
        public AreaDto DesignerDto { get; set; }

        [Parameter]
        public EventCallback<(ObjectTypes type, object dto, Dictionary<string, object> meta)> OnNavigateChild { get; set; }
        [Parameter] public Dictionary<string, object>? Metadata { get; set; }

        #endregion

        #region Properties
        [Inject] private IDesignerServices _designer { get; set; }
        [Inject] private IJSRuntime JS { get; set; }

        /*Combo Box*/
        private IEnumerable<AreaDto>? AlternativeAreaItems { get; set; }
        private BaseDesignerDto OriginalArea = new();

        private List<Models.DTO.Designer.ZoneDto> RelatedZones { get; set; } = new();
        private List<EquipmentDesignerDto?> RelatedEquipment { get; set; } = new();
        private List<ProcessDto> RelatedProcesses { get; set; } = new();

        private bool ShowTabs => RelatedZones.Any() || RelatedEquipment.Any() || RelatedProcesses.Any();

        private string? areaTypeName;
        private (string Name, string Description)? _tooltipData;

        #region Validation Button

        public bool IsModalSave { get; set; }

        private bool CanSave => _isDirty && !_hasErrors;

        private string _baselineFingerprint = string.Empty;
        private bool _isDirty = false;
        private bool _hasErrors = false;

        public bool HasErrors => _hasErrors;
        public bool IsSidebarDirty => _isDirty;

        private EditContext? editContext;
        #endregion

        #region Jump Re-renders

        private bool _isInitialized;
        private Guid _lastDtoId;

        #endregion

        private Dictionary<string, object> CurrentMetadata => Metadata ?? new();

        #endregion

        #region Methods

        protected override void LoadData()
        {
            OriginalArea.X = DesignerDto.X;
            OriginalArea.Y = DesignerDto.Y;
            OriginalArea.Width = DesignerDto.Width;
            OriginalArea.Height = DesignerDto.Height;

            // Search for objects related to the AreaId to display in the Tabs
            RelatedZones = _designer.GetZonesDtoByIdAreaNoTracking(DesignerDto.Id ?? Guid.Empty).ToList();
            RelatedEquipment = _designer.GetEquipmentDesignerDtoByIdArea(DesignerDto.Id ?? Guid.Empty).ToList();
            RelatedProcesses = _designer.GetProcesByType(DesignerDto.Id ?? Guid.Empty).ToList();

            AlternativeAreaItems = _designer.GetAreasDtoByLayout(DesignerDto.LayoutId ?? Guid.Empty).Where(i => i.Id != DesignerDto.Id).ToList(); //Filter the Area selected to show the list
        }

        private async Task CancelButtonClick()
        {
            // Check if the event originates from DesignerComponent (double click): Avoid displaying the confirmation modal and close the DesignerRightSideComponent.
            if (Metadata.TryGetValue("FromDesignerComponent", out var fromP1Obj) && fromP1Obj is bool fromP1 && fromP1)
            {
                _isDirty = false;
                await OnClickSideBar.InvokeAsync();
                return;
            }

            await OnClickSideBar.InvokeAsync();
        }

        protected override async Task OnInitializedAsync()
        {
            await Task.CompletedTask;
        }

        protected override void OnParametersSet()
        {
            if (DesignerDto == null)
                return;

            if (_isInitialized && DesignerDto.Id == _lastDtoId) // Avoid restarting if it is the same object
                return;
            _isInitialized = true;
            _lastDtoId = DesignerDto.Id ?? Guid.Empty;

            var areaType = DesignerDto?.AreaType ?? AreaType.Dock;
            areaTypeName = L.Loc(GetAreaTypeString(areaType));
            var tooltip = TooltipExtensions.AreaTypes.FirstOrDefault(x => x.Type == areaType);
            _tooltipData = (L.Loc(tooltip.Name), L.Loc(tooltip.Description));

            if (editContext != null)
            {
                editContext.OnFieldChanged -= OnAnyFieldChanged;
                editContext.OnValidationStateChanged -= OnValidationStateChanged;
            }

            editContext = new EditContext(DesignerDto);
            editContext.OnFieldChanged += OnAnyFieldChanged;
            editContext.OnValidationStateChanged += OnValidationStateChanged;

            CaptureBaseline();
            LoadData();
        }

        public override async Task SaveEvent()
        {
            if (editContext == null)
                return;
            bool isValid = editContext.Validate();
            MarkDirty();
            _hasErrors = !isValid;

            if (!isValid || !_isDirty)
            {
                await InvokeAsync(StateHasChanged);
                return;
            }

            try
            {
                MapModelValuesArea(DesignerDto); // Mapping to save updated Area data
                await _designer.UpdateArea(DesignerDto);

                List<ProcessDto> processDtoList = _designer.GetProcessesByIdAreaNoTracking(DesignerDto.Id ?? Guid.Empty).ToList();
                List<Models.DTO.Designer.ZoneDto> stationDtoList = _designer.GetZonesDtoByIdAreaNoTracking(DesignerDto.Id ?? Guid.Empty).Where(s => s.ZoneType == ZoneType.Dock || s.ZoneType == ZoneType.Aisle || s.ZoneType == ZoneType.Buffer || s.ZoneType == ZoneType.Stage).ToList();
                List<ProcessDirectionPropertyDto> processDirectionPropertyDtos = new();
                List<RouteDto> routes = new();
                List<StepDto> stepList = new();

                List<ShelfDto> shelfDtoList = new List<ShelfDto>(); // Start ShelfDto listing
                List<RackDto> rackDtoList = _designer.GetRacksDtoByIdAreaNoTracking(DesignerDto.Id ?? Guid.Empty).ToList();
                shelfDtoList.AddRange(_designer.MapperRackDtoListToShelfDtoList(rackDtoList)); // Adds converted items to ShelfDto format

                List<DriveInDto> driveInDtoList = _designer.GetDriveInsDtoByIdAreaNoTracking(DesignerDto.Id ?? Guid.Empty).ToList();
                shelfDtoList.AddRange(_designer.MapperDriveInDtoListToShelfDtoList(driveInDtoList)); // Adds converted items to ShelfDto format

                List<ChaoticStorageDto> chaoticStorageDtoList = _designer.GetChaoticStoragesDtoByIdAreaNoTracking(DesignerDto.Id ?? Guid.Empty).ToList();
                shelfDtoList.AddRange(_designer.MapperChaoticStorageDtoListToShelfDtoList(chaoticStorageDtoList)); // Adds converted items to ShelfDto format

                List<AutomaticStorageDto> automaticDtoList = _designer.GetAutomaticStoragesDtoByIdAreaNoTracking(DesignerDto.Id ?? Guid.Empty).ToList();
                shelfDtoList.AddRange(_designer.MapperAutomaticStorageDtoListToShelfDtoList(automaticDtoList)); // Adds converted items to ShelfDto format


                foreach (var process in processDtoList)
                {
                    stepList.AddRange(_designer.GetStepsDtoByProcessIdNoTracking(process.Id ?? Guid.Empty).ToList());
                }
                
                if (OriginalArea.X != DesignerDto.X || OriginalArea.Y != DesignerDto.Y ||
                    OriginalArea.Width != DesignerDto.Width || OriginalArea.Height != DesignerDto.Height) 
                {
                    processDirectionPropertyDtos = UpdateProcessDirectionRelatedToProcess(processDtoList);
                    routes = UpdateRouteRelatedToArea((Guid)DesignerDto.Id);
                }

                // Send AreaDto, ProcessDto, StationDto and RackDto data to drawAreaOnCanvas
                var newViewport = _designer.GetVieportsByTypeObject(DesignerDto.Id ?? Guid.NewGuid(), "Area");
                await JS.InvokeVoidAsync("drawObjectsOnCanvas", newViewport);
                await JS.InvokeVoidAsync("callRecalculateRelatedElements");
                await JS.InvokeVoidAsync("SaveOnPostback");

                var meta = Metadata is not null ? new Dictionary<string, object>(Metadata) : new Dictionary<string, object>();
                // Prevents navigation to DesignerRightSideComponent from thinking that the origin is from DesignerComponent.
                if (meta.ContainsKey("FromDesignerComponent"))
                    meta.Remove("FromDesignerComponent");

                await OnNavigateChild.InvokeAsync((ObjectTypes.Area, DesignerDto, meta));

                // Controls behavior based on the origin of the event
                if (IsModalSave)
                    IsModalSave = false; // Cause due to modal Save: SideBar does not close
                else
                    await CancelButtonClick(); // Origin from the Save of the same AreaComponent: Closes the SideBar
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving the record: {ex.Message}");
            }
            CaptureBaseline();
            StateHasChanged();
        }

        private void MapModelValuesArea(AreaDto dto) 
        {
            dto.Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id;
            dto.AreaTypeName = GetAreaTypeString(dto.AreaType ?? AreaType.Dock);
            dto.CanvasObjectType = "Area";
            dto.AlternativeAreaId = dto.AlternativeArea?.Id ?? Guid.Empty;
            dto.StatusObject = "StoredInBase";
        }

        private List<ProcessDirectionPropertyDto> UpdateProcessDirectionRelatedToProcess(List<ProcessDto> processDtos) 
        {
            IEnumerable<ProcessDirectionPropertyDto> directionPropertyDtos = _designer.GetProcessDirectionsByListProcess(processDtos);
            if(directionPropertyDtos.Any())
            {
                directionPropertyDtos.ToList().ForEach(item => item.ViewPort = string.Empty);
                _designer.UpdateListProcessDirectionProperties(directionPropertyDtos);
                
            }
            return directionPropertyDtos.ToList();
        }

        private List<RouteDto> UpdateRouteRelatedToArea(Guid idArae) 
        {
            List<RouteDto> routesDto = _designer.GetRoutesDtoByIdArea(idArae).ToList();
            if (routesDto.Any()) 
            {
                routesDto.ForEach(item => item.ViewPort = string.Empty);
                _designer.UpdateListRoutesDto(routesDto);
            }
            return routesDto;
        }

        private Object CastObjectTypeZones(ObjectTypes objectTypes, Object objectData)
        {
            Guid? zoneId = ((Models.DTO.Designer.ZoneDto)objectData)?.Id ?? Guid.Empty;

            return objectTypes switch
            {
                ObjectTypes.Dock => _designer.GetDocksDtoByIdZoneAsNoTracking(zoneId ?? Guid.Empty) ?? new DockDto(),
                ObjectTypes.Aisle => _designer.GetAisleDtoByIdZoneAsNoTracking(zoneId ?? Guid.Empty) ?? new AisleDto(),
                ObjectTypes.Buffer => _designer.GetBufferDtoByIdZoneAsNoTracking(zoneId ?? Guid.Empty) ?? new BufferDto(),
                ObjectTypes.Rack => _designer.GetRackDtoByIdZoneAsNoTracking(zoneId ?? Guid.Empty) ?? new RackDto(),
                ObjectTypes.DriveIn => _designer.GetDriveInDtoByIdZoneAsNoTracking(zoneId ?? Guid.Empty) ?? new DriveInDto(),
                ObjectTypes.CaoticStorage => _designer.GetChaoticStorageDtoByIdZoneAsNoTracking(zoneId ?? Guid.Empty) ?? new ChaoticStorageDto(),
                ObjectTypes.AutomaticStorage => _designer.GetAutomaticStorageDtoByIdZoneAsNoTracking(zoneId ?? Guid.Empty) ?? new AutomaticStorageDto(),
                ObjectTypes.Stage => _designer.GetStagesDtoByIdZoneAsNoTracking(zoneId ?? Guid.Empty) ?? new StageDto(),
                _ => new()
            };
        }

        private async Task NavigateToChild(ObjectTypes type, object dto)
        {
            var inherited = new Dictionary<string, object>(CurrentMetadata);

            // When navigating between sibling components, the origin is no longer DesignerComponent.
            if (inherited.ContainsKey("FromDesignerComponent"))
                inherited.Remove("FromDesignerComponent");

            inherited["SourceComponent"] = nameof(AreaComponent);
            inherited["RequestedType"] = type.ToString();
            inherited["FromBrother"] = true;

            await OnNavigateChild.InvokeAsync((type, dto, inherited));
        }

        #region Validation Button

        private void OnAnyFieldChanged(object? sender, FieldChangedEventArgs e)
        {
            MarkDirty();
            InvokeAsync(StateHasChanged);
        }

        private void OnValidationStateChanged(object? sender, ValidationStateChangedEventArgs e)
        {
            _hasErrors = editContext!.GetValidationMessages().Any();
            InvokeAsync(StateHasChanged);
        }

        public void CaptureBaseline()
        {
            _baselineFingerprint = BuildFingerprint();
            _isDirty = false;
            _hasErrors = false;
        }

        public void MarkDirty()
        {
            _isDirty = BuildFingerprint() != _baselineFingerprint;
        }

        private string BuildFingerprint()
        {
            return string.Join("|",
                DesignerDto?.Id,
                DesignerDto?.Name,
                DesignerDto?.X,
                DesignerDto?.Y,
                DesignerDto?.Width,
                DesignerDto?.Height,
                DesignerDto?.AreaType,
                DesignerDto?.AlternativeAreaId,
                DesignerDto?.LayoutId
            );
        }

        public void ResetBaseline()
        {
            CaptureBaseline();
            StateHasChanged();
        }

        public void RestoreOriginalState()
        {
            // We request the Area from the backend
            if (DesignerDto?.Id is null)
                return;
            var refreshed = _designer.GetAreasById(DesignerDto.Id.Value);
            if (refreshed != null)
            {
                DesignerDto = refreshed;
                LoadData();
                CaptureBaseline();
            }

            StateHasChanged();
        }

        #endregion

        #endregion
    }
}
