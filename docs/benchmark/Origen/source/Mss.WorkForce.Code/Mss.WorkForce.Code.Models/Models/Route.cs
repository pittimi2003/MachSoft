using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.Models.Models
{
    public class Route : IFillable, ICloneable
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public Guid DepartureAreaId { get; set; }
        public required Area DepartureArea { get; set; }
		public Guid ArrivalAreaId { get; set; }
		public required Area ArrivalArea { get; set; }
		public bool Bidirectional { get; set; }
        public int? TimeMin { get; set; }
        public string? ViewPort { get; set; }
        public void Fill(ApplicationDbContext context)
        {
            this.DepartureArea = context.Areas.FirstOrDefault(x => x.Id == DepartureAreaId)!;
            this.ArrivalArea = context.Areas.FirstOrDefault(x => x.Id == ArrivalAreaId)!;
        }
        public object Clone()
        {
            Route clonedRoute = (Route)this.MemberwiseClone();
            clonedRoute.Id = Guid.NewGuid();
            return clonedRoute;
        }
    }
}
