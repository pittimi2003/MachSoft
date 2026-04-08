namespace Mss.WorkForce.Code.Models.ModelUpdate
{
    public class Update
    {
        public List<OrderScheduleUpdate> OrderSchedule { get; set; }
        public List<AvailableWorkerUpdate> AvailableWorker { get; set; }
        public List<EquipmentGroupUpdate> EquipmentGroup { get; set; }
        public List<ShiftUpdate> Shift { get; set; }
        public List<ScheduleWorkerUpdate> ScheduleWorker { get; set; }
        public List<ScheduleUpdate> Schedule { get; set; }
    }
}
