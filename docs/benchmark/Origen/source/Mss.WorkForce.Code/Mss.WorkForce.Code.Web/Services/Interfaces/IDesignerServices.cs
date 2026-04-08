using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.DTO.Designer;
using Mss.WorkForce.Code.Models.DTO.Designer.LocationZones;
using Mss.WorkForce.Code.Models.DTO.Designer.Process;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.Resources;
using static Mss.WorkForce.Code.Models.DTO.Enums.Designer.ObjectsTypes;
using Buffer = Mss.WorkForce.Code.Models.Models.Buffer;
using InboundFlowGraphDto = Mss.WorkForce.Code.Models.DTO.Designer.InboundFlowGraphDto;
using OutboundFlowGraphDto = Mss.WorkForce.Code.Models.DTO.Designer.OutboundFlowGraphDto;
using Route = Mss.WorkForce.Code.Models.Models.Route;

namespace Mss.WorkForce.Code.Web.Services
{
    public interface IDesignerServices : ILayout, IObjects, IArea, IViewports, IRoute, IEquipment, IAisle,
        IDock, IBuffer, IZone, IProcess, IRack, IProcessDirectionProperty, ICustomProcess, IInbounds, IStage,
        ILoading, IPicking, IPutaway, IShipping, IReception, IFlowGraph, IInboundFlowGraph, IOutboundFlowGraph, IDockSelectionStrategy, IStep, IReplenishment, IPacking,
        CheckConfiguration, IShelf, IDriveIn, IAutomaticStorage, IChaotic, ICustomFlowGraph, ICalculateNameToValidations, IAvailableDocksPerStage, IDependencies
    {

    }

    #region Interface Layout
    public interface ILayout
    {
        Task DeleteLayout(LayoutDto itemDto);
        Task UpdateLayout(LayoutDto itemDto);
        Task DeleteListLayout(IEnumerable<LayoutDto> itemListDto);
        Task CloneLayout(LayoutDto itemDto);
        Task UpdateListLayout(List<LayoutDto> itemListDto);
        Task AddLayout(LayoutDto itemDto);
        IEnumerable<LayoutDto> GetLayoutsDto(Guid guid);
        LayoutDto? GetLayoutDto(Guid Id);
        Layout? GetLayout(Guid Id);
    }
    #endregion

    #region Objects
    public interface IObjects
    {
        Task DeleteListObjects(IEnumerable<ObjectsDto> itemListDto);
        Task UpdateListObjects(IEnumerable<ObjectsDto> itemListDto);
        Task AddListObjects(IEnumerable<ObjectsDto> itemDto);
        IEnumerable<ObjectsDto> GetObjects(Guid Id);

    }
    #endregion

    #region Area
    public interface IArea
    {
        Task AddArea(AreaDto itemListDto);
        Task UpdateListAreas(IEnumerable<AreaDto> itemListDto);
        Task AddListAreas(IEnumerable<AreaDto> itemDtoList);
        Task UpdateArea(AreaDto itemDto);
        Task DeleteListAreas(IEnumerable<AreaDto> itemListDto);
        IEnumerable<AreaDto> GetAreasDtoByLayout(Guid layoutId);
        string GetVieportsByTypeObject(Guid idObject, string TypeObject);
        AreaDto? GetAreasById(Guid itemId);
    }
    #endregion

    #region Viewports
    public interface IViewports
    {
        string GetViewportsByLayoutId(Guid layouId, Guid warehouseId);
    }
    #endregion

    #region Route
    public interface IRoute
    {
        Task AddRouteDto(RouteDto itemListDto);
        Task UpdateRouteDto(RouteDto itemListDto);
        Task UpdateListRoutesDto(IEnumerable<RouteDto> itemListDto);
        Task DeleteListRoutes(IEnumerable<Route> itemListDto);
        IEnumerable<Route?> GetRoutesByIdArea(Guid areaId);
        List<RouteDto?> GetRoutesDtoByIdArea(Guid areaId);
        RouteDto GetRoutesDtoById(Guid itemId);
        IEnumerable<Route> GetRoutesListByRouteDtoList(HashSet<RouteDto> itemsList);
    }
    #endregion

