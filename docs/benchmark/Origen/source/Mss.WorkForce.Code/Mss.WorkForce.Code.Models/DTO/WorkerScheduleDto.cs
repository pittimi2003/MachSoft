using System.Xml.Linq;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class WorkerScheduleDto : IBaseModel, ICloneable
    {

        #region Properties

        public Guid  BreakProfileId { get; set; }
        public string BreakProfileName { get; set; } = string.Empty;
        public Guid RolId { get; set; }
        public string RolName { get; set; } = string.Empty;
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid ShiftId { get; set; }
        public string ShiftName { get; set; } = string.Empty ;
        public Guid WorkerId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public OperationType DataOperationType { get; set; }
        public bool IsDependencies { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #endregion

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not WorkerScheduleDto order)
                return false;

            return BreakProfileId == order.BreakProfileId &&
                RolId == order.RolId &&
                Id == order.Id &&
                ShiftId == order.ShiftId &&
                WorkerId == order.WorkerId;
        }

        public bool GetDependencies()
        {
            throw new NotImplementedException();
        }

        public object GetProperty(string propertyName)
        {
            throw new NotImplementedException();
        }

        public void SetDependencies(bool val)
        {
            throw new NotImplementedException();
        }
    }
}
