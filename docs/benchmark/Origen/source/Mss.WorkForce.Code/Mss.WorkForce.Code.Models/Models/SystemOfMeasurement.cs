namespace Mss.WorkForce.Code.Models.Models
{
    public class SystemOfMeasurement
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}