    #region Equipment
    public interface IEquipment
    {
        Task<Guid> AddEquipmentDesignerDto(EquipmentDesignerModalDto itemDto, bool isCreateNewArea);

        Task AddEquipmentDesignerDtoCopy(EquipmentDesignerDto itemDto);
        Task UpdateEquipmentDesignerDto(EquipmentDesignerDto itemDto);
        Task DeleteListEquipmentGroup(IEnumerable<EquipmentGroup> itemList);
        IEnumerable<EquipmentGroup?> GetEquipmentGroupByIdArea(Guid areaId);
        IEnumerable<TypeEquipment> GetTypeEquipmentByWarehouseIdNoTracking(Guid layoutId);
        EquipmentDesignerDto GetEquipmentDesignerDtoById(Guid itemId);
        IEnumerable<EquipmentGroup> GetEquipmentGroupsListByEquipmentDesignerDtoList(IEnumerable<EquipmentDesignerDto> equipmentDesignerDtos);
        IEnumerable<EquipmentDesignerDto?> GetEquipmentDesignerDtoByIdArea(Guid areaId);
    }
    #endregion

    #region Aisle
    public interface IAisle
    {
        Task<Guid> AddAisle(List<AisleDto> aisleDtoList, bool isCreateNewArea);
        Task UpdateAisle(AisleDto aisleDto);
        Task DeleteAisle(Aisle aisle);
        Task DeleteListAisle(IEnumerable<Aisle> aisleList);
        IEnumerable<AisleDto> GetAislesByZoneByIdAreaNoTracking(Guid areaId);
        AisleDto? GetAisleDtoByIdZoneAsNoTracking(Guid itemId);
        Aisle? GetAisleByIdNoTracking(Guid itemId);
        Aisle? GetAisleByIdZoneAsNoTracking(Guid itemId);
        IEnumerable<Aisle?> GetAislesByIdArea(Guid areaId);
    }
    #endregion

    #region Dock
    public interface IDock
    {
        Task AddDockCopy(DockDto dockDtoList);
        Task<Guid> AddDock(List<DockDto> dockDtoList, bool isCreateNewArea);
        Task UpdateDock(DockDto dockDto);
        Task DeleteDock(Dock dockDto);
        Task DeleteListDock(IEnumerable<Dock> dockList);
        DockDto GetDocksDtoByIdZoneAsNoTracking(Guid zoneId);
        IEnumerable<DockDto> GetDocksDtoByIdAreaNoTracking(Guid areaId);
        IEnumerable<DockZoneSettingsDto> GetDockDtosByLayoutId(Guid layoutId);
        DockDto? GetDockDtoByIdNoTracking(Guid itemId);
        Dock? GetDockByIdNoTracking(Guid itemId);
        Dock? GetDockByIdZoneAsNoTracking(Guid itemId);
        IEnumerable<Dock?> GetDocksByIdAreaNoTracking(Guid areaId);
        (int nextInbound, int nextOutbound) GetNextDockRangesInboundOutbound(Guid areaId);
    }
    #endregion

    #region Stage
    public interface IStage 
    {
        Task<Guid> AddStage(List<StageDto> stageDtoList, bool isCreateNewArea);
        Task AddStageCopy(StageDto stageDto);
        Task UpdateStage(StageDto stageDto);
        Task DeleteStage(Stage stage);
        Task DeleteListStage(IEnumerable<Stage> stageList);
        IEnumerable<StageDto> GetStagesDtoByIdAreaNoTracking(Guid areaId);
        StageDto GetStagesDtoByIdZoneAsNoTracking(Guid zoneId);
        Stage? GetStagesByIdZoneAsNoTracking(Guid itemId);
        StageDto? GetStageDtoByIdNoTracking(Guid itemId);
        Stage? GetStageByIdNoTracking(Guid itemId);
        IEnumerable<Stage?> GetStagesByIdAreaNoTracking(Guid areaId);
    }
    #endregion

