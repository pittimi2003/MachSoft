namespace Mss.WorkForce.Code.Models.Models
{
    public class Deliveries
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string PackagingType { get; set; }
        public int? NumPackages { get; set; }
    }
}
