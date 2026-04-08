using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.DTO.Preview;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.ModelUpdate;
using Mss.WorkForce.Code.Models.Resources;
using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Web
{
    public interface ISimulateService
    {
        /// <summary>
        /// Ejecuta el proceso de simulación de planeación de tareas en el almacén especificado.
        /// </summary>
        /// <param name="warehouseId">Identificador único del almacén donde se ejecutará la simulación.</param>
        /// <returns>GUID de la simulación de planeación creada.</returns>
        Task<Guid> ExecutePlanningSimulation(Guid warehouseId, SimulationCase simulationCase=SimulationCase.Planning);

        /// <summary>
        /// Obtiene la lista de tareas de labor asociadas a un identificador de planificación específico.
        /// </summary>
        /// <param name="planningId">Identificador único de la planificación</param>
        /// <returns>Una colección enumerable de tareas para el Gantt de labor management </returns>
        GanttDataConvertDto<LaborTaskGantt> GetLaborTasksByPlanningId(Guid planningId, UserFormatOptions userFormat);

        /// <summary>
        /// Obtiene la lista de tareas de labor equipments asociadas a un identificador de planificación específico.
        /// </summary>
        /// <param name="planningId">Identificador único de la planificación</param>
        /// <returns>Una colección enumerable de tareas para el Gantt de labor equipments </returns>
        GanttDataConvertDto<LaborEquipmentGantt> GetLaborEquipmentsByPlanningId(Guid planningId, UserFormatOptions userFormat);

        /// <summary>
        /// Obtiene la lista de tareas de yard asociadas a una planificación en específico.
        /// </summary>
        /// <param name="planningId">Identificador único de la planificación</param>
        /// <returns>Un objeto que contiene las tareas y configuraciones requeridas para construir el Gantt de yard</returns>
        GanttDataConvertDto<YardTaskGantt> GetYardsByPlanningId(Guid planningId, UserFormatOptions userFormat);

        Task<GanttDataConvertDto<TaskData>> GetSimulateData(Guid WarehouseId, UserFormatOptions userFormat, EnumViewPlanning View);

        Task<PreviewData> GetSimulateDataPreview(Guid WarehouseId, TemporalidadModel Temp, List<VehicleLoadDto> workLoad, List<ShiftRolDto> Shifts, UserFormatOptions userFormat);

        Task<List<SimulationLogCheck>> GetSimulateDataLog(Guid WarehouseId, TemporalidadModel Temp, List<LoadDto> loadInput, List<LoadDto> loadOut, List<ShiftRolDto> Shifts);
        Task<List<WorkerWhatIf>> GetWhatIfLog(Guid WarehouseId, TemporalidadModel Temp, List<LoadDto> loadInput, List<LoadDto> loadOut, List<ShiftRolDto> Shifts);
        Task UpdateModel(string model);

        Task SaveChangesInDataBase(OperationDB model);

       
        Task<Guid> CloneLayout(Guid layoutId);

        Task SaveDataLockTaskPlanning(List<TaskData> selectTask, Guid WarehouseId, double offset);
        Task CancelTaskPlanning(List<TaskData> selectTask, Guid WarehouseId);

        Task SaveDataChangePriorityPlanning(string newPriority, List<TaskData> ListSelectTask, Guid WarehouseId);

        Task<Dictionary<string, List<ResourceMessage>>> CheckConfiguration(Guid warehouseId);

        int GetWidgets(eGroupWidgets eGroupWidgets);
        Task SaveDataPreview(TemporalidadModel temporalidadSeleccionada, List<VehicleLoadDto> load, List<ShiftRolDto> shifts);
    }
}