    #region AvailableDocksPerStage
    public interface IAvailableDocksPerStage 
    {
        Task DeleteListAvailableDocksPerStages(IEnumerable<AvailableDocksPerStageDto> availableDocksList);
        Task AddAvailableDocksPerStages(IEnumerable<AvailableDocksPerStageDto> availableDtoList);
        IEnumerable<AvailableDocksPerStageDto> GetAvailableDocksPerStageDtoByStageId(Guid stageId);

        IEnumerable<AvailableDocksPerStageDto> GetAvailableDocksPerStageDtoByZoneId(Guid zoneId);

        
    }

    #endregion

    #region Buffer
    public interface IBuffer
    {
        Task<Guid> AddBuffer(List<BufferDto> bufferDtoList, bool isCreateNewArea);
        Task AddBufferCopy(BufferDto bufferDtoList);
        Task UpdateBuffer(BufferDto bufferDto);
        Task DeleteBuffer(Buffer buffer);
        Task DeleteListBuffer(IEnumerable<Buffer> bufferList);
        IEnumerable<BufferDto> GetBuffersDtoByIdAreaNoTracking(Guid areaId);
        BufferDto? GetBufferDtoByIdNoTracking(Guid itemId);
        Buffer? GetBufferByIdNoTracking(Guid itemId);
        Buffer? GetBufferByIdZoneAsNoTracking(Guid itemId);
        BufferDto? GetBufferDtoByIdZoneAsNoTracking(Guid itemId);
        
        IEnumerable<Buffer?> GetBuffersByIdAreaNoTracking(Guid areaId);
        BufferDto GetBuffersDtoByIdZoneAsNoTracking(Guid zoneId);
    }
    #endregion

    #region Zone
    public interface IZone
    {
        Task AddZoneDto(Models.DTO.Designer.ZoneDto itemDto);
        Task UpdateZoneDto(Models.DTO.Designer.ZoneDto itemDto);
        Task DeleteListZones(IEnumerable<Zone> itemList);
        Task UpdateListZones(IEnumerable<Models.DTO.Designer.ZoneDto> itemDtoList);
        IEnumerable<Zone> GetZonesByAreaNoTracking(Guid areaId);
        IEnumerable<Models.DTO.Designer.ZoneDto> GetZonesDtoByIdAreaNoTracking(Guid areaId);
        Models.DTO.Designer.ZoneDto? GetZoneDtoById(Guid zoneDto);
        Zone? GetZoneByIdNoTracking(Guid zoneDto);
    }
    #endregion

    #region Process
    public interface IProcess
    {
        Task AddProcess(ProcessDto itemDto);
        Task AddProcessList(List<ProcessDto> itemDtoList);
        Task UpdateProcess(ProcessDto itemDto);
        Task UpdateListProcesses(IEnumerable<ProcessDto> itemListDto);
        Task DeleteListProcesses(IEnumerable<Process> itemList);
        IEnumerable<ProcessDto> GetProcessesByIdAreaNoTracking(Guid itemArea);
        IEnumerable<ProcessDto> GetProcessesByLayoutIdNoTracking(Guid layoutId);
        Process? GetProcessByIdNoTracking(Guid itemId);
        ProcessDto? GetProcessDtoByIdNoTracking(Guid itemId);
        IEnumerable<Process?> GetProcessByIdAreaNoTracking(Guid areaId);
        IEnumerable<Process>? GetProcessListByProcessDtoList(IEnumerable<ProcessDto> processDtos);
        List<ProcessDto> GetProcesByType(Guid areaId);

        List<ParentFlowDto> GetParentFlows(Guid layoutId);
    }
    #endregion

    #region Shelf
    public interface IShelf
    {
        ShelfDto MapperRackDtoToShelfDtoService(RackDto rackDto);

