using System.Text.Json.Serialization;

namespace Mss.WorkForce.Code.Models.DTO.Enums
{
    public enum TaskType
    {
        HighTask = 1,
        ParentTask = 2,
        SubTask = 3     //child
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GanttTooltipType
    {
        None,

        // For gantt planning
        PlanningGeneral,
        PlanningFlow,
        PlanningOrder,
        PlanningActivity,
        PlanningPriority,
        PlanningEstimation,
        PlanningTrailer,
        PlanningEstimationIn,
        PlanningEstimationInOrder,
        PlanningEstimationOut,
        PlanningEstimationOutOrder,
        PlanningWarehouse,
        PlanningWarehouseOrder,


        // For Equipments
        LaborEquipmentGeneral,
        LaborEquipmentOrder,
        LaborEquipmentActivity,

        // For workers
        LaborWorkerGeneral,
        LaborWorkerOrder,
        LaborWorkerActivity,

        // ForYard
        YardGeneral,
        YardOrder,

    }

}
