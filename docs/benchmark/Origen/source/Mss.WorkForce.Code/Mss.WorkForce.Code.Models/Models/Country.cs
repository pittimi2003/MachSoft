namespace Mss.WorkForce.Code.Models.Models
{
    public class Country
    {
        public Guid Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}