        List<ShelfDto> MapperRackDtoListToShelfDtoList(List<RackDto> rackDtos);

        List<ShelfDto> MapperDriveInDtoListToShelfDtoList(List<DriveInDto> driveInDtos);

        List<ShelfDto> MapperAutomaticStorageDtoListToShelfDtoList(List<AutomaticStorageDto> driveInDtos);

        List<ShelfDto> MapperChaoticStorageDtoListToShelfDtoList(List<ChaoticStorageDto> chaoticStorageDtos);
    }


    public interface IRack
    {
        Task<Guid> AddRackList(List<RackDto> rackDtoList, bool isCreateNewArea);
        Task AddRack(RackDto rackDto);
        Task UpdateRack(RackDto RackDto);
        Task DeleteRack(Rack Rack);
        Task DeleteListRack(IEnumerable<Rack> RackList);
        IEnumerable<RackDto> GetRacksDtoByIdAreaNoTracking(Guid areaId);
        RackDto? GetRackDtoByIdNoTracking(Guid itemId);
        Rack? GetRackByIdNoTracking(Guid itemId);
        Rack? GetRackByIdZoneAsNoTracking(Guid itemId);
        RackDto? GetRackDtoByIdZoneAsNoTracking(Guid itemId);
        Rack? GetRackByIdAsNoTracking(Guid rackId);
        IEnumerable<Rack?> GetRacksByIdAreaNoTracking(Guid areaId);
        IEnumerable<Rack>? GetRacksListByRackDtoList(IEnumerable<RackDto> rackDtos);
    }

    #endregion

    #region DriveIn
    public interface IDriveIn
    {
        Task<Guid> AddDriveInList(List<DriveInDto> driveInDtoList, bool isCreateNewArea);
        Task AddDriveIn(DriveInDto driveInDto);
        Task UpdateDriveIn(DriveInDto driveInDto);
        Task DeleteDriveIn(DriveIn driveIn);
        Task DeleteListDriveIn(IEnumerable<DriveIn> driveInList);
        IEnumerable<DriveInDto> GetDriveInsDtoByIdAreaNoTracking(Guid areaId);
        DriveInDto? GetDriveInDtoByIdNoTracking(Guid itemId);
        DriveInDto GetDriveInDtoByIdZoneAsNoTracking(Guid zoneId);
        IEnumerable<DriveIn?> GetDriveInsByIdAreaNoTracking(Guid areaId);
        DriveIn? GetDriveInByIdAsNoTracking(Guid driveInId);
    }
    #endregion

    #region Chaotic
    public interface IChaotic
    {
        Task<Guid> AddChaoticList(List<ChaoticStorageDto> chaoticStorageDtoList, bool isCreateNewArea);
        Task AddChaoticStorage(ChaoticStorageDto chaoticStorageDtoList);
        Task UpdateChaotic(ChaoticStorageDto chaoticStorageDto);
        Task DeleteChaotic(ChaoticStorage chaotic);
        Task DeleteListChaoticStorage(IEnumerable<ChaoticStorage> chaoticStorageList);
        IEnumerable<ChaoticStorageDto> GetChaoticStoragesDtoByIdAreaNoTracking(Guid areaId);
        ChaoticStorageDto? GetChaoticStorageDtoByIdNoTracking(Guid itemId);
        ChaoticStorageDto GetChaoticStorageDtoByIdZoneAsNoTracking(Guid zoneId);
        IEnumerable<ChaoticStorage?> GetChaoticStoragesByIdAreaNoTracking(Guid areaId);
        ChaoticStorage? GetChaoticByIdAsNoTracking(Guid chaoticId);
    }
    #endregion

