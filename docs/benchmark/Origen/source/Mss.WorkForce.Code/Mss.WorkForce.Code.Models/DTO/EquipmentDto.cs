

namespace Mss.WorkForce.Code.Models.DTO
{
    public class EquipmentDto : IDataOperation, ICloneable
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid EquipmentId { get; set; }
        public int Equipments { get; set; }
        public Guid EquipmentTypeId { get; set; }
        public string EquipmentTypeName { get; set; } = string.Empty;
        public Guid AreaId { get; set; }
        public string AreaName { get; set; } = string.Empty;
        public OperationType DataOperationType { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not EquipmentDto order)
                return false;

            return Id == order.Id &&
                Name == order.Name &&
                Equipments == order.Equipments &&
                EquipmentTypeName == order.EquipmentTypeName &&
                AreaName == order.AreaName;
        }


    }
}
