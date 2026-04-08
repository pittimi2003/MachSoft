namespace Mss.WorkForce.Code.Models.Models
{
    public class Flow: ICloneable
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public Warehouse Warehouse { get; set; }

        public Guid WarehouseId { get; set; }

        public object Clone()
        {
            Flow flow = (Flow)this.MemberwiseClone();
            flow.Id = Guid.NewGuid();
            return flow;
        }
    }
}