    #region AutomaticStorage
    public interface IAutomaticStorage
    {
        Task<Guid> AddAutomaticStorageList(List<AutomaticStorageDto> automaticStorageDtoList, bool isCreateNewArea);
        Task AddAutomaticStorage(AutomaticStorageDto automaticStorageDto);
        Task UpdateAutomaticStorage(AutomaticStorageDto automaticStorageDto);
        Task DeleteAutomaticStorage(AutomaticStorage automaticStorage);
        Task DeleteListAutomaticStorage(IEnumerable<AutomaticStorage> automaticStorageList);
        IEnumerable<AutomaticStorageDto> GetAutomaticStoragesDtoByIdAreaNoTracking(Guid areaId);
        AutomaticStorageDto? GetAutomaticStorageDtoByIdNoTracking(Guid itemId);
        AutomaticStorageDto GetAutomaticStorageDtoByIdZoneAsNoTracking(Guid zoneId);
        IEnumerable<AutomaticStorage?> GetAutomaticStoragesByIdAreaNoTracking(Guid areaId);
        AutomaticStorage? GetAutomaticStorageByIdAsNoTracking(Guid automaticStorageId);
    }
    #endregion

    #region ProcessDirectionProperty
    public interface IProcessDirectionProperty
    {
        Task AddProcessDirectionProperty(ProcessDirectionPropertyDto dto);
        Task UpdateProcessDirectionProperty(ProcessDirectionPropertyDto itemDto);
        Task UpdateListProcessDirectionProperties(IEnumerable<ProcessDirectionPropertyDto> itemListDto);
        Task DeleteListProcessDirectionProperties(IEnumerable<ProcessDirectionProperty> itemListDto);
        ProcessDirectionProperty? GetProcessDirectionPropertyByIdNoTracking(Guid itemId);
        ProcessDirectionPropertyDto? GetProcessDirectionPropertyDtoByIdNoTracking(Guid itemId);
        IEnumerable<ProcessDirectionPropertyDto>? GetProcessDirectionPropertyDtoByIdLayout(Guid itemId);
        IEnumerable<ProcessDirectionProperty>? GetProcessDirectionPropertyListByProcessDirectionPropertyDtoList(IEnumerable<ProcessDirectionPropertyDto> processDtos);
        IEnumerable<ProcessDirectionProperty?> GetProcessDirectionsByIdProcess(Guid processId);
        IEnumerable<ProcessDirectionPropertyDto> GetProcessDirectionsByListProcess(List<ProcessDto> processDtos);

        object GetProcessAndDirectionsByFlow(Guid layoutId, List<Guid> flows);
    }
    #endregion|

    #region Custom Process Implementation

    public interface ICustomProcess
    {
        Task AddCustomProcess(CustomProcessDto itemDto);
        Task UpdateCustomProcess(CustomProcessDto itemDto);
        CustomProcessDto? GetCustomProcessById(Guid Id);
        CustomProcessDto GetCustomProcessDtoByProcessId(Guid processId);
        IEnumerable<CustomProcess?> GetCustomProcessDtoByProcess(Guid processId);
        Task DeleteCustomProcess(CustomProcessDto itemDto);
    }

    #endregion

    #region Inbounds Process Implementation

    public interface IInbounds
    {
        Task AddInbounds(InboundDto itemDto);
        InboundDto? GetInboundsById(Guid Id);
        InboundDto GetInboundDtoByProcessId(Guid processId);
        IEnumerable<Inbound?> GetInboundListByProcessId(Guid processId);
        Task UpdateInbound(InboundDto dto);
        Task DeleteInbound(InboundDto dto);
    }

    #endregion

    #region Loading Process Implementation
    public interface ILoading
    {
        Task AddLoading(LoadingDto itemDto);
        Task UpdateLoading(LoadingDto itemDto);
        Task DeleteLoading(LoadingDto itemDto);
        LoadingDto? GetLoadingDtoById(Guid id);
        LoadingDto? GetLoadingDtoByProcessId(Guid processId);
        IEnumerable<Loading?> GetLoadingDtoListByProcessId(Guid processId);
    }
    #endregion

