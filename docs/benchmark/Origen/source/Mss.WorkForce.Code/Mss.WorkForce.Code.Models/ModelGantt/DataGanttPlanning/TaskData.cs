using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Models.ModelGantt
{
    /// <summary>
    /// Clase que contiene las propiedades del gantt que se visualizan para las columnas
    /// </summary>
    public class TaskData : GanttTaskBase
    {

		private DateTimeOffset _slaDate;

		public TaskData() { }

        public TaskData(UserFormatOptions userFormat) : base(userFormat) { }

		[DisplayAttributes(1, "", true, ComponentType.CheckBox, "", GroupTypes.None, false, true, true, levelFilterType: LevelFilterType.ThirdLevel)]
		public override bool Multiselect { get; set; }

		[DisplayAttributes(3, "Flow", true, ComponentType.None, "", GroupTypes.None, false, true, true, traslateCaption: true, levelFilterType: LevelFilterType.SecondLevel)]
		public override string WorkFlow { get; set; }

		[DisplayAttributes(2, "", true, ComponentType.None, "", GroupTypes.None, false, true, true, traslateCaption: true, levelFilterType: LevelFilterType.FirstLevel)]
        public string? IDCode { get; set; }

        [DisplayAttributes(4, "WMS code", true, ComponentType.None, "", GroupTypes.None, false, true, true, levelFilterType: LevelFilterType.ThirdLevel)]
        public string? title { get; set; }

        [DisplayAttributes(7, "Priority", true, ComponentType.None, "", GroupTypes.None, false, true, true, traslateCaption: true, levelFilterType: LevelFilterType.ThirdLevel)]
        public override string? Priority { get; set; }

        [DisplayAttributes(8, "Status", true, ComponentType.None, "", GroupTypes.None, false, true, true, traslateCaption: true, levelFilterType: LevelFilterType.ThirdLevel)]
        public string? Status { get; set; }

        [DisplayAttributes(9, "Activity", true, ComponentType.None, "", GroupTypes.None, false, true, true, levelFilterType: LevelFilterType.AllLevels)]
        public string? ActivityTitle { get; set; }// Reception, Putaway        

        [DisplayAttributes(12, "Customer", true, ComponentType.None, "", GroupTypes.None, false, false, false, levelFilterType: LevelFilterType.ThirdLevel)]
        public string? Customer { get; set; }

        [DisplayAttributes(13, "Dock", true, ComponentType.None, "", GroupTypes.None, false, false, false, levelFilterType: LevelFilterType.AllLevels)]
        public string? DockName { get; set; }

        [DisplayAttributes(14, "Trailer", true, ComponentType.None, "", GroupTypes.None, false, false, false, levelFilterType: LevelFilterType.ThirdLevel)]
        public string? Trailer { get; set; }

        [DisplayAttributes(15, "Locked", true, ComponentType.Booleano, "", GroupTypes.None, false, true, false, levelFilterType: LevelFilterType.ThirdLevel)]
        public bool isBlock { get; set; }

        [DisplayAttributes(16, "Locked time", true, ComponentType.CommetText, "", GroupTypes.None, false, false, false, levelFilterType: LevelFilterType.ThirdLevel)]
        public string? EndBlockDate
		{
			get
			{
				return _blocktask != DateTimeOffset.MinValue ? 
					 _blocktask.DateTime.ToUserTime(_userFormat.HourFormat) :
                 null;
			}
		}

        [DisplayAttributes(17, "Resource", true, ComponentType.None, "", GroupTypes.None, false, false, false, levelFilterType: LevelFilterType.FourthLevel)]
        public string? Resource { get; set; }

        [DisplayAttributes(18, "Supplier", true, ComponentType.None, "", GroupTypes.None, false, false, false, levelFilterType: LevelFilterType.ThirdLevel)]
        public string? Supplier { get; set; }

        [DisplayAttributes(19, "Alerts", true, ComponentType.Booleano, "", GroupTypes.None, false, true, true, levelFilterType: LevelFilterType.ThirdLevel)]
        public bool HasAlerts { get; set; } = false;

        [DisplayAttributes(20, "Carrier", true, ComponentType.None, "", GroupTypes.None, false, false, false, levelFilterType: LevelFilterType.ThirdLevel)]
        public string? Carrier { get; set; }

        // [DisplayAttributes(20, "Order delay", true, ComponentType.None, "", GroupTypes.None, false, false, false)]
        public string? DelayedOrder { get; set; }

        [DisplayAttributes(21, "Release time", true, ComponentType.CommetText, "", GroupTypes.None, false, false, false, levelFilterType: LevelFilterType.ThirdLevel)]
        public string ReleaseDate
		{
			get
			{
				return _releaseDate != DateTimeOffset.MinValue ?
					 _releaseDate.DateTime.ToUserTime(_userFormat.HourFormat) :
				 string.Empty;
			}
		}

		[DisplayAttributes(22, "Creation time", true, ComponentType.CommetText, "", GroupTypes.None, false, false, false, levelFilterType: LevelFilterType.ThirdLevel)]
        public string CreationDate
		{
			get
			{
				return _creationDate != DateTimeOffset.MinValue ?
					 _creationDate.DateTime.ToUserTime(_userFormat.HourFormat) :
				 string.Empty;
			}
		}


		#region Campos SLA

		[DisplayAttributes(23, "Order delay (min)", true, ComponentType.None, "", GroupTypes.None, false, true, false, levelFilterType: LevelFilterType.ThirdLevel)]
        public string TimeOrderDelay { get; set; }

        [DisplayAttributes(25, "Actual work time (min)", true, ComponentType.None, "", GroupTypes.None, false, true, false, levelFilterType: LevelFilterType.ThirdLevel)]
        public string ActualWorkTime { get; set; }

        [DisplayAttributes(26, "SLA target time", true, ComponentType.None, "", GroupTypes.None, false, true, false, levelFilterType: LevelFilterType.ThirdLevel)]
        public string SLATargetTime
		{
			get { return _slaDate != DateTimeOffset.MinValue ? _slaDate.ToUserTime(_userFormat.HourFormat) : string.Empty; }
		}

		public DateTimeOffset SLATargetDateTime
		{
			get => _slaDate;
			set => _slaDate = UtcToTimeZoneOffSet(value);
		}

		[DisplayAttributes(27, "SLA met", true, ComponentType.None, "", GroupTypes.None, false, true, false, traslateCaption: true, levelFilterType: LevelFilterType.ThirdLevel)]
        public string SLAMet { get; set; }

        [DisplayAttributes(29, "Shift", true, ComponentType.None, "", GroupTypes.None, false, true, false, levelFilterType: LevelFilterType.FourthLevel)]
        public string Shift { get; set; }

        public string HandlingUnitType { get; set; }

        public string AssignedResources { get; set; }

        #endregion


        public bool? isParentData { get; set; } = false;

        public DateTimeOffset blockDate { get; set; }

        public virtual void FillDataOrder(InputOrder inputOrder, EnumViewPlanning granularity)
        {
            this.title = inputOrder?.OrderCode ?? string.Empty;
            this.isBlock = inputOrder?.IsBlocked ?? false;
            this.Status = inputOrder?.Status ?? string.Empty;
            this.Customer = inputOrder?.Account ?? string.Empty;
            this.Priority = granularity != EnumViewPlanning.Priority ? inputOrder?.Priority ?? string.Empty : string.Empty;
            this.Trailer = granularity != EnumViewPlanning.Trailer ? inputOrder?.VehicleCode ?? string.Empty : string.Empty;
            this.BlockTaskTime = inputOrder?.BlockDate ?? DateTimeOffset.MinValue;
            this.ReleaseDateTime = inputOrder?.ReleaseDate ?? DateTimeOffset.MinValue;
            this.CreationDateTime = inputOrder?.CreationDate ?? DateTimeOffset.MinValue;
            this.VehicleDate = inputOrder?.AppointmentDate ?? this.EndDate;
            this.CommintedDate = inputOrder?.AppointmentDate ?? this.EndDate;
        }

        public virtual void FillDataActivity(InputOrder inputOrder)
        {
            this.isBlock = inputOrder?.IsBlocked ?? false;
            this.Supplier = inputOrder?.Supplier ?? string.Empty;
            this.Carrier = inputOrder?.Carrier ?? string.Empty;
        }

        private DateTimeOffset _vehicleDate;
        public DateTimeOffset VehicleDate
        {
            get => _vehicleDate;
            set => _vehicleDate = UtcToTimeZoneOffSet(value);
        }

        public virtual string VehicleHour => VehicleDate.ToUserTime(_userFormat.HourFormat);

    }
}
