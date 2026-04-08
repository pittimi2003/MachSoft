using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class EquipmentGroup : IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public int Equipments { get; set; }
        public Guid TypeEquipmentId { get; set; }
        public required TypeEquipment TypeEquipment { get; set; }
        public Guid AreaId { get; set; }
        public required Area Area { get; set; }
        public string? ViewPort { get; set; }
        public void Fill(ApplicationDbContext context)
        {
            this.TypeEquipment = context.TypeEquipment.FirstOrDefault(x => x.Id == TypeEquipmentId)!;
            this.Area = context.Areas.FirstOrDefault(x => x.Id == AreaId)!;
        }
        public object Clone()
        {
            EquipmentGroup clonedEquipmentGroup = (EquipmentGroup)this.MemberwiseClone();
            clonedEquipmentGroup.Id = Guid.NewGuid();
            return clonedEquipmentGroup;
        }
    }
}