    #region Picking Process Implemantation
    public interface IPicking
    {
        Task AddPicking(PickingDto itemDto);
        Task UpdatePicking(PickingDto itemDto);
        Task DeletePicking(PickingDto itemDto);
        PickingDto? GetPickingDtoById(Guid id);
        PickingDto? GetPickingDtoByProcessId(Guid processId);
        IEnumerable<Picking?> GetPickingListByProcessId(Guid processId);
    }
    #endregion

    #region Putaway Process Implementation
    public interface IPutaway
    {
        Task AddPutaway(PutawayDto itemDto);
        Task UpdatePutaway(PutawayDto itemDto);
        Task DeletePutaway(PutawayDto itemDto);
        PutawayDto? GetPutawayDtoById(Guid id);
        PutawayDto? GetPutawayDtoByProcessId(Guid processId);
        IEnumerable<Putaway?> GetPutawayListByProcessId(Guid processId);
    }
    #endregion

    #region Shipping Process Implementation
    public interface IShipping
    {
        Task AddShipping(ShippingDto itemDto);
        Task UpdateShipping(ShippingDto itemDto);
        Task DeleteShipping(ShippingDto itemDto);
        ShippingDto? GetShippingDtoById(Guid id);
        ShippingDto? GetShippingDtoByProcessId(Guid processId);
        IEnumerable<Shipping?> GetShippingListByProcessId(Guid processId);
    }
    #endregion

    #region Reception Process Implementation
    public interface IReception
    {
        Task AddReception(ReceptionDto itemDto);
        Task UpdateReception(ReceptionDto itemDto);
        Task DeleteReception(ReceptionDto itemDto);
        ReceptionDto? GetReceptionDtoById(Guid id);
        ReceptionDto? GetReceptionDtoByProcessId(Guid processId);
        IEnumerable<Reception?> GetReceptionByProcessId(Guid processId);
    }

    #endregion

    #region Replenishments Process Implementation

    public interface IReplenishment
    {
        Task AddReplenishment(ReplenishmentDto replenishmentDto);
        Task UpdateReplenishment(ReplenishmentDto replenishmentDto);
        Task DeleteReplenishment(ReplenishmentDto replenishmentDto);
        ReplenishmentDto? GetReplenishmentDtoById(Guid id);
        ReplenishmentDto? GetReplenishmentDtoByProcessId(Guid processid);
        IEnumerable<Replenishment?> GetReplenishmentByProcessId(Guid processId);
    }

    #endregion

    #region Packing Process Implementation

    public interface IPacking
    {
        Task AddPacking(PackingDto PackingDto);
        Task UpdatePacking(PackingDto PackingDto);
        Task DeletePacking(PackingDto PackingDto);
        PackingDto? GetPackingDtoById(Guid id);
        PackingDto? GetPackingDtoByProcessId(Guid processid);
        IEnumerable<Packing?> GetPackingByProcessId(Guid processId);
    }

    #endregion

    #region Dependencies Implementation

    public interface IDependencies
    {
        bool HasProcessDependenciesToFlow(Guid flowId);
    }

    #endregion

    #region FlowGraphs Implementation

    public interface IFlowGraph
    {
        Task DeleteFlowGraphAsync(Guid flowId, FlowType type);
        (Guid? FlowId, string FlowName) GetFlowInfoFromType(Guid childId, FlowType type);
        FlowDto? GetFlowDtoById(Guid flowId);
    }

    #endregion

    #region InboundFlowGraphs Implementation
    public interface IInboundFlowGraph
    {
        Task AddInboundFlowGraph(InboundFlowGraphDto dto);
        Task UpdateInboundFlowGraph(InboundFlowGraphDto dto);
        Task DeleteInboundFlowGraph(InboundFlowGraphDto dto);
        Task DeleteInboundFlowGraphs(IEnumerable<InboundFlowGraphDto> dtos);
        InboundFlowGraphDto? GetInboundFlowGraphById(Guid id);

        InboundFlowGraphDto? GetInboundFlowGraphByWarehouseId(Guid warehouseId);


