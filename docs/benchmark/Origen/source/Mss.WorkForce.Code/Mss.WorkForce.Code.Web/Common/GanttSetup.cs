using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.SignalR;

namespace Mss.WorkForce.Code.Web.Common
{
    public class GanttSetup
    {
        public Guid WarehouseId { get; set; } = Guid.Empty;

        public GanttView typeGantt { get; set; }

        public object value { get; set; } = new();

        /// <summary>
        /// Recarga las columnas por vista
        /// </summary>

        public bool ReloadColumnsView { get; set; } = true;

        public List<ColumnChoose> ColumnChooseProperties { get; set; } = new List<ColumnChoose>();

        public bool ReloadSimulation { get; set; } = false;

        public bool ReloadData { get; set; } = false;

        public bool EditGantt { get; set; } = true;

        public bool ShowToltip { get; set; } = true;

        /// <summary>
        /// Mantiene las columnas de la configuración inicial
        /// </summary>

        public bool ChooserColumnDefault { get; set; } = false;
        public string NameTimeZone { get; set; }
        public double Offset { get; set; }
        public EnumViewPlanning SelectedView { get; set; }
        public bool ApplyFilterStatus { get; set; } = false;

        public bool ApplyOrderByStart { get; set; } = false;
    }
}
