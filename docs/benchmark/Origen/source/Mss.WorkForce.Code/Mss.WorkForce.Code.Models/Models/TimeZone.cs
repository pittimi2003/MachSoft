namespace Mss.WorkForce.Code.Models.Models
{
    public class TimeZone
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double OffSet { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