        IEnumerable<InboundFlowGraphDto> GetInboundFlowGraphsByWarehouseId(Guid warehouseId);
    }

    #endregion

    #region OutboundFlowGraphs Implementation
    public interface IOutboundFlowGraph
    {
        Task AddOutboundFlowGraph(OutboundFlowGraphDto dto);
        Task UpdateOutboundFlowGraph(OutboundFlowGraphDto dto);
        Task DeleteOutboundFlowGraph(OutboundFlowGraphDto dto);
        Task DeleteOutboundFlowGraphs(IEnumerable<OutboundFlowGraphDto> dtos);
        OutboundFlowGraphDto? GetOutboundFlowGraphById(Guid id);
        OutboundFlowGraphDto? GetOutboundFlowGraphByWarehouseId(Guid warehouseId);
        IEnumerable<OutboundFlowGraphDto> GetOutboundFlowGraphsByWarehouseId(Guid warehouseId);
    }

    #endregion

    #region CustomFlowGraphs Implementation
    public interface ICustomFlowGraph
    {
        CustomFlowGraphDto? GetCustomFlowGraphById(Guid id);
        CustomFlowGraphDto? GetCustomFlowGraphByWarehouseId(Guid warehouseId);
        Task AddCustomFlowGraph(CustomFlowGraphDto dto);
        Task UpdateCustomFlow(CustomFlowGraphDto dto);
        Task DeleteCustomFlowGraph(CustomFlowGraphDto dto);
    }

    #endregion

    #region Implementation
    public interface IDockSelectionStrategy
    {
        Task AddDockSelectionStrategy(DockSelectionStrategyDto dto);
        Task UpdateDockSelectionStrategy(DockSelectionStrategyDto dto);
        Task DeleteDockSelectionStrategy(DockSelectionStrategyDto dto);
        DockSelectionStrategyDto? GetDockSelectionStrategyById(Guid id);
        IEnumerable<DockSelectionStrategyDto> GetDockSelectionStrategiesByWarehouseId(Guid warehouseId);
    }

    #endregion

    #region Steps Implementation
    public interface IStep
    {
        Task AddStepDtoList(IEnumerable<StepDto> stepDtoList);
        Task UpdateStepDto(IEnumerable<StepDto> stepDtoList);
        Task DeleteStepsDtoList(IEnumerable<StepDto> stepList);
        StepDto GetStepsById(Guid stepId);
        StepDto? GetStepsDtoByProcessId(Guid processId);
        IEnumerable<StepDto> GetStepsDtoByProcessIdNoTracking(Guid processId);
    }
    #endregion


    #region Configuration Check

    public interface CheckConfiguration
    {
        Task<Dictionary<string, List<ResourceMessage>>> CheckConfiguration(Guid warehouseId);
    }

    #endregion

    #region Calculate Name To Validations

    public interface ICalculateNameToValidations
    {
        (bool itsUnique, IEnumerable<string> suggestions) CalculateNameToValidations(string nameToCompare, Guid areaId, Guid? currentId = null);
    }

    #endregion

    #region Navigation between DesignerRightSideComponent and Components

    // Used to determine whether the component has pending changes before: Navigating to a sibling, Back button, and Cancel button.
    public interface ITrackChanges
    {
        bool IsSidebarDirty { get; }
        bool HasErrors { get; }

        void CaptureBaseline(); // Create Base object
        void MarkDirty();       // FieldChanged
    }

    // Indicates that pending changes must be saved before navigating.
    public interface IChildSaver
    {
        Task SaveEvent();
    }

    // Distinguishes between normal saving and saving triggered from the modal
    public interface IModalAware
    {
        bool IsModalSave { get; set; }
    }

    // Used for the No case of the DesignerRightSideComponent modal to discard changes before navigating
    public interface IRestorable
    {
        void RestoreOriginalState();
    }

    #endregion

    #region Status Internal / Implementation
    // Updates the cleanup status of the object
    public interface IResetBaseline
    {
        void ResetBaseline();
    }

    #endregion
}