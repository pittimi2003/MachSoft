using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.Interface;
using Mss.WorkForce.Code.Models.ModelGantt.DataGanttPlanning;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Models.ModelGantt
{
    public abstract class GanttTaskBase : ITaskForGantt, IColorTask
    {
        public UserFormatOptions _userFormat;

		protected GanttTaskBase()
        {
            _userFormat = new UserFormatOptions();
        }

        protected GanttTaskBase(UserFormatOptions userFormat)
        {
            _userFormat = userFormat;
        }


        private DateTimeOffset _startDate;
        private DateTimeOffset _endDate;
        private DateTimeOffset _commintDate;

		public DateTimeOffset _blocktask;
		public DateTimeOffset _releaseDate;
		public DateTimeOffset _creationDate;

		public Guid id { get; set; }
        public Guid? parentId { get; set; }
        public int index { get; set;}
        public int levelTask { get; set; }
        public string? title { get; set; }
		public virtual bool Multiselect { get; set; }

        [DisplayAttributes(5, "Start date", true, ComponentType.Date, "", GroupTypes.None, false, true, true)]
        public virtual string start
        {
            get { return _startDate != DateTimeOffset.MinValue ? GetFormatDateForGantt(_startDate.DateTime) : null; }
        }

        public string DateTimeFormatt => _userFormat.FullDate;
        public char DecimalSeparator => _userFormat.DecimalSeparator;

        [DisplayAttributes(10, "End date", true, ComponentType.Date, "", GroupTypes.None, false, false, false, levelFilterType: LevelFilterType.AllLevels)]
        public virtual string end 
        {
            get { return _endDate != DateTimeOffset.MinValue ? GetFormatDateForGantt(_endDate.DateTime) : null;}
        }

        [DisplayAttributes(11, "Progress (%)", true, ComponentType.CommetText, "", GroupTypes.None, false, false, false, levelFilterType: LevelFilterType.AllLevels)]
        [MeasureAttributes(MeasuresType.Percent)]
        public int progress { get; set; }

        public bool IsOutbound { get; set; }

        private GanttTooltipType tooltipType;

        public GanttTooltipType TooltipType
        {
            get { return this.tooltipType; }
            set
            {
                this.tooltipType = value;
                SelectColorType(value);
            }
        }

        public List<SegmentDto>? segments { get; set; }
        public bool FillProgress { get; set; } = false;
        public bool IsChildTask { get; set; }
        public bool isOnTime { get; set; } = true;
		public bool isExpand { get; set; } = false;
		public bool isOffOverlay { get; set; } = true;

        public virtual string? WorkFlow { get; set; }

		/// <summary>
		/// Este bloque de fechas se ocupa para realizar calculos, no se muestran en el gantt ni en el selector de columnas
		/// </summary>
		public DateTimeOffset StartDate
        {
            get => _startDate;
            set => _startDate = UtcToTimeZoneOffSet(value);
        }

        public DateTimeOffset EndDate 
        { 
            get => _endDate;
            set => _endDate = UtcToTimeZoneOffSet(value);
        }

        public DateTimeOffset CommintedDate 
        { 
            get => _commintDate; 
            set => _commintDate = UtcToTimeZoneOffSet(value); 
        }

		public DateTimeOffset BlockTaskTime
		{
			get => _blocktask;
			set => _blocktask = UtcToTimeZoneOffSet(value);
		}

		public DateTimeOffset ReleaseDateTime
		{
			get => _releaseDate;
			set => _releaseDate = UtcToTimeZoneOffSet(value);
		}

		public DateTimeOffset CreationDateTime
		{
			get => _creationDate;
			set => _creationDate = UtcToTimeZoneOffSet(value);
		}

		/// <summary>
		/// Color barra de progreso tarea gantt
		/// </summary>
		public string color { get; set; } = string.Empty;
        public string BackgroundColorRow { get; set; } = string.Empty;

        [DisplayAttributes(6, "Committed time", true, ComponentType.DateTime, "", GroupTypes.None, false, true, true, levelFilterType: LevelFilterType.ThirdLevel)]
        public virtual string CommittedHour => CommintedDate.ToUserTime(_userFormat.HourFormat);

        public string? ActivityTitle { get; set; }
        public string? Resource { get; set; }
        public string? Customer { get; set; }
        public string? Status { get; set; }
        public virtual string? Priority { get; set; }
        public string? Supplier { get; set; }
        public string? Carrier { get; set; }
        public string? DelayedOrder { get; set; }
        public string? WorkTime { get; set; }
        public string? EquipmentGroup { get; set; }
        public string? EquipmentType { get; set; }
        public int TotalOperators { get; set; }
        public int TotalEquipments { get; set; }
        public bool isEstimated { get; set; }
        public object ProcessEstimated { get; set; }
        public Guid? InputOrderId { get; set; }        

        public bool ActionBlock { get; set; }

        public bool ActionCancel { get; set; }

        public bool ActionUnlock { get; set; }

        public bool ActionPriority { get; set; }

        public IEnumerable<AlertMessageDto> Alerts { get; set; }        

        public virtual void SelectColorType(GanttTooltipType tooltipType)
        {
            switch (tooltipType)
            {
                case GanttTooltipType.PlanningGeneral:
                case GanttTooltipType.LaborWorkerGeneral:
                case GanttTooltipType.LaborEquipmentGeneral:
                case GanttTooltipType.PlanningPriority:
                case GanttTooltipType.PlanningEstimation:
                    this.BackgroundColorRow = GanttColors.DefaultColor;
                    this.color = GanttColors.Smoke800Color;
                    break;
                case GanttTooltipType.PlanningEstimationIn:
                case GanttTooltipType.PlanningEstimationInOrder:
                    this.BackgroundColorRow = GanttColors.Green100Color;
                    this.color = GanttColors.Green800Color;
                    break;
                case GanttTooltipType.PlanningEstimationOut:
                case GanttTooltipType.PlanningEstimationOutOrder:
                    this.BackgroundColorRow = GanttColors.BlueColor;
                    this.color = GanttColors.Blue600Color;
                    break;
                case GanttTooltipType.PlanningWarehouseOrder:
                case GanttTooltipType.PlanningWarehouse:
                    this.BackgroundColorRow = GanttColors.OrangeColor;
                    this.color = GanttColors.OrangeColor;
                    break;
                case GanttTooltipType.YardGeneral:
                    this.color = GanttColors.Smoke800Color;
                    this.BackgroundColorRow = GanttColors.DefaultColor;
                    break;
                case GanttTooltipType.PlanningFlow:
                case GanttTooltipType.PlanningOrder:
                case GanttTooltipType.LaborEquipmentOrder:
                case GanttTooltipType.LaborWorkerOrder:
                    this.color = this.IsOutbound ? GanttColors.Blue600Color : GanttColors.Green800Color;
                    this.BackgroundColorRow = this.IsOutbound ? GanttColors.BlueColor : GanttColors.Green100Color;
                    this.WorkFlow = FillWorkFlow(this.IsOutbound);
                    break;

                case GanttTooltipType.LaborEquipmentActivity:
                case GanttTooltipType.LaborWorkerActivity:
                case GanttTooltipType.PlanningActivity:
                    this.color = this.IsOutbound ? GanttColors.Blue600Color : GanttColors.Green800Color;
                    this.BackgroundColorRow = GanttColors.None;
                    break;

                case GanttTooltipType.PlanningTrailer:
                    this.BackgroundColorRow = GanttColors.DefaultColor;
                    this.WorkFlow = FillWorkFlow(this.IsOutbound);
                    break;

                default:
                    this.BackgroundColorRow = GanttColors.None;
                    this.color = GanttColors.Smoke800Color;
                    break;
            }
        }

        public virtual string FillWorkFlow(bool isOut)
        {
            return isOut ? "OUTPUT PROFILE" : "INPUT PROFILE";
        }

        public virtual DateTimeOffset UtcToTimeZone(DateTimeOffset dateUtc)
        {
            TimeZoneInfo timeZoneInfo;
            try
            {
                timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(_userFormat.TimeZoneId);
                return TimeZoneInfo.ConvertTime(dateUtc, timeZoneInfo);
            }
            catch (TimeZoneNotFoundException)
            {
                return dateUtc;
            }
        }

        public virtual DateTimeOffset UtcToTimeZoneOffSet(DateTimeOffset dateUtc)
        {
            if (dateUtc == DateTimeOffset.MinValue)
                return dateUtc;

            var hours = Math.Clamp(_userFormat.TimeZoneOffSet, -14, 14);
            try
            {
                return dateUtc.AddHours(hours);
            }
            catch
            {
                return DateTimeOffset.MinValue;
            }
        }

        public virtual string GetFormatDateForGantt(DateTime dateTime) => $"new Date({dateTime.Year}, {dateTime.Month - 1} ,  {dateTime.Day}, {dateTime.Hour}, {dateTime.Minute}, {dateTime.Second})";

	}
}
