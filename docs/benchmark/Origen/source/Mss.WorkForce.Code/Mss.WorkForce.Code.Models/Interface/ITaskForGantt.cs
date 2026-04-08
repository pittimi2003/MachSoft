using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Models.ModelGantt.DataGanttPlanning;

namespace Mss.WorkForce.Code.Models.Interface
{
    /// <summary>
    /// Clase que contiene las propiedades del gantt que son necesarias para pintar/representar las tareas en el diagrama
    /// </summary>
    interface ITaskForGantt
    {
        #region Properties

        string? ActivityTitle { get; set; }
        /// <summary>
        /// Indica la actividad (Out, In, Reposition)
        /// </summary>
        bool IsOutbound { get; set; }
        IEnumerable<AlertMessageDto> Alerts { get; set; }
        string BackgroundColorRow { get; set; }
        string? Carrier { get; set; }
        DateTimeOffset CommintedDate { get; set; }
        string CommittedHour { get; }
        string? Customer { get; set; }
        string? DelayedOrder { get; set; }
        string end { get; }
        DateTimeOffset EndDate { get; set; }
        string? EquipmentGroup { get; set; }
        string? EquipmentType { get; set; }
        /// <summary>
        /// Indica si se quiere calcular la barra de progreso en base a los hijos
        /// </summary>
        bool FillProgress { get; set; }
        Guid id { get; set; }
        Guid? InputOrderId { get; set; }
        /// <summary>
        /// Indica si la tarea corresponde al último nivel de la jerarquía (sin hijos).
        /// </summary>
        bool IsChildTask { get; set; }
        bool isEstimated { get; set; }
        bool isOffOverlay { get; set; }
        bool isOnTime { get; set; }
        Guid? parentId { get; set; }
        string? Priority { get; set; }
        object ProcessEstimated { get; set; }
        int progress { get; set; }
        string? Resource { get; set; }
        List<SegmentDto>? segments { get; set; }
        string start { get; }
        DateTimeOffset StartDate { get; set; }
        string? Status { get; set; }
        string? Supplier { get; set; }
        string? title { get; set; }
        GanttTooltipType TooltipType { get; set; }
        int TotalEquipments { get; set; }
        int TotalOperators { get; set; }
        string? WorkTime { get; set; }

        #endregion
    }
}

