namespace Mss.WorkForce.Code.Models.DTO
{

    public class ScheduleWorkerDto : PenelEditorOperations
    {
        #region Properties

        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid WorkerId { get; set; }
        public string WorkerName { get; set; }
        public int WorkerNumber { get; set; }
        public Guid RolId { get; set; }
        public Guid? AvailableWorkerId { get; set; }
        public Guid? BreakProfileId { get; set; }
        public Guid? ShiftId { get; set; }
        public OperationType DataOperationType { get; set; }
        public double? CustomInitHour{ get; set; }
        public double? CustomEndHour { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public object GetProperty(string propertyName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